using Netkraft;
using Netkraft.ChannelSocket;
using Netkraft.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace TestApplication
{
    class Program
    {
        

        public static int Sent = 0;
        public static int Acked = 0;
        public static int Recived = 0;
        private static SemaphoreSlim mut = new SemaphoreSlim(0);

       
        static void Main(string[] args)
        {
            //TimeTests();

            mut.Release();
            mut.Release();
            
            mut.Wait();
            Console.WriteLine("first lock" + mut.CurrentCount);
            mut.Wait();
            Console.WriteLine("second lock" + mut.CurrentCount);
            ChannelSocketStuff();
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
                Console.WriteLine("Receive Fail: " + (Sent - Recived));
                Console.ForegroundColor = Sent - Acked == 0 ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine("Acked Fail: " + (Sent - Acked));
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        static void ChannelSocketStuff()
        {
            ChannelSocket client1 = new ChannelSocket(3003, 10, .7f);
            ChannelSocket client2 = new ChannelSocket(3004, 10, .7f);
            IPEndPoint client2Address = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3004);
            Random r = new Random();
            //Check lists
            List<int> sentIds = new List<int>();
            List<int> ackedIds = new List<int>();
            List<int> received = new List<int>();

            //TEST START!
            //Client 1
            SemaphoreSlim mut = new SemaphoreSlim(2);
            mut.Wait();
            new Thread(() =>
            {
                byte[] buffer = new byte[1000];
                //Send one thousand messages with a loss rate of 90%
                for (int i = 0; i < 1000; i++)
                {
                    int id = r.Next(9999999);
                    Console.WriteLine("send: " + i);
                    buffer = BitConverter.GetBytes(id);
                    client1.Send(buffer, client2Address, ChannelId2.Reliable, () => { ackedIds.Add(id); Console.WriteLine("acked: " + id); });
                    sentIds.Add(id);
                }
                mut.Release();
            }).Start();
            //Client 2
            mut.Wait();
            new Thread(() =>
            {
                byte[] buffer = new byte[1000];
                IPEndPoint endpoint;
                //receive all the messages%
                for (int i = 0; i < 1000; i++)
                {
                    Console.WriteLine("receive: " + i);
                    int size = client2.Receive(out buffer, out endpoint, ChannelId2.Reliable);
                    int id = BitConverter.ToInt32(buffer, 0);
                    received.Add(id);
                }
                mut.Release();
            }).Start();

            mut.Wait();
            Console.WriteLine("First Lock released");
            mut.Wait();
            Console.WriteLine("Second Lock released");
            //Tally results
            Thread.Sleep(1000);
            Console.WriteLine("Sent: " + sentIds.Count);
            Console.WriteLine("Received: " + received.Count);
            Console.WriteLine("Acked: " + ackedIds.Count);


            Console.WriteLine($"Acked Fail: " + (sentIds.Count - ackedIds.Count));
            if (ackedIds.Count == sentIds.Count && received.Count == ackedIds.Count)
                Console.WriteLine("No loss everything was grand!");
            

            while (true)
                Thread.Sleep(100);
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
        static void TestAllTypesWritebles()
        {
            Stream s = new MemoryStream();
            TestAllTypes thing = new TestAllTypes();
            WritableSystem.Write(s, thing);
            s.Seek(0, SeekOrigin.Begin);
            TestAllTypes thing2 = WritableSystem.Read<TestAllTypes>(s);
        }
        static void TestDeltaCompression()
        {
            CompressMe mes1  = new CompressMe{ CompressID = 1, CompressID2 = new integerThing { value = "h" }, num1 = 1, num2 = 2, num3 = 3, num4 = 4, str = "hej grabben :(" };
            CompressMe mes2 = new CompressMe { CompressID = 98, CompressID2 = new integerThing { value = "h" }, num1 = 112, num2 = 3, num3 = 4, num4 = 5, str = "hej grabben" };
            CompressMe mes3 = new CompressMe { CompressID = 99, CompressID2 = new integerThing { value = "h" }, num1 = 11222, num2 = 234, num3 = 55, num4 = 234234, str = "" };
            CompressMe key = new CompressMe  { CompressID = 100, CompressID2 = new integerThing { value = "h" }, num1 = 1, num2 = 2, num3 = 3, num4 = 4, str = "hej grabben :(" };

            MemoryStream normal = new MemoryStream();
            WritableSystem.Write(normal, key);
            MemoryStream derp = new MemoryStream();
            //WritableSystem.WriteDeltaCompress(derp, mes3, key);
            Console.WriteLine("Delta compressed size: " + derp.Position + " Normal message size: " + normal.Position);

            //Read and decompress
            key.CompressID = 100;
            derp.Seek(0, SeekOrigin.Begin);
            //CompressMe newObj = WritableSystem.ReadDeltaCompress<CompressMe>(derp, key);
            CompressMe newObj2 = WritableSystem.Read<CompressMe>(derp);
            Console.WriteLine(newObj2.CompressID);
            Console.WriteLine(newObj2.num1);
            Console.WriteLine(newObj2.num2);
            Console.WriteLine(newObj2.num3);
            Console.WriteLine(newObj2.num4);
            Console.WriteLine(newObj2.str);
            Console.ReadLine();
        }
        static void TimeTests()
        {
            Stopwatch stopWatch = new Stopwatch();
            MemoryStream s = new MemoryStream();
            TestAllTypes thing = new TestAllTypes();
            while (true)
            {
                stopWatch.Start();
                WritableSystem.Write(s, thing);
                stopWatch.Stop();
                Console.WriteLine($"Time to write a TestAllTypes: {stopWatch.ElapsedMilliseconds}");
                s.Seek(0, SeekOrigin.Begin);
                stopWatch.Reset();
                stopWatch.Start();
                thing = WritableSystem.Read<TestAllTypes>(s);
                stopWatch.Stop();
                Console.WriteLine($"Time to read a TestAllTypes: {stopWatch.ElapsedMilliseconds}");
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

    public struct arrayWritable : IWritable
    {
        public string NumArrayDesc;
        public int[] NumArray;
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
        public arrayWritable[] arrayOfStructs = new arrayWritable[] {
            new arrayWritable{ NumArray = new int[] { 1,2,3}, NumArrayDesc = "hej 123" },
            new arrayWritable{ NumArray = new int[] { 1,2,3}, NumArrayDesc = "hej 123" },
            new arrayWritable{ NumArray = new int[] { 1,2,3}, NumArrayDesc = "hej 123" },
            new arrayWritable{ NumArray = new int[] { 1,2,3}, NumArrayDesc = "hej 123" },
            new arrayWritable{ NumArray = new int[] { 1,2,3}, NumArrayDesc = "hej 123" }
        };

        public void OnReceive(ClientConnection Context)
        {
            Console.WriteLine($"Succes: {intvar}, {uintvar}, {shortvar}, {ushortvar}, {longvar}, {ulongvar}, {bytevar}, {singelvar}, {doublevar}, {stringvar}");
            Console.WriteLine($"Succes: {arrayOfStructs[0].NumArray[0]}, {arrayOfStructs[0].NumArray[1]}, {arrayOfStructs[0].NumArray[2]}, {arrayOfStructs[0].NumArrayDesc}");
        }
    }

    public struct integerThing : IWritable
    {
        public string value;
    }
    public struct CompressMe : IWritable
    {
        [DeltaCompressedField]
        public int CompressID;
        [DeltaCompressedField]
        public integerThing CompressID2;
        public int num1;
        public int num2;
        public int num3;
        public int num4;
        public string str;
    }
    public struct HelloWritable : IWritable
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
