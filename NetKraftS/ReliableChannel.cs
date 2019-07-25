using Netkraft.Messaging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Netkraft
{
    class ReliableChannel : Channel
    {
        private MemoryStream _queueMessageStream = new MemoryStream();
        private MemoryStream _receiveStream = new MemoryStream();
        private int _messagesInReceiveStream = 0;
        private List<object>[] _messageQueues = new List<object>[32];
        private int[] _messageQueuesIds = new int[32];
        private List<ReliableAcknowledgmentMessage> _reliableMessagesToSend = new List<ReliableAcknowledgmentMessage>();

        private byte _currentAcknowledgeID = 0;
        private byte _largestRecivedAcknowledgeID;
        private uint _ackMask;
        public ReliableChannel(NetkraftClient masterClient, ClientConnection connection)
        {
            _masterClient = masterClient;
            _connection = connection;
            for(int i=0;i<32;i++)
            {
                _messageQueues[i] = new List<object>();
                _messageQueuesIds[i] = -1;
            }
        }
       
        public override void AddToQueue(object message)
        {
            _messageQueues[_currentAcknowledgeID % 32].Add(message);
        }
        public override void SendImmediately(object message)
        {
            AddToQueue(message);
            SendQueue();
        }
        public override void SendQueue()
        {
            //Resend all old message that have not been acknowledged
            for (int i = 0;i<32;i++)
                SendQueueSpecific(i);
            //Send current message
            _messageQueuesIds[_currentAcknowledgeID % 32] = _currentAcknowledgeID;
            SendQueueSpecific(_currentAcknowledgeID % 32);
            //Reseting
            _currentAcknowledgeID++;
            _messageQueues[_currentAcknowledgeID % 32].Clear();
            _messageQueuesIds[_currentAcknowledgeID % 32] = -1;
        }
        private void SendQueueSpecific(int id)
        {
            if (_messageQueues[id].Count == 0 || _messageQueuesIds[id] == -1) return;
            Console.WriteLine("Resending: " + _messageQueuesIds[id]);
            _queueMessageStream.Seek(0, SeekOrigin.Begin);
            //Writing header
            ByteConverter.WriteByte(_queueMessageStream, (byte)2);//Channel type
            ByteConverter.WriteByte(_queueMessageStream, (byte)_messageQueuesIds[id]);//Acknowledge ID
            ByteConverter.WriteUInt16(_queueMessageStream, (ushort)_messageQueues[id].Count);//Amount of messages encoded
            //Writing body Write all messages
            foreach (object o in _messageQueues[id])
            {
                Message.WriteMessage(_queueMessageStream, o);
                ((IReliableMessage)o).OnSend(_connection);
            }
            //Sending message
            _masterClient.SendStream(_queueMessageStream, (int)_queueMessageStream.Position, _connection);
        }
        //Internal methods Called on Recive Thread!
        public override void Recive(byte[] buffer, int size)
        {
            lock (_receiveStream)
            {
                BitConverter.ToChar(buffer, 0);//This would switch to the correct channel method
                byte id = (byte)BitConverter.ToChar(buffer, 1);
                if(IsIdAlreadyRecived(id)) return;//Stop the same message from being recived multiple times.
                uint mask = GetMaskForId(id);//Acknowledge ID
                _messagesInReceiveStream += BitConverter.ToUInt16(buffer, 2);//Amount of messages encoded
                _receiveStream.Write(buffer, 4, size - 4); //4 because of header (byte + byte + ushort)
                lock(_reliableMessagesToSend)
                    _reliableMessagesToSend.Add(new ReliableAcknowledgmentMessage{ Mask = mask, Id = id });
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
                    ((IReliableMessage)Message.ReadMessage(_receiveStream, _connection)).OnReceive(_connection);
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
        private bool IsIdAlreadyRecived(int id)
        {
            byte bigId = _largestRecivedAcknowledgeID;
            // Gives a value from (-128 <---> 128)
            int delta = Math.Abs(id - bigId) > 128 ? (id - bigId < 0 ? (id - bigId) + 256 : id - (bigId + 256)) : id - bigId;
            if (delta > 0) return false; //Id is larger than the largest already recived
            delta *= -1; //Since delta is used for bitwise operations, cant be negative.
            return ((_ackMask >> delta) & 1) == 1;
        }
        private uint GetMaskForId(byte id)  
        {
            byte bigId = _largestRecivedAcknowledgeID;
            // Gives a value from (-128 <---> 128)
            int delta = Math.Abs(id - bigId) > 128 ? (id - bigId < 0 ? (id - bigId) + 256 : id - (bigId + 256)) : id - bigId;
            //If delta is greater than 0 we move the mask to the left and add the one at the beginning
            //If delta is smaller or equal to 0 we move a 1 mask to the left and add it to the existing mask
            _ackMask = delta > 0 ? (_ackMask << delta) | 1 : _ackMask | (uint)(1 << (delta * -1));
            _largestRecivedAcknowledgeID = delta > 0 ? id : bigId;
            return _ackMask;
        }

        private struct ReliableAcknowledgmentMessage : IUnreliableMessage
        {
            public uint Mask;
            public byte Id;

            public void OnReceive(ClientConnection Context)
            {
                ReliableChannel con = (ReliableChannel)Context.GetChannelOfType(typeof(IReliableMessage));
                string MaskMessage = "Received acknowledgment for: ";
                for (int i = 0; i < 32; i++)
                {
                    int messageID = ((Id - i) + 256) % 256;
                    //If id is confirmed call acknowledgment on all message objects for that UDP packet
                    MaskMessage += ((Mask >> i) & 1) + " ";
                    if (((Mask >> i) & 1) == 1)
                    {
                        //con._sentMessageQueues[messageID % 32].ForEach(x => ((IReliableMessage)x).OnAcknowledgment(Context));
                        con._messageQueuesIds[messageID % 32] = -1;
                        con._messageQueues[messageID % 32].Clear();
                    }    
                }
                Console.WriteLine(MaskMessage + "For id: " + Id);
            }

            public void OnSend(ClientConnection Context)
            {
                Console.WriteLine("Sending ack message");
            }
        }
    }
}
