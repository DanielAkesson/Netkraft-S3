using Netkraft.Messaging;
using NetKraft;
using System;
using System.Collections.Generic;
using System.IO;

namespace Netkraft
{
    class UnreliableChannel : Channel
    {
        private MemoryStream _queueMessageStream = new MemoryStream();
        private MemoryStream _receiveStream = new MemoryStream();
        private int _messagesInReceiveStream = 0;
        private Queue<object> _messageQueue = new Queue<object>();

        public UnreliableChannel(NetkraftClient masterClient, ClientConnection connection)
        {
            _masterClient = masterClient;
            _connection = connection;
        }
        public override void AddToQueue(object message)
        {
            _messageQueue.Enqueue(message);
        }
        public override void SendImmediately(object message)
        {
            AddToQueue(message);
            SendQueue();
        }
        public override void SendQueue()
        {
            if (_messageQueue.Count == 0) return;
            _queueMessageStream.Seek(0, SeekOrigin.Begin);
            //Writing header
            ByteConverter.WriteByte(_queueMessageStream, ChannelId.Unreliable);//Channel type
            ByteConverter.WriteUInt16(_queueMessageStream, (ushort)_messageQueue.Count);//Amount of messages encoded
            //Writing body
            while(_messageQueue.Count > 0)
            {
                Message.WriteMessage(_queueMessageStream, _messageQueue.Dequeue());
                //TODO: stop adding if stream is too big for udp packet, and instead send multiple diffrent messages.
            }
            //Sending message
            _masterClient.SendStream(_queueMessageStream, (int)_queueMessageStream.Position, _connection);
        }
        //Internal methods Called on Recive Thread!
        public override void Receive(byte[] buffer, int size)
        {
            lock (_receiveStream)
            {
                //Channel acting!
                _messagesInReceiveStream += BitConverter.ToUInt16(buffer, 1);
                _receiveStream.Write(buffer, 3, size - 3); //3 because of header (byte + ushort).
            }
        }
        //Called on main thread.
        public override void ReceiveTick()
        {
            //TODO: make a permenant solution
            if (_messagesInReceiveStream == 0) return;
            lock (_receiveStream)
            {
                //Read
                _receiveStream.Seek(0, SeekOrigin.Begin);
                for (int i = 0; i < _messagesInReceiveStream; i++)
                    ((IUnreliableMessage)Message.ReadMessage(_receiveStream, _connection)).OnReceive(_connection);

                //Reset
                _receiveStream.Seek(0, SeekOrigin.Begin);
                _messagesInReceiveStream = 0;
            }
        }
        public override void ReceiveTickRestrictive()
        {
            //TODO: make a permenant solution
            if (_messagesInReceiveStream == 0) return;
            lock (_receiveStream)
            {
                //Read
                _receiveStream.Seek(0, SeekOrigin.Begin);
                for (int i = 0; i < _messagesInReceiveStream; i++)
                {
                    IUnreliableMessage message = (IUnreliableMessage)Message.ReadMessage(_receiveStream, _connection);
                    if (message is RequestJoin)
                        message.OnReceive(_connection);
                }  
                //Reset
                _receiveStream.Seek(0, SeekOrigin.Begin);
                _messagesInReceiveStream = 0;
            }
        }
    }
}
