
// First, check if QUIC is supported.
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Text;

if (!QuicConnection.IsSupported)
{
    Console.WriteLine("QUIC is not supported, check for presence of libmsquic and support of TLS 1.3.");
    return;
}
else
{
    Console.WriteLine("QUIC is supported.");
}

bool isRunning = true;

async ValueTask RunClients()
{
    while (Volatile.Read(ref isRunning))
    {
        var clientConnectionOptions = new QuicClientConnectionOptions()
        {
            RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 5000),
            DefaultStreamErrorCode = 321,
            DefaultCloseErrorCode = 654,
            MaxInboundBidirectionalStreams = 1,
            ClientAuthenticationOptions = new SslClientAuthenticationOptions()
            {
                ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 },
                RemoteCertificateValidationCallback = delegate { return true; }
            }
        };
        try
        {
            await using var connection = await QuicConnection.ConnectAsync(clientConnectionOptions);
            await using var stream = await connection.OpenOutboundStreamAsync(QuicStreamType.Unidirectional);
            await stream.WriteAsync(Encoding.UTF8.GetBytes($"Hello from client stream {stream}"));
        } catch {}
    }
}

var clientTask = RunClients();

Console.WriteLine($"Press any key to stop the server and clients {Environment.ProcessId}.");
Console.ReadKey();
isRunning = false;
await clientTask;