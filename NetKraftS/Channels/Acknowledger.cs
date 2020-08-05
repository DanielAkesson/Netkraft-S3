using Netkraft;
using Netkraft.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netkraft
{
    public class Acknowledger
    {
        private ulong _ackMask;
        private ulong _receivedMask;
        private Action<int> _onAcknowledged;
        private int _largestReceivedId;

        public Acknowledger(Action<int> onAcknowledged)
        {
            _onAcknowledged = onAcknowledged;
        }
        public void OnSendMessage(int id)
        {
            ulong mask = ~(ulong)1 << (id % 64);
            _ackMask &= mask;
        }
        public void OnReceiveMessage(int id)
        {
            int bigId = _largestReceivedId;
            // Gives a value from (-32 <---> 32)
            int delta = Math.Abs(id - bigId) > 32 ? (id - bigId < 0 ? (id - bigId) + 64 : id - (bigId + 64)) : id - bigId;

            if (delta > 0)
            {
                _largestReceivedId = id;
                //Remove all values in receive mask 32 above this new largest id.
                for (int i = 0; i < 32; i++)
                    SetMask(ref _receivedMask, (id + i), false);
            }
            SetMask(ref _receivedMask, id, true);
        }
        public void OnReceiveAcknowledgement(uint mask, int id)
        {
            string MaskMessage = "Received acknowledgment for: ";
            for (int i = 0; i < 32; i++)
            {
                int messageID = ((id - i) + 64) % 64;
                //If id is confirmed call acknowledgment on all message objects for that UDP packet
                MaskMessage += ((mask >> (31 - i)) & 1) + " ";
                if (((mask >> (31 - i)) & 1) == 1 && !MaskIsTrue(_ackMask, messageID))
                {
                    _onAcknowledged?.Invoke(messageID);
                    SetMask(ref _ackMask, messageID, true);
                }
            }
            //Console.WriteLine(MaskMessage + "For id: " + id);
        }
        public uint GetReceiveMaskForId(int id)
        {
            return GetUintMaskForId(_receivedMask, id);
        }
        public bool MessageHasBeenReceived(int id)
        {
            return MaskIsTrue(_receivedMask, id % 64);
        }
        //Mask operations
        private uint GetUintMaskForId(ulong mask, int id)
        {
            byte Idc = (byte)((64 - (id - 31)) % 64);
            //Bitwise rotation left!
            ulong spun = mask << Idc | mask >> (64 - Idc);
            return (uint)(spun);
        }
        private void SetMask(ref ulong mask, int id, bool value)
        {
            int Idc = ((id + 64) % 64);
            ulong alterMask = ((ulong)1 << Idc);
            mask = value ? mask | alterMask : mask & ~alterMask;
        }
        private bool MaskIsTrue(ulong mask, int id)
        {
            int Idc = ((id + 64) % 64);
            return ((mask >> Idc) & 1) == 1;
        }
    }
}
