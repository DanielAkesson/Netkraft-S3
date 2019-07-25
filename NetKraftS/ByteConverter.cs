using Netkraft.Messaging;
using System.IO;

namespace Netkraft
{
    internal class ByteConverter
    {
        internal static byte[] buffer = new byte[1024];
        internal static void AddBasicTypes()
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
        internal static void WriteInt32(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((int)value);
        }
        internal static object ReadInt32(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadInt32();
        }
        internal static void WriteUInt32(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((uint)value);
        }
        internal static object ReadUInt32(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadUInt32();
        }
        internal static void WriteInt64(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((long)value);
        }
        internal static object ReadInt64(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadInt64();
        }
        internal static void WriteUInt64(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ulong)value);
        }
        internal static object ReadUInt64(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadUInt64();
        }
        internal static void WriteInt16(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((short)value);
        }
        internal static object ReadInt16(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadInt16();
        }
        internal static void WriteUInt16(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)value);
        }
        internal static object ReadUInt16(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadUInt16();
        }
        internal static void WriteByte(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((byte)value);
        }
        internal static object ReadByte(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadByte();
        }
        //Exponents
        internal static void WriteSingle(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((float)value);
        }
        internal static object ReadSingle(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadSingle();
        }
        internal static void WriteDouble(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((double)value);
        }
        internal static object ReadDouble(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadDouble();
        }
        //Misc
        internal static void WriteString(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((string)value);
        }
        internal static object ReadString(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadString();
        }
    }
}
