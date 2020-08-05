using Netkraft.Messaging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Netkraft
{
    //Supports both Reliable messages Wand reliable Ackwnoalged messages!
    class ReliableChannel : Channel
    {
        private MemoryStream _queueMessageStream = new MemoryStream();
        private MemoryStream _receiveStream = new MemoryStream();
        private int _messagesInReceiveStream = 0;
        private List<object>[] _messageQueues = new List<object>[64];
        private List<ReliableAcknowledgmentMessage> _reliableMessagesToSend = new List<ReliableAcknowledgmentMessage>();
        private bool _messageHasBeenAdded = false;
        private int _currentId = 0;
        private Acknowledger _acker;

        public ReliableChannel(NetkraftClient masterClient, ClientConnection connection)
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
            for (int i = 0; i < 64; i++)
            {
                _messageQueues[i] = new List<object>();
            }
        }

        public override void AddToQueue(object message)
        {
            _messageQueues[_currentId % 64].Add(message);

            _messageHasBeenAdded = true;
        }
        public override void SendImmediately(object message)
        {
            AddToQueue(message);
            SendQueue();
        }
        public override void SendQueue()
        {
            //Resend all old message that have not been acknowledged
            for (int i = 0; i < 64; i++)
                SendQueueSpecific(i);
            //Send current message
            SendQueueSpecific(_currentId % 64);
            //Reseting
            if (_messageHasBeenAdded)
            {
                _acker.OnSendMessage(_currentId % 64);
                _currentId++;
                _messageQueues[_currentId % 64].Clear();
            }
            _messageHasBeenAdded = false;
        }
        private void SendQueueSpecific(int id)
        {
            if (_messageQueues[id].Count == 0) return;
            //Console.WriteLine("Resending: " + id % 64);
            _queueMessageStream.Seek(0, SeekOrigin.Begin);
            //Writing header
            ByteConverter.WriteByte(_queueMessageStream, ChannelId.Reliable);//Channel type
            ByteConverter.WriteByte(_queueMessageStream, (byte)(id % 64));//Acknowledge ID
            ByteConverter.WriteUInt16(_queueMessageStream, (ushort)_messageQueues[id % 64].Count);//Amount of messages encoded
            //Writing body Write all messages
            foreach (object o in _messageQueues[id])
            {
                Message.WriteMessage(_queueMessageStream, o);
            }
            //Sending message
            _masterClient.SendStream(_queueMessageStream, (int)_queueMessageStream.Position, _connection);
        }
        //Internal methods Called on Recive Thread!
        public override void Receive(byte[] buffer, int size)
        {
            lock (_receiveStream)
            {
                byte id = (byte)BitConverter.ToChar(buffer, 1);
                if (_acker.MessageHasBeenReceived(id)) return;//Stop the same message from being recived multiple times.
                _acker.OnReceiveMessage(id);
                uint mask = _acker.GetReceiveMaskForId(id);//Acknowledge ID
                _messagesInReceiveStream += BitConverter.ToUInt16(buffer, 2);//Amount of messages encoded
                _receiveStream.Write(buffer, 4, size - 4); //4 because of header (byte + byte + ushort)
                lock (_reliableMessagesToSend)
                    _reliableMessagesToSend.Add(new ReliableAcknowledgmentMessage { Mask = mask, Id = id });
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
                {
                    object mess = Message.ReadMessage(_receiveStream, _connection);
                    ((IReliableMessage)mess).OnReceive(_connection);
                }

                //Send reliable ack
                lock (_reliableMessagesToSend)
                    foreach (ReliableAcknowledgmentMessage RAM in _reliableMessagesToSend)
                        _connection.AddToQueue(RAM);
                //Reset
                _reliableMessagesToSend.Clear();
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
                    object mess = Message.ReadMessage(_receiveStream, _connection);
                    if (mess is RequestJoin)
                        ((IReliableMessage)mess).OnReceive(_connection);
                }

                //Send reliable ack
                lock (_reliableMessagesToSend)
                    foreach (ReliableAcknowledgmentMessage RAM in _reliableMessagesToSend)
                        _connection.AddToQueue(RAM);

                //Reset
                _reliableMessagesToSend.Clear();
                _receiveStream.Seek(0, SeekOrigin.Begin);
                _messagesInReceiveStream = 0;
            }
        }
        private struct ReliableAcknowledgmentMessage : IUnreliableMessage
        {
            public uint Mask;
            public byte Id;

            public void OnReceive(ClientConnection Context)
            {
                ReliableChannel con = (ReliableChannel)Context.GetMessageChannel(ChannelId.Reliable);
                con._acker.OnReceiveAcknowledgement(Mask, Id);
            }
        }
    }
}
