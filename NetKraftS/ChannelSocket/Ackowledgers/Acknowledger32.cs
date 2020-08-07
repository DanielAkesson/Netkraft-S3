using Netkraft;
using Netkraft.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netkraft
{
    public class Acknowledger32
    {
        private uint _aliveMask = 0; //A mask of all messages sent from this client that are waiting acknowledgement
        private uint _receivedMask = 0; // A mask of all messages received by coupled client This is reset 16 id infront of the bigest received so far
        private int _largestReceivedId;
        private Action<int> _onAcknowledged;

        public Acknowledger32(Action<int> onAcknowledged)
        {
            _onAcknowledged = onAcknowledged;
        }
        public void OnSendMessage(int id)
        {
            SetMask(ref _aliveMask, id, true);
        }
        public void OnReceiveMessage(int id)
        {
            int bigId = _largestReceivedId;
            // Gives a value from (-16 <---> 16)
            int delta = Math.Abs(id - bigId) > 16 ? (id - bigId < 0 ? (id - bigId) + 32 : id - (bigId + 32)) : id - bigId;

            if (delta > 0)
            {
                _largestReceivedId = id;
                //Remove all values in receive mask 16 above this new largest id.
                for (int i = 0; i < 16; i++)
                    SetMask(ref _receivedMask, (id + i), false);
            }
            SetMask(ref _receivedMask, id, true);
        }
        public void OnReceiveAcknowledgement(ushort mask, int id)
        {
            for (int i = 0; i < 16; i++)
            {
                int messageID = ((id - i) + 32) % 32;
                if (((mask >> (15 - i)) & 1) == 1 && MaskIsTrue(_aliveMask, messageID))
                {
                    _onAcknowledged?.Invoke(messageID);
                    SetMask(ref _aliveMask, messageID, false);
                }
            }
        }
        public ushort GetReceiveMaskForId(int id)
        {
            return (ushort)LeftBitwiseRotation(_receivedMask, 15 - id);
        }
        public bool MessageHasBeenReceived(int id)
        {
            return MaskIsTrue(_receivedMask, id % 32);
        }
        public bool MessageisAlive(int id)
        {
            return MaskIsTrue(_aliveMask, id % 32);
        }
        //Mask operations
        private uint RightBitwiseRotation(uint mask, int amount)
        {
            return mask << (32 - amount) | mask >> amount;
        }
        private uint LeftBitwiseRotation(uint mask, int amount)
        {
            return mask << amount | mask >> (32 - amount);
        }
        private void SetMask(ref uint mask, int id, bool value)
        {
            int Idc = ((id + 32) % 32);
            uint alterMask = ((uint)1 << Idc);
            mask = value ? mask | alterMask : mask & ~alterMask;
        }
        private bool MaskIsTrue(uint mask, int id)
        {
            int Idc = ((id + 32) % 32);
            return ((mask >> Idc) & 1) == 1;
        }
    }
}
