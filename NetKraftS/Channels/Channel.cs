using Netkraft;
namespace NetKraft
{
    public abstract class Channel
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
    public enum ChannelId : byte
    {
        Unreliable = 0, UnreliableAcknowledged = 1, Reliable = 2, ReliableAcknowledged = 3
    }
}
