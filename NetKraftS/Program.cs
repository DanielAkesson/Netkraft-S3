using System;
using System.IO;
using System.Diagnostics;
using NetkraftMessage;
using static NetkraftMessage.Message;
using System.Threading;
using System.Collections.Generic;

class Program
{
    private static MemoryStream s = new MemoryStream();

    static void Main(string[] args)
    {
        TestSending();
        Console.Read();
    }
    static void TestSerilization()
    {
        s.Seek(0, SeekOrigin.Begin);
        int[][] data = new int[2][];
        data[0] = new int[] { 1, 2 };
        data[1] = new int[] { 3, 4 };
        Player q = new Player {transform = new Vector3(1,2,3), rotation = new Quaternion(1,2,3,4), Name = "Hej jag heter daniel",  numbers = data };
        WritableSystem.Write(s, q);
        //Read
        s.Seek(0, SeekOrigin.Begin);
        Player n = (Player)WritableSystem.Read(s, typeof(Player));
        Console.WriteLine(n.Name);
    }
    static void TestSending()
    {
        NetkraftClient Client1 = new NetkraftClient(2000);
        Client1.FakeLossProcent = 30;
        Client1.AddEndPoint("127.0.0.1", 2001);
        NetkraftClient Client2 = new NetkraftClient(2001);
        Client2.AddEndPoint("127.0.0.1", 2000);

        while(true)
        {
            Thread.Sleep(100); 
            Hello mes = new Hello();
            mes.Setup();
            Client1.AddToQueue(mes);
            Client1.SendQueue();
            Client1.ReceiveTick();
            Client2.SendQueue();
            Client2.ReceiveTick();
        }
    }

    public struct Hello : IReliableMessage
    {
        public Vector3 transform;
        public Quaternion rotation;
        public string Name;
        public int[][] numbers;

        public void Setup()
        {
            numbers = new int[2][];
            numbers[0] = new int[] { 1, 2 };
            numbers[1] = new int[] { 3, 4 };
            transform = new Vector3(1, 2, 3);
            rotation = new Quaternion(1, 2, 3, 4);
            Name = "Hej jag heter daniel";
        }
        public string MyToString()
        {
            string value = "Transform: " + transform.x + ", " + transform.y + ", " + transform.z;
            value += " Rotation: " + rotation.x + ", " + rotation.y + ", " + rotation.z + ", " + rotation.w;
            value += " Name: " + Name;
            value += " 2DArray: " + numbers[0][0] + ", " + numbers[0][1] + ", " + numbers[1][0] + ", " + numbers[1][1];
            return value;
        }

        public void OnAcknowledgment(ClientConnection Context)
        {
        }
        public void OnReceive(ClientConnection Context)
        {
            //Console.WriteLine("Receiving Hello message: " + this.MyToString());
        }
        public void OnSend(ClientConnection Context)
        {
            //Console.WriteLine("Sending Hello message: " + this.MyToString());
        }
    }

    [Writable]
    public struct Player
    {
        public Vector3 transform;
        public Quaternion rotation;
        public string Name;
        public int[][] numbers;
        public override string ToString()
        {
            return "Name: " + Name + ", Trasform: " + transform + ", Rotation: " + rotation
                + " [0,0]: " + numbers[0][0] + " [0,1]: " + numbers[0][1] + " [1,0]: " + numbers[1][0] + " [1,1]: " + numbers[1][1];
        }
    }
    [Writable]
    public struct Quaternion
    {
        public Quaternion(float x, float y, float z,float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public float x, y, z, w;
        public override string ToString()
        {
            return "x:" + x + " y:" + y + " z:" + z + " w:" + w;
        }
    }
    [Writable]
    public struct Vector3
    {
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public float x,y,z;
        public override string ToString()
        {
            return "x:" + x + " y:" + y + " z:" + z;
        }
    }
}
