

using System.Diagnostics.Tracing;
using System.Text;

internal sealed class HttpEventListener : EventListener
{
    public const EventKeywords TasksFlowActivityIds = (EventKeywords)0x80;
    public const EventKeywords Debug = (EventKeywords)0x20000;

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "Private.InternalDiagnostics.System.Net.Http" ||
            eventSource.Name == "System.Net.Http" ||
            eventSource.Name == "System.Net.Sockets" ||
            eventSource.Name == "System.Net.Security" ||
            eventSource.Name == "System.Net.NameResolution")
        {
            EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All);
        }
        else if (eventSource.Name == "System.Threading.Tasks.TplEventSource")
        {
            // Attach ActivityId to the events.
            EnableEvents(eventSource, EventLevel.LogAlways, TasksFlowActivityIds);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        // It's a counter, parse the data properly.
        if (eventData.EventId == -1)
        {
            var sb = new StringBuilder().Append($"{eventData.TimeStamp:HH:mm:ss.fffffff}  {eventData.EventSource.Name}  ");
            var counterPayload = (IDictionary<string, object>)(eventData.Payload![0]!);
            bool appendSeparator = false;
            foreach (var counterData in counterPayload)
            {
                if (appendSeparator)
                {
                    sb.Append(", ");
                }
                sb.Append(counterData.Key).Append(": ").Append(counterData.Value);
                appendSeparator = true;
            }
            Console.WriteLine(sb.ToString());
        }
        else
        {
            var sb = new StringBuilder().Append($"{eventData.TimeStamp:HH:mm:ss.fffffff}  {eventData.ActivityId}.{eventData.RelatedActivityId}  {eventData.EventSource.Name}.{eventData.EventName}(");
            for (int i = 0; i < eventData.Payload?.Count; i++)
            {
                sb.Append(eventData.PayloadNames?[i]).Append(": ").Append(eventData.Payload[i]);
                if (i < eventData.Payload?.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(")");
            Console.WriteLine(sb.ToString());
        }
    }
}