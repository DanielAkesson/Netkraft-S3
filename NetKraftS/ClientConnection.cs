using Netkraft.Messaging;
using NetKraft;
using System;
using System.Collections.Generic;
using System.Net;

namespace Netkraft
{
    public class ClientConnection
    {
        public NetkraftClient MasterClient;
        public IPEndPoint IpEndPoint;
        private Dictionary<ChannelId, Channel> _typeToChannel = new Dictionary<ChannelId, Channel>();
        private List<Channel> _channels = new List<Channel>();
        private List<NetkraftPlayer> _clientPlayers = new List<NetkraftPlayer>();

        public ClientConnection(NetkraftClient masterClient, IPEndPoint ipEndPoint)
        {
            MasterClient = masterClient;
            IpEndPoint = ipEndPoint;
            AddChannel(new UnreliableChannel(masterClient, this), ChannelId.Unreliable);
            AddChannel(new UnreliableAcknowledgedChannel(masterClient, this), ChannelId.UnreliableAcknowledged);
            ReliableChannel RC = new ReliableChannel(masterClient, this);
            AddChannel(RC, ChannelId.Reliable);
        }
        private void AddChannel(Channel channel, ChannelId channelId)
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
            foreach (Channel c in _channels)
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
            foreach (Channel c in _channels)
                c.ReceiveTick();
        }
        internal void ReceiveTickRestrictive()
        {
            foreach (Channel c in _channels)
                c.ReceiveTickRestrictive();
        }
        internal Channel GetMessageChannel(object message)
        {
            Channel channel = null;
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
        internal Channel GetMessageChannel(ChannelId id)
        {
            return _typeToChannel[id];
        }
    }
}
