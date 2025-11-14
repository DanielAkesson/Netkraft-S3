using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using Netkraft.WritableSystem;

namespace NetkraftTest
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
            Writable.WriteRaw(stream, (Int16)1);
            Writable.WriteRaw(stream, (Int16)0);
            Writable.WriteRaw(stream, (Int16)(-1));
            Writable.WriteRaw(stream, Int16.MaxValue);
            Writable.WriteRaw(stream, Int16.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<Int16>(stream), (Int16)1);
            Assert.AreEqual(Writable.ReadRaw<Int16>(stream), (Int16)0);
            Assert.AreEqual(Writable.ReadRaw<Int16>(stream), (Int16)(-1));
            Assert.AreEqual(Writable.ReadRaw<Int16>(stream), Int16.MaxValue);
            Assert.AreEqual(Writable.ReadRaw<Int16>(stream), Int16.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //Int32
            Writable.WriteRaw(stream, (Int32)1);
            Writable.WriteRaw(stream, (Int32)0);
            Writable.WriteRaw(stream, (Int32)(-1));
            Writable.WriteRaw(stream, Int32.MaxValue);
            Writable.WriteRaw(stream, Int32.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<Int32>(stream), (Int32)1);
            Assert.AreEqual(Writable.ReadRaw<Int32>(stream), (Int32)0);
            Assert.AreEqual(Writable.ReadRaw<Int32>(stream), (Int32)(-1));
            Assert.AreEqual(Writable.ReadRaw<Int32>(stream), Int32.MaxValue);
            Assert.AreEqual(Writable.ReadRaw<Int32>(stream), Int32.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //Int64
            Writable.WriteRaw(stream, (Int64)1);
            Writable.WriteRaw(stream, (Int64)0);
            Writable.WriteRaw(stream, (Int64)(-1));
            Writable.WriteRaw(stream, Int64.MaxValue);
            Writable.WriteRaw(stream, Int64.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<Int64>(stream), (Int64)1);
            Assert.AreEqual(Writable.ReadRaw<Int64>(stream), (Int64)0);
            Assert.AreEqual(Writable.ReadRaw<Int64>(stream), (Int64)(-1));
            Assert.AreEqual(Writable.ReadRaw<Int64>(stream), Int64.MaxValue);
            Assert.AreEqual(Writable.ReadRaw<Int64>(stream), Int64.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //short
            Writable.WriteRaw(stream, (short)1);
            Writable.WriteRaw(stream, (short)0);
            Writable.WriteRaw(stream, (short)(-1));
            Writable.WriteRaw(stream, short.MaxValue);
            Writable.WriteRaw(stream, short.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<short>(stream), (short)1);
            Assert.AreEqual(Writable.ReadRaw<short>(stream), (short)0);
            Assert.AreEqual(Writable.ReadRaw<short>(stream), (short)(-1));
            Assert.AreEqual(Writable.ReadRaw<short>(stream), short.MaxValue);
            Assert.AreEqual(Writable.ReadRaw<short>(stream), short.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //int
            Writable.WriteRaw(stream, (int)1);
            Writable.WriteRaw(stream, (int)0);
            Writable.WriteRaw(stream, (int)(-1));
            Writable.WriteRaw(stream, int.MaxValue);
            Writable.WriteRaw(stream, int.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<int>(stream), (int)1);
            Assert.AreEqual(Writable.ReadRaw<int>(stream), (int)0);
            Assert.AreEqual(Writable.ReadRaw<int>(stream), (int)(-1));
            Assert.AreEqual(Writable.ReadRaw<int>(stream), int.MaxValue);
            Assert.AreEqual(Writable.ReadRaw<int>(stream), int.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //long
            Writable.WriteRaw(stream, (long)1);
            Writable.WriteRaw(stream, (long)0);
            Writable.WriteRaw(stream, (long)(-1));
            Writable.WriteRaw(stream, long.MaxValue);
            Writable.WriteRaw(stream, long.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<long>(stream), (long)1);
            Assert.AreEqual(Writable.ReadRaw<long>(stream), (long)0);
            Assert.AreEqual(Writable.ReadRaw<long>(stream), (long)(-1));
            Assert.AreEqual(Writable.ReadRaw<long>(stream), long.MaxValue);
            Assert.AreEqual(Writable.ReadRaw<long>(stream), long.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
        }
        [TestMethod]
        public void UInts()
        {
            //UInt16
            Writable.WriteRaw(stream, (UInt16)1);
            Writable.WriteRaw(stream, (UInt16)0);
            Writable.WriteRaw(stream, UInt16.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<UInt16>(stream), (UInt16)1);
            Assert.AreEqual(Writable.ReadRaw<UInt16>(stream), (UInt16)0);
            Assert.AreEqual(Writable.ReadRaw<UInt16>(stream), UInt16.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

            //UInt32
            Writable.WriteRaw(stream, (UInt32)1);
            Writable.WriteRaw(stream, (UInt32)0);
            Writable.WriteRaw(stream, UInt32.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<UInt32>(stream), (UInt32)1);
            Assert.AreEqual(Writable.ReadRaw<UInt32>(stream), (UInt32)0);
            Assert.AreEqual(Writable.ReadRaw<UInt32>(stream), UInt32.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

            //UInt64
            Writable.WriteRaw(stream, (UInt64)1);
            Writable.WriteRaw(stream, (UInt64)0);
            Writable.WriteRaw(stream, UInt64.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<UInt64>(stream), (UInt64)1);
            Assert.AreEqual(Writable.ReadRaw<UInt64>(stream), (UInt64)0);
            Assert.AreEqual(Writable.ReadRaw<UInt64>(stream), UInt64.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

            //byte
            Writable.WriteRaw(stream, (byte)1);
            Writable.WriteRaw(stream, (byte)0);
            Writable.WriteRaw(stream, byte.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<byte>(stream), (byte)1);
            Assert.AreEqual(Writable.ReadRaw<byte>(stream), (byte)0);
            Assert.AreEqual(Writable.ReadRaw<byte>(stream), byte.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

            //ushort
            Writable.WriteRaw(stream, (ushort)1);
            Writable.WriteRaw(stream, (ushort)0);
            Writable.WriteRaw(stream, ushort.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<ushort>(stream), (ushort)1);
            Assert.AreEqual(Writable.ReadRaw<ushort>(stream), (ushort)0);
            Assert.AreEqual(Writable.ReadRaw<ushort>(stream), ushort.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

            //uint
            Writable.WriteRaw(stream, (uint)1);
            Writable.WriteRaw(stream, (uint)0);
            Writable.WriteRaw(stream, uint.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<uint>(stream), (uint)1);
            Assert.AreEqual(Writable.ReadRaw<uint>(stream), (uint)0);
            Assert.AreEqual(Writable.ReadRaw<uint>(stream), uint.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

            //ulong
            Writable.WriteRaw(stream, (ulong)1);
            Writable.WriteRaw(stream, (ulong)0);
            Writable.WriteRaw(stream, ulong.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<ulong>(stream), (ulong)1);
            Assert.AreEqual(Writable.ReadRaw<ulong>(stream), (ulong)0);
            Assert.AreEqual(Writable.ReadRaw<ulong>(stream), ulong.MaxValue);
            stream.Seek(0, SeekOrigin.Begin);

        }
        [TestMethod]
        public void FloatingPointNumbers()
        {
            //Single
            Writable.WriteRaw(stream, (Single)1);
            Writable.WriteRaw(stream, (Single)0);
            Writable.WriteRaw(stream, (Single)(-1));
            Writable.WriteRaw(stream, Single.MaxValue);
            Writable.WriteRaw(stream, Single.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<Single>(stream), (Single)1);
            Assert.AreEqual(Writable.ReadRaw<Single>(stream), (Single)0);
            Assert.AreEqual(Writable.ReadRaw<Single>(stream), (Single)(-1));
            Assert.AreEqual(Writable.ReadRaw<Single>(stream), Single.MaxValue);
            Assert.AreEqual(Writable.ReadRaw<Single>(stream), Single.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //Double
            Writable.WriteRaw(stream, (Double)1);
            Writable.WriteRaw(stream, (Double)0);
            Writable.WriteRaw(stream, (Double)(-1));
            Writable.WriteRaw(stream, Double.MaxValue);
            Writable.WriteRaw(stream, Double.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<Double>(stream), (Double)1);
            Assert.AreEqual(Writable.ReadRaw<Double>(stream), (Double)0);
            Assert.AreEqual(Writable.ReadRaw<Double>(stream), (Double)(-1));
            Assert.AreEqual(Writable.ReadRaw<Double>(stream), Double.MaxValue);
            Assert.AreEqual(Writable.ReadRaw<Double>(stream), Double.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //float
            Writable.WriteRaw(stream, (float)1);
            Writable.WriteRaw(stream, (float)0);
            Writable.WriteRaw(stream, (float)(-1));
            Writable.WriteRaw(stream, float.MaxValue);
            Writable.WriteRaw(stream, float.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<float>(stream), (float)1);
            Assert.AreEqual(Writable.ReadRaw<float>(stream), (float)0);
            Assert.AreEqual(Writable.ReadRaw<float>(stream), (float)(-1));
            Assert.AreEqual(Writable.ReadRaw<float>(stream), float.MaxValue);
            Assert.AreEqual(Writable.ReadRaw<float>(stream), float.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

            //double
            Writable.WriteRaw(stream, (double)1);
            Writable.WriteRaw(stream, (double)0);
            Writable.WriteRaw(stream, (double)(-1));
            Writable.WriteRaw(stream, double.MaxValue);
            Writable.WriteRaw(stream, double.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<double>(stream), (double)1);
            Assert.AreEqual(Writable.ReadRaw<double>(stream), (double)0);
            Assert.AreEqual(Writable.ReadRaw<double>(stream), (double)(-1));
            Assert.AreEqual(Writable.ReadRaw<double>(stream), double.MaxValue);
            Assert.AreEqual(Writable.ReadRaw<double>(stream), double.MinValue);
            stream.Seek(0, SeekOrigin.Begin);

        }
        [TestMethod]
        public void Boolean()
        {
            //bool
            Writable.WriteRaw(stream, true);
            Writable.WriteRaw(stream, false);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<bool>(stream), true);
            Assert.AreEqual(Writable.ReadRaw<bool>(stream), false);
            stream.Seek(0, SeekOrigin.Begin);
            //Boolean
            Writable.WriteRaw(stream, (Boolean)true);
            Writable.WriteRaw(stream, (Boolean)false);
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<Boolean>(stream), (Boolean)true);
            Assert.AreEqual(Writable.ReadRaw<Boolean>(stream), (Boolean)false);
            stream.Seek(0, SeekOrigin.Begin);

        }
        [TestMethod]
        public void Strings()
        {
            //strings
            Writable.WriteRaw(stream, "hej");
            Writable.WriteRaw(stream, "š↓▲ÿc§π√R╙╟Ä╥E");
            Writable.WriteRaw(stream, 'K');
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<string>(stream), "hej");
            Assert.AreEqual(Writable.ReadRaw<string>(stream), "š↓▲ÿc§π√R╙╟Ä╥E");
            Assert.AreEqual(Writable.ReadRaw<char>(stream), 'K');
            stream.Seek(0, SeekOrigin.Begin);
        }
        [TestMethod]
        public void ArrayTypes()
        {
            //strings
            Writable.WriteRaw(stream, new int[]{ 0, 1, 2, 3, 4 });
            Writable.WriteRaw(stream, new string[] { "0", "1", "2", "3", "4" });
            Writable.WriteRaw(stream, new int[]{ });
            stream.Seek(0, SeekOrigin.Begin);
            bool isEqual1 = Enumerable.SequenceEqual(Writable.ReadRaw<int[]>(stream), new int[] { 0, 1, 2, 3, 4 });
            Assert.IsTrue(isEqual1);
            bool isEqual2 = Enumerable.SequenceEqual(Writable.ReadRaw<string[]>(stream), new string[] { "0", "1", "2", "3", "4" });
            Assert.IsTrue(isEqual2);
            bool isEqual3 = Enumerable.SequenceEqual(Writable.ReadRaw<int[]>(stream), new int[] { });
            Assert.IsTrue(isEqual3);
            stream.Seek(0, SeekOrigin.Begin);
        }

        //Custom write functions
        internal static byte[] buffer = new byte[1024];
        private struct Vector3
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
            Writable.WriteRaw(stream, new Vector3{ x = 0.0f, y = -1.0f, z = 1.0f, });
            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(Writable.ReadRaw<Vector3>(stream), new Vector3 { x = 0.0f, y = -1.0f, z = 1.0f, });
            stream.Seek(0, SeekOrigin.Begin);
        }

        //Custom IWritable structs!
        private struct Construct : IWritable
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
            Writable.WriteRaw(stream, 
                new Construct
                {
                    a = "what's up",
                    b = -1,
                    c = new Vector3 { x = 0.0f, y = -1.0f, z = 1.0f, },
                    d = new int[]{ 0, 1, 2, 3 }
                });
            stream.Seek(0, SeekOrigin.Begin);
            Construct value = Writable.ReadRaw<Construct>(stream);
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

        //Custom IWritable structs!
        private class ConstructClass : IWritable
        {
            public Vector3 a;
            public override bool Equals(object obj)
            {
                return (obj is ConstructClass) && ((ConstructClass)obj).a.Equals(a);
            }
        }
        [TestMethod]
        public void Classes()
        {
            //strings
            Writable.WriteRaw(stream,
                new ConstructClass
                {
                    a = new Vector3 { x = 0.0f, y = -1.0f, z = 1.0f, },
                });
            stream.Seek(0, SeekOrigin.Begin);
            ConstructClass value = Writable.ReadRaw<ConstructClass>(stream);
            Assert.AreEqual(value,
                new ConstructClass
                {
                    a = new Vector3 { x = 0.0f, y = -1.0f, z = 1.0f, },
                });
            stream.Seek(0, SeekOrigin.Begin);
        }

        //Hierarchy
        private class BaseClass: IWritable
        {
            public int baseint;
            public override bool Equals(object obj)
            {
                return (obj is BaseClass) && ((BaseClass)obj).baseint.Equals(baseint);
            }
        }
        private class ChildClass: BaseClass, IWritable
        {
            public int childint;
            public override bool Equals(object obj)
            {
                return (obj is ChildClass) && ((ChildClass)obj).childint.Equals(childint) && ((ChildClass)obj).baseint.Equals(baseint);
            }
        }
        [TestMethod]
        public void Hierarchy()
        {
            //strings
            Writable.WriteRaw(stream,
                new ChildClass
                {
                    baseint = 1,
                    childint = 2,
                });

            stream.Seek(0, SeekOrigin.Begin);
            ChildClass value = Writable.ReadRaw<ChildClass>(stream);
            Assert.AreEqual(value,
                new ChildClass
                {
                    baseint = 1,
                    childint = 2,
                });
            stream.Seek(0, SeekOrigin.Begin);
        }
    }
}
