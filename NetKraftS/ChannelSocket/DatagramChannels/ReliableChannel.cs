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
        //Connections
        private Dictionary<IPEndPoint, Connection> connections = new Dictionary<IPEndPoint, Connection>();

        //header and socket stuff
        private ChannelSocket sock;
        private Timer sendTimer = new Timer();
        private readonly uint channelMask = 15;     //00001111
        private readonly uint additionalMask = 240; //11110000
        public ReliableChannel(ChannelSocket socket, int sendIntervalMS)
        {
            sock = socket;
            new Thread(() => { SendLoop(sendIntervalMS); }).Start();
        }
        public override void Send(byte[] buffer, int offset, int size, SocketFlags socketFlags, IPEndPoint RemoteEP, Action onAcknowledge)
        {
            AddEndpoint(RemoteEP);//Will return false if endpoint already exists
            SendMessage message = new SendMessage
            {
                payload = new byte[size],
                offset = offset,
                size = size,
                socketFlags = socketFlags,
                acknowledgeCallback = onAcknowledge
            };
            //I do a ConstrainedCopy here since the buffer is a pointer to the users array.
            //The fact that the user might change it while it being resent over and over... is causing enormous headache...
            Array.ConstrainedCopy(buffer, 0, message.payload, 0, size);
            connections[RemoteEP].waitingQueue.Enqueue(message);
            PushWaitingQueue(RemoteEP);
        }
        public override void Deliver(ref byte[] buffer, int size, IPEndPoint from)
        {
            AddEndpoint(from);//Will return false if endpoint already exists
            byte additional = (byte)((buffer[0] & additionalMask) >> 4);
            byte id = buffer[1];

            //Switch based on additional header data
            switch (additional)
            {
                case 0: //standard message
                    //We received a message!
                    //lets check that it hasn't been received before!
                    if (!connections[from].acknowledger.MessageHasBeenReceived(id))
                    {
                        connections[from].acknowledger.OnReceiveMessage(id);
                        sock.Deliver(from, ChannelId.Reliable, size);
                    }
                    //Acknowledge the message.
                    byte[] receiveMask = new byte[6];
                    Array.ConstrainedCopy(BitConverter.GetBytes(connections[from].acknowledger.GetIntReceiveMaskForId(id)), 0, receiveMask, 2, 4);
                    WriteHeader(ref receiveMask, id, 1);
#if DEBUG
                    if (r.NextDouble() < successRate)
                        sock.socket.SendTo(receiveMask, from);
#else
                    sock.socket.SendTo(receiveMask, from);
#endif
                    break;

                case 1://Acknowledgment message
                    //We received an acknowledgment of a previous message, cool!
                    uint mask = BitConverter.ToUInt32(buffer, 2);//2 because we ignore the header
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

            Connection connection = new Connection{
                currentID = 0,
                acknowledger = new Acknowledger256((x) => { AcknowledgementCallback(endPoint, (byte)x); }),
                aliveMessages = new SendMessage[256],
                waitingQueue = new Queue<SendMessage>()
            };
            connections.Add(endPoint, connection);
            return true;
        }
        private void PushWaitingQueue(IPEndPoint to)
        {
            lock (connections[to])
            {
                while (connections[to].waitingQueue.Count > 0)
                {
                    //Can push current? 
                    if (connections[to].acknowledger.RangeisAlive(connections[to].currentID + 1, 2))
                      return;// we cant push the current id because it would cause the receiver mask to forget ids that are currently being resent

                    byte id = (byte)(connections[to].currentID % 256);
                    connections[to].aliveMessages[id] = connections[to].waitingQueue.Dequeue();
                   
                    //add the new payload to the alive message array!
                    WriteHeader(ref connections[to].aliveMessages[id].payload, id, 0);
                    connections[to].acknowledger.OnSendMessage(id);//Set up mask
                    connections[to].currentID = (connections[to].currentID + 1) % 256; // increment id
                }
            }
        }
        private void AcknowledgementCallback(IPEndPoint endpoint, byte id)
        {
            connections[endpoint].aliveMessages[id].acknowledgeCallback?.Invoke();
        }
        private void WriteHeader(ref byte[] payload, byte id, byte additional)
        {
            //Adding header to message with Channel in the first two bit, if this message is an acknowledgment message in the third bit and ID in the other five.
            payload[0] = (byte)((additional << 4) | (byte)((byte)ChannelId2.Reliable & channelMask));
            payload[1] = id;
        }
        private void SendLoop(int sendIntervalMS)
        {
            //TODO: can be optimized to only run when send has occurred without a ack being received
            while(true)
            {
                Thread.Sleep(sendIntervalMS);
                //Resend all messages that have not been acknowledged in the masked messages 
                foreach (IPEndPoint peeps in connections.Keys)
                {
                    //push messages backed up in the waiting queue to the alive messages!
                    PushWaitingQueue(peeps);
                    //Resend all alive messages that have not been acknowledged
                    for (int i = 0; i < 256; i++)
                    {
#if DEBUG
                        if (connections[peeps].acknowledger.MessageisAlive(i) && (r.NextDouble() < successRate))
                            sock.socket.SendTo(connections[peeps].aliveMessages[i].payload, peeps); //Push to socket
#else
                            if (connections[peeps].acknowledger.MessageisAlive(i))
                            sock.socket.SendTo(connections[peeps].aliveMessages[i].payload, peeps); //Push to socket
#endif
                    }
                }
            }
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
            public int offset;
            public int size;
            public SocketFlags socketFlags;
            public Action acknowledgeCallback;
            public byte[] payload;
        }
    }
}
