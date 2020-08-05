using System;
using System.Net;
using System.Threading;

namespace Netkraft.ChannelSocket
{
    class UnreliableChannel2 : Channel2
    {
        public override void Deliver(byte[] buffer, int size, IPEndPoint from)
        {
            return;
        }

        public override int Receive(out byte[] buffer, out IPEndPoint sender)
        {
            while (true)
                Thread.Sleep(100);
            return -1;
        }

        public override bool RemoveEndpoint(IPEndPoint toBeRemoved)
        {
            throw new NotImplementedException();
        }

        public override void Send(byte[] buffer, IPEndPoint to, Action onAcknowledge)
        {
            throw new NotImplementedException();
        }
    }
}
