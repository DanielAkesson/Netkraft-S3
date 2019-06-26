using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace NetkraftMessage
{
    public static class WritableSystem
    {
        static WritableSystem()
        {
            ByteConverter.AddBasicTypes();
            //Get all Writables and Im´Message types in the current Domain
            List<Type> WritableTypes = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
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
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((uint)array.Length);//Place header for array length
            for (int i=0; i< array.Length; i++)
                BinaryFunctions[typeof(T)].writer(stream, array[i]);
        }
        private static object ReadArray<T>(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            uint length = reader.ReadUInt32();//Read header for array length
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
                array[i] = (T)BinaryFunctions[typeof(T)].reader(stream);
            return array;
        }

        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
        public class SkipIndex : System.Attribute { }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class Writable : System.Attribute { }
}