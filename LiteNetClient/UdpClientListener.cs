using Example;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteNetClient
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

    class UdpClientListener : INetEventListener
    {
        private readonly NetSerializer m_NetSerializer;

        public UdpClientListener()
        {
            m_NetSerializer= new NetSerializer();
            m_NetSerializer.RegisterCustomType<ProtoBufSerializable>();
            m_NetSerializer.SubscribeReusable<ProtoBufPacket, NetPeer>(OnSamplePacketReceived);
        }

        private void OnSamplePacketReceived(ProtoBufPacket samplePacket, NetPeer peer)
        {
            Person p = Person.Deserialize(samplePacket.Pbs.MsgWrapper.Data);
            Console.WriteLine("[Server] ReceivedPacket from {0}:\n{1}", peer.EndPoint, p.Name);
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine("[Client] connected to: {0}:{1}", peer.EndPoint.Host, peer.EndPoint.Port);

            MessageWrapper m_MsgWrapper = new MessageWrapper();
            Person newPerson = new Person { Name = "战斗吧，少年", Age = 101};
            m_MsgWrapper.ID = 200;
            m_MsgWrapper.Data = Person.SerializeToBytes(newPerson);

            ProtoBufPacket p = new ProtoBufPacket { Pbs = new ProtoBufSerializable { MsgWrapper = m_MsgWrapper } };
            byte[] data = m_NetSerializer.Serialize(p);
            Console.WriteLine("Sending to server (length {0}):\n", data.Length);
            peer.Send(data, SendOptions.ReliableOrdered);

        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine("[Client] disconnected: " + disconnectInfo.Reason);
        }

        public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            Console.WriteLine("[Client] error! " + socketErrorCode);
        }

        public void OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            m_NetSerializer.ReadAllPackets(reader, peer);
        }

        public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {

        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {

        }
    }
}
