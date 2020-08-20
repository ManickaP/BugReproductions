using System;
using System.IO;
using System.Diagnostics.Tracing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace http_client_activity_missing
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var listener = new HttpEventListener();
            using var client = new HttpClient();
            Console.WriteLine(Environment.ProcessId);
            while (true)
            {
                await Task.Delay(10_000);
                //http://corefx-net-http2.azurewebsites.net/EmptyContent.ashx 
                //var response = await client.GetStringAsync(@"https://github.com/dotnet/runtime/blob/master/src/libraries/System.Net.Http/src/HttpDiagnosticsGuide.md");
                using var response = await client.GetAsync(@"https://github.com/dotnet/runtime/blob/master/src/libraries/System.Net.Http/src/HttpDiagnosticsGuide.md", HttpCompletionOption.ResponseContentRead);
                Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fffffff}Got response");
                using var responseStream = response.Content.ReadAsStream();
                using var reader = new StreamReader(responseStream);
                var data = await reader.ReadToEndAsync();
                Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fffffff}Got response: {data.Length}");
            }
        }
    }

    internal sealed class HttpEventListener : EventListener
    {
        public const EventKeywords TasksFlowActivityIds = (EventKeywords)0x80;
        //public const EventKeywords Debug = (EventKeywords)0x20000;

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "System.Net.Http" ||
                eventSource.Name == "System.Net.Sockets" ||
                eventSource.Name == "System.Net.Security" ||
                eventSource.Name == "System.Net.NameResolution")
            {
                EnableEvents(eventSource, EventLevel.LogAlways);
            }
            else if (eventSource.Name == "System.Threading.Tasks.TplEventSource")
            {
                EnableEvents(eventSource, EventLevel.LogAlways, TasksFlowActivityIds /*| Debug*/);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            var sb = new StringBuilder().Append($"{eventData.TimeStamp:HH:mm:ss.fffffff}[{eventData.EventName}]{eventData.ActivityId}.{eventData.RelatedActivityId} ");
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
