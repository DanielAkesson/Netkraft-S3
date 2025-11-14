using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Netkraft.WritableSystem
{
    //Interfaces
    /// <summary>
    /// Any class or struct that inherits <see cref="IWritable"/> will be supported by the <see cref="Writable"/> and can be read from or writen to a stream.
    /// <para></para>
    /// </summary>
    public interface IWritable{}
    public static class Writable
    {
        static Writable()
        {
            if (WritableHashSettings.HashSeed < 0) { WritableHashSettings.HashSeed = 0; }
            Trace.WriteLine("Initialize writable system");
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //Scan trough assembly and find all methods with a write or read attribute in the domain.
            AddAllWritableFieldTypes(assemblies);

            //Get all Writable and IMessage types in the current domain
            List<Type> WritableTypes = new List<Type>();
            foreach (Assembly a in assemblies)
                WritableTypes.AddRange(a.GetTypes().Where(x => TypeIsWritable(x)));

            //Add each writable type as it's own writable type!
            foreach (Type t in WritableTypes)
                AddSuportedType(t, (s, o) => WriteWritable(s, o), (s) => ReadWritable(s, t));

            //Special read and write for Objects
            object WriteWritable(Stream stream, object obj)
            {
                try
                {
                    //obj is a Writable type
                    if (MetaInformation.ContainsKey(obj.GetType()))
                    {
                        List<FieldInfo> metaDataField = MetaInformation[obj.GetType()].fields;
                        foreach (FieldInfo fi in metaDataField)
                            BinaryFunctions[fi.FieldType].writer(stream, fi.GetValue(obj));
                        List<PropertyInfo> metaDataProps = MetaInformation[obj.GetType()].properties;
                        foreach (PropertyInfo pro in metaDataProps)
                            BinaryFunctions[pro.PropertyType].writer(stream, pro.GetValue(obj));
                        return obj;
                    }
                }
                catch (Exception e) { Console.WriteLine(e.StackTrace); throw e; }
                return obj;
            }
            object ReadWritable(Stream stream, Type writableType)
            {
                //TODO: These objects can be initialized on start and reused here!
                object data = FormatterServices.GetUninitializedObject(writableType);
                try
                {
                    //obj is a Writable or IMessage type
                    if (MetaInformation.ContainsKey(writableType))
                    {
                        List<FieldInfo> metaDataField = MetaInformation[writableType].fields;
                        foreach (FieldInfo fi in metaDataField)
                            fi.SetValue(data, BinaryFunctions[fi.FieldType].reader(stream));
                        List<PropertyInfo> metaDataProps = MetaInformation[writableType].properties;
                        foreach (PropertyInfo pro in metaDataProps)
                            pro.SetValue(data, BinaryFunctions[pro.PropertyType].reader(stream));
                        return data;
                    }
                }
                catch (Exception e) { Console.WriteLine(e.StackTrace); throw e; }
                return data;
            }

            //Do calculations for field members
            foreach (Type t in WritableTypes)
                AddWritable(t);
        }
        private static bool TypeIsWritable(Type t)
        {
            return typeof(IWritable).IsAssignableFrom(t);
        }
        private static void AddAllWritableFieldTypes(Assembly[] assemblies)
        {
            foreach (Assembly a in assemblies)
            {
                MethodInfo[] WriteMethods = a.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)).Where(m => m.GetCustomAttributes(typeof(WriteFunction), false).Length > 0).ToArray();
                MethodInfo[] ReadMethods = a.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)).Where(m => m.GetCustomAttributes(typeof(ReadFunction), false).Length > 0).ToArray();
                //Add types. Find method for same type if not both are declared type is ignored
                Dictionary<Type, MethodInfo> writeMethodsByType = new Dictionary<Type, MethodInfo>();
                foreach (MethodInfo wm in WriteMethods)
                {
                    Type wt = ((WriteFunction)wm.GetCustomAttribute(typeof(WriteFunction), false)).type;
                    if (!writeMethodsByType.ContainsKey(wt))
                        writeMethodsByType.Add(wt, wm);
                }
                foreach (MethodInfo rm in ReadMethods)
                {
                    Type rt = ((ReadFunction)rm.GetCustomAttribute(typeof(ReadFunction), false)).type;
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
                    Type wt = ((WriteFunction)wm.GetCustomAttribute(typeof(WriteFunction), false)).type;

                    //Find errors
                    if (!errorByType.ContainsKey(wt))
                        errorByType.Add(wt, $"The Write function for {wt.Name} does not have an opposing Read function!");
                    else
                    {
                        generalErrors.Add($"Write function for {wt.Name} Has multiple declarations which is not supported");
                        continue;
                    }

                    if (wm.GetParameters().Length != 2)
                    {
                        generalErrors.Add($"Write function for {wt.Name} wrong number of parameters. Only accepts methods of void (Stream, object)");
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
                    Type rt = ((ReadFunction)rm.GetCustomAttribute(typeof(ReadFunction), false)).type;

                    if (!duplicateCheck.Contains(rt))
                        duplicateCheck.Add(rt);
                    else
                    {
                        generalErrors.Add($"Read function for type {nameof(rt)} Has multiple declarations which is illegal");
                        continue;
                    }

                    //Find errors
                    if (rm.GetParameters().Length != 1)
                    {
                        generalErrors.Add($"Read function for type {nameof(rt)} wrong number of parameters. Only accepts methods of void (Stream)");
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
                    Type rt = ((ReadFunction)rm.GetCustomAttribute(typeof(ReadFunction), false)).type;
                    if (writeMethodsByType.ContainsKey(rt))
                        errorByType.Remove(rt);
                    else
                        errorByType.Add(rt, $"The Read function for type {nameof(rt)} does not have an opposing Write function!");
                }

                //Read out general errors
                foreach (string s in generalErrors)
                {
                    Trace.WriteLine(s);
                    throw new Exception(s);
                }

                //Read out mismatches
                foreach (string s in errorByType.Values)
                {
                    Trace.WriteLine(s);
                    throw new Exception(s);
                }
#endif
            }
        }

        private static MemoryStream compressStream1 = new MemoryStream();
        private static MemoryStream compressStream2 = new MemoryStream();
        //Binary Reader-Writer
        private static readonly int _supportedArrayDimensionDepth = 4;
        private static Dictionary<Type, (Action<Stream, object> writer, Func<Stream, object> reader)> BinaryFunctions = new Dictionary<Type, (Action<Stream, object> writer, Func<Stream, object> reader)>();
        private static Dictionary<Type, (List<FieldInfo> fields, List<PropertyInfo> properties)> MetaInformation = new Dictionary<Type, (List<FieldInfo> fields, List<PropertyInfo> properties)>();
        private static Dictionary<Type, ushort> _typeToHash = new Dictionary<Type, ushort>();
        private static Dictionary<ushort, Type> _hashToType = new Dictionary<ushort, Type>();
        //Public methods
        public static object Write(Stream stream, object obj)
        {
            WriteRaw(stream, _typeToHash[obj.GetType()]);
            BinaryFunctions[obj.GetType()].writer(stream, obj);
            return obj;
        }
        public static object Read(Stream stream)
        {
            ushort hash = ReadRaw<ushort>(stream);
            return ReadRaw(stream, _hashToType[hash]);
        }

        public static object WriteRaw(Stream stream, object obj)
        {
            BinaryFunctions[obj.GetType()].writer(stream, obj);
            return obj;
        }
        public static T ReadRaw<T>(Stream stream)
        {
            return (T)ReadRaw(stream, typeof(T));
        }
        public static object ReadRaw(Stream stream, Type writableType)
        {
            return BinaryFunctions[writableType].reader(stream);
        }

        public static object WriteDelta(Stream stream, object obj, object key)
        {
            compressStream1.Seek(0, SeekOrigin.Begin);
            compressStream2.Seek(0, SeekOrigin.Begin);
            Write(compressStream1, obj);
            Write(compressStream2, key);

            //Header
            ushort objLength = (ushort)compressStream1.Position;
            stream.Write(BitConverter.GetBytes(objLength), 0, 2); //HEADER
            long MaskPosition = stream.Position;
            //Mask
            byte[] Mask = new byte[(byte)Math.Ceiling(objLength / 8f)];
            stream.Write(Mask, 0, Mask.Length); //Temporary mask

            //Write all Compressed data:
            compressStream1.Seek(0, SeekOrigin.Begin);
            compressStream2.Seek(0, SeekOrigin.Begin);
            for (int i = 0; i < objLength; i++)
            {
                int objData = compressStream1.ReadByte();
                int keyData = compressStream2.ReadByte();
                byte delta = (byte)(objData ^ keyData);//Perform exclusive or op to get the difference!
                if (delta == 0) //If no difference is found ignore encoding this byte... this is how the compression saves space.
                    continue;

                Mask[i / 8] |= (byte)(1 << (7 - (i % 8))); //Add this byte to the mask describing which bytes are included in the encoding.
                stream.WriteByte(delta);//Write this byte to output
            }

            //Over-write the temporary mask with the real one on the stream
            stream.Seek(MaskPosition, SeekOrigin.Begin);
            stream.Write(Mask, 0, Mask.Length);
            return obj;
        }
        public static T ReadDelta<T>(Stream stream, object key)
        {
            compressStream2.Seek(0, SeekOrigin.Begin);
            compressStream1.Seek(0, SeekOrigin.Begin);
            WriteRaw(compressStream2, key);
            compressStream2.Seek(0, SeekOrigin.Begin);
            //Read header
            ushort originalMessageSize = (ushort)BinaryFunctions[typeof(ushort)].reader(stream);
            byte[] mask = new byte[(int)Math.Ceiling(originalMessageSize / 8f)];
            stream.Read(mask, 0, mask.Length);
            //Decompress and write object to compressStream1
            for (int i = 0; i < originalMessageSize; i++)
            {
                int isCompressed = (mask[(int)(i / 8f)] >> 7 - (i % 8)) & 1;
                compressStream1.WriteByte(isCompressed == 0 ? (byte)compressStream2.ReadByte() : (byte)(compressStream2.ReadByte() ^ stream.ReadByte()));
            }
            //Read original object from the stream
            compressStream1.Seek(0, SeekOrigin.Begin);
            return ReadRaw<T>(compressStream1);
        }

        //Private methods
        private static void AddWritable(Type writableType)
        {
            if (MetaInformation.ContainsKey(writableType)) return;
            List<FieldInfo> fields = new List<FieldInfo>();
            List<PropertyInfo> properties = new List<PropertyInfo>();

            // Collect fields from base types first so meta ordering is natural (base -> derived)
            Stack<Type> hierarchy = new Stack<Type>();
            for (Type t = writableType; t != null && t != typeof(object); t = t.BaseType)
                hierarchy.Push(t);
            while (hierarchy.Count > 0)
            {
                Type t = hierarchy.Pop();
                FieldInfo[] declaredFields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                fields.AddRange(declaredFields.Where(ValidateFieldInfo));
                PropertyInfo[] declaredProperies = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                properties.AddRange(declaredProperies.Where(ValidatePropertyInfo));
            }

            fields.AddRange(writableType.GetFields().Where(x => ValidateFieldInfo(x) && x.DeclaringType == writableType).ToList());
            MetaInformation.Add(writableType, (fields, properties));
        }
        private static void AddSuportedType(Type type, Action<Stream, object> writerFunction, Func<Stream, object> readerFunction)
        {
            //Normal type support
            BinaryFunctions.Add(type, (writerFunction, readerFunction));
            AddHashToType(type);

            //Add the array support to the type!
            Type t = type;
            for (int i = 0; i < _supportedArrayDimensionDepth; i++)
            {
                Type y = t.MakeArrayType();
                BinaryFunctions.Add(y, ((s, o) => WriteArray(s, o), (s) => ReadArray(s, y)));
                AddHashToType(y);
                t = t.MakeArrayType();
            }
#if DEBUG
            Console.WriteLine($"Add supported type: {type.Name} hash: {_typeToHash[type]}" );
#endif

            //Special read and write for Arrays
            object WriteArray(Stream stream, object obj)
            {
                Type temp_t = obj.GetType();
                if (t.IsArray)
                {
                    //This type is an array and we need to write each index in a row
                    //TODO for multi dimensional arrays this is inefficient packing since the size is written multiple times
                    //Instead this could be written as a flat array using GetArrayRank function on the type beforehand!
                    BinaryFunctions[typeof(uint)].writer(stream, (uint)(obj as Array).Length);
                    //We need to write all values for this array!
                    for (int i = 0; i < (obj as Array).Length; i++)
                    {
                        object o = (obj as Array).GetValue(i);
                        BinaryFunctions[temp_t.GetElementType()].writer(stream, o);
                    }
                }
                return obj;
            }
            object ReadArray(Stream stream, Type writableType)
            {
                object data = null;
                //array of supported type or Writable IMessage
                if (writableType.IsArray)
                {
                    //This type is an array and we need to read each index in a row
                    //TODO for multi dimensional arrays this is inefficient packing since the size is written multiple times
                    //Instead this could be written as a flat array using GetArrayRank function on the type beforehand!
                    uint length = (uint)BinaryFunctions[typeof(uint)].reader(stream);
                    data = Array.CreateInstance(writableType.GetElementType(), length);
                    //We need to read all values for this array!
                    for (int i = 0; i < length; i++)
                    {
                        object o = BinaryFunctions[writableType.GetElementType()].reader(stream);
                        (data as Array).SetValue(o, i);
                    }
                }
                return data;
            }
            void AddHashToType(Type typ)
            {
                ushort hash = StringHasher.HashStringTo16Bit(WritableHashSettings.HashSeed, typ.Name);
                _typeToHash.Add(typ, hash);
                if (!_hashToType.ContainsKey(hash))
                    _hashToType.Add(hash, typ);
                else
                {
#if DEBUG
                    string message = $"The message type {typ.Name} and {_hashToType[hash].Name} has a hash collision with hash {hash}. Please Change the value of WritableHashSettings.HashSeed to something other then {WritableHashSettings.HashSeed}";
                    Trace.WriteLine(message);
                    throw new Exception(message);
#endif
                }
            }
        }
        private static bool ValidateFieldInfo(FieldInfo info)
        {
            WritableField attribute = info.GetCustomAttribute<WritableField>();
            Type t = info.FieldType;
            return attribute != null && !info.IsStatic && BinaryFunctions.ContainsKey(t);
        }
        private static bool ValidatePropertyInfo(PropertyInfo info)
        {
            WritableField attribute = info.GetCustomAttribute<WritableField>();
            Type t = info.PropertyType;
            return attribute != null && BinaryFunctions.ContainsKey(t);
        }
    }
    public static class WritableHashSettings
    {
        private static int _hashSeed = -1;
        public static ushort HashSeed
        {
            get { return (ushort)_hashSeed; }
            set
            {
                if (_hashSeed > 0)
                {
                    Trace.WriteLine("The MessageHashSettings.HashSeed has to be set before the MessageSystem is used, to avoid reinitialization");
                    throw new Exception("The MessageHashSettings.HashSeed has to be set before the MessageSystem is used, to avoid reinitialization");
                }
                else
                    _hashSeed = value;
            }
        }
    }
    //Attributes
    /// <summary>
    /// If added above a field or property inside a <see cref="IWritable"/> said field will be included when written to byte array by <see cref="IWritable"/>.
    /// If compress is true this field will be delta compressible, if no it will always be sent raw
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class WritableField : Attribute 
    {
        public bool Compress = true;
        public WritableField(bool compress = true) { Compress = compress; }
    }
    /// <summary>
    /// Assign a method to write an object of <see cref="Type"/> to a <see cref="Stream"/>. 
    /// <see cref="WriteFunction"/> can only be applied to methods that are public and take the parameters <see cref="Stream"/> and <see cref="object"/>. 
    /// The method should return Void.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class WriteFunction : Attribute
    {
        public Type type;
        public WriteFunction(Type type)
        {
            this.type = type;
        }
    }
    /// <summary>
    /// Assign a method to read an object of <see cref="Type"/> from a <see cref="Stream"/>. 
    /// <see cref="ReadFunction"/> can only be applied to methods that are public and take the parameters <see cref="Stream"/>. 
    /// The method should return the <see cref="object"/> read from the stream
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReadFunction : Attribute
    {
        public Type type;
        public ReadFunction(Type type)
        {
            this.type = type;
        }
    }
}