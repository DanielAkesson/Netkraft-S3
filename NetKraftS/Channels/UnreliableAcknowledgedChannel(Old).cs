using Netkraft.Messaging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Netkraft
{
    [Obsolete]
    class UnreliableAcknowledgedChannelOld : ChannelOld
    {
        private MemoryStream _queueMessageStream = new MemoryStream();
        private MemoryStream _receiveStream = new MemoryStream();
        private int _messagesInReceiveStream = 0;
        private List<object>[] _messageQueues = new List<object>[64];
        private List<UnreliableAcknowledgmentMessage> _AckMessagesToSend = new List<UnreliableAcknowledgmentMessage>();
        private int _currentId = 0;
        private Acknowledger _acker;

        public UnreliableAcknowledgedChannelOld(NetkraftClient masterClient, ClientConnection connection)
        {
            _masterClient = masterClient;
            _connection = connection;
            _acker = new Acknowledger((x) =>
            {
                //Console.WriteLine("Acknowledge: " + x);
                foreach (object o in _messageQueues[x % 64])
                {
                    if (o is IAcknowledged)
                        ((IAcknowledged)o).OnAcknowledgment(connection);
                }
                _messageQueues[x % 64].Clear();
            });
            for (int i=0;i<64;i++)
                _messageQueues[i] = new List<object>();
        }
       
        public override void AddToQueue(object message)
        {
            _messageQueues[_currentId % 64].Add(message);
        }
        public override void SendImmediately(object message)
        {
            AddToQueue(message);
            SendQueue();
        }
        public override void SendQueue()
        {
            if (_messageQueues[_currentId % 64].Count == 0)
                return;
            _queueMessageStream.Seek(0, SeekOrigin.Begin);
            //Writing header
            ByteConverter.WriteByte(_queueMessageStream, (byte)1);//Channel type
            ByteConverter.WriteByte(_queueMessageStream, (byte)(_currentId % 64));//Acknowledge ID
            ByteConverter.WriteUInt16(_queueMessageStream, (ushort)_messageQueues[_currentId % 64].Count);//Amount of messages encoded
            //Writing body
            foreach (object o in _messageQueues[_currentId % 64])
            {
                Message.WriteMessage(_queueMessageStream, o);
            }
            _acker.OnSendMessage(_currentId % 64);
            _currentId++;
            _messageQueues[_currentId % 64].Clear();
            //Sending message
            _masterClient.SendStream(_queueMessageStream, (int)_queueMessageStream.Position, _connection);
        }
        //Internal methods Called on Recive Thread!
        public override void Receive(byte[] buffer, int size)
        {
            lock (_receiveStream)
            {
                byte id = (byte)BitConverter.ToChar(buffer, 1);
                _acker.OnReceiveMessage(id);
                uint mask = _acker.GetReceiveMaskForId(id);//Acknowledge ID
                _messagesInReceiveStream += BitConverter.ToUInt16(buffer, 2);//Amount of messages encoded
                _receiveStream.Write(buffer, 4, size - 4); //4 because of header (byte + byte + ushort)
                lock(_AckMessagesToSend)
                    _AckMessagesToSend.Add(new UnreliableAcknowledgmentMessage{ Mask = mask, Id = id });
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
                //Send reliable ack
                lock (_AckMessagesToSend)
                    foreach (UnreliableAcknowledgmentMessage RAM in _AckMessagesToSend)
                        _connection.AddToQueue(RAM);
                //Reset
                _AckMessagesToSend.Clear();
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
                    
                //Send reliable ack
                lock (_AckMessagesToSend)
                    foreach (UnreliableAcknowledgmentMessage RAM in _AckMessagesToSend)
                        _connection.AddToQueue(RAM);
                //Reset
                _AckMessagesToSend.Clear();
                _receiveStream.Seek(0, SeekOrigin.Begin);
                _messagesInReceiveStream = 0;
            }
        }

        private struct UnreliableAcknowledgmentMessage : IUnreliableMessage
        {
            public uint Mask;
            public byte Id;

            public void OnReceive(ClientConnection Context)
            {
                UnreliableAcknowledgedChannelOld con = (UnreliableAcknowledgedChannelOld)Context.GetMessageChannel(ChannelId.UnreliableAcknowledged);
                con._acker.OnReceiveAcknowledgement(Mask, Id);
            }
        }
    } 
}
