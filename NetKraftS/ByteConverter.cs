using Netkraft.Messaging;
using System.IO;

namespace Netkraft
{
    internal class ByteConverter
    {
        internal static byte[] buffer = new byte[1024];
        //Ints
        [WritableFieldTypeWrite(typeof(int))]
        internal static void WriteInt32(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((int)value);
        }
        [WritableFieldTypeRead(typeof(int))]
        internal static object ReadInt32(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadInt32();
        }
        [WritableFieldTypeWrite(typeof(uint))]
        internal static void WriteUInt32(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((uint)value);
        }
        [WritableFieldTypeRead(typeof(uint))]
        internal static object ReadUInt32(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadUInt32();
        }
        [WritableFieldTypeWrite(typeof(long))]
        internal static void WriteInt64(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((long)value);
        }
        [WritableFieldTypeRead(typeof(long))]
        internal static object ReadInt64(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadInt64();
        }
        [WritableFieldTypeWrite(typeof(ulong))]
        internal static void WriteUInt64(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ulong)value);
        }
        [WritableFieldTypeRead(typeof(ulong))]
        internal static object ReadUInt64(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadUInt64();
        }
        [WritableFieldTypeWrite(typeof(short))]
        internal static void WriteInt16(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((short)value);
        }
        [WritableFieldTypeRead(typeof(short))]
        internal static object ReadInt16(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadInt16();
        }
        [WritableFieldTypeWrite(typeof(ushort))]
        internal static void WriteUInt16(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)value);
        }
        [WritableFieldTypeRead(typeof(ushort))]
        internal static object ReadUInt16(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadUInt16();
        }
        [WritableFieldTypeWrite(typeof(byte))]
        internal static void WriteByte(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((byte)value);
        }
        [WritableFieldTypeRead(typeof(byte))]
        internal static object ReadByte(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadByte();
        }
        //Exponents
        [WritableFieldTypeWrite(typeof(float))]
        internal static void WriteSingle(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((float)value);
        }
        [WritableFieldTypeRead(typeof(float))]
        internal static object ReadSingle(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadSingle();
        }
        [WritableFieldTypeWrite(typeof(double))]
        internal static void WriteDouble(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((double)value);
        }
        [WritableFieldTypeRead(typeof(double))]
        internal static object ReadDouble(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadDouble();
        }
        //Misc
        [WritableFieldTypeWrite(typeof(string))]
        internal static void WriteString(Stream stream, object value)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((string)value);
        }
        [WritableFieldTypeRead(typeof(string))]
        internal static object ReadString(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadString();
        }
    }
}
