using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Netkraft.Messaging
{
    /// <summary>
    /// Any class or struct that inherits this Interface needs to inherit from <see cref="IReliableMessage"/> / <see cref="IUnreliableMessage"/> interfaces or needs the <see cref="Writable"/> attribute.
    /// <para></para>
    /// </summary>
    public interface IDeltaCompressed
    {
        void OnAcknowledgment(ClientConnection Context);
    }

    public static class WritableSystem
    {
        static WritableSystem()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //Scan trough assembly and find all methods with a write or read attribute in the domain.
            AddAllWritableFieldTypes(assemblies);

            //Get all Writables and IMessage types in the current domain
            List<Type> WritableTypes = new List<Type>();
            foreach (Assembly a in assemblies)
                WritableTypes.AddRange(a.GetTypes().Where(x => TypeIsWritable(x)));

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
                || typeof(IReliableMessage).IsAssignableFrom(t);
        }
        private static void AddAllWritableFieldTypes(Assembly[] assemblies)
        {
            
            foreach (Assembly a in assemblies)
            {
                MethodInfo[] WriteMethods = a.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)).Where(m => m.GetCustomAttributes(typeof(WritableFieldTypeWrite), false).Length > 0).ToArray();
                MethodInfo[] ReadMethods = a.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)).Where(m => m.GetCustomAttributes(typeof(WritableFieldTypeRead), false).Length > 0).ToArray();
                //Add types. Find method for same type if not both are declared type is ignored
                Dictionary<Type, MethodInfo> writeMethodsByType = new Dictionary<Type, MethodInfo>();
                foreach (MethodInfo wm in WriteMethods)
                {
                    Type wt = ((WritableFieldTypeWrite)wm.GetCustomAttribute(typeof(WritableFieldTypeWrite), false)).type;
                    if(!writeMethodsByType.ContainsKey(wt))
                        writeMethodsByType.Add(wt, wm);
                }

                foreach (MethodInfo rm in ReadMethods)
                {
                    Type rt = ((WritableFieldTypeRead)rm.GetCustomAttribute(typeof(WritableFieldTypeRead), false)).type;
                    if (writeMethodsByType.ContainsKey(rt))
                        AddSuportedType(rt, (Action<Stream, object>)writeMethodsByType[rt].CreateDelegate(typeof(Action<Stream, object>)), (Func<Stream, object>)rm.CreateDelegate(typeof(Func<Stream, object>)));
                }
#if DEBUG
                //Write out warnings and errors
                Dictionary<Type, string> errorByType = new Dictionary<Type, string>();
                List<string> generalErrors = new List<string>();

                //Write function errors
                foreach (MethodInfo wm in WriteMethods)
                {
                    Type wt = ((WritableFieldTypeWrite)wm.GetCustomAttribute(typeof(WritableFieldTypeWrite), false)).type;

                    //Find errors
                    if(!errorByType.ContainsKey(wt))
                        errorByType.Add(wt, $"The Write function for {wt.Name} does not have an opposing Read function!");
                    else
                    {
                        generalErrors.Add($"Write function for {wt.Name} Has multiple delcarations which is illegal");
                        continue;
                    }

                    if (wm.GetParameters().Length != 2)
                    {
                        generalErrors.Add($"Write function for {wt.Name} wrong number of paramters. Only accepts methods of void (Stream, object)");
                        continue;
                    }

                    if (!wm.ReturnType.IsAssignableFrom(typeof(void)))
                        generalErrors.Add($"Write function for {wt.Name} needs to return void. Not {wm.ReturnType.Name}");

                    if (!wm.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(Stream)))
                    {
                        Type pt = wm.GetParameters()[0].ParameterType;
                        generalErrors.Add($"First parameter of Write function for {wt.Name} has to be of type Stream. Not {pt.Name}");
                    }

                    if (!wm.GetParameters()[1].ParameterType.IsAssignableFrom(typeof(object)))
                    {
                        Type pt = wm.GetParameters()[1].ParameterType;
                        generalErrors.Add($"Second parameter of Write function for {wt.Name} has to be of type object. Not {pt.Name}");
                    } 
                }
                //Read function errors
                HashSet<Type> duplicateCheck = new HashSet<Type>();
                foreach (MethodInfo rm in ReadMethods)
                {
                    Type rt = ((WritableFieldTypeRead)rm.GetCustomAttribute(typeof(WritableFieldTypeRead), false)).type;

                    if(!duplicateCheck.Contains(rt))
                        duplicateCheck.Add(rt);
                    else
                    {
                        generalErrors.Add($"Read function for type {nameof(rt)} Has multiple delcarations which is illegal");
                        continue;
                    }

                    //Find errors
                    if (rm.GetParameters().Length != 1)
                    {
                        generalErrors.Add($"Read function for type {nameof(rt)} wrong number of paramters. Only accepts methods of void (Stream)");
                        continue;
                    }

                    if (!rm.ReturnType.IsAssignableFrom(typeof(object)))
                        generalErrors.Add($"Read function for type {nameof(rt)} needs to return object. Not {nameof(rm.ReturnType)}");

                    if (!rm.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(Stream)))
                    {
                        Type pt = rm.GetParameters()[0].ParameterType;
                        generalErrors.Add($"First parameter of Read function for type {nameof(rt)} has to be of type Stream. Not {nameof(pt)}");
                    }
                }

                foreach (MethodInfo rm in ReadMethods)
                {
                    Type rt = ((WritableFieldTypeRead)rm.GetCustomAttribute(typeof(WritableFieldTypeRead), false)).type;
                    if (writeMethodsByType.ContainsKey(rt))
                        errorByType.Remove(rt);
                    else
                        errorByType.Add(rt, $"The Read function for type {nameof(rt)} does not have an opposing Write function!");
                }
                //Read out general errors
                foreach (string s in generalErrors)
                    throw new Exception(s);

                //Read out missmatches
                foreach (string s in errorByType.Values)
                    throw new Exception(s);
#endif
            }
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
            //Console.WriteLine("Add supported type: " + type.Name);
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
    //Attributes
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
    /// <summary>
    /// This attributes describes where delta compression will start in a Message or Writable hat inherits from <see cref="IDeltaCompressed"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DeltaCompress : Attribute { }
    /// <summary>
    /// Assign a method to write an object of <see cref="Type"/> to a <see cref="Stream"/>. 
    /// <see cref="WritableFieldTypeWrite"/> can only be applied to methods that are public and take the parameters <see cref="Stream"/> and <see cref="object"/>. 
    /// The method should return Void.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class WritableFieldTypeWrite : Attribute
    {
        public Type type;
        public WritableFieldTypeWrite(Type type)
        {
            this.type = type;
        }
    }
    /// <summary>
    /// Assign a method to read an object of <see cref="Type"/> from a <see cref="Stream"/>. 
    /// <see cref="WritableFieldTypeRead"/> can only be applied to methods that are public and take the parameters <see cref="Stream"/>. 
    /// The method should return the <see cref="object"/> read from the stream
    /// </summary>
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