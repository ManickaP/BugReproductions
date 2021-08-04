using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace cookie_test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var handler = new HttpClientHandler();
            handler.CookieContainer.Add(new System.Net.Cookie("container_test1", "1", path: "/", domain: "localhost"));
            handler.CookieContainer.Add(new System.Net.Cookie("container_test2", "2", path: "/", domain: "localhost"));
            using var client = new HttpClient(handler);
            // Infinite time out for debugging
            client.Timeout = TimeSpan.FromMilliseconds(-1);

            var message = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5001");
            message.Version = HttpVersion.Version20;
            message.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
            message.Headers.Add("Cookie", new [] {"test1=1","test2=2"});
            message.Headers.Add("Cookie", "test3=3");
            message.Headers.TryAddWithoutValidation("Cookie", new [] {"test4=4", "test5=5"});
            message.Headers.TryAddWithoutValidation("Cookie", "test6=6");
            var response = await client.SendAsync(message);
        }
    }
}
