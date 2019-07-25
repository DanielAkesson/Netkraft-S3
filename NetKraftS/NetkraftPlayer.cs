using Netkraft.Messaging;
using System;

namespace Netkraft
{
    public struct NetkraftPlayer : IUnreliableMessage
    {
        public int ConnectionId;
        public int PlayerId;
        public bool Disconnected;

        public void OnAcknowledgment(ClientConnection Context)
        {
            throw new NotImplementedException();
        }

        public void OnReceive(ClientConnection Context)
        {
            throw new NotImplementedException();
        }

        public void OnSend(ClientConnection Context)
        {
            throw new NotImplementedException();
        }
    }
}
