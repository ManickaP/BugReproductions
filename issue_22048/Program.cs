using System;
using System.Net.NetworkInformation;

namespace issue_22048
{
    class Program
    {
        static void Main(string[] args)
        {
            try {
                if (NetworkInterface.GetIsNetworkAvailable()) {
                    Console.WriteLine("Available!");
                } else {
                    Console.WriteLine("Unavailable!");
                }
            } catch (Exception) {
                Console.WriteLine("Crashed!");
            }
        }
    }
}
