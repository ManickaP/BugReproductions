using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

namespace trim_test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var x = Process.GetCurrentProcess();
            Console.WriteLine(OperatingSystem.IsLinux());
            using var client = new HttpClient();
            using var response = await client.GetAsync("https://www.microsoft.com");
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);
        }
    }
}
