using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Netkraft.Messaging
{
    //Interfaces
    /// <summary>
    /// Any class or struct that inherits <see cref="IWritable"/> will be supported by the <see cref="WritableSystem"/> and can be read from or writen to a stream.
    /// <para></para>
    /// </summary>
    public interface IWritable{}
    public static class WritableSystem
    {
        private struct WritableMetaInformation
        {
            public List<FieldMetaInformation> fields;
            public List<FieldMetaInformation> deltaFields;
        }
        private struct FieldMetaInformation
        {
            public FieldInfo fieldInfo;
            public WritableMetaInformation parentWritable;
            public Action<Stream, object> writeFunction;
            public Func<Stream, object> readFunction;
        }
        static WritableSystem()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //Scan trough assembly and find all methods with a write or read attribute in the domain.
            AddAllWritableFieldTypes(assemblies);

            //Get all Writables and IMessage types in the current domain
            List<Type> WritableTypes = new List<Type>();
            foreach (Assembly a in assemblies)
                WritableTypes.AddRange(a.GetTypes().Where(x => TypeIsWritable(x)));

            //Do calculations for field members
            foreach (Type t in WritableTypes)
                AddWritable(t);
        }
        private static bool TypeIsWritable(Type t)
        {
            return typeof(IWritable).IsAssignableFrom(t)
                || typeof(IUnreliableMessage).IsAssignableFrom(t)
                || typeof(IReliableMessage).IsAssignableFrom(t);
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
                    if(!writeMethodsByType.ContainsKey(wt))
                        writeMethodsByType.Add(wt, wm);
                }

                foreach (MethodInfo rm in ReadMethods)
                {
                    Type rt = ((ReadFunction)rm.GetCustomAttribute(typeof(ReadFunction), false)).type;
                    if (writeMethodsByType.ContainsKey(rt))
                        AddSuportedField(rt, (Action<Stream, object>)writeMethodsByType[rt].CreateDelegate(typeof(Action<Stream, object>)), (Func<Stream, object>)rm.CreateDelegate(typeof(Func<Stream, object>)));
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
                    Type rt = ((ReadFunction)rm.GetCustomAttribute(typeof(ReadFunction), false)).type;

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
                    Type rt = ((ReadFunction)rm.GetCustomAttribute(typeof(ReadFunction), false)).type;
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

        private static MemoryStream compressStream1 = new MemoryStream();
        private static MemoryStream compressStream2 = new MemoryStream();
        //Binary Reader-Writer
        private static readonly int _supportedArrayDimensionDepth = 4;
        private static Dictionary<Type, WritableMetaInformation> writables = new Dictionary<Type, WritableMetaInformation>();
        private static Dictionary<Type, Func<Stream, object>> readerFunctions = new Dictionary<Type, Func<Stream, object>>();
        private static Dictionary<Type, Action<Stream, object>> writersFunctions = new Dictionary<Type, Action<Stream, object>>();
        
        //private method infos for generic methods
        private static readonly MethodInfo _writeArrayMetod = typeof(WritableSystem).GetMethod("WriteArray", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _readArrayMetod = typeof(WritableSystem).GetMethod("ReadArray", BindingFlags.Static | BindingFlags.NonPublic);

        //Private methods
        private static void AddSuportedField(Type fieldType, Action<Stream, object> writerFunction, Func<Stream, object> readerFunction) {
            Console.WriteLine("Add supported type: " + fieldType.Name);
            //Normal type support
            readerFunctions.Add(fieldType, readerFunction);
            writersFunctions.Add(fieldType, writerFunction);
            //X dimension array support
            Type ArrayCarryType = fieldType;
            Action<Stream, object> writeMultiDelegate;
            Func<Stream, object> readMultiDelegate;
            for (int i = 0; i < _supportedArrayDimensionDepth; i++) {
                //Standard multi array
                readMultiDelegate = (Func<Stream, object>)_readArrayMetod.MakeGenericMethod(ArrayCarryType).CreateDelegate(typeof(Func<Stream, object>));
                writeMultiDelegate = (Action<Stream, object>)_writeArrayMetod.MakeGenericMethod(ArrayCarryType).CreateDelegate(typeof(Action<Stream, object>));
                ArrayCarryType = ArrayCarryType.MakeArrayType();
                readerFunctions.Add(ArrayCarryType, readMultiDelegate);
                writersFunctions.Add(ArrayCarryType, writeMultiDelegate);
            }
        }
        private static void AddWritable(Type writableType) {
            if (writables.ContainsKey(writableType)) return;
            //Writable Meta Information
            WritableMetaInformation writable = new WritableMetaInformation {
                fields = new List<FieldMetaInformation>(),
                deltaFields = new List<FieldMetaInformation>()
            };

            //Fields Meta information
            foreach(FieldInfo Fi in writableType.GetFields().Where(x => ValidateFieldInfo(x) && x.DeclaringType == writableType).ToList()){
                FieldMetaInformation fieldMeta = new FieldMetaInformation {
                    fieldInfo = Fi,
                    parentWritable = writable,
                    readFunction = readerFunctions[Fi.FieldType],
                    writeFunction = writersFunctions[Fi.FieldType]
                };
                (Fi.GetCustomAttribute<DeltaCompressedField>() == null ? writable.fields : writable.deltaFields).Add(fieldMeta);
            }
            writables.Add(writableType, writable);
            //Add the writable as it's own supported type!
            Func<Stream, object> read = (Stream x) => { return ReadInternal(x, writableType); };
            AddSuportedField(writableType, WriteInternal, read);
        }
        private static bool ValidateFieldInfo(FieldInfo info) {
            SkipIndex attribute = info.GetCustomAttribute<SkipIndex>();
            Type t = info.FieldType;
            return attribute == null && !info.IsStatic && readerFunctions.ContainsKey(t) && writersFunctions.ContainsKey(t);
        }

        //Public methods
        public static void Write(Stream stream, object obj) {
            writersFunctions[obj.GetType()](stream, obj);
        }
        public static T Read<T>(Stream stream) {
            return (T)readerFunctions[typeof(T)](stream);
        }
        public static object Read(Stream stream, Type writableType) {
            return readerFunctions[writableType](stream);
        }

        //I dono bud ▼
        public static T ReadDeltaCompress<T>(Stream stream, object key) {
            //Read the object raw to infear key elements
            compressStream1.Seek(0, SeekOrigin.Begin);
            compressStream2.Seek(0, SeekOrigin.Begin);
            //Write key delta fields
            foreach (FieldMetaInformation fmi in writables[key.GetType()].deltaFields)
                fmi.writeFunction(compressStream2, fmi.fieldInfo.GetValue(key));
            compressStream2.Seek(0, SeekOrigin.Begin);
            //Read header
            short originalMessageSize = (short)readerFunctions[typeof(short)](stream);
            byte[] mask = new byte[(int)Math.Ceiling(originalMessageSize / 8f)];
            stream.Read(mask, 0, mask.Length);
            //Decompress and write object to compressStream1
            for (int i = 0; i < originalMessageSize; i++) {
                int isCompressed = (mask[(int)(i / 8f)] >> 7 - (i % 8)) & 1;
                compressStream1.WriteByte(isCompressed == 0 ? (byte)compressStream2.ReadByte() : (byte)(compressStream2.ReadByte() ^ stream.ReadByte()));
            }
            //Read original object from the stream
            compressStream1.Seek(0, SeekOrigin.Begin);
            object data = FormatterServices.GetUninitializedObject(key.GetType());
                foreach (FieldMetaInformation fmi in writables[key.GetType()].fields)
                    fmi.fieldInfo.SetValue(data, readerFunctions[fmi.fieldInfo.FieldType](stream));
            return (T)data;
        }
        public static void WriteDeltaCompress(Stream stream, object obj, object key) {
            compressStream1.Seek(0, SeekOrigin.Begin);
            compressStream2.Seek(0, SeekOrigin.Begin);
            //Write object delta fields
            foreach (FieldMetaInformation fmi in writables[obj.GetType()].deltaFields)
                fmi.writeFunction(compressStream1, fmi.fieldInfo.GetValue(obj));
            //Write key delta fields
            foreach (FieldMetaInformation fmi in writables[key.GetType()].deltaFields)
                fmi.writeFunction(compressStream2, fmi.fieldInfo.GetValue(key));

            //Find the mask length
            ushort objLength = (ushort)compressStream1.Position;
            byte[] Mask = new byte[(byte)Math.Ceiling(objLength / 8f)];
            stream.Write(BitConverter.GetBytes(objLength), 0, 2); //HEADER
            long MaskPosition = stream.Position;
            stream.Write(Mask, 0, Mask.Length); //Temporary mask

            //Write all Compressed data:
            compressStream1.Seek(0, SeekOrigin.Begin);
            compressStream2.Seek(0, SeekOrigin.Begin);
            for (int i = 0; i < objLength; i++) {
                int objData = compressStream1.ReadByte();
                int keyData = compressStream2.ReadByte();
                byte delta = (byte)(objData ^ keyData);
                if (delta == 0)
                    continue;

                Mask[i / 8] |= (byte)(1 << (7 - (i % 8)));
                stream.WriteByte(delta);
            }

            //Re-write the mask into the stream
            stream.Seek(MaskPosition, SeekOrigin.Begin);
            stream.Write(Mask, 0, Mask.Length);
        }
        //Method info functions
        /// <summary>
        /// Read internal describes how a Writable object can be read. 
        /// <p>This function is never actually called but instead is used in system reflection so that the actual writable types can be added as a supported type of its own.</p>
        /// <p>This function is essentially the same as <see cref="ReadFunction"/> but generic and meant for all <see cref="IWritable"/></p>
        /// </summary>
        private static object ReadInternal(Stream stream, Type type) {
            object data = FormatterServices.GetUninitializedObject(type);
            try{
                foreach (FieldMetaInformation fmi in writables[type].fields)
                    fmi.fieldInfo.SetValue(data, readerFunctions[fmi.fieldInfo.FieldType](stream));
            }
            catch (Exception e) { throw e; }
            return data;
        }
        //This is the same as above however specificlly for arrays
        private static object ReadArray<T>(Stream stream) {
            int length = (int)readerFunctions[typeof(uint)](stream);
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
                array[i] = (T)readerFunctions[typeof(T)](stream);
            return array;
        }
        /// <summary>
        /// Write internal describes how a Writable object can be written. 
        /// <p>This function is never actually called but instead is used in system reflection so that the actual writable types can be added as a supported type of its own.</p>
        /// <p>This function is essentially the same as <see cref="WriteFunction"/> but generic and meant for all <see cref="IWritable"/></p>
        /// </summary>
        private static void WriteInternal(Stream stream, object obj) {
            try{
                foreach (FieldMetaInformation fmi in writables[obj.GetType()].fields)
                    fmi.writeFunction(stream, fmi.fieldInfo.GetValue(obj));
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); throw e; }
         }
        //This is the same as above however specificlly for arrays
        private static void WriteArray<T>(Stream stream, object value) {
            T[] array = (T[])value;
            writersFunctions[typeof(uint)](stream, array.Length);
            for (int i = 0; i < array.Length; i++)
                writersFunctions[typeof(T)](stream, array[i]);
        }

        //TODO: Clean this mess ▼
        /*
        private static void WriteDeltaCompressInternal(Stream stream, object obj, object key)
        {
            compressStream1.Seek(0, SeekOrigin.Begin);
            compressStream2.Seek(0, SeekOrigin.Begin);
            Write(compressStream1, obj);
            Write(compressStream2, key);

            ushort objLength = (ushort)compressStream1.Position;
            byte[] Mask = new byte[(byte)Math.Ceiling(objLength / 8f)];
            stream.Write(BitConverter.GetBytes(objLength), 0, 2); //HEADER
            long MaskPosition = stream.Position;
            stream.Write(Mask, 0, Mask.Length); //Temporary mask

            //Write all Compressed data:
            compressStream1.Seek(0, SeekOrigin.Begin);
            compressStream2.Seek(0, SeekOrigin.Begin);
            for (int i = 0; i < objLength; i++)
            {
                int objData = compressStream1.ReadByte();
                int keyData = compressStream2.ReadByte();
                byte delta = (byte)(objData ^ keyData);
                if (delta == 0)
                    continue;

                Mask[i / 8] |= (byte)(1 << (7 - (i % 8)));
                stream.WriteByte(delta);
            }

            //Re-write the mask into the stream
            stream.Seek(MaskPosition, SeekOrigin.Begin);
            stream.Write(Mask, 0, Mask.Length);
        }
        //Todo implement so it works without GetDefaulkt in IDeltaCompressed
        private static object GetNonDeltaFieldsDefault(object obj)
        {
            Type t = obj.GetType();
            foreach (FieldInfo fi in metaInformation[t])
                if (fi.GetCustomAttribute<DeltaCompressedField>() != null)
                    fi.SetValue(obj, FormatterServices.GetUninitializedObject(fi.FieldType));

            return obj;
        }
        */
    }
    
    //Attributes
    /// <summary>
    /// If added to a field inside a <see cref="IWritable"/> or Message Inteface said field will not be included when sent by <see cref="NetkraftClient"/> or writen to byte array by <see cref="WritableSystem"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class SkipIndex : Attribute{}
    /// <summary>
    /// This attributes describes where delta compression will start in a Message or Writable that inherits from <see cref="IDeltaCompressed"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DeltaCompressedField : Attribute { }
    /// <summary>
    /// Add to any field in a <see cref="IDeltaCompressed"/> struct or class to ignore this field when delta compressing./>. 
    /// <p>This field will be read before calling <see cref="IDeltaCompressed.DecompressKeyRead"/> so only values that has this attribute can be trusted to derive what key is accompanied with this writable.</p>
    /// <p>This is mainly used to allow ids and key identifiers so that the receiving client or server can open the delta compressed messages.</p> 
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