using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netkraft.Messaging;
using Netkraft;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace NetKraftTest
{
    [TestClass]
    public class ReliableAckChannelTests
    {
        private static Dictionary<int, bool> _messageAcked = new Dictionary<int, bool>();
        private static Dictionary<int, bool> _messageRecived = new Dictionary<int, bool>();
        private static Dictionary<int, bool> _messageSent = new Dictionary<int, bool>();

        [TestMethod]
        public void Test1000Messages()
        {
            // Arrange
            NetkraftClient client1 = new NetkraftClient(2000);
            NetkraftClient client2 = new NetkraftClient(3000);
            client1.AddEndPoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000));
            client2.AddEndPoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000));
            client1.FakeLossProcent = 40;
            // Act
            for (int i = 0; i < 1000; i++)
            {
                client1.AddToQueue(new TestMessage {index = i});
                _messageSent.Add(i, true);
                client1.SendQueue();
                client2.SendQueue();
                client1.ReceiveTick();
                client2.ReceiveTick();
                Thread.Sleep(10);
            }

            //Allow catchup
            for(int i= 0;i<1000;i++)
            {
                client1.ReceiveTick();
                client2.ReceiveTick();
                Thread.Sleep(10);
            }

            //Asserts
            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(_messageSent.ContainsKey(i) && _messageSent[i], true, "Message was not sent!: " + i);
                Assert.AreEqual(_messageRecived.ContainsKey(i) && _messageRecived[i], true, "Message was never recived!: " + i);
                Assert.AreEqual(_messageAcked.ContainsKey(i) && _messageAcked[i], true, "Message was not acknowlaged!: " + i);
            }
        }

        public struct TestMessage : IReliableAcknowledgedMessage
        {
            public int index;

            public void OnAcknowledgment(ClientConnection Context)
            {
                Assert.AreEqual(_messageSent.ContainsKey(index), true, "Acknowlaged message that has not been sent!: " + index);
                _messageAcked.Add(index, true);
            }

            public void OnReceive(ClientConnection Context)
            {
                Assert.AreEqual(_messageSent.ContainsKey(index), true, "Recived a message that has not been sent!: " + index);
                _messageRecived.Add(index, true);
            }

            public void OnSend(ClientConnection Context){}
        }
    }
}
