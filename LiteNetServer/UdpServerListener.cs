using Example;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteNetServer
{
    struct ProtoBufSerializable : INetSerializable
    {
        public MessageWrapper MsgWrapper { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutBytesWithLength(MessageWrapper.SerializeToBytes(MsgWrapper));
        }

        public void Desereialize(NetDataReader reader)
        {
            var data = reader.GetBytesWithLength();
            var message = MessageWrapper.Deserialize(data);
            MsgWrapper = message;
        }
    }

    class ProtoBufPacket
    {
        public ProtoBufSerializable Pbs { get; set; }
    }

    class UdpServerListener : INetEventListener
    {
        public NetManager m_Server;
        private readonly NetSerializer m_NetSerializer;

        public UdpServerListener()
        {
            m_NetSerializer = new NetSerializer();
            m_NetSerializer.RegisterCustomType<ProtoBufSerializable>(); 
            m_NetSerializer.SubscribeReusable<ProtoBufPacket, NetPeer>(OnSamplePacketReceived);
    }

        private void OnSamplePacketReceived(ProtoBufPacket samplePacket, NetPeer peer)
        {
            Person p = Person.Deserialize(samplePacket.Pbs.MsgWrapper.Data);
            Console.WriteLine("[Server] ReceivedPacket from {0}:\n{1}", peer.EndPoint, p.Name);
            m_Server.SendToAll(m_NetSerializer.Serialize(samplePacket), SendOptions.ReliableOrdered);
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine("[Server] Peer connected: " + peer.EndPoint);
            var peers = m_Server.GetPeers();
            foreach (var netPeer in peers)
            {
                Console.WriteLine("ConnectedPeersList: id={0}, ep={1}", netPeer.ConnectId, netPeer.EndPoint);
            }
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine("[Server] Peer disconnected: " + peer.EndPoint + ", reason: " + disconnectInfo.Reason);
        }

        public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            Console.WriteLine("[Server] error: " + socketErrorCode);
        }

        public void OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            Console.WriteLine("[Server] received data. Processing...");
            m_NetSerializer.ReadAllPackets(reader, peer);
        }

        public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
            Console.WriteLine("[Server] ReceiveUnconnected: {0}", reader.GetString(100));
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {


        }
    }
}
