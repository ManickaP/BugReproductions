using System;
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
        
        await client.GetAsync(@"https://github.com/dotnet/runtime/blob/master/src/libraries/System.Net.Http/src/HttpDiagnosticsGuide.md");
    }
}

internal sealed class HttpEventListener : EventListener
{
    public const EventKeywords TasksFlowActivityIds = (EventKeywords)0x80;
    //public const EventKeywords Debug = (EventKeywords)0x20000;

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "System.Net.Http")
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
        var sb = new StringBuilder().Append($"{eventData.TimeStamp:HH:mm:ss.fffffff}[{eventData.EventName}]{eventData.ActivityId} ");
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
