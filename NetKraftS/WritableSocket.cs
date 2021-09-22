using Netkraft;
using Netkraft.ChannelSocket;
using Netkraft.WritableSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetKraft
{
    class WritableSocket
    {
        
//        internal Socket socket;
//        private Channel[] channels;
//        private readonly uint channelMask = 15;   //00001111
//        MemoryStream stream = new MemoryStream();
//#if DEBUG
//        public float SuccessRate
//        {
//            set
//            {
//                for (int i = 0; i < channels.Length; i++)
//                {
//                    channels[i].successRate = value;
//                }
//            }
//        }
//#endif
//        //Receive vars
//        private readonly byte[] _buffer = new byte[65536]; //UDP messages can't exceed 65507 bytes so this should always be sufficient

//        //Constructor
//        WritableSocket(int listenPort, int tickRateInMS)
//        {
//            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//Socket that supports IPV4
//            socket.Bind(new IPEndPoint(IPAddress.Any, listenPort));
//            channels = new Channel[] { new UnreliableChannel(socket), new UnreliableAcknowledgedChannel(socket), new ReliableChannel(socket, tickRateInMS) };
//            new Thread(ReceiveLoop).Start();
//        }

//        public void Send(byte[] payload, IPEndPoint receiver, ChannelId channel, Action onAcknowledge = null)
//        {
//            channels[(int)channel].Send(payload, receiver, onAcknowledge);
//        }
//        public int Receive(out byte[] buffer, out IPEndPoint sender, ChannelId channel)
//        {
//            return channels[(int)channel].Receive(out buffer, out sender);
//        }
//        public void Send(object payload, IPEndPoint receiver, ChannelId channel, Action onAcknowledge = null)
//        {
//            stream.Seek(0, SeekOrigin.Begin);
//            Writable.Write(stream, payload);
//            stream.Read(_buffer, 0, (int)stream.Position);
//            channels[(int)channel].Send(_buffer, receiver, onAcknowledge);
//        }
//        public object Receive(out IPEndPoint sender, ChannelId channel)
//        {
//            socket.Receive();
//            return channels[(int)channel].Receive(out buffer, out sender);
//        }
//        private void ReceiveLoop()
//        {
//            EndPoint _sender = new IPEndPoint(IPAddress.Any, 0);
//            while (true)
//            {
//                int size = socket.ReceiveFrom(_buffer, ref _sender);
//                byte channel = (byte)(_buffer[0] & channelMask);
//                channels[channel].Deliver(_buffer, size, (IPEndPoint)_sender);
//            }
//        }
    }
}
