using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiteNetClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var listener = new UdpClientListener();
            NetManager client = new NetManager(listener, "myapp");
            client.MergeEnabled = true;
            if(!client.Start())
            {
                Console.WriteLine("Client start failed!");
                return; 
            }
            client.Connect("192.168.1.132", 3721);

            while(!Console.KeyAvailable)
            {
                client.PollEvents();
                Thread.Sleep(10);
            }
            client.Stop();
            Console.ReadKey();
            Console.WriteLine("ClientStats:\n BytesReceived: {0}\n PacketsReceived: {1}\n BytesSent: {2}\n PacketsSent: {3}",
                client.BytesReceived,
                client.PacketsReceived,
                client.BytesSent,
                client.PacketsSent);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
