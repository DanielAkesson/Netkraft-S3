using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netkraft;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace NetkraftTest
{
    /// <summary>
    /// Acknowledgement test
    /// </summary>
    [TestClass]
    public class Acknowledgement256Test
    {
        [TestMethod]
        public void Receive()
        {
            List<int> acked = new List<int>();
            Acknowledger256 ack = new Acknowledger256((i) => { acked.Add(i); });
            //Lady 1 sends messages
            ack.OnReceiveMessage(0);
            ack.OnReceiveMessage(5);
            ack.OnReceiveMessage(3);
            ack.OnReceiveMessage(9);
            ushort mask = ack.GetShortReceiveMaskForId(9);
            Assert.AreEqual(35392, mask);
        }
        [TestMethod]
        public void Acked16()
        {
            List<int> acked = new List<int>();
            Acknowledger256 ack = new Acknowledger256((i) => { acked.Add(i); });
            //Lady 1 sends messages
            ack.OnSendMessage(0);
            ack.OnSendMessage(5);
            ack.OnSendMessage(3);
            ack.OnSendMessage(9);
            //Dude 2 receives
            ack.OnReceiveMessage(0);
            ack.OnReceiveMessage(5);
            ack.OnReceiveMessage(3);
            ack.OnReceiveMessage(9);
            //He send back his ack mask
            ushort mask = ack.GetShortReceiveMaskForId(9);
            //Lady check if all is acked
            ack.OnReceiveAcknowledgement(mask, 9);
            Assert.IsTrue(acked.Contains(0));
            Assert.IsTrue(acked.Contains(5));
            Assert.IsTrue(acked.Contains(3));
            Assert.IsTrue(acked.Contains(9));
        }
        [TestMethod]
        public void Acked32()
        {
            List<int> acked = new List<int>();
            Acknowledger256 ack = new Acknowledger256((i) => { acked.Add(i); });
            //Lady 1 sends messages
            ack.OnSendMessage(0);
            ack.OnSendMessage(5);
            ack.OnSendMessage(3);
            ack.OnSendMessage(9);
            ack.OnSendMessage(15);
            ack.OnSendMessage(19);
            ack.OnSendMessage(23);
            ack.OnSendMessage(30);
            //Dude 2 receives
            ack.OnReceiveMessage(0);
            ack.OnReceiveMessage(5);
            ack.OnReceiveMessage(3);
            ack.OnReceiveMessage(9);
            ack.OnReceiveMessage(15);
            ack.OnReceiveMessage(19);
            ack.OnReceiveMessage(23);
            ack.OnReceiveMessage(30);
            //He send back his ack mask
            uint mask = ack.GetIntReceiveMaskForId(30);
            //Lady check if all is acked
            ack.OnReceiveAcknowledgement(mask, 30);
            Assert.IsTrue(acked.Contains(0));
            Assert.IsTrue(acked.Contains(5));
            Assert.IsTrue(acked.Contains(3));
            Assert.IsTrue(acked.Contains(9));
            Assert.IsTrue(acked.Contains(15));
            Assert.IsTrue(acked.Contains(19));
            Assert.IsTrue(acked.Contains(23));
            Assert.IsTrue(acked.Contains(30));
        }
       
        [TestMethod]
        public void RealTestCase()
        {
            List<int> acked = new List<int>();
            Acknowledger256 ladyAck = new Acknowledger256((i) => { acked.Add(i); });
            Acknowledger256 DudeAck = new Acknowledger256((i) => { });
            //Lady 1 sends messages
            for (int i=0;i<128;i++)
            {
                ladyAck.OnSendMessage(i);
            }

            //Dude receives some of them
            DudeAck.OnReceiveMessage(0);
            DudeAck.OnReceiveMessage(2);
            DudeAck.OnReceiveMessage(3);
            DudeAck.OnReceiveMessage(5);
            DudeAck.OnReceiveMessage(7);
            DudeAck.OnReceiveMessage(9);
            DudeAck.OnReceiveMessage(14);
            DudeAck.OnReceiveMessage(100);
            DudeAck.OnReceiveMessage(67);
            DudeAck.OnReceiveMessage(58);
            DudeAck.OnReceiveMessage(114);

            //Dude 2 looses all ack responses except for some
            ushort mask = DudeAck.GetShortReceiveMaskForId(9);
            ladyAck.OnReceiveAcknowledgement(mask, 9);
            mask = DudeAck.GetShortReceiveMaskForId(14);
            ladyAck.OnReceiveAcknowledgement(mask, 14);
            mask = DudeAck.GetShortReceiveMaskForId(67);
            ladyAck.OnReceiveAcknowledgement(mask, 67);
            mask = DudeAck.GetShortReceiveMaskForId(100);
            ladyAck.OnReceiveAcknowledgement(mask, 100);
            mask = DudeAck.GetShortReceiveMaskForId(114);
            ladyAck.OnReceiveAcknowledgement(mask, 114);

            //Lady 1 should still see that all these messages has arrived intact and get acknowledgemnets for them.
            Assert.IsTrue(acked.Contains(0));
            Assert.IsTrue(acked.Contains(2));
            Assert.IsTrue(acked.Contains(3));
            Assert.IsTrue(acked.Contains(5));
            Assert.IsTrue(acked.Contains(7));
            Assert.IsTrue(acked.Contains(9));
            Assert.IsTrue(acked.Contains(14));
            Assert.IsTrue(acked.Contains(58));
            Assert.IsTrue(acked.Contains(67));
            Assert.IsTrue(acked.Contains(100));
            Assert.IsTrue(acked.Contains(114));
        }
    }
}
