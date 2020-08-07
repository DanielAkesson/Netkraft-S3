using Netkraft;
using System;
using System.Net;

namespace Netkraft
{
    public abstract class Channel
    {
        public abstract void Send(byte[] buffer, IPEndPoint to, Action onAcknowledge);
        public abstract void Deliver(byte[] buffer, int size, IPEndPoint from);
        public abstract int Receive(out byte[] buffer, out IPEndPoint sender);
        public abstract bool RemoveEndpoint(IPEndPoint toBeRemoved);
    }
    public enum ChannelId2 : byte
    {
        Unreliable = 0, UnreliableAcknowledged = 1, Reliable = 2, SlowReliable = 3
    }
}
