using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetkraftMessage
{
    public class ByteConverter
    {
        public static byte[] buffer = new byte[1024];
        public static void AddBasicTypes()
        {
            //Ints
            WritableSystem.AddSuportedType<long>(WriteInt64, ReadInt64);
            WritableSystem.AddSuportedType<ulong>(WriteUInt64, ReadUInt64);
            WritableSystem.AddSuportedType<int>(WriteInt32, ReadInt32);
            WritableSystem.AddSuportedType<uint>(WriteUInt32, ReadUInt32);
            WritableSystem.AddSuportedType<short>(WriteInt16, ReadInt16);
            WritableSystem.AddSuportedType<ushort>(WriteUInt16, ReadUInt16);
            WritableSystem.AddSuportedType<byte>(WriteByte, ReadByte);
            //Exponents
            WritableSystem.AddSuportedType<float>(WriteSingle, ReadSingle);
            WritableSystem.AddSuportedType<double>(WriteDouble, ReadDouble);
            //Misc
            WritableSystem.AddSuportedType<string>(WriteString, ReadString);
        }
        //Ints
        public static void WriteInt32(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((int)value);
        }
        public static object ReadInt32(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadInt32();
        }
        public static void WriteUInt32(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((uint)value);
        }
        public static object ReadUInt32(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadUInt32();
        }
        public static void WriteInt64(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((long)value);
        }
        public static object ReadInt64(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadInt64();
        }
        public static void WriteUInt64(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ulong)value);
        }
        public static object ReadUInt64(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadUInt64();
        }
        public static void WriteInt16(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((short)value);
        }
        public static object ReadInt16(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadInt16();
        }
        public static void WriteUInt16(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)value);
        }
        public static object ReadUInt16(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadUInt16();
        }
        public static void WriteByte(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((byte)value);
        }
        public static object ReadByte(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadByte();
        } 
        //Exponents
        public static void WriteSingle(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((float)value);
        }
        public static object ReadSingle(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadSingle();
        }
        public static void WriteDouble(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((double)value);
        }
        public static object ReadDouble(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadDouble();
        }
        //Misc
        public static void WriteString(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((string)value);
        }
        public static object ReadString(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadString();
        }
    }
}
