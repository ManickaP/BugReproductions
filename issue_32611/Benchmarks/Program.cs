using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace issue_32611
{
    [MemoryDiagnoser]
    public class Program
    {
        public const string domainName = "ibm.com";

        public const string knownDomainName = "microsoft.com";

        static void Main(string[] args) => BenchmarkSwitcher.FromTypes(new[] { typeof(Program) }).Run(args);

        [Benchmark]
        public string GetHostName() => Dns.GetHostName();

        public IEnumerable<string> DomainNames() => new[] {
            Dns.GetHostName(),
            domainName,
            knownDomainName
        };

        [Benchmark]
        [ArgumentsSource(nameof(DomainNames))]
        public IPAddress[] GetHostAddresses(string hostName) => Dns.GetHostAddresses(hostName);

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

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct sockaddr
        {
            internal short sa_family;
            internal short sa_port;
            internal fixed byte data[20];
            internal uint scope_id;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct ifaddrs
        {
            internal readonly void* ifa_next;
            internal readonly char* ifa_name;
            internal readonly uint ifa_flags;
            internal readonly sockaddr* ifa_addr;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct addrinfo
        {
            internal int ai_flags;
            internal int ai_family;
            internal readonly int ai_socktype;
            internal readonly int ai_protocol;
            internal readonly int ai_addrlen;
            internal readonly sockaddr* ai_addr;
            internal readonly char* ai_canonname;
            internal readonly addrinfo* ai_next;
            internal readonly void* ifa_next;
            internal readonly char* ifa_name;
            internal readonly uint ifa_flags;
            internal readonly sockaddr* ifa_addr;
        }

        [DllImport("libc", EntryPoint = "getifaddrs")]
        unsafe static extern int getifaddrs(ref void* addrs);

        [DllImport("libc")]
        unsafe static extern void freeifaddrs(void* ifa);

        [DllImport("libc")]
        unsafe static extern int getaddrinfo(string node, string service, void* hints, ref void* res);

        [DllImport("libc")]
        unsafe static extern int freeaddrinfo(void* ai);

        [Benchmark]
        public unsafe IPAddress[] getifaddrs()
        {
            void* pinput = null;
            int ret = getifaddrs(ref pinput);
            int count = 0;

            ifaddrs* entry = (ifaddrs*)pinput;
            while (entry != null)
            {
                if (entry->ifa_addr->sa_family == 2 || entry->ifa_addr->sa_family == 10)
                {
                    count++;
                }
                entry = (ifaddrs*)entry->ifa_next;
            }

            var addresses = new IPAddress[count];

            entry = (ifaddrs*)pinput;
            count = 0;
            while (entry != null)
            {
                if (entry->ifa_addr->sa_family == 2)
                {
                    addresses[count] = new IPAddress(new ReadOnlySpan<byte>(entry->ifa_addr->data, 4));
                    count++;
                }
                else if (entry->ifa_addr->sa_family == 10)
                {
                    addresses[count] = new IPAddress(new ReadOnlySpan<byte>(entry->ifa_addr->data + 4, 16));
                    addresses[count].ScopeId = entry->ifa_addr->scope_id;
                    count++;
                }

                entry = (ifaddrs*)entry->ifa_next;
            }

            freeifaddrs(pinput);

            return addresses;
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(DomainNames))]
        public unsafe IPAddress[] getaddrinfo(string hostName)
        {
            void* pinput = null;

            addrinfo hint = new addrinfo();
            hint.ai_family = 0;
            hint.ai_flags = 2;

            int result = getaddrinfo(hostName, null, (void*)(&hint), ref pinput);

            int count = 0;

            addrinfo* entry = (addrinfo*)pinput;
            while (entry != null)
            {
                if (entry->ai_family == 2 || entry->ai_family == 10)
                {
                    count++;
                }
                entry = (addrinfo*)entry->ai_next;
            }

            var addresses = new IPAddress[count];

            entry = (addrinfo*)pinput;
            count = 0;
            while (entry != null)
            {
                if (entry->ai_family == 2)
                {
                    addresses[count] = new IPAddress(new ReadOnlySpan<byte>(entry->ai_addr->data, 4));
                    count++;
                }
                else if (entry->ai_family == 10)
                {
                    addresses[count] = new IPAddress(new ReadOnlySpan<byte>(entry->ai_addr->data + 4, 16));
                    //addresses[count].ScopeId = entry->ifa_addr->scope_id;
                    count++;
                }

                entry = (addrinfo*)entry->ai_next;
            }

            freeifaddrs(pinput);

            return addresses;
        }
    }
}
