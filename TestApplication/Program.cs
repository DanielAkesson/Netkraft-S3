using Netkraft;
using Netkraft.Messaging;
using System;
using System.IO;
using System.Threading;

namespace TestApplication
{
    class Program
    {

        public static int Sent = 0;
        public static int Acked = 0;
        public static int Recived = 0;
        static void Main(string[] args)
        {
            TestDeltaCompression();
        }

        static void TestReliableAcked()
        {
            NetkraftClient client1 = new NetkraftClient(2001);
            NetkraftClient client2 = new NetkraftClient(3000);

            client1.Host();
            client2.Join("127.0.0.1", 2001);
            client2.FakeLossPercentage = 50;
            client1.FakeLossPercentage = 50;
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
            client2.FakeLossPercentage = 0;
            client1.FakeLossPercentage = 0;
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


            client1.FakeLossPercentage = 10;
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
        static void TestDeltaCompression()
        {
            CompressMe obj = new CompressMe() { num1 = 1, num2 = 2, num3 = 3, num4 = 4, str = "hej grabben :) BLBÄBLBÄBLBÄBLBÄBLBÄB" };
            CompressMe key = new CompressMe() { num1 = 112, num2 = 3, num3 = 4, num4 = 5, str = "hej grabben" };

            MemoryStream normal = new MemoryStream();
            WritableSystem.Write(normal, obj);
            MemoryStream derp = new MemoryStream();
            WritableSystem.WriteWithDeltaCompress(derp, obj, key);
            Console.WriteLine("Delta compressed size: " + derp.Position + " Normal merssage size: " + normal.Position);
            derp.Seek(0, SeekOrigin.Begin);
            CompressMe newObj = WritableSystem.ReadWithDeltaCompress<CompressMe>(derp, key);

            Console.WriteLine(newObj.num1);
            Console.WriteLine(newObj.num2);
            Console.WriteLine(newObj.num3);
            Console.WriteLine(newObj.num4);
            Console.WriteLine(newObj.str);
            Thread.Sleep(1000000000);
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
    public struct CompressMe : IDeltaCompressed
    {
        public int num1;
        public int num2;
        public int num3;
        public int num4;
        public string str;
        public object DecompressKey()
        {
            return null;
        }
    }

    [Writable]
    public struct HelloWritable
    {
        public string NumArrayDesc;
        public int[] NumArray;
        public new string ToString()
        {
            string temp = "Name: " + NumArrayDesc + " Numbs: ";
            foreach (int i in NumArray)
                temp += i + ",";
            return temp;
        }
    }
}
