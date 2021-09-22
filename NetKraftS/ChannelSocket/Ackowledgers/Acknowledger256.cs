
using System;
using System.Numerics;

namespace Netkraft
{
    public class Acknowledger256
    {
        //A mask of all messages sent from this client that are waiting acknowledgment
        public Uint256 _aliveMask = new Uint256(0);
        // A mask of all messages received by coupled client This is reset 16 id in front of the biggest received so far
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

                //cx000ccc
                //x000cccc
                //0000x000
                //Remove all values in receive mask 128 above this new largest id.
                _receivedMask = LeftBitwiseRotation(_receivedMask, 255-id);
                _receivedMask = _receivedMask >> 128;
                _receivedMask = RightBitwiseRotation(_receivedMask, (127+256-id)%256);
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
                    _onAcknowledged?.Invoke(messageID);
                    SetMask(ref _aliveMask, messageID, false);
                }
            }
        }
        public void OnReceiveAcknowledgement(uint mask, int id)
        {
            for (int i = 0; i < 32; i++)
            {
                int messageID = (id - i + 256) % 256;
                if (((mask >> (31 - i)) & 1) == 1 && MaskIsTrue(_aliveMask, messageID))
                {
                    _onAcknowledged?.Invoke(messageID);
                    SetMask(ref _aliveMask, messageID, false);
                }
            }
        }
        public void OnReceiveAcknowledgement(ulong mask, int id)
        {
            for (int i = 0; i < 64; i++)
            {
                int messageID = (id - i + 256) % 256;
                if (((mask >> (63 - i)) & 1) == 1 && MaskIsTrue(_aliveMask, messageID))
                {
                    _onAcknowledged?.Invoke(messageID);
                    SetMask(ref _aliveMask, messageID, false);
                }
            }
        }
        public ushort GetShortReceiveMaskForId(int id)
        {
            //15 because we want our id to be at the 15th position before the cut to ushort happen.
            return (ushort)LeftBitwiseRotation(_receivedMask, 15 - id);
        }
        public uint GetIntReceiveMaskForId(int id)
        {
            //31 because we want our id to be at the 31th position before the cut to uint happen.
            return (uint)LeftBitwiseRotation(_receivedMask, 31 - id);
        }
        public ulong GetLongReceiveMaskForId(int id)
        {
            //63 because we want our id to be at the 63th position before the cut to ulong happen.
            return (ulong)LeftBitwiseRotation(_receivedMask, 63 - id);
        }
        public bool MessageHasBeenReceived(int id)
        {
            return MaskIsTrue(_receivedMask, id % 256);
        }
        public bool MessageisAlive(int id)
        {
            return MaskIsTrue(_aliveMask, id);
        }
        public bool RangeisAlive(int startId, int rangeInlongs)
        {
            Uint256 temp = RightBitwiseRotation(_aliveMask, startId);
            bool result = temp.Longs()[0] != 0 && rangeInlongs > 0;         //1
            result = temp.Longs()[1] != 0 && rangeInlongs > 1 || result;    //2
            result = temp.Longs()[2] != 0 && rangeInlongs > 2 || result;    //3
            result = temp.Longs()[3] != 0 && rangeInlongs > 3 || result;    //4
            return result;
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

        public override string ToString()
        {
            string aliveMaskString =
                Convert.ToString((long)_aliveMask.Longs()[3], 2).PadLeft(32, '0') + " " +
                Convert.ToString((long)_aliveMask.Longs()[2], 2).PadLeft(32, '0') + " " +
                Convert.ToString((long)_aliveMask.Longs()[1], 2).PadLeft(32, '0') + " " +
                Convert.ToString((long)_aliveMask.Longs()[0], 2).PadLeft(32, '0');

            string receivedMaskString =
                Convert.ToString((long)_receivedMask.Longs()[3], 2).PadLeft(32, '0') + " " +
                Convert.ToString((long)_receivedMask.Longs()[2], 2).PadLeft(32, '0') + " " +
                Convert.ToString((long)_receivedMask.Longs()[1], 2).PadLeft(32, '0') + " " +
                Convert.ToString((long)_receivedMask.Longs()[0], 2).PadLeft(32, '0');

            return $"\nAlive mask:   {aliveMaskString} \nReceive mask: {receivedMaskString}\n";
        }
    }
}
