using System;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace issue_32611
{
    class Program
    {
        static void Main(string[] args)
        {
            var pid = Process.GetCurrentProcess().Id;
            for (int i = 0; i < 100_000_000; ++i) {
                string hostname = Dns.GetHostName();
                Console.WriteLine("My hostname is '{0}' in '{1}'", hostname, pid);
                Console.WriteLine("My ip addresses are:");
                foreach (var address in Dns.GetHostAddresses(hostname)) {
                    Console.WriteLine("\t{0}", address);
                }
            }
        }
    }
}
