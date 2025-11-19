using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Netkraft.ChannelSocket
{
    public class ChannelSocket
    {
        internal Socket socket;
       
        //Channel deliver data
        private IPEndPoint sender;
        private ChannelId channel;
        private int size;

        //private
        private byte[] localBuffer = new byte[65536]; //UDP messages can't exceed 65507 bytes so this should always be sufficient
        private Channel[] channels;
        private bool includeHeader;
        private readonly uint channelMask = 15;   //00001111
        private BlockingCollection<deliverdPackage> deliveredPackages = new BlockingCollection<deliverdPackage>();
        private bool ReceiveLoopThreadRunning = true;
#if DEBUG
        public float SuccessRate
        {
            set
            {
                for (int i = 0; i < channels.Length; i++)
                {
                    channels[i].successRate = value;
                }
            }
        }
#endif
        //Constructor
        /// <summary>
        /// This is a callback method for when a message is read from a <see cref="NetkraftClient"/>.
        /// The object this method is called on will have the correct context and data.
        /// <para>This object is reused every time a message of this class is received, meaning it's a volatile pointer and can not be stored.
        /// If you want this message object stored <see cref="CopyMessage"/> that will allow deep copies of messages to be made. </para>
        /// </summary>
        /// <param name="listenPort">The port to listen to</param>
        /// <param name="tickRateInMS">The resend rate for the reliable channel</param>
        /// <param name="userIncludedHeaderSpace">If true, requires user to add two bytes of header space in messages send trough the socket. If false the payload array will be reinitialized for every send!</param>
        public ChannelSocket(int listenPort, int tickRateInMS, bool userIncludedHeaderSpace)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//Socket that supports IPV4
            socket.Bind(new IPEndPoint(IPAddress.Any, listenPort));
            channels = new Channel[] { new UnreliableChannel(this), new UnreliableAcknowledgedChannel(this), new ReliableChannel(this, tickRateInMS) };
            includeHeader = userIncludedHeaderSpace;
            new Thread(ReceiveLoop).Start();
        }

        //Public functions
        public void Send(byte[] buffer, ChannelId channel, int offset, int size, SocketFlags socketFlags, IPEndPoint RemoteEP, Action onAcknowledge = null)
        {
            addHeaderSpace(ref buffer, ref offset, ref size);
            channels[(int)channel].Send(buffer, offset, size, socketFlags, RemoteEP, onAcknowledge);
        }
        public void Send(byte[] buffer, ChannelId channel, int size, SocketFlags socketFlags, IPEndPoint RemoteEP, Action onAcknowledge = null)
        {
            Send(buffer, channel, 0, size, socketFlags, RemoteEP, onAcknowledge);
        }
        public void Send(byte[] buffer, ChannelId channel, SocketFlags socketFlags, IPEndPoint RemoteEP, Action onAcknowledge = null)
        {
            Send(buffer, channel, 0, buffer.Length, socketFlags, RemoteEP, onAcknowledge);
        }
        public void Send(byte[] buffer, ChannelId channel, IPEndPoint RemoteEP, Action onAcknowledge = null)
        {
            Send(buffer, channel, 0, buffer.Length, SocketFlags.None, RemoteEP, onAcknowledge);
        }
        private void addHeaderSpace(ref byte[] payload, ref int offset, ref int size)
        {
            if (includeHeader)
                return;

            //Resizing array to add two empty bytes in the beginning for header info.
            byte[] copy = new byte[size + 2];
            Array.ConstrainedCopy(payload, offset, copy, 2, size);
            size += 2;
            offset = 0;
            payload = copy;
        }

        //TODO: Add more ReceiveFrom alternatives to match a normal UDP socket!
        public int Receive(ref byte[] buffer, out IPEndPoint sender, out ChannelId channel)
        {
            deliverdPackage pack = deliveredPackages.Take();
            buffer = pack.buffer;
            sender = pack.sender;
            channel = pack.channel;
            return pack.size;
        }
        internal void Deliver(IPEndPoint sender, ChannelId channel, int size)
        {
            deliverdPackage pack = new deliverdPackage();
            pack.size = size - 2;
            pack.buffer = new byte[pack.size];
            Array.ConstrainedCopy(localBuffer, 2, pack.buffer, 0, pack.size);
            pack.sender = sender;
            pack.channel = channel;
            deliveredPackages.Add(pack);
        }
        private void ReceiveLoop()
        {
            EndPoint _sender = new IPEndPoint(IPAddress.Any, 0);
            while (ReceiveLoopThreadRunning)
            {
                try
                {
                    int size = socket.ReceiveFrom(localBuffer, ref _sender);
                    byte channel = (byte)(localBuffer[0] & channelMask);
                    channels[channel].Deliver(ref localBuffer, size, (IPEndPoint)_sender);
                }
                catch (SocketException socketException)
                {
                    SocketError code = socketException.SocketErrorCode;
                    switch(code)
                    {
                        case SocketError.Interrupted:
                        case SocketError.OperationAborted:
                            //Socket closed
                            return;
                    }
                }
            }
        }

        public void Close()
        {
            foreach(Channel c in channels)
                c.Dispose();

            ReceiveLoopThreadRunning = false;
            socket.Close();
        }

        private struct deliverdPackage
        {
            public byte[] buffer;
            public int size;
            public ChannelId channel;
            public IPEndPoint sender;

        }
    }
}
