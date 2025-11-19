using Netkraft;
using System;
using System.Net;
using System.Net.Sockets;

namespace Netkraft
{
    public abstract class Channel
    {
#if DEBUG
        //Debug variables!
        public float successRate = 1.0f;
        protected Random r = new Random();
#endif
        //Send functions!
        public abstract void Send(byte[] buffer, int offset, int size, SocketFlags socketFlags, IPEndPoint RemoteEP, Action onAcknowledge);

        //Receive functions
        public abstract void Deliver(ref byte[] buffer, int size, IPEndPoint from);
        
        //Management functions
        public abstract bool RemoveEndpoint(IPEndPoint toBeRemoved);
        public virtual void Dispose() { }
    }
    public enum ChannelId : byte
    {
        Unreliable = 0, UnreliableAcknowledged = 1, Reliable = 2, SlowReliable = 3
    }
}
