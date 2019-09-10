using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netkraft;
using System.Linq;
namespace NetKraftTest
{
    /// <summary>
    /// Acknowledgement test
    /// </summary>
    [TestClass]
    public class AcknowledgementClassTest
    {
        Acknowledger ack;
        [TestMethod]
        public void TestMasks()
        {
            ack = new Acknowledger(Acknowledgement);
            ack.OnReceiveMessage(0);
            ack.OnReceiveMessage(2);
            ack.OnReceiveMessage(3);
            ack.OnReceiveMessage(5);
            ack.OnReceiveMessage(7);
            ack.OnReceiveMessage(9);
            ack.OnReceiveMessage(14);
            uint mask = ack.GetReceiveMaskForId(9);
            ack.OnReceiveAcknowledgement(mask, 9);
            mask = ack.GetReceiveMaskForId(14);
            ack.OnReceiveAcknowledgement(mask, 14);
        }

        public void Acknowledgement(int i)
        {
            int[] numbers = new int[] { 0, 2, 3, 5, 7, 9, 14 };
            Assert.AreEqual(numbers.Contains(i), true);
        }

        [TestMethod]
        public void TestLoopAround()
        {
            List<int> sentNumbers = new List<int>();
            List<int> beenAcked = new List<int>();
            ack = new Acknowledger(Acknowledgement);
            Random r = new Random();

            for (int i=0;i<60;i++)
            {
                if(r.NextDouble() > 0.5)
                {
                    ack.OnReceiveMessage(i);
                    sentNumbers.Add(i);
                }
            }

            uint mask = ack.GetReceiveMaskForId(3);
            ack.OnReceiveAcknowledgement(mask, 3);

            void Acknowledgement(int i)
            {
                Assert.AreEqual(beenAcked.Contains(i), false);
                Assert.AreEqual(sentNumbers.Contains(i), true);
                beenAcked.Add(i);
            }
        }
    }
}
