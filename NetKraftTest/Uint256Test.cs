
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netkraft;
namespace NetkraftTest
{
    [TestClass]
    public class Uint256Test
    {
        [TestMethod]
        public void computation()
        {
        }
        [TestMethod]
        public void Equals()
        {
        }
        [TestMethod]
        public void Bitwise()
        {
            //Simple
            Uint256 num = new Uint256(255);
            //Left
            num <<= 1;
            Assert.AreEqual((ulong)510, num.Longs()[0]);
            Assert.AreEqual((ulong)0, num.Longs()[1]);
            Assert.AreEqual((ulong)0, num.Longs()[2]);
            Assert.AreEqual((ulong)0, num.Longs()[3]);
            //Right
            num >>= 1;
            Assert.AreEqual((ulong)255, num.Longs()[0]);
            Assert.AreEqual((ulong)0, num.Longs()[1]);
            Assert.AreEqual((ulong)0, num.Longs()[2]);
            Assert.AreEqual((ulong)0, num.Longs()[3]);
            //Left
            num <<= 3;
            Assert.AreEqual((ulong)2040, num.Longs()[0]);
            Assert.AreEqual((ulong)0, num.Longs()[1]);
            Assert.AreEqual((ulong)0, num.Longs()[2]);
            Assert.AreEqual((ulong)0, num.Longs()[3]);
            //Right
            num >>= 3;
            Assert.AreEqual((ulong)255, num.Longs()[0]);
            Assert.AreEqual((ulong)0, num.Longs()[1]);
            Assert.AreEqual((ulong)0, num.Longs()[2]);
            Assert.AreEqual((ulong)0, num.Longs()[3]);
            //More
            num = new Uint256(255);
            //Left
            num <<= 64;
            Assert.AreEqual((ulong)0, num.Longs()[0]);
            Assert.AreEqual((ulong)255, num.Longs()[1]);
            Assert.AreEqual((ulong)0, num.Longs()[2]);
            Assert.AreEqual((ulong)0, num.Longs()[3]);
            //Right
            num >>= 64;
            Assert.AreEqual((ulong)255, num.Longs()[0]);
            Assert.AreEqual((ulong)0, num.Longs()[1]);
            Assert.AreEqual((ulong)0, num.Longs()[2]);
            Assert.AreEqual((ulong)0, num.Longs()[3]);

            num = new Uint256(1);
            //Left
            num <<= 64;
            Assert.AreEqual((ulong)0, num.Longs()[0]);
            Assert.AreEqual((ulong)1, num.Longs()[1]);
            Assert.AreEqual((ulong)0, num.Longs()[2]);
            Assert.AreEqual((ulong)0, num.Longs()[3]);
            //Right
            num >>= 64;
            Assert.AreEqual((ulong)1, num.Longs()[0]);
            Assert.AreEqual((ulong)0, num.Longs()[1]);
            Assert.AreEqual((ulong)0, num.Longs()[2]);
            Assert.AreEqual((ulong)0, num.Longs()[3]);

            //Shit that fails...
            num = new Uint256(255);
            //Left
            num <<= 57;
            Assert.AreEqual((ulong)18302628885633695744, num.Longs()[0]);
            Assert.AreEqual((ulong)1, num.Longs()[1]);
            Assert.AreEqual((ulong)0, num.Longs()[2]);
            Assert.AreEqual((ulong)0, num.Longs()[3]);
            //Right
            num >>= 57;
            Assert.AreEqual((ulong)255, num.Longs()[0]);
            Assert.AreEqual((ulong)0, num.Longs()[1]);
            Assert.AreEqual((ulong)0, num.Longs()[2]);
            Assert.AreEqual((ulong)0, num.Longs()[3]);

            //Left
            num <<= 128 + 57;
            Assert.AreEqual((ulong)0, num.Longs()[0]);
            Assert.AreEqual((ulong)0, num.Longs()[1]);
            Assert.AreEqual((ulong)18302628885633695744, num.Longs()[2]);
            Assert.AreEqual((ulong)1, num.Longs()[3]);
            //Right
            num >>= 128;
            Assert.AreEqual((ulong)18302628885633695744, num.Longs()[0]);
            Assert.AreEqual((ulong)1, num.Longs()[1]);
            Assert.AreEqual((ulong)0, num.Longs()[2]);
            Assert.AreEqual((ulong)0, num.Longs()[3]);
            //Right
            num >>= 57;
            Assert.AreEqual((ulong)255, num.Longs()[0]);
            Assert.AreEqual((ulong)0, num.Longs()[1]);
            Assert.AreEqual((ulong)0, num.Longs()[2]);
            Assert.AreEqual((ulong)0, num.Longs()[3]);
        }
    }
}
