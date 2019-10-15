using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace issue_32611
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var x = await Dns.GetHostAddressesAsync("localhost");
            string hostname = Dns.GetHostName();
            var y = Dns.EndGetHostByName(Dns.BeginGetHostByName("", null, null));
            Console.WriteLine("My hostname is '{0}'", hostname);
            Console.WriteLine("My ip addresses are:");
            foreach (var address in Dns.GetHostAddresses(hostname)) {
                Console.WriteLine("\t{0}", address);
            }
        }
    }
}
