// See https://aka.ms/new-console-template for more information
using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Security;
using System.Text;

//using var _ = new HttpEventListener();

var client = new HttpClient(new SocketsHttpHandler()
{
    SslOptions = new SslClientAuthenticationOptions()
    {
        RemoteCertificateValidationCallback = delegate { return true; }
    }
})
{
    DefaultRequestVersion = HttpVersion.Version30,
    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
};
var resp = await client.GetAsync("https://localhost:5001");//("https://localhost:5001/sendBytes?length=5");

//Console.WriteLine(resp);

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