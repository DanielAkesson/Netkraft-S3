using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Netkraft.ChannelSocket
{
    class UnreliableChannel : Channel
    {
        //Message queues
        private Queue<ReceivedMessage> receiveQueue = new Queue<ReceivedMessage>();

        //header and socket stuff
        private Socket sock;
        private SemaphoreSlim receiveLock = new SemaphoreSlim(0);
        private readonly uint channelMask = 15; //00001111
        private readonly uint additionalMask = 240; //11110000
        //Test variables
        //TODO: REMOVE THE ARTIFICAIL FAILIURE RATE!
        public double sendSuccessRate = 1f;
        private Random r = new Random();
        public UnreliableChannel(Socket socket, float successRate)
        {
            sendSuccessRate = successRate;
            sock = socket;
        }
        public override void Send(byte[] buffer, IPEndPoint to, Action onAcknowledge)
        {
            //add the new payload to the alive message array!
            byte[] payload = AddHeader(buffer, 0);
            if (r.NextDouble() > sendSuccessRate)
                sock.SendTo(payload, to);
        }
        public override int Receive(out byte[] buffer, out IPEndPoint sender)
        {
            //Block until message reaches queue
            receiveLock.Wait();
            ReceivedMessage m;
            lock (receiveQueue)
                m = receiveQueue.Dequeue();
            buffer = m.buffer;
            sender = m.sender;
            return m.buffer.Length;
        }
        public override void Deliver(byte[] buffer, int size, IPEndPoint from)
        {
            byte additional = (byte)((buffer[0] & additionalMask) >> 4);

            //Switch based oin additinal header data
            switch (additional)
            {
                case 0: //standard message
                    //We recevied a message!
                    byte[] message = new byte[size];
                    Array.Copy(buffer, message, message.Length);
                    lock (receiveQueue)
                        receiveQueue.Enqueue(new ReceivedMessage { buffer = message, sender = from });
                    receiveLock.Release(); // stop blocking the receive method!
                    break;
            }
        }
        public override bool RemoveEndpoint(IPEndPoint toBeRemoved)
        {
            return true;
        }

        //private stuff
        private byte[] AddHeader(byte[] payload, byte additional)
        {
            //create a buffer 2 larger then user payload to fit header.
            byte[] buffer = new byte[payload.Length + 1];
            //Adding header to message with Channel in the first two bit, if this message is an ack message in the third bit and ID in the other five.
            buffer[0] = (byte)((additional << 4) | (byte)((byte)ChannelId2.Unreliable & channelMask));
            payload.CopyTo(buffer, 1); //Copy the user payload into the message buffer
            return buffer;
        }
        //Help structures
        struct ReceivedMessage
        {
            public byte[] buffer;
            public IPEndPoint sender;
        }
    }
}
