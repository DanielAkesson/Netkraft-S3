using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Netkraft.Messaging
{
    public static class WritableSystem
    {
        static WritableSystem()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //Add all writable field types
            foreach (Assembly a in assemblies)
            {
                MethodInfo[] WriteMethods = a.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)).Where(m => m.GetCustomAttributes(typeof(WritableFieldTypeWrite), false).Length > 0).ToArray();
                MethodInfo[] ReadMethods = a.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)).Where(m => m.GetCustomAttributes(typeof(WritableFieldTypeRead), false).Length > 0).ToArray();
                //TODO: dictionaries for write/read by type to avoid double loop
                //Add types. Find method for same type if not both are declared type is ignored
                foreach(MethodInfo wm in WriteMethods)
                {
                    Type wt = ((WritableFieldTypeWrite)wm.GetCustomAttribute(typeof(WritableFieldTypeWrite), false)).type;
                    foreach (MethodInfo rm in ReadMethods)
                    {
                        Type rt = ((WritableFieldTypeRead)rm.GetCustomAttribute(typeof(WritableFieldTypeRead), false)).type;
                        if (rt == wt)
                            AddSuportedType(rt, (Action<Stream, object>)wm.CreateDelegate(typeof(Action<Stream, object>)), (Func<Stream, object>)rm.CreateDelegate(typeof(Func<Stream, object>)));
                    }
                }
            }

            //Get all Writables and IMessage types in the current Domain
            List<Type> WritableTypes = new List<Type>();
            foreach (Assembly A in assemblies)
                WritableTypes.AddRange(A.GetTypes().Where(x => TypeIsWritable(x)));

            //Add each writable type as it's own writabletype!
            foreach (Type t in WritableTypes)
                AddSuportedType(t, (s, o) => Write(s, o), (Func<Stream, object>)_readReadInternal.MakeGenericMethod(t).CreateDelegate(typeof(Func<Stream, object>)));
                
            //Do calculations for field members
            foreach (Type t in WritableTypes)
                AddWritable(t);
        }
        private static bool TypeIsWritable(Type t)
        {
            return t.GetCustomAttribute(typeof(Writable)) != null 
                || typeof(IUnreliableMessage).IsAssignableFrom(t)
                || typeof(IUnreliableAcknowledgedMessage).IsAssignableFrom(t)
                || typeof(IReliableMessage).IsAssignableFrom(t)
                || typeof(IReliableAcknowledgedMessage).IsAssignableFrom(t);
        }

        //Binary Reader-Writer
        private static readonly int _supportedArrayDimensionDepth = 4;
        private static Dictionary<Type, (Action<Stream, object> writer, Func<Stream, object> reader)> BinaryFunctions = new Dictionary<Type, (Action<Stream, object> writer, Func<Stream, object> reader)>();
        private static Dictionary<Type, List<FieldInfo>> MetaInformation = new Dictionary<Type, List<FieldInfo>>();
        private static Dictionary<Type, Func<Stream, object>> GenericReadFunctions = new Dictionary<Type, Func<Stream, object>>();
        
        //private method infos for generic methods
        private static readonly MethodInfo _writeArrayMetod = typeof(WritableSystem).GetMethod("WriteArray", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _readArrayMetod = typeof(WritableSystem).GetMethod("ReadArray", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _readReadInternal = typeof(WritableSystem).GetMethod("ReadInternal", BindingFlags.Static | BindingFlags.NonPublic);
        
        //public setting methods
        public static void AddSuportedType<T>(Action<Stream, object> writerFunction, Func<Stream, object> readerFunction)
        {
            AddSuportedType(typeof(T), writerFunction, readerFunction);
        }
        public static void AddSuportedType(Type type, Action<Stream, object> writerFunction, Func<Stream, object> readerFunction)
        {
            Console.WriteLine("Add supported type: " + type.Name);
            //Normal type support
            BinaryFunctions.Add(type, (writerFunction, readerFunction));
            //X dimension array support
            Type ArrayCarryType = type;
            Action<Stream, object> writeMultiDelegate;
            Func<Stream, object> readMultiDelegate;
            for (int i = 0; i< _supportedArrayDimensionDepth; i++)
            {
                //Standard multi array
                writeMultiDelegate = (Action<Stream, object>)_writeArrayMetod.MakeGenericMethod(ArrayCarryType).CreateDelegate(typeof(Action<Stream, object>));
                readMultiDelegate = (Func<Stream, object>)_readArrayMetod.MakeGenericMethod(ArrayCarryType).CreateDelegate(typeof(Func<Stream, object>));
                BinaryFunctions.Add(ArrayCarryType.MakeArrayType(), (writeMultiDelegate, readMultiDelegate));
                ArrayCarryType = ArrayCarryType.MakeArrayType();
            }
        }
        
        //Public methods
        public static object Write(Stream stream, object obj)
        {
            try
            {
                List<FieldInfo> metaData = MetaInformation[obj.GetType()];
                foreach (FieldInfo fi in metaData)
                    BinaryFunctions[fi.FieldType].writer(stream, fi.GetValue(obj));
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); throw e; }
            return obj;
        }
        public static T Read<T>(Stream stream)
        {
            return (T)ReadInternal<T>(stream);
        }
        public static object Read(Stream stream, Type writableType)
        {
            return GenericReadFunctions[writableType](stream);
        }

        //Private methods
        private static object ReadInternal<T>(Stream stream)
        {
            object data = FormatterServices.GetUninitializedObject(typeof(T));
            try
            {
                List<FieldInfo> metaData = MetaInformation[typeof(T)];
                foreach (FieldInfo fi in metaData)
                    fi.SetValue(data, BinaryFunctions[fi.FieldType].reader(stream));
            }
            catch (Exception e) { throw e; }
            return (T)data;
        }
        private static void AddWritable(Type writableType)
        {
            if (MetaInformation.ContainsKey(writableType)) return;
            List<FieldInfo> fields = new List<FieldInfo>();
            fields.AddRange(writableType.GetFields().Where(x => ValidateFieldInfo(x) && x.DeclaringType == writableType).ToList());
            MetaInformation.Add(writableType, fields);
            GenericReadFunctions.Add(writableType, (Func<Stream, object>)_readReadInternal.MakeGenericMethod(writableType).CreateDelegate(typeof(Func<Stream, object>)));
        }
        private static bool ValidateFieldInfo(FieldInfo info)
        {
            SkipIndex attribute = info.GetCustomAttribute<SkipIndex>();
            Type t = info.FieldType;
            return attribute == null && !info.IsStatic && BinaryFunctions.ContainsKey(t);
        }
        private static void WriteArray<T>(Stream stream, object value)
        {
            T[] array = (T[])value;
            BinaryWriter writer = new BinaryWriter(stream); //TODO: use bitconverter
            writer.Write((uint)array.Length);//Place header for array length
            for (int i=0; i< array.Length; i++)
                BinaryFunctions[typeof(T)].writer(stream, array[i]);
        }
        private static object ReadArray<T>(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream); //TODO: use bitcovnerter
            uint length = reader.ReadUInt32();//Read header for array length
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
                array[i] = (T)BinaryFunctions[typeof(T)].reader(stream);
            return array;
        }   
    }
    /// <summary>
    /// If added to a class or struct all public fields will be writable to a byte array by <see cref="WritableSystem"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class Writable : Attribute { }

    /// <summary>
    /// If added above a field inside a <see cref="Writable"/> or <see cref="Message"/> Inteface said field will not be included when sent by <see cref="NetkraftClient"/> or writen to byte array by <see cref="WritableSystem"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class SkipIndex : Attribute{}

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class WritableFieldTypeWrite : Attribute
    {
        public Type type;
        public WritableFieldTypeWrite(Type type)
        {
            this.type = type;
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class WritableFieldTypeRead : Attribute
    {
        public Type type;
        public WritableFieldTypeRead(Type type)
        {
            this.type = type;
        }
    }
}