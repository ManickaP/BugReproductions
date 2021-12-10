// See https://aka.ms/new-console-template for more information
using System.Diagnostics.Tracing;
using System.Net;
using System.Text;

//using var _ = new HttpEventListener();

var client = new HttpClient()
{
    DefaultRequestVersion = HttpVersion.Version30,
    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
};
var resp = await client.GetAsync("https://quic.westus.cloudapp.azure.com/");
Console.WriteLine($"status: {resp.StatusCode}, version: {resp.Version}");

/*internal sealed class HttpEventListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "Private.InternalDiagnostics.System.Net.Quic" ||
            eventSource.Name == "Private.InternalDiagnostics.System.Net.Http")
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
}*/