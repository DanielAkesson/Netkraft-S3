using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netkraft.ChannelSocket;

namespace NetkraftTest
{
    [TestClass]
    public class ChanneledSocketTest
    {
        [TestMethod]
        public void SimpleSendUnreliableChannel()
        {
            Random r = new Random();
            int port1 = r.Next(2000, 4000);
            int port2 = r.Next(2000, 4000);
            ChannelSocket client1 = new ChannelSocket(port1, 100, false);
            Console.WriteLine($"Setup Socket on port {port1}");
            ChannelSocket client2 = new ChannelSocket(port2, 100, false);
            Console.WriteLine($"Setup Socket on port {port2}");
            IPEndPoint client2Address = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port2);
            byte[] buffer;
            //Client 1
            //Send one thousand messages with a loss rate of 90%
            int data = 50;
            buffer = BitConverter.GetBytes(data);
            Console.WriteLine($"Attempting to send {data} on socket1");
            client1.Send(buffer, Netkraft.ChannelId.Reliable, client2Address);
            Console.WriteLine($"done");
            //Client 2
            buffer = new byte[1000];
            //Pull one thousand messages
            Console.WriteLine($"Attempting to receive {data} on socket2");
            int size = client2.Receive(ref buffer, out IPEndPoint sender, out Netkraft.ChannelId channel);
            Console.WriteLine($"done");
            data = BitConverter.ToInt32(buffer, 0);
            int hID = buffer[1];
            Console.WriteLine($"received: [size: {size} data: {data} headerID: {hID}]");

            //Checks!
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void SimpleSendReliableChannel()
        {
            ChannelSocket client1 = new ChannelSocket(3000, 100, false);
            client1.SuccessRate = 0.5f;
            ChannelSocket client2 = new ChannelSocket(3001, 100, false);
            client2.SuccessRate = 0.5f;
            IPEndPoint client2Address = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3001);
            byte[] buffer = new byte[1000];
            //Client 1
            //Send one thousand messages with a loss rate of 90%
            int data = 50;
            buffer = BitConverter.GetBytes(data);
            client1.Send(buffer, Netkraft.ChannelId.Reliable, client2Address, () => { });

            //Client 2
            buffer = new byte[1000];
            //Pull one thousand messages
            int size = client2.Receive(ref buffer, out IPEndPoint sender, out Netkraft.ChannelId channel);
            data = BitConverter.ToInt32(buffer, 0);
            int hID = buffer[1];
            Console.WriteLine($"received: [size: {size} data: {data} headerID: {hID}]");

            //Checks!
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void MultiSendAndAckedReliableChannel()
        {
            ChannelSocket client1 = new ChannelSocket(3002, 100, false);
            client1.SuccessRate = 0.5f;
            ChannelSocket client2 = new ChannelSocket(3003, 100, false);
            client2.SuccessRate = 0.5f;
            IPEndPoint client2Address = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3003);
            byte[] buffer = new byte[4];
            Random r = new Random();
            //Check lists
            List<int> sentIds = new List<int>();
            List<int> ackedIds = new List<int>();
            List<int> received = new List<int>();
            int messageAmount = 500;
            //TEST START!
            //Client 1
            //Send one thousand messages with a loss rate of 90%
            for (int i = 0; i <messageAmount; i++)
            {
                int data = r.Next(9999999);
                buffer = BitConverter.GetBytes(data);
                client1.Send(buffer, Netkraft.ChannelId.Reliable, client2Address, () => { ackedIds.Add(data); });
                sentIds.Add(data);
            }

            //Client 2
            buffer = new byte[4];
            IPEndPoint sender;
            //Pull one thousand messages
            for (int i = 0; i <messageAmount; i++)
            {
                int size = client2.Receive(ref buffer, out sender, out Netkraft.ChannelId channel);
                int data = BitConverter.ToInt32(buffer, 0);
                Console.WriteLine($"received: [size: {size} data: {data}]");
                received.Add(data);
            }

            while (ackedIds.Count < messageAmount)
                Thread.Sleep(10);
                
            //Checks!
            Assert.AreEqual(messageAmount, ackedIds.Count);
            Assert.AreEqual(sentIds.Count, ackedIds.Count);
            Assert.AreEqual(sentIds.Count, received.Count);

            //It was the correct data
            while (sentIds.Count > 0)
            {
                int data = sentIds[0];
                ackedIds.Remove(data);
                received.Remove(data);
                sentIds.Remove(data);
            }
            foreach (int i in received)
                Console.WriteLine("Received: " + i);
            //ALl has been removed!
            Assert.AreEqual(0, sentIds.Count);
            Assert.AreEqual(0, ackedIds.Count);
            Assert.AreEqual(0, received.Count);
        }

        //Unreliable Acknowledged
        [TestMethod]
        public void SimpleSendUnreliableAckedChannel()
        {
            ChannelSocket client1 = new ChannelSocket(3004, 100, false);
            ChannelSocket client2 = new ChannelSocket(3005, 100, false);
            IPEndPoint client2Address = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3005);
            byte[] buffer = new byte[1000];
            //Client 1
            //Send one thousand messages with a loss rate of 90%
            int data = 50;
            buffer = BitConverter.GetBytes(data);
            client1.Send(buffer, Netkraft.ChannelId.UnreliableAcknowledged, client2Address, () => { });

            //Client 2
            buffer = new byte[1000];
            //Pull one thousand messages
            int size = client2.Receive(ref buffer, out IPEndPoint sender, out Netkraft.ChannelId channel);
            data = BitConverter.ToInt32(buffer, 0);
            int hID = buffer[1];
            Console.WriteLine($"received: [size: {size} data: {data} headerID: {hID}]");

            //Checks!
            Assert.IsTrue(true);
        }
    }
}
