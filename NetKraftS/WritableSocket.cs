using Netkraft.WritableSystem;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Netkraft
{
    public class WritableSocket
    {
        internal ChannelSocket.ChannelSocket socket;
        private MemoryStream sendStream = new MemoryStream(8);
        private MemoryStream queueStream = new MemoryStream(8);
        private MemoryStream receiveStream = new MemoryStream(8);
        private byte queueSize = 0;
        private byte[] localBuffer = new byte[65536]; //UDP messages can't exceed 65507 bytes so this should always be sufficient
#if DEBUG
        public float SuccessRate
        {
            set
            {
                socket.SuccessRate = value;
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
        public WritableSocket(int listenPort, int tickRateInMS)
        {
            socket = new ChannelSocket.ChannelSocket(listenPort, tickRateInMS, true);//Socket that supports IPV4
            sendStream.Seek(3, SeekOrigin.Begin);
            queueStream.Seek(3, SeekOrigin.Begin);
        }

        //Public functions
        public void AddToSendQueue(object writable)
        {
            Writable.Write(queueStream, writable);
            queueSize++;
        }
        public void SendQueue(ChannelId channel, SocketFlags socketFlags, IPEndPoint RemoteEP, Action onAcknowledge = null)
        {
            int pos = (int)queueStream.Position;
            queueStream.Seek(2, SeekOrigin.Begin);
            Writable.WriteRaw(queueStream, queueSize);
            socket.Send(queueStream.GetBuffer(), channel, 0, pos, socketFlags, RemoteEP, onAcknowledge);
            queueStream.Seek(3, SeekOrigin.Begin);
            queueSize = 0;
        }
        public void SendQueue(ChannelId channel, IPEndPoint RemoteEP, Action onAcknowledge = null)
        {
            SendQueue(channel, SocketFlags.None, RemoteEP, onAcknowledge);
        }
        public void Send(object writable, ChannelId channel, SocketFlags socketFlags, IPEndPoint RemoteEP, Action onAcknowledge = null)
        {
            sendStream.Seek(2, SeekOrigin.Begin);
            Writable.WriteRaw(sendStream, (byte)1);
            Writable.Write(sendStream, writable);
            socket.Send(sendStream.GetBuffer(), channel, 0, (int)sendStream.Position, socketFlags, RemoteEP, onAcknowledge);
            sendStream.Seek(3, SeekOrigin.Begin);
        }
        public void Send(object writable, ChannelId channel, IPEndPoint RemoteEP, Action onAcknowledge = null)
        {
            Send(writable, channel, SocketFlags.None, RemoteEP, onAcknowledge);
        }
        public object[] Receive(out IPEndPoint sender, out ChannelId channel)
        {
            int size = socket.Receive(ref localBuffer, out sender, out channel);
            byte queueSize = localBuffer[0];
            object[] objectArray = new object[queueSize];
            //Retrieve objects from buffer
            receiveStream.Seek(0, SeekOrigin.Begin);
            receiveStream.Write(localBuffer, 0, size);
            receiveStream.Seek(1, SeekOrigin.Begin);
            for (int i = 0; i < queueSize; i++)
            {
                objectArray[i] = Writable.Read(receiveStream);
            }
                
            return objectArray;
        }
    }
}
