using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetkraftMessage
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
            AddChannel(new ReliableChannel(masterClient, this), typeof(IReliableMessage));
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
        internal void Recive(byte[] buffer, int size)
        {
            byte channel = (byte)BitConverter.ToChar(buffer, 0);
            _channels[channel].Recive(buffer, size);
        }
        internal void ReceiveTick()
        {
            foreach (Channel c in _channels)
                c.ReceiveTick();
        }
        internal Channel GetChannelOfType(Type channelType)
        {
            return _typeToChannel[channelType];
        }
    }
    public abstract class Channel
    {
        protected NetkraftClient _masterClient;
        protected ClientConnection _connection;
        
        public abstract void AddToQueue(object message);
        public abstract void SendImmediately(object message);
        public abstract void SendQueue();
        public abstract void Recive(byte[] buffer, int size);
        public abstract void ReceiveTick();
    }
}
