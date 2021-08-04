using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace h2c_inf_loop
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var client = new HttpClient() {
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
                DefaultRequestVersion = HttpVersion.Version20,
                Timeout = TimeSpan.FromDays(1)
            };
            try {
                var _ = await client.GetAsync("http://corefx-net-http11.azurewebsites.net/echo.ashx");
            } catch {}
            var response = await client.GetAsync("http://corefx-net-http11.azurewebsites.net/echo.ashx");
        }
    }
}
