using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Timer = System.Timers.Timer;

namespace Netkraft.ChannelSocket
{
    class UnreliableAcknowledgedChannel : Channel
    {
        //Connections
        private Dictionary<IPEndPoint, Connection> connections = new Dictionary<IPEndPoint, Connection>();

        //header and socket stuff
        private ChannelSocket sock;
        private readonly uint channelMask = 15; //00001111
        private readonly uint additionalMask = 240; //11110000
        public UnreliableAcknowledgedChannel(ChannelSocket socket)
        {
            sock = socket;
        }
        public override void Send(byte[] buffer, int offset, int size, SocketFlags socketFlags, IPEndPoint RemoteEP, Action onAcknowledge)
        {
            AddEndpoint(RemoteEP);//Will return false if endpoint already exists
            byte id = (byte)(connections[RemoteEP].currentID % 256);
            //add the new payload to the alive message array!
            WriteHeader(ref buffer, id, 0);
            connections[RemoteEP].acknowledger.OnSendMessage(id);//Set up mask
            connections[RemoteEP].callbacks[id] = onAcknowledge;
            connections[RemoteEP].currentID = (connections[RemoteEP].currentID + 1) % 256; // increment id
#if DEBUG
            if (r.NextDouble() < successRate)
                sock.socket.SendTo(buffer, RemoteEP);
#else
            sock.socket.SendTo(buffer, RemoteEP);
#endif
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
                    //lets check if it have been received before!
                    if (!connections[from].acknowledger.MessageHasBeenReceived(id))
                    {
                        //WOW, new stuff! let's deliver to our socket!
                        connections[from].acknowledger.OnReceiveMessage(id);
                        sock.Deliver(from, ChannelId.UnreliableAcknowledged, size);
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

            IPEndPoint ip = endPoint;
            Acknowledger256 ack = new Acknowledger256((x) => { AcknowledgementCallback(ip, (byte)x); });
            Action[] messageArray = new Action[256];
            Connection connection = new Connection
            {
                currentID = 0,
                callbacks = messageArray,
                acknowledger = ack
            };
            connections.Add(endPoint, connection);
            return true;
        }
        private void AcknowledgementCallback(IPEndPoint endpoint, byte id)
        {
            connections[endpoint].callbacks[id]();
        }
        private void WriteHeader(ref byte[] payload, byte id, byte additional)
        {
            //Adding header to message with Channel in the first two bit, if this message is an ack message in the third bit and ID in the other five.
            payload[0] = (byte)((additional << 4) | (byte)((byte)ChannelId2.UnreliableAcknowledged & channelMask));
            payload[1] = id;
        }
        //Help structures
        class Connection
        {
            public int currentID;
            public Acknowledger256 acknowledger;
            public Action[] callbacks;
        }
    }
}