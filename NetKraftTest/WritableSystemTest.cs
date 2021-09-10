using System;
using Netkraft.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace NetKraftTest
{
    [TestClass]
    public class WritableSystemTest
    {
        MemoryStream stream = new MemoryStream();
        //Basic types
        [TestMethod]
        public void Ints()
        {
            //Int16
            WritableSystem.Write(stream, (Int16)1);
            WritableSystem.Write(stream, (Int16)0);
            WritableSystem.Write(stream, (Int16)(-1));
            WritableSystem.Write(stream, Int16.MaxValue);
            WritableSystem.Write(stream, Int16.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<Int16>(stream), (Int16)1);
            Assert.AreEqual(WritableSystem.Read<Int16>(stream), (Int16)0);
            Assert.AreEqual(WritableSystem.Read<Int16>(stream), (Int16)(-1));
            Assert.AreEqual(WritableSystem.Read<Int16>(stream), Int16.MaxValue);
            Assert.AreEqual(WritableSystem.Read<Int16>(stream), Int16.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //Int32
            WritableSystem.Write(stream, (Int32)1);
            WritableSystem.Write(stream, (Int32)0);
            WritableSystem.Write(stream, (Int32)(-1));
            WritableSystem.Write(stream, Int32.MaxValue);
            WritableSystem.Write(stream, Int32.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<Int32>(stream), (Int32)1);
            Assert.AreEqual(WritableSystem.Read<Int32>(stream), (Int32)0);
            Assert.AreEqual(WritableSystem.Read<Int32>(stream), (Int32)(-1));
            Assert.AreEqual(WritableSystem.Read<Int32>(stream), Int32.MaxValue);
            Assert.AreEqual(WritableSystem.Read<Int32>(stream), Int32.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //Int64
            WritableSystem.Write(stream, (Int64)1);
            WritableSystem.Write(stream, (Int64)0);
            WritableSystem.Write(stream, (Int64)(-1));
            WritableSystem.Write(stream, Int64.MaxValue);
            WritableSystem.Write(stream, Int64.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<Int64>(stream), (Int64)1);
            Assert.AreEqual(WritableSystem.Read<Int64>(stream), (Int64)0);
            Assert.AreEqual(WritableSystem.Read<Int64>(stream), (Int64)(-1));
            Assert.AreEqual(WritableSystem.Read<Int64>(stream), Int64.MaxValue);
            Assert.AreEqual(WritableSystem.Read<Int64>(stream), Int64.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //short
            WritableSystem.Write(stream, (short)1);
            WritableSystem.Write(stream, (short)0);
            WritableSystem.Write(stream, (short)(-1));
            WritableSystem.Write(stream, short.MaxValue);
            WritableSystem.Write(stream, short.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<short>(stream), (short)1);
            Assert.AreEqual(WritableSystem.Read<short>(stream), (short)0);
            Assert.AreEqual(WritableSystem.Read<short>(stream), (short)(-1));
            Assert.AreEqual(WritableSystem.Read<short>(stream), short.MaxValue);
            Assert.AreEqual(WritableSystem.Read<short>(stream), short.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //int
            WritableSystem.Write(stream, (int)1);
            WritableSystem.Write(stream, (int)0);
            WritableSystem.Write(stream, (int)(-1));
            WritableSystem.Write(stream, int.MaxValue);
            WritableSystem.Write(stream, int.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<int>(stream), (int)1);
            Assert.AreEqual(WritableSystem.Read<int>(stream), (int)0);
            Assert.AreEqual(WritableSystem.Read<int>(stream), (int)(-1));
            Assert.AreEqual(WritableSystem.Read<int>(stream), int.MaxValue);
            Assert.AreEqual(WritableSystem.Read<int>(stream), int.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //long
            WritableSystem.Write(stream, (long)1);
            WritableSystem.Write(stream, (long)0);
            WritableSystem.Write(stream, (long)(-1));
            WritableSystem.Write(stream, long.MaxValue);
            WritableSystem.Write(stream, long.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<long>(stream), (long)1);
            Assert.AreEqual(WritableSystem.Read<long>(stream), (long)0);
            Assert.AreEqual(WritableSystem.Read<long>(stream), (long)(-1));
            Assert.AreEqual(WritableSystem.Read<long>(stream), long.MaxValue);
            Assert.AreEqual(WritableSystem.Read<long>(stream), long.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
        }
        [TestMethod]
        public void UInts()
        {
            //UInt16
            WritableSystem.Write(stream, (UInt16)1);
            WritableSystem.Write(stream, (UInt16)0);
            WritableSystem.Write(stream, UInt16.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<UInt16>(stream), (UInt16)1);
            Assert.AreEqual(WritableSystem.Read<UInt16>(stream), (UInt16)0);
            Assert.AreEqual(WritableSystem.Read<UInt16>(stream), UInt16.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

            //UInt32
            WritableSystem.Write(stream, (UInt32)1);
            WritableSystem.Write(stream, (UInt32)0);
            WritableSystem.Write(stream, UInt32.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<UInt32>(stream), (UInt32)1);
            Assert.AreEqual(WritableSystem.Read<UInt32>(stream), (UInt32)0);
            Assert.AreEqual(WritableSystem.Read<UInt32>(stream), UInt32.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

            //UInt64
            WritableSystem.Write(stream, (UInt64)1);
            WritableSystem.Write(stream, (UInt64)0);
            WritableSystem.Write(stream, UInt64.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<UInt64>(stream), (UInt64)1);
            Assert.AreEqual(WritableSystem.Read<UInt64>(stream), (UInt64)0);
            Assert.AreEqual(WritableSystem.Read<UInt64>(stream), UInt64.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

            //byte
            WritableSystem.Write(stream, (byte)1);
            WritableSystem.Write(stream, (byte)0);
            WritableSystem.Write(stream, byte.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<byte>(stream), (byte)1);
            Assert.AreEqual(WritableSystem.Read<byte>(stream), (byte)0);
            Assert.AreEqual(WritableSystem.Read<byte>(stream), byte.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

            //ushort
            WritableSystem.Write(stream, (ushort)1);
            WritableSystem.Write(stream, (ushort)0);
            WritableSystem.Write(stream, ushort.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<ushort>(stream), (ushort)1);
            Assert.AreEqual(WritableSystem.Read<ushort>(stream), (ushort)0);
            Assert.AreEqual(WritableSystem.Read<ushort>(stream), ushort.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

            //uint
            WritableSystem.Write(stream, (uint)1);
            WritableSystem.Write(stream, (uint)0);
            WritableSystem.Write(stream, uint.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<uint>(stream), (uint)1);
            Assert.AreEqual(WritableSystem.Read<uint>(stream), (uint)0);
            Assert.AreEqual(WritableSystem.Read<uint>(stream), uint.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

            //ulong
            WritableSystem.Write(stream, (ulong)1);
            WritableSystem.Write(stream, (ulong)0);
            WritableSystem.Write(stream, ulong.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<ulong>(stream), (ulong)1);
            Assert.AreEqual(WritableSystem.Read<ulong>(stream), (ulong)0);
            Assert.AreEqual(WritableSystem.Read<ulong>(stream), ulong.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

        }
        [TestMethod]
        public void FloatingPointNumbers()
        {
            //Single
            WritableSystem.Write(stream, (Single)1);
            WritableSystem.Write(stream, (Single)0);
            WritableSystem.Write(stream, (Single)(-1));
            WritableSystem.Write(stream, Single.MaxValue);
            WritableSystem.Write(stream, Single.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<Single>(stream), (Single)1);
            Assert.AreEqual(WritableSystem.Read<Single>(stream), (Single)0);
            Assert.AreEqual(WritableSystem.Read<Single>(stream), (Single)(-1));
            Assert.AreEqual(WritableSystem.Read<Single>(stream), Single.MaxValue);
            Assert.AreEqual(WritableSystem.Read<Single>(stream), Single.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //Double
            WritableSystem.Write(stream, (Double)1);
            WritableSystem.Write(stream, (Double)0);
            WritableSystem.Write(stream, (Double)(-1));
            WritableSystem.Write(stream, Double.MaxValue);
            WritableSystem.Write(stream, Double.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<Double>(stream), (Double)1);
            Assert.AreEqual(WritableSystem.Read<Double>(stream), (Double)0);
            Assert.AreEqual(WritableSystem.Read<Double>(stream), (Double)(-1));
            Assert.AreEqual(WritableSystem.Read<Double>(stream), Double.MaxValue);
            Assert.AreEqual(WritableSystem.Read<Double>(stream), Double.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //float
            WritableSystem.Write(stream, (float)1);
            WritableSystem.Write(stream, (float)0);
            WritableSystem.Write(stream, (float)(-1));
            WritableSystem.Write(stream, float.MaxValue);
            WritableSystem.Write(stream, float.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<float>(stream), (float)1);
            Assert.AreEqual(WritableSystem.Read<float>(stream), (float)0);
            Assert.AreEqual(WritableSystem.Read<float>(stream), (float)(-1));
            Assert.AreEqual(WritableSystem.Read<float>(stream), float.MaxValue);
            Assert.AreEqual(WritableSystem.Read<float>(stream), float.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //double
            WritableSystem.Write(stream, (double)1);
            WritableSystem.Write(stream, (double)0);
            WritableSystem.Write(stream, (double)(-1));
            WritableSystem.Write(stream, double.MaxValue);
            WritableSystem.Write(stream, double.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<double>(stream), (double)1);
            Assert.AreEqual(WritableSystem.Read<double>(stream), (double)0);
            Assert.AreEqual(WritableSystem.Read<double>(stream), (double)(-1));
            Assert.AreEqual(WritableSystem.Read<double>(stream), double.MaxValue);
            Assert.AreEqual(WritableSystem.Read<double>(stream), double.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

        }
        [TestMethod]
        public void Boolean()
        {
            //bool
            WritableSystem.Write(stream, true);
            WritableSystem.Write(stream, false);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<bool>(stream), true);
            Assert.AreEqual(WritableSystem.Read<bool>(stream), false);
            stream.Seek(0, SeekOrigin.Begin);
            //Boolean
            WritableSystem.Write(stream, (Boolean)true);
            WritableSystem.Write(stream, (Boolean)false);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<Boolean>(stream), (Boolean)true);
            Assert.AreEqual(WritableSystem.Read<Boolean>(stream), (Boolean)false);
            stream.Seek(0, SeekOrigin.Begin);

        }
        [TestMethod]
        public void Strings()
        {
            //strings
            WritableSystem.Write(stream, "hej");
            WritableSystem.Write(stream, "š↓▲ÿc§π√R╙╟Ä╥E");
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<string>(stream), "hej");
            Assert.AreEqual(WritableSystem.Read<string>(stream), "š↓▲ÿc§π√R╙╟Ä╥E");
            stream.Seek(0, SeekOrigin.Begin);
        }
        [TestMethod]
        public void ArrayTypes()
        {
            //strings
            WritableSystem.Write(stream, new int[]{ 0, 1, 2, 3, 4 });
            WritableSystem.Write(stream, new string[] { "0", "1", "2", "3", "4" });
            WritableSystem.Write(stream, new int[]{ });
            stream.Seek(0, SeekOrigin.Begin);
            bool isEqual1 = Enumerable.SequenceEqual(WritableSystem.Read<int[]>(stream), new int[] { 0, 1, 2, 3, 4 });
            Assert.IsTrue(isEqual1);
            bool isEqual2 = Enumerable.SequenceEqual(WritableSystem.Read<string[]>(stream), new string[] { "0", "1", "2", "3", "4" });
            Assert.IsTrue(isEqual2);
            bool isEqual3 = Enumerable.SequenceEqual(WritableSystem.Read<int[]>(stream), new int[] { });
            Assert.IsTrue(isEqual3);
            stream.Seek(0, SeekOrigin.Begin);
        }

        //Custom write functions
        internal static byte[] buffer = new byte[1024];
        struct Vector3
        {
            public float x, y, z;
            public override bool Equals(object obj)
            {
                return (obj is Vector3) && ((Vector3)obj).x == x && ((Vector3)obj).y == y && ((Vector3)obj).z == z;
            }
        }
        [WriteFunction(typeof(Vector3))]
        internal static void WriteVector3(Stream stream, object value)
        {
            stream.Write(BitConverter.GetBytes(((Vector3)value).x), 0, 4);
            stream.Write(BitConverter.GetBytes(((Vector3)value).y), 0, 4);
            stream.Write(BitConverter.GetBytes(((Vector3)value).z), 0, 4);
        }
        [ReadFunction(typeof(Vector3))]
        internal static object ReadVector3(Stream stream)
        {
            stream.Read(buffer, 0, 4);
            stream.Read(buffer, 4, 4);
            stream.Read(buffer, 8, 4);
            return new Vector3
            {
                x = BitConverter.ToSingle(buffer, 0),
                y = BitConverter.ToSingle(buffer, 4),
                z = BitConverter.ToSingle(buffer, 8)
            };
        }
        [TestMethod]
        public void CustomFunctions()
        {
            //strings
            WritableSystem.Write(stream, new Vector3{ x = 0.0f, y = -1.0f, z = 1.0f, });
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(WritableSystem.Read<Vector3>(stream), new Vector3 { x = 0.0f, y = -1.0f, z = 1.0f, });
            stream.Seek(0, SeekOrigin.Begin);
        }

        //Custom IWritable Structs!
        struct Construct : IWritable
        {
            public string a;
            public int b;
            public Vector3 c;
            public int[] d;
            public override bool Equals(object obj)
            {
                return (obj is Construct) && 
                    ((Construct)obj).a == a && 
                    ((Construct)obj).b == b && 
                    ((Construct)obj).c.Equals(c) &&
                    Enumerable.SequenceEqual(((Construct)obj).d, d);
            }
        }
        [TestMethod]
        public void Structs()
        {
            //strings
            WritableSystem.Write(stream, 
                new Construct
                {
                    a = "what's up",
                    b = -1,
                    c = new Vector3 { x = 0.0f, y = -1.0f, z = 1.0f, },
                    d = new int[]{ 0, 1, 2, 3 }
                });
            stream.Seek(0, SeekOrigin.Begin);
            Construct value = WritableSystem.Read<Construct>(stream);
            System.Diagnostics.Trace.WriteLine(value.a);
            System.Diagnostics.Trace.WriteLine(value.b);
            System.Diagnostics.Trace.WriteLine(value.c.x + " " + value.c.y + " " + value.c.z);
            System.Diagnostics.Trace.WriteLine(value.d);
            Assert.AreEqual(value, 
                new Construct
                {
                    a = "what's up",
                    b = -1,
                    c = new Vector3 { x = 0.0f, y = -1.0f, z = 1.0f, },
                    d = new int[] { 0, 1, 2, 3 }
                });
            stream.Seek(0, SeekOrigin.Begin);
        }
    }
}
