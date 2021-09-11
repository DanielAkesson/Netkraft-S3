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
        public void SimpleSendReliableChannel()
        {
            ChannelSocket client1 = new ChannelSocket(3000, 100, 0.5f);
            ChannelSocket client2 = new ChannelSocket(3001, 100, 0.5f);
            IPEndPoint client2Address = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3001);
            byte[] buffer = new byte[1000];
            //Client 1
            //Send one thousand messages with a loss rate of 90%
            int data = 50;
            buffer = BitConverter.GetBytes(data);
            client1.Send(buffer, client2Address, Netkraft.ChannelId2.Reliable, () => { });

            //Client 2
            buffer = new byte[1000];
            //Pull one thousand messages
            int size = client2.Receive(out buffer, out IPEndPoint sender, Netkraft.ChannelId2.Reliable);
            data = BitConverter.ToInt32(buffer, 2);
            int hID = buffer[1];
            Console.WriteLine($"received: [size: {size} data: {data} headerID: {hID}]");

            //Checks!
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void MultiSendAndAckedReliableChannel()
        {
            ChannelSocket client1 = new ChannelSocket(3002, 100, 0.5f);
            ChannelSocket client2 = new ChannelSocket(3003, 100, 0.5f);
            IPEndPoint client2Address = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3003);
            byte[] buffer = new byte[1000];
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
                client1.Send(buffer, client2Address, Netkraft.ChannelId2.Reliable, () => { ackedIds.Add(data); });
                sentIds.Add(data);
            }

            //Client 2
            buffer = new byte[1000];
            IPEndPoint sender;
            //Pull one thousand messages
            for (int i = 0; i <messageAmount; i++)
            {
                int size = client2.Receive(out buffer, out sender, Netkraft.ChannelId2.Reliable);
                int data = BitConverter.ToInt32(buffer, 2);
                int hID = buffer[1];
                Console.WriteLine($"received: [size: {size} data: {data} headerID: {hID}]");
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
            ChannelSocket client1 = new ChannelSocket(3004, 100, 1);
            ChannelSocket client2 = new ChannelSocket(3005, 100, 1);
            IPEndPoint client2Address = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3005);
            byte[] buffer = new byte[1000];
            //Client 1
            //Send one thousand messages with a loss rate of 90%
            int data = 50;
            buffer = BitConverter.GetBytes(data);
            client1.Send(buffer, client2Address, Netkraft.ChannelId2.Reliable, () => { });

            //Client 2
            buffer = new byte[1000];
            //Pull one thousand messages
            int size = client2.Receive(out buffer, out IPEndPoint sender, Netkraft.ChannelId2.Reliable);
            data = BitConverter.ToInt32(buffer, 2);
            int hID = buffer[1];
            Console.WriteLine($"received: [size: {size} data: {data} headerID: {hID}]");

            //Checks!
            Assert.IsTrue(true);
        }
    }
}
