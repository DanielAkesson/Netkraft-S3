
using System;
using System.Numerics;

namespace Netkraft
{
    public class Acknowledger256
    {
        //A mask of all messages sent from this client that are waiting acknowledgement
        private Uint256 _aliveMask = new Uint256(0);
        // A mask of all messages received by coupled client This is reset 16 id infront of the bigest received so far
        private Uint256 _receivedMask = new Uint256(0);
        private int _largestReceivedId;
        private Action<int> _onAcknowledged;

        public Acknowledger256(Action<int> onAcknowledged)
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
            // Gives a value from (-128 <---> 128)
            int delta = Math.Abs(id - bigId) > 128 ? (id - bigId < 0 ? (id - bigId) + 256 : id - (bigId + 256)) : id - bigId;
            if (delta > 0)
            {
                _largestReceivedId = id;
                //Remove all values in receive mask 16 above this new largest id.
                //TODO: This can be improved by using shifting to remove the bits instead of a for loop!
                for (int i = 0; i < 128; i++)
                    SetMask(ref _receivedMask, (id + i), false);
            }
            SetMask(ref _receivedMask, id, true);
        }
        public void OnReceiveAcknowledgement(ushort mask, int id)
        {
            for (int i = 0; i < 16; i++)
            {
                int messageID = (id - i + 256) % 256;
                if (((mask >> (15 - i)) & 1) == 1 && MaskIsTrue(_aliveMask, messageID))
                {
                    Console.WriteLine("Ack: " + messageID);
                    _onAcknowledged?.Invoke(messageID);
                    SetMask(ref _aliveMask, messageID, false);
                }
            }
        }
        public ushort GetReceiveMaskForId(int id)
        {
            //15 beacuse we want our id to be at the 15th position before the cut to ushort happen.
            return (ushort)LeftBitwiseRotation(_receivedMask, 15 - id);
        }
        public bool MessageHasBeenReceived(int id)
        {
            return MaskIsTrue(_receivedMask, id % 256);
        }
        public bool MessageisAlive(int id)
        {
            return MaskIsTrue(_aliveMask, id % 256);
        }
        //Mask operations
        private Uint256 RightBitwiseRotation(Uint256 mask, int amount)
        {
            return mask << (256 - amount) | mask >> amount;
        }
        private Uint256 LeftBitwiseRotation(Uint256 mask, int amount)
        {
            return mask << amount | mask >> (256 - amount);
        }
        private void SetMask(ref Uint256 mask, int id, bool value)
        {
            int Idc = ((id + 256) % 256);
            mask = value ? mask | (new Uint256(1) << Idc) : mask & ~(new Uint256(1) << Idc);
        }
        private bool MaskIsTrue(Uint256 mask, int id)
        {
            int Idc = ((id + 256) % 256);
            return ((mask >> Idc) & new Uint256(1)) == 1;
        }
    }
}
