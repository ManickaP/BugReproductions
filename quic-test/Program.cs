using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using var httpListener = new HttpEventListener();

string certificatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "testservereku.contoso.com.pfx");
X509Certificate2 serverCertificate = new X509Certificate2(File.ReadAllBytes(certificatePath), "testcertificate", X509KeyStorageFlags.Exportable);

var listener = await QuicListener.ListenAsync(new QuicListenerOptions() {
    ListenEndPoint = new IPEndPoint(IPAddress.Loopback, 0),
    ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 },
    ConnectionOptionsCallback = (_, _, _) =>
    {
        var serverOptions = new QuicServerConnectionOptions()
        {
            DefaultStreamErrorCode = 12345,
            IdleTimeout = TimeSpan.FromSeconds(5),
            ServerAuthenticationOptions = new SslServerAuthenticationOptions
            {
                ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 },
                ServerCertificate = serverCertificate
            }
        };
        return ValueTask.FromResult(serverOptions);
    }
});

var connection = await QuicConnection.ConnectAsync(new QuicClientConnectionOptions(){
    DefaultStreamErrorCode = 54321,
    ClientAuthenticationOptions = new SslClientAuthenticationOptions() {
        ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 },
        RemoteCertificateValidationCallback = delegate { return true; }
    },
    RemoteEndPoint = listener.LocalEndPoint
});
var serverConnection = await listener.AcceptConnectionAsync();

var stream = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional);
await stream.WriteAsync(new byte[1]);
var serverStream = await serverConnection.AcceptInboundStreamAsync();

//await connection.CloseAsync(123);
await connection.DisposeAsync();
Thread.Sleep(TimeSpan.FromSeconds(10));

internal sealed class HttpEventListener : EventListener
{

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        // Allow internal Quic logging
        if (eventSource.Name == "Private.InternalDiagnostics.System.Net.Quic")
        {
            EnableEvents(eventSource, EventLevel.LogAlways);
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
        try {
            Console.WriteLine(sb.ToString());
        } catch { }
    }
}