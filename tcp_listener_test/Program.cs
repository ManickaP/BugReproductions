using System;
using System.Net;
using System.Net.Sockets;

namespace tcp_listener_test
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new TcpListener(IPAddress.Loopback, 0);
            Console.WriteLine(x.LocalEndpoint);
            x.Start();
            Console.WriteLine(x.LocalEndpoint);
            Console.WriteLine("Hello World!");
        }
    }
}
