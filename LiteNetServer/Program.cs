using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiteNetServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var listener = new UdpServerListener();
            NetManager server = new NetManager(listener, 10000 , "myapp");
            listener.m_Server = server;
            if(!server.Start(3721))
            {
                Console.WriteLine("Server start failed!");
                Console.ReadKey();
                return;
            }

            while(!Console.KeyAvailable)
            {
                server.PollEvents();
                Thread.Sleep(10);
            }

            server.Stop();
            Console.ReadKey();
            Console.WriteLine("ServStats:\n BytesReceived: {0}\n PacketsReceived: {1}\n BytesSent: {2}\n PacketsSent: {3}",
                server.BytesReceived,
                server.PacketsReceived,
                server.BytesSent,
                server.PacketsSent);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
