using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Timer = System.Timers.Timer;

namespace Netkraft.ChannelSocket
{
    class UnreliableAcknowledgedChannel2 : Channel2
    {
        //Message queues
        private Queue<ReceivedMessage> receiveQueue = new Queue<ReceivedMessage>();
        //Connections
        private Dictionary<IPEndPoint, Connection> connections = new Dictionary<IPEndPoint, Connection>();

        //header and socket stuff
        private Socket sock;
        private readonly uint channelMask = 15; //00001111
        private readonly uint additionalMask = 240; //11110000
        private int updateInterval;
        //Test variables
        //TODO: REMOVE THE ARTIFICAIL FAILIURE RATE!
        public double sendSuccessRate = 1f;
        private Random r = new Random();
        public UnreliableAcknowledgedChannel2(Socket socket, int updateIntervalMS, float successRate)
        {
            sendSuccessRate = successRate;
            updateInterval = updateIntervalMS;
            sock = socket;
        }
        public override void Send(byte[] buffer, IPEndPoint to, Action onAcknowledge)
        {
            AddEndpoint(to);//Will return false if endpoint already exists
            byte id = (byte)(connections[to].currentID % 256);
            //add the new payload to the alive message array!
            byte[] payload = AddHeader(buffer, id, 0);
            connections[to].acknowledger.OnSendMessage(id);//Set up mask
            connections[to].aliveMessages[id].payload = payload;
            connections[to].aliveMessages[id].acknowledgeCallback = onAcknowledge;
            connections[to].currentID = (connections[to].currentID + 1) % 256; // increment id
        }
        public override int Receive(out byte[] buffer, out IPEndPoint sender)
        {
            //Block until message reaches queue
            while (receiveQueue.Count <= 0)
                Thread.Sleep(updateInterval);

            ReceivedMessage m = receiveQueue.Dequeue();
            buffer = m.buffer;
            sender = m.sender;
            return m.buffer.Length;
        }
        public override void Deliver(byte[] buffer, int size, IPEndPoint from)
        {
            AddEndpoint(from);//Will return false if endpoint already exists
            byte additional = (byte)((buffer[0] & additionalMask) >> 4);
            byte id = buffer[1];

            //Switch based oin additinal header data
            switch (additional)
            {
                case 0: //standard message
                    //We recevied a message!
                    //lets check that it hasn't been received before!
                    if (!connections[from].acknowledger.MessageHasBeenReceived(id))
                    {
                        //WOW, new stuff! let's push it to our receive queue
                        connections[from].acknowledger.OnReceiveMessage(id);
                        byte[] message = new byte[size];
                        Array.Copy(buffer, message, message.Length);
                        receiveQueue.Enqueue(new ReceivedMessage { buffer = message, sender = from });
                    }
                    //Acknowledge the message.
                    byte[] receiveMask = BitConverter.GetBytes(connections[from].acknowledger.GetReceiveMaskForId(id));
                    byte[] payload = AddHeader(receiveMask, id, 1);

                    //TODO: REMOVE THE ARTIFICIAL FAILIURE RATE!
                    if ((r.NextDouble() < sendSuccessRate))
                        sock.SendTo(payload, from); //Push to socket
                    break;

                case 1://Acknowledgement message
                    //We recevied an acknowledgement of a previous message, cool!
                    ushort mask = BitConverter.ToUInt16(buffer, 2);//2 becuase we ignore the header
                    connections[from].acknowledger.OnReceiveAcknowledgement(mask, id);
                    break;
            }
        }
        public override bool RemoveEndpoint(IPEndPoint toBeRemoved)
        {
            if (!connections.ContainsKey(toBeRemoved))
                return false;
            return connections.Remove(toBeRemoved);
        }

        //private stuff
        private bool AddEndpoint(IPEndPoint endPoint)
        {
            if (connections.ContainsKey(endPoint))
                return false;

            IPEndPoint ip = endPoint;
            Acknowledger256 ack = new Acknowledger256((x) => { AcknowledgementCallback(ip, (byte)x); });
            SendMessage[] messageArray = new SendMessage[256];
            for (int i = 0; i < 256; i++)
                messageArray[i] = new SendMessage { };

            Connection connection = new Connection
            {
                currentID = 0,
                acknowledger = ack,
                aliveMessages = messageArray,
            };
            connections.Add(endPoint, connection);
            return true;
        }
        private void AcknowledgementCallback(IPEndPoint endpoint, byte id)
        {
            connections[endpoint].aliveMessages[id].acknowledgeCallback();
        }
        private byte[] AddHeader(byte[] payload, byte id, byte additional)
        {
            //create a buffer 2 larger then user payload to fit header.
            byte[] buffer = new byte[payload.Length + 2];
            //Adding header to message with Channel in the first two bit, if this message is an ack message in the third bit and ID in the other five.
            buffer[0] = (byte)((additional << 4) | ((byte)ChannelId2.UnreliableAcknowledged & channelMask));
            buffer[1] = id;
            payload.CopyTo(buffer, 2); //Copy the user payload into the message buffer
            return buffer;
        }
        //Help structures
        class Connection
        {
            public int currentID;
            public Acknowledger256 acknowledger;
            public SendMessage[] aliveMessages;
        }
        struct SendMessage
        {
            public Action acknowledgeCallback;
            public byte[] payload;
        }
        struct ReceivedMessage
        {
            public byte[] buffer;
            public IPEndPoint sender;
        }
    }