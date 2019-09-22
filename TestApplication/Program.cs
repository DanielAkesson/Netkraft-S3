using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Netkraft;
using Netkraft.Messaging;
using System.Net;

namespace TestApplication
{
    class Program
    {

        public static int Sent = 0;
        public static int Acked = 0;
        public static int Recived = 0;
        static void Main(string[] args)
        {
            TestAllTypes();
        }

        static void TestReliableAcked()
        {
            NetkraftClient client1 = new NetkraftClient(2001);
            NetkraftClient client2 = new NetkraftClient(3000);

            client1.Host();
            client2.Join("127.0.0.1", 2001);
            client2.FakeLossProcent = 50;
            client1.FakeLossProcent = 50;
            while (true)
            {
                //Send message to host
                if(new Random().NextDouble() > 0.5)
                {
                    client2.AddToQueue(new SimpleHello { HelloMessage = "Tjabba" });
                    Sent++;
                }
                client1.SendQueue();
                client2.SendQueue();
                client1.ReceiveTick();
                client2.ReceiveTick();
                Thread.Sleep(10);
                Console.ForegroundColor = Sent - Recived == 0 ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine("Recive Fail: " + (Sent - Recived));
                Console.ForegroundColor = Sent - Acked == 0 ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine("Acked Fail: " + (Sent - Acked));
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        static void TestUnreliableAcked()
        {
            NetkraftClient client1 = new NetkraftClient(2001);
            NetkraftClient client2 = new NetkraftClient(3000);

            client1.Host();
            client2.Join("127.0.0.1", 2001);
            client2.FakeLossProcent = 0;
            client1.FakeLossProcent = 0;
            while (true)
            {
                //Send message to host
                if (new Random().NextDouble() > 0.5)
                {
                    client2.AddToQueue(new SimpleHello2 { HelloMessage = "Tjabba" });
                    Sent++;
                }
                client1.SendQueue();
                client2.SendQueue();
                client1.ReceiveTick();
                client2.ReceiveTick();
                Thread.Sleep(10);
                Console.ForegroundColor = Sent - Recived == 0 ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine("Recive Fail: " + (Sent - Recived));
                Console.ForegroundColor = Sent - Acked == 0 ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine("Acked Fail: " + (Sent - Acked));
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        static void TestMessage()
        {
            NetkraftClient client1 = new NetkraftClient(2000);
            NetkraftClient client2 = new NetkraftClient(3000);

            client1.Host();
            client2.Join("127.0.0.1", 2000);


            client1.FakeLossProcent = 10;
            while (true)
            {
                Console.WriteLine("Sending");
                client1.AddToQueue(new Hello
                {
                    HelloMessage = "Hello world!",
                    Num1 = 100,
                    Num2 = -200,
                    HelloWritableValue = new HelloWritable
                    {
                        NumArrayDesc = "5 Elements",
                        NumArray = new int[] { 1, 2, 3, 4, 5 }
                    }
                });
                client1.SendQueue();
                client2.SendQueue();

                client1.ReceiveTick();
                client2.ReceiveTick();
                Thread.Sleep(100);
            }
        }
        static void TestAllTypes()
        {
            NetkraftClient client1 = new NetkraftClient(2000);
            NetkraftClient client2 = new NetkraftClient(3000);

            client1.Host();
            client2.Join("127.0.0.1", 2000);
            while (true)
            {
                Console.WriteLine("Sending");
                client1.AddToQueue(new TestAllTypes());
                client1.SendQueue();
                client2.SendQueue();

                client1.ReceiveTick();
                client2.ReceiveTick();
                Thread.Sleep(100);
            }
        }
    }

    public struct Hello : IReliableMessage, IAcknowledged
    {
        public string HelloMessage;
        public int Num1;
        [SkipIndex]
        public int Num2;
        public HelloWritable HelloWritableValue;

        public void OnAcknowledgment(ClientConnection Context)
        {
            Console.WriteLine("Acked");
        }

        public void OnReceive(ClientConnection Context)
        {
            Console.WriteLine("Recevied: " 
                + " HelloMessage: " + HelloMessage + System.Environment.NewLine
                + " Num1: " + Num1 + System.Environment.NewLine
                + " Num2: " + Num2 + System.Environment.NewLine
                + " HelloWritableValue: " + HelloWritableValue.ToString());
        }

        public void OnSend(ClientConnection Context){}
    }

    public struct SimpleHello : IReliableMessage, IAcknowledged
    {
        public string HelloMessage;

        public void OnReceive(ClientConnection Context)
        {
            Program.Recived++;
        }

        public void OnAcknowledgment(ClientConnection Context)
        {
            Program.Acked++;
        }
    }

    public struct SimpleHello2 : IUnreliableMessage, IAcknowledged
    {
        public string HelloMessage;

        public void OnReceive(ClientConnection Context)
        {
            Program.Recived++;
        }

        public void OnAcknowledgment(ClientConnection Context)
        {
            Program.Acked++;
        }
    }

    public class TestAllTypes : IUnreliableMessage
    {
        public int intvar = -1;
        public uint uintvar = 1;
        public short shortvar = -2;
        public ushort ushortvar = 2;
        public long longvar = -3;
        public ulong ulongvar = 3;
        public byte bytevar = 4;
        public float singelvar = 5.5f;
        public double doublevar = 6.6d;
        public string stringvar = "Hello";
        public HelloWritable WritableVar = new HelloWritable
        {
            NumArrayDesc = "5 Elements",
            NumArray = new int[] { 1, 2, 3, 4, 5 }
        };

        public void OnReceive(ClientConnection Context)
        {
            Console.WriteLine($"Succes: {intvar}, {uintvar}, {shortvar}, {ushortvar}, {longvar}, {ulongvar}, {bytevar}, {singelvar}, {doublevar}, {stringvar}");
        }
    }


    [Writable]
    public struct HelloWritable
    {
        public string NumArrayDesc;
        public int[] NumArray;
        public string ToString()
        {
            string temp = "Name: " + NumArrayDesc + " Numbs: ";
            foreach (int i in NumArray)
                temp += i + ",";
            return temp;
        }
    }
}
