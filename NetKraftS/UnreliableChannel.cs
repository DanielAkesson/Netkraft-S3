using Netkraft.Messaging;
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
        private List<object> _messageQueue = new List<object>();

        public UnreliableChannel(NetkraftClient masterClient, ClientConnection connection)
        {
            _masterClient = masterClient;
            _connection = connection;
        }
        public override void AddToQueue(object message)
        {
            _messageQueue.Add(message);
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
            ByteConverter.WriteByte(_queueMessageStream, (byte)0);//Channel type
            ByteConverter.WriteUInt16(_queueMessageStream, (ushort)_messageQueue.Count);//Amount of messages encoded
            //Writing body
            foreach (object o in _messageQueue)
            {
                Message.WriteMessage(_queueMessageStream, o);
                ((IUnreliableMessage)o).OnSend(_connection);
            }
            _messageQueue.Clear();
            //Sending message
            _masterClient.SendStream(_queueMessageStream, (int)_queueMessageStream.Position, _connection);
        }
        //Internal methods Called on Recive Thread!
        public override void Recive(byte[] buffer, int size)
        {
            lock (_receiveStream)
            {
                BitConverter.ToChar(buffer, 0);//This would switch to the correct channel method
                //Channel acting!
                _messagesInReceiveStream += BitConverter.ToUInt16(buffer, 1);
                _receiveStream.Write(buffer, 3, size - 3); //3 because of header (byte + ushort).
            }
        }
        //Called on main thread.
        public override void ReceiveTick()
        {
            //TODO: make a permenant solution
            lock (_receiveStream)
            {
                if (_messagesInReceiveStream == 0) return;
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
            lock (_receiveStream)
            {
                if (_messagesInReceiveStream == 0) return;
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
