using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System;
using Netkraft.Messaging;

namespace Netkraft
{
    public class NetkraftClient
    {
        private Socket _socket;
        private List<ClientConnection> _clientConnections = new List<ClientConnection>();
        private Dictionary<IPEndPoint, ClientConnection> _iPEndPointToClientConnection = new Dictionary<IPEndPoint, ClientConnection>();

        private MemoryStream _instantMessageStream = new MemoryStream();
        //Receive vars
        private MemoryStream _receiveStream = new MemoryStream();
        private byte[] _buffer = new byte[65536]; //UDP messages can't exceed 65507 bytes so this should always be sufficient
        private EndPoint _sender = new IPEndPoint(IPAddress.Any, 0);
        //State vars
        public int MaxAllowedPlayers { get; } = 0;
        public Dictionary<IPEndPoint, List<NetkraftPlayer>> PlayersForConnection { get; } = new Dictionary<IPEndPoint, List<NetkraftPlayer>>();
        public List<NetkraftPlayer> AllPlayers;

        private Random rand = new Random();
        public int FakeLossProcent = 0;

        //Constuctor
        public NetkraftClient(int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//Socket that supports IPV4
            _socket.Bind(new IPEndPoint(IPAddress.Any, port));
            new Thread(Receive).Start();
        }
        private void Receive()
        {
            while (true)
            {
                int size = _socket.ReceiveFrom(_buffer, ref _sender);
                if (!_iPEndPointToClientConnection.ContainsKey((IPEndPoint)_sender)) return;
                _iPEndPointToClientConnection[(IPEndPoint)_sender].Recive(_buffer, size);
            }
        }
        internal void SendStream(MemoryStream stream, int SizeOfStreamToSend, ClientConnection client)
        {
            if (rand.Next(1, 100) < FakeLossProcent) return;
            _socket.SendTo(stream.GetBuffer(), 0, SizeOfStreamToSend, SocketFlags.None, client.IpEndPoint);
        }

        //Public methods!
        /// <summary>
        /// Send a message to all connected clients Immediately not wating for the next tick.
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendImmediately(object message)
        {
            foreach (ClientConnection CC in _clientConnections)
                CC.SendImmediately(message);
        }
        public void AddToQueue(object message)
        {
            foreach (ClientConnection CC in _clientConnections)
                CC.AddToQueue(message);
        }
        public void SendQueue()
        {
            foreach (ClientConnection CC in _clientConnections)
                CC.SendQueue();
        }

        public void AddEndPoint(IPEndPoint ipEndPoint)
        {
            ClientConnection c = new ClientConnection(this, ipEndPoint);
            _clientConnections.Add(c);
            if(!_iPEndPointToClientConnection.ContainsKey(ipEndPoint))
                _iPEndPointToClientConnection.Add(ipEndPoint, c);
        }
        public void AddEndPoint(string ip, int port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            AddEndPoint(endPoint);
        }
        public void ReceiveTick()
        {
            foreach (ClientConnection CC in _clientConnections)
                CC.ReceiveTick();
        }
    }
    
    class RequestJoin : IUnreliableMessage
    {
        public string LocalIp;
        public string MacAddress;

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
