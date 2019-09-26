using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System;
using System.Runtime.InteropServices;

public class Program
{
    [StructLayout (LayoutKind.Sequential)]
    internal unsafe struct sockaddr {
        internal short sa_family;
        internal short sa_port;
        internal fixed byte data[20];
        internal uint scope_id;
    }

    [StructLayout (LayoutKind.Sequential)]
    internal unsafe struct ifaddrs {
        internal readonly void *ifa_next;
        internal readonly char *ifa_name;
        internal readonly uint ifa_flags;
        internal readonly sockaddr  *ifa_addr;
    }

    [DllImport("libc", EntryPoint = "getifaddrs")]
    unsafe static extern int getifaddrs(ref void * addrs);

    [DllImport("libc")]
    unsafe static extern void freeifaddrs(void* ifa);

    //static void Main(string[] args) => BenchmarkSwitcher.FromTypes(new[] { typeof(Program) }).Run(args);
        /*foreach (var addr in new Program().getifaddrs()) {
            Console.WriteLine(addr);
        }*/

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

    [Benchmark]
    public IPAddress[] GetAllNetworkInterfaces()
    {
        var addresses = new IPAddress[0];

        var list = NetworkInterface.GetAllNetworkInterfaces();

        return addresses;
    }
    [Benchmark]
    public unsafe IPAddress[] getifaddrs()
    {
        void * pinput = null;
        int ret = getifaddrs(ref pinput);
        int count = 0;

        ifaddrs * entry = (ifaddrs*)pinput;
        while (entry != null)
        {
             if (entry->ifa_addr->sa_family == 2 || entry->ifa_addr->sa_family == 10)
            {
                count ++;
            }
            entry = (ifaddrs*)entry->ifa_next;
         }

        var addresses = new IPAddress[count];

        entry = (ifaddrs*)pinput;
        count = 0;
        while (entry != null)
        {
            if (entry->ifa_addr->sa_family == 2){
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
}