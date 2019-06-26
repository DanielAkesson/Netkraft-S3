using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetkraftMessage;

namespace NetkraftMessage
{
    class UnreliableAcknowledgedChannel : Channel
    {
        private MemoryStream _queueMessageStream = new MemoryStream();
        private MemoryStream _receiveStream = new MemoryStream();
        private int _messagesInReceiveStream = 0;
        private List<object>[] _messageQueues = new List<object>[32];
        private List<UnreliableAcknowledgmentMessage> _AckMessagesToSend = new List<UnreliableAcknowledgmentMessage>();

        private byte _currentAcknowledgeID = 0;
        private byte _largestRecivedAcknowledgeID;
        private uint _ackMask;
        public UnreliableAcknowledgedChannel(NetkraftClient masterClient, ClientConnection connection)
        {
            _masterClient = masterClient;
            _connection = connection;
            for(int i=0;i<32;i++)
                _messageQueues[i] = new List<object>();
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
            if (_messageQueues[_currentAcknowledgeID % 32].Count == 0) return;
            _queueMessageStream.Seek(0, SeekOrigin.Begin);
            //Writing header
            ByteConverter.WriteByte(_queueMessageStream, (byte)1);//Channel type
            ByteConverter.WriteByte(_queueMessageStream, _currentAcknowledgeID);//Acknowledge ID
            ByteConverter.WriteUInt16(_queueMessageStream, (ushort)_messageQueues[_currentAcknowledgeID % 32].Count);//Amount of messages encoded
            //Writing body
            foreach (object o in _messageQueues[_currentAcknowledgeID % 32])
            {
                Message.WriteMessage(_queueMessageStream, o);
                ((IUnreliableAcknowledgedMessage)o).OnSend(_connection);
            }
            _currentAcknowledgeID++;
            _messageQueues[_currentAcknowledgeID % 32].Clear();
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
                uint mask = GetMaskForId(id);//Acknowledge ID
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
                    ((IUnreliableAcknowledgedMessage)Message.ReadMessage(_receiveStream, _connection)).OnReceive(_connection);
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

        private struct UnreliableAcknowledgmentMessage : IUnreliableMessage
        {
            public uint Mask;
            public byte Id;

            public void OnReceive(ClientConnection Context)
            {
                UnreliableAcknowledgedChannel con = (UnreliableAcknowledgedChannel)Context.GetChannelOfType(typeof(IUnreliableAcknowledgedMessage));
                string MaskMessage = "Received acknowledgment for: ";
                for (int i = 0; i < 32; i++)
                {
                    int messageID = ((Id - i) + 256) % 256;
                    //If id is confirmed call acknowledgment on all message objects for that UDP packet
                    MaskMessage += ((Mask >> i) & 1) + " ";
                    if (((Mask >> i) & 1) == 1)
                    {
                        con._messageQueues[messageID % 32].ForEach(x => ((IUnreliableAcknowledgedMessage)x).OnAcknowledgment(Context));
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
