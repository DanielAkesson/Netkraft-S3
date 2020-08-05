using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Netkraft.ChannelSocket
{
    public class ChannelSocket
    {
        internal Socket socket;
        private Channel2[] channels;
        private readonly uint channelMask = 15;   //00001111
        //Receive vars
        private readonly byte[] _buffer = new byte[65536]; //UDP messages can't exceed 65507 bytes so this should always be sufficient

        //Constuctor
        public ChannelSocket(int listenPort, int tickRateInMS, float successRate)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//Socket that supports IPV4
            socket.Bind(new IPEndPoint(IPAddress.Any, listenPort));
            channels = new Channel2[] { new UnreliableChannel2(), new UnreliableAcknowledgedChannel2(socket, 100, successRate), new ReliableChannel2(socket, 100, successRate) };
            new Thread(ReceiveLoop).Start();
        }

        public void Send(byte[] payload, IPEndPoint receiver, ChannelId2 channel, Action onAcknowledge = null)
        {
            channels[(int)channel].Send(payload, receiver, onAcknowledge);
        }
        public int Receive(out byte[] buffer, out IPEndPoint sender, ChannelId2 channel)
        {
            return channels[(int)channel].Receive(out buffer, out sender);
        }

        private void ReceiveLoop()
        {
            EndPoint _sender = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                int size = socket.ReceiveFrom(_buffer, ref _sender);
                byte channel = (byte)(_buffer[0] & channelMask);
                channels[channel].Deliver(_buffer, size, (IPEndPoint)_sender);
            }
        }
    }
}
