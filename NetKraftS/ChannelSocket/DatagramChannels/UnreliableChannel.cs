using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Netkraft.ChannelSocket
{
    class UnreliableChannel : Channel
    {
        //header and socket stuff
        private ChannelSocket sock;
        private readonly uint channelMask = 15; //00001111
        private readonly uint additionalMask = 240; //11110000

        public UnreliableChannel(ChannelSocket socket)
        {
            sock = socket;
        }

        public override void Send(byte[] buffer, int offset, int size, SocketFlags socketFlags, IPEndPoint RemoteEP, Action onAcknowledge)
        {
            //add the new payload to the alive message array!
            WriteHeader(ref buffer, 0);
#if DEBUG
            if (r.NextDouble() < successRate)
                sock.socket.SendTo(buffer, offset, size, socketFlags, RemoteEP);
#else
             sock.socket.SendTo(buffer, offset, size, socketFlags, RemoteEP);
#endif
        }
        public override void Deliver(ref byte[] buffer, int size, IPEndPoint from)
        {
            byte additional = (byte)((buffer[0] & additionalMask) >> 4);
            //Switch based on additional header data
            switch (additional)
            {
                case 0: //standard message
                    //We received a message!
                    sock.Deliver(from, ChannelId.Unreliable, size);
                    break;
            }
        }
        public override bool RemoveEndpoint(IPEndPoint toBeRemoved)
        {
            return true;
        }

        //private stuff
        private void WriteHeader(ref byte[] payload, byte additional)
        {
            //Adding header to message with Channel in the first two bit, if this message is an ack message in the third bit and ID in the other five.
            payload[0] = (byte)((additional << 4) | (byte)((byte)ChannelId.Unreliable & channelMask));
        }
    }
}
