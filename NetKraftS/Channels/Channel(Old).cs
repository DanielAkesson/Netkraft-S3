using System;

namespace Netkraft
{
    [Obsolete]
    public abstract class ChannelOld
    {
        protected NetkraftClient _masterClient;
        protected ClientConnection _connection;

        public abstract void AddToQueue(object message);
        public abstract void SendImmediately(object message);
        public abstract void SendQueue();
        public abstract void Receive(byte[] buffer, int size);
        public abstract void ReceiveTick();
        public abstract void ReceiveTickRestrictive();
    }
    public enum ChannelId2 : byte
    {
        Unreliable = 0, UnreliableAcknowledged = 1, Reliable = 2, ReliableAcknowledged = 3
    }
}
