using System.Diagnostics.Tracing;
using System.Text;

internal sealed class HttpEventListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "Private.InternalDiagnostics.System.Net.Http" ||
            eventSource.Name == "Private.InternalDiagnostics.System.Net.Quic" ||
            eventSource.Name == "Private.InternalDiagnostics.System.Net.Primitives")
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