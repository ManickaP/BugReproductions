using System;
using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace client_test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Old .NET Core 3.1 settings to allow H2C, replaced with VersionPolicy = HttpVersionPolicy.RequestVersionExact
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var listener = new HttpEventListener();
            // Not needed in clear text scenario, just a remnant of the original test.
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = delegate { return true; }
            };
            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(1000),
                DefaultRequestVersion = HttpVersion.Version20,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
            };

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            try
            {
                using var response = await client.GetAsync("http://localhost:5001/sleepFor?seconds=100", cts.Token);
                Console.WriteLine(response);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                // Handle timeout
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // Old test of header frame sending.
            /*var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5001")
            {
                Version = HttpVersion.Version20,
                VersionPolicy = HttpVersionPolicy.RequestVersionExact
            };
            var oneKbString = new string('a', 1024);
            // The default frame size limit is 16kb, and the total server header size limit is 32kb.
            for (var i = 0; i < 20; i++)
            {
                request.Headers.Add("header" + i, oneKbString + i);
            }
            var result = await client.SendAsync(request);

            Console.WriteLine(request);
            Console.WriteLine(result);*/
        }
    }

    internal sealed class HttpEventListener : EventListener
    {

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "Private.InternalDiagnostics.System.Net.Http")
                EnableEvents(eventSource, EventLevel.LogAlways);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            var sb = new StringBuilder().Append($"{eventData.TimeStamp:HH:mm:ss.fffffff}[{eventData.EventName}] ");
            for (int i = 0; i < eventData.Payload?.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(eventData.PayloadNames?[i]).Append(": ").Append(eventData.Payload[i]);
            }
            Console.WriteLine(sb.ToString());
        }
    }

}
