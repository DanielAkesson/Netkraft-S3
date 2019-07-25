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
        static void Main(string[] args)
        {
            NetkraftClient client1 = new NetkraftClient(2000);
            NetkraftClient client2 = new NetkraftClient(3000);

            client1.AddEndPoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000));
            client2.AddEndPoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000));

            client1.SendImmediately(new Hello {
                HelloMessage = "Hello world!",
                Num1 = 100,
                Num2 = -200,
                HelloWritableValue = new HelloWritable
                {
                    NumArrayDesc = "5 Elements",
                    NumArray = new int[] {1,2,3,4,5}
                }
            });
            while (true)
            {
                client2.ReceiveTick();
                Thread.Sleep(100);
            }
        }
    }

    [Writable]
    public struct Hello : IUnreliableMessage
    {
        public string HelloMessage;
        public int Num1;
        [SkipIndex]
        public int Num2;
        public HelloWritable HelloWritableValue;

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