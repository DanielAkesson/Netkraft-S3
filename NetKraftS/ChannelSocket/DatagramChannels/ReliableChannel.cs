using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Timer = System.Timers.Timer;
using System.Threading;

namespace Netkraft.ChannelSocket
{
    //Supports both Reliable messages and reliable Acknowledged messages!
    class ReliableChannel : Channel
    {
        //Message queues
        private Queue<ReceivedMessage> receiveQueue = new Queue<ReceivedMessage>();
        //Connections
        private Dictionary<IPEndPoint, Connection> connections = new Dictionary<IPEndPoint, Connection>();

        //header and socket stuff
        private Socket sock;
        private Timer sendTimer = new Timer();
        private SemaphoreSlim receiveLock = new SemaphoreSlim(0);
        private int sendIntervalMS;
        private readonly uint channelMask = 15;     //00001111
        private readonly uint additionalMask = 240; //11110000

        //Test variables
        //TODO: REMOVE THE ARTIFICAIL FAILIURE RATE!
        public double sendSuccessRate = 1f;
        private Random r = new Random();
        public ReliableChannel(Socket socket, int sendIntervalMS, float successRate)
        {
            sendSuccessRate = successRate;
            sock = socket;
            this.sendIntervalMS = sendIntervalMS;
            sendTimer.Interval = sendIntervalMS;
            sendTimer.Elapsed += (x,y) => 
            {
                lock (connections)
                {
                    //Resend all messages that have not been ackowledged in the masked messages 
                    foreach (IPEndPoint peeps in connections.Keys)
                    {
                        //push messages backed up in the waiting queue to the alive messages!
                        PushWaitingQueue(peeps);
                        //Resend all alive messages that have not been acknowledged
                        for (int i = 0; i < 256; i++)
                        {
                            //TODO: REMOVE THE ARTIFICIAL FAILIURE RATE!
                            if (connections[peeps].acknowledger.MessageisAlive(i) && (r.NextDouble() < sendSuccessRate))
                                sock.SendTo(connections[peeps].aliveMessages[i].payload, peeps); //Push to socket
                        }
                    }
                }
            };
            sendTimer.AutoReset = true;
            sendTimer.Enabled = true;
        }
        public override void Send(byte[] buffer, IPEndPoint to, Action onAcknowledge)
        {
            AddEndpoint(to);//Will return false if endpoint already exists
            connections[to].waitingQueue.Enqueue(new SendMessage { payload = buffer, acknowledgeCallback = onAcknowledge });
            PushWaitingQueue(to);
        }
        public override int Receive(out byte[] buffer, out IPEndPoint sender)
        {
            receiveLock.Wait(); //Block until message reaches queue
            //pull a message
            ReceivedMessage m;
            lock (receiveQueue)
                m = receiveQueue.Dequeue();
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
                        lock (receiveQueue)
                            receiveQueue.Enqueue(new ReceivedMessage { buffer = message, sender = from }); 
                        receiveLock.Release();
                    }
                    //Acknowledge the message.
                    byte[] receiveMask = BitConverter.GetBytes(connections[from].acknowledger.GetIntReceiveMaskForId(id));
                    byte[] payload = AddHeader(receiveMask, id, 1);

                    //TODO: REMOVE THE ARTIFICAIL FAILIURE RATE!
                    if ((r.NextDouble() < sendSuccessRate))
                        sock.SendTo(payload, from); //Push to socket
                    break;

                case 1://Acknowledgement message
                    //We recevied an acknowledgement of a previous message, cool!
                    uint mask = BitConverter.ToUInt32(buffer, 2);//2 becuase we ignore the header
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
            for (int i =0;i<256;i++)
                messageArray[i] = new SendMessage { };

            Connection connection = new Connection{
                currentID = 0,
                acknowledger = ack,
                aliveMessages = messageArray,
                waitingQueue = new Queue<SendMessage>()
            };
            connections.Add(endPoint, connection);
            return true;
        }
        private void PushWaitingQueue(IPEndPoint to)
        {
            while (connections[to].waitingQueue.Count > 0)
            {
                //Can push current? 
                if (connections[to].acknowledger.RangeisAlive(connections[to].currentID + 1, 2))
                    return;// we cant push the current id beacuse it would cause the receiver mask to forget ids that are currenly being resent

                SendMessage m = connections[to].waitingQueue.Dequeue();
                byte id = (byte)(connections[to].currentID % 256);
                //add the new payload to the alive message array!
                byte[] buffer = AddHeader(m.payload, id, 0);
                connections[to].acknowledger.OnSendMessage(id);//Set up mask
                connections[to].aliveMessages[id].payload = buffer;
                connections[to].aliveMessages[id].acknowledgeCallback = m.acknowledgeCallback;
                connections[to].currentID = (connections[to].currentID + 1) % 256; // increment id
            }
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
            buffer[0] = (byte)((byte)(additional << 4) | ((byte)ChannelId2.Reliable & (byte)channelMask) );
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
            public Queue<SendMessage> waitingQueue;
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
}
