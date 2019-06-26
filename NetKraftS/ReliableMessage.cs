using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetkraftMessage;

namespace NetkraftMessage
{
    //TODO: Remove this class
    /*
    class ReliableMessage
    {
        public byte ReliableId;
        private bool _originalMessage = true;
        //Reliable Data
        private static Dictionary<MessageContex, List<ReliableMessage>> _messagesToResend = new Dictionary<MessageContex, List<ReliableMessage>>();
        private static Dictionary<MessageContex, byte> _biggestReliableIDReceived = new Dictionary<MessageContex, byte>();
        private static Dictionary<MessageContex, byte> _lastReliableIDSent = new Dictionary<MessageContex, byte>();
        private static Dictionary<MessageContex, uint> _reliableAckMask = new Dictionary<MessageContex, uint>();
        private static HashSet<int> _queuedAcknowledgments = new HashSet<int>();
        sealed protected override void PostReadMessage()
        {
            if (!_biggestReliableIDReceived.ContainsKey(Context)) _biggestReliableIDReceived.Add(Context, 0);
            if (!_reliableAckMask.ContainsKey(Context)) _reliableAckMask.Add(Context, 0);

            //Call on receive and update our mask if the message is new.
            if (!IsIdAlreadyRecived(ReliableId))
            {
                GetMaskForId(ReliableId);
                Console.WriteLine("Receive reliable message " + ReliableId);
                OnReceive();
            }

            //Send acknowlegement
            ReliableAcknowledgmentMessage Ack = GetMessage<ReliableAcknowledgmentMessage>();
            Ack.Mask = _reliableAckMask[Context];
            Ack.id = _biggestReliableIDReceived[Context];
            if(!_queuedAcknowledgments.Contains(ReliableId))
            {
                Console.WriteLine("Receive reliable message " + ReliableId + " Sending ack: " + Ack.id);
                Context.NetKraftClientSource.SendToQueue(Ack, Context.SenderIpEndpoint);
                _queuedAcknowledgments.Add(ReliableId);
            }
        }
        sealed protected override void PreWriteMessage()
        {
            // Escape if this is a resending
            if (!_originalMessage) return;
            //Inizilized dics
            if (!_lastReliableIDSent.ContainsKey(Context)) _lastReliableIDSent.Add(Context, 0);
            if (!_messagesToResend.ContainsKey(Context))_messagesToResend.Add(Context, new List<ReliableMessage>());
            //Set ReliableId
            ReliableId = _lastReliableIDSent[Context];
            _lastReliableIDSent[Context]++;
        }
        sealed protected override void PostWriteMessage()
        {
            if (!_originalMessage)
            {
                Console.WriteLine("Resending ReliableId: " + ReliableId);
                return;
            }
            Console.WriteLine("Sending ReliableId: " + ReliableId);
            //Add message to be resent but only if they are new
            ReliableMessage temp = (ReliableMessage)CopyMessage();
            temp._originalMessage = false;
            _messagesToResend[Context].Add(temp);
        }
        internal static void QueueSendCallback(MessageContex Context)
        {
            if (!_messagesToResend.ContainsKey(Context)) _messagesToResend.Add(Context, new List<ReliableMessage>());
            //Send all messages for this context that have yet to recive it's acknowledgment
            for(int i=0; i< _messagesToResend[Context].Count;i++)
                Context.NetKraftClientSource.SendToQueue(_messagesToResend[Context][i], Context.SenderIpEndpoint);
            _queuedAcknowledgments.Clear();
        }

        private bool IsIdAlreadyRecived(int id)
        {
            byte bigId = _biggestReliableIDReceived[Context];
            // Gives a value from (-128 <---> 128)
            int delta = Math.Abs(id - bigId) > 128 ? (id - bigId < 0 ? (id - bigId) + 256 : id - (bigId + 256)) : id - bigId; 
            if (delta > 0) return false; //Id is larger than the largest already recived
            delta *= -1; //Since delta is used for bitwise operations, cant be negative.
            return ((_reliableAckMask[Context] >> delta) & 1) == 1;
        }
        private uint GetMaskForId(byte id)
        {
            byte bigId = _biggestReliableIDReceived[Context];
            // Gives a value from (-128 <---> 128)
            int delta = Math.Abs(id - bigId) > 128 ? (id - bigId < 0 ? (id - bigId) + 256 : id - (bigId + 256)) : id - bigId;
            //If delta is greater than 0 we move the mask to the left and add the one at the beginning.
            //If delta is smaller or equal to 0 we move a 1 mask to the left and add it to the existing mask.
            _reliableAckMask[Context] = delta > 0 ? (_reliableAckMask[Context] << delta) | 1 : _reliableAckMask[Context] | (uint)(1 << (delta * -1));
            _biggestReliableIDReceived[Context] = delta > 0 ? id : bigId;
            return _reliableAckMask[Context];
        }
        class ReliableAcknowledgmentMessage : Message
        {
            public uint Mask;
            public byte id;

            public override void OnReceive()
            {
                Console.Write("Received acknowledgment for: ");
                for (int i = 0; i < 32; i++)
                {
                    int messageID = ((id - i) + 256) % 256;
                    //If id is confirmed remove message from being resent.
                    Console.Write(((Mask >> i) & 1) + " ");
                    if (((Mask >> i) & 1) == 1)
                        _messagesToResend[Context].RemoveAll(x => x.ReliableId == messageID);
                }
                Console.WriteLine("For id: " + id);
            }
        }
    } 
    */
}