﻿using Netkraft.Messaging;
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
        private Dictionary<Type, Channel> _typeToChannel = new Dictionary<Type, Channel>();
        private List<Channel> _channels = new List<Channel>();
        private List<NetkraftPlayer> _clientPlayers = new List<NetkraftPlayer>();

        public ClientConnection(NetkraftClient masterClient, IPEndPoint ipEndPoint)
        {
            MasterClient = masterClient;
            IpEndPoint = ipEndPoint;
            AddChannel(new UnreliableChannel(masterClient, this), typeof(IUnreliableMessage));
            AddChannel(new UnreliableAcknowledgedChannel(masterClient, this), typeof(IUnreliableAcknowledgedMessage));
            ReliableChannel RC = new ReliableChannel(masterClient, this);
            AddChannel(RC, typeof(IReliableMessage));
            AddChannel(RC, typeof(IReliableAcknowledgedMessage));
        }
        private void AddChannel(Channel channel, Type channelType)
        {
            _typeToChannel.Add(channelType, channel);
            _channels.Add(channel);
        }

        public void AddToQueue(object message)
        {
            _typeToChannel[Message.GetChannelType(message)].AddToQueue(message);
        }
        public void SendImmediately(object message)
        {
            _typeToChannel[Message.GetChannelType(message)].SendImmediately(message);
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
        internal Channel GetChannelOfType(Type channelType)
        {
            return _typeToChannel[channelType];
        }
    }
}
