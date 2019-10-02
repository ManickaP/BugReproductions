using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace issue_32611
{
    public class Program
    {
        static void Main(string[] args) => BenchmarkSwitcher.FromTypes(new[] { typeof(Program) }).Run(args);

        [Benchmark]
        public string GetHostName() => Dns.GetHostName();


        [Benchmark]
        public IPAddress[] GetHostAddresses() => Dns.GetHostAddresses(Dns.GetHostName());

        [Benchmark]
        public IPAddress[] Adapters()
        {
            var addresses = new List<IPAddress>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    addresses.Add(ip.Address);
                }
            }
            return addresses.ToArray();
        }
    }
}
