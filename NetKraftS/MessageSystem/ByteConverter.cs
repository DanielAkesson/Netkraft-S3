using Netkraft.Messaging;
using System.IO;
using System;
using System.Text;

namespace Netkraft
{
    internal class ByteConverter
    {
        internal static byte[] buffer = new byte[1024];
        [WritableFieldTypeWrite(typeof(int))]
        internal static void WriteInt32(Stream stream, object value)
        {
            stream.Write(BitConverter.GetBytes((int)value), 0, 4);
        }
        [WritableFieldTypeRead(typeof(int))]
        internal static object ReadInt32(Stream stream)
        {
            stream.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }
        [WritableFieldTypeWrite(typeof(uint))]
        internal static void WriteUInt32(Stream stream, object value)
        {
            stream.Write(BitConverter.GetBytes((uint)value), 0, 4);
        }
        [WritableFieldTypeRead(typeof(uint))]
        internal static object ReadUInt32(Stream stream)
        {
            stream.Read(buffer, 0, 4);
            return BitConverter.ToUInt32(buffer, 0);
        }
        [WritableFieldTypeWrite(typeof(long))]
        internal static void WriteInt64(Stream stream, object value)
        {
            stream.Write(BitConverter.GetBytes((long)value), 0, 8);
        }
        [WritableFieldTypeRead(typeof(long))]
        internal static object ReadInt64(Stream stream)
        {
            stream.Read(buffer, 0, 8);
            return BitConverter.ToInt64(buffer, 0);
        }
        [WritableFieldTypeWrite(typeof(ulong))]
        internal static void WriteUInt64(Stream stream, object value)
        {
            stream.Write(BitConverter.GetBytes((ulong)value), 0, 8);
        }
        [WritableFieldTypeRead(typeof(ulong))]
        internal static object ReadUInt64(Stream stream)
        {
            stream.Read(buffer, 0, 8);
            return BitConverter.ToUInt64(buffer, 0);
        }
        [WritableFieldTypeWrite(typeof(short))]
        internal static void WriteInt16(Stream stream, object value)
        {
            stream.Write(BitConverter.GetBytes((short)value), 0, 2);
        }
        [WritableFieldTypeRead(typeof(short))]
        internal static object ReadInt16(Stream stream)
        {
            stream.Read(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }
        [WritableFieldTypeWrite(typeof(ushort))]
        internal static void WriteUInt16(Stream stream, object value)
        {
            stream.Write(BitConverter.GetBytes((ushort)value), 0, 2);
        }
        [WritableFieldTypeRead(typeof(ushort))]
        internal static object ReadUInt16(Stream stream)
        {
            stream.Read(buffer, 0, 2);
            return BitConverter.ToUInt16(buffer, 0);
        }
        [WritableFieldTypeWrite(typeof(byte))]
        internal static void WriteByte(Stream stream, object value)
        {
            stream.WriteByte((byte)value);
        }
        [WritableFieldTypeRead(typeof(byte))]
        internal static object ReadByte(Stream stream)
        {
            stream.Read(buffer, 0, 1);
            return buffer[0];
        }
        //Exponents
        [WritableFieldTypeWrite(typeof(float))]
        internal static void WriteSingle(Stream stream, object value)
        {
            stream.Write(BitConverter.GetBytes((float)value), 0, 4);
        }
        [WritableFieldTypeRead(typeof(float))]
        internal static object ReadSingle(Stream stream)
        {
            stream.Read(buffer, 0, 4);
            return BitConverter.ToSingle(buffer, 0);
        }
        [WritableFieldTypeWrite(typeof(double))]
        internal static void WriteDouble(Stream stream, object value)
        {
            stream.Write(BitConverter.GetBytes((double)value), 0, 8);
        }
        [WritableFieldTypeRead(typeof(double))]
        internal static object ReadDouble(Stream stream)
        {
            stream.Read(buffer, 0, 8);
            return BitConverter.ToDouble(buffer, 0);
        }
        //Misc
        [WritableFieldTypeWrite(typeof(string))]
        internal static void WriteString(Stream stream, object value)
        {
            //Header
            byte[] bytes = Encoding.UTF8.GetBytes((string)value);
            ushort length = (ushort)bytes.Length;
            stream.Write(BitConverter.GetBytes(length), 0, 2);

            //Body
            stream.Write(bytes, 0, length);
        }
        [WritableFieldTypeRead(typeof(string))]
        internal static object ReadString(Stream stream)
        {
            //Header
            stream.Read(buffer, 0, 2);
            ushort length = BitConverter.ToUInt16(buffer, 0);

            //Body
            byte[] body = new byte[length];
            stream.Read(body, 0, length);
            return Encoding.UTF8.GetString(body);
        }
    }
}
