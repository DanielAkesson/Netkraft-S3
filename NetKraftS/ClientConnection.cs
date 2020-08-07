using Netkraft.Messaging;
using System;
using System.Collections.Generic;
using System.Net;

namespace Netkraft
{
    [Obsolete]
    public class ClientConnection
    {
        public NetkraftClient MasterClient;
        public IPEndPoint IpEndPoint;
        private Dictionary<ChannelId, ChannelOld> _typeToChannel = new Dictionary<ChannelId, ChannelOld>();
        private List<ChannelOld> _channels = new List<ChannelOld>();

        public ClientConnection(NetkraftClient masterClient, IPEndPoint ipEndPoint)
        {
            MasterClient = masterClient;
            IpEndPoint = ipEndPoint;
            AddChannel(new UnreliableChannelOld(masterClient, this), ChannelId.Unreliable);
            AddChannel(new UnreliableAcknowledgedChannelOld(masterClient, this), ChannelId.UnreliableAcknowledged);
            ReliableChannelOld RC = new ReliableChannelOld(masterClient, this);
            AddChannel(RC, ChannelId.Reliable);
        }
        private void AddChannel(ChannelOld channel, ChannelId channelId)
        {
            _typeToChannel.Add(channelId, channel);
            _channels.Add(channel);
        }

        public void AddToQueue(object message)
        {
            GetMessageChannel(message).AddToQueue(message);
        }
        public void SendImmediately(object message)
        {
            GetMessageChannel(message).SendImmediately(message);
        }
        public void SendQueue()
        {
            foreach (ChannelOld c in _channels)
                c.SendQueue();
        }
        //internal
        internal void Receive(byte[] buffer, int size)
        {
            //This would switch to the correct channel method
            byte channel = (byte)BitConverter.ToChar(buffer, 0);
            _channels[channel].Receive(buffer, size);
        }
        internal void ReceiveTick()
        {
            foreach (ChannelOld c in _channels)
                c.ReceiveTick();
        }
        internal void ReceiveTickRestrictive()
        {
            foreach (ChannelOld c in _channels)
                c.ReceiveTickRestrictive();
        }
        internal ChannelOld GetMessageChannel(object message)
        {
            ChannelOld channel = null;
            if (message is IReliableMessage)
                channel = _typeToChannel[ChannelId.Reliable];
            else if (message is IUnreliableMessage)
            {
                if (message is IAcknowledged)
                    channel = _typeToChannel[ChannelId.UnreliableAcknowledged];
                else
                    channel = _typeToChannel[ChannelId.Unreliable];
            }
            return channel;
        }
        internal ChannelOld GetMessageChannel(ChannelId id)
        {
            return _typeToChannel[id];
        }
    }
}
