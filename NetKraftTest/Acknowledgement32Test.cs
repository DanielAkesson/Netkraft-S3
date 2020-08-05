using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netkraft;
using System.Linq;
namespace NetkraftTest
{
    /// <summary>
    /// Acknowledgement test
    /// </summary>
    [TestClass]
    public class Acknowledgement32Test
    {
        Acknowledger32 ack;

        [TestMethod]
        public void TestMasks()
        {
            ack = new Acknowledger32(Acknowledgement);
            //Lady 1 sends 7 messages
            ack.OnReceiveMessage(0);
            ack.OnReceiveMessage(2);
            ack.OnReceiveMessage(3);
            ack.OnReceiveMessage(5);
            ack.OnReceiveMessage(7);
            ack.OnReceiveMessage(9);
            ack.OnReceiveMessage(14);

            //Dude 2 looses all ack responses except foir 9 and 14
            ushort mask = ack.GetReceiveMaskForId(9);
            ack.OnReceiveAcknowledgement(mask, 9);
            mask = ack.GetReceiveMaskForId(14);
            ack.OnReceiveAcknowledgement(mask, 14);
            //Dude 1 should still see that all messages has arrived intact and get acknowledgemnets for them.
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
            ack = new Acknowledger32(Acknowledgement);
            Random r = new Random();

            for (int i=0;i<32;i++)
            {
                if(r.NextDouble() > 0.5)
                {
                    ack.OnReceiveMessage(i);
                    sentNumbers.Add(i);
                }
            }

            ushort mask = ack.GetReceiveMaskForId(3);
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
