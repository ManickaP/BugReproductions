using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

internal partial class Program
{
    private static async Task Main_(string[] args)
    {
        using var quicListener = new QuicEventListener();

        async Task WorkWithStream(Stream stream)
        {
            Console.WriteLine("Poo");
            await using (stream)
            {
                byte[] buffer = new byte[1024];
                int count = 0;
                int a = 0x010b;
                while ((count = await stream.ReadAsync(buffer)) > 0)
                {
                    await stream.WriteAsync(buffer.AsMemory(0, count));
                }
            }
        }

        string certificatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "testservereku.contoso.com.pfx");
        X509Certificate2 serverCertificate = new X509Certificate2(File.ReadAllBytes(certificatePath), "testcertificate", X509KeyStorageFlags.Exportable);

        bool isRunning = true;

        // First, check if QUIC is supported.
        if (!QuicListener.IsSupported)
        {
            Console.WriteLine("QUIC is not supported, check for presence of libmsquic and support of TLS 1.3.");
            return;
        }

        // We want the same configuration for each incoming connection, so we prepare the connection option upfront and reuse them.
        // This represents the minimal configuration necessary to accept a connection.
        var serverConnectionOptions = new QuicServerConnectionOptions()
        {
            // Used to abort stream if it's not properly closed by the user.
            // See https://www.rfc-editor.org/rfc/rfc9000.html#name-application-protocol-error-
            DefaultStreamErrorCode = 123,

            // Used to close the connection if it's not done by the user.
            // See https://www.rfc-editor.org/rfc/rfc9000.html#name-application-protocol-error-
            DefaultCloseErrorCode = 456,

            // Same options as for server side SslStream.
            ServerAuthenticationOptions = new SslServerAuthenticationOptions
            {
                // List of supported application protocols, must be the same or subset of QuicListenerOptions.ApplicationProtocols.
                ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 },
                // Server certificate, it can also be provided via ServerCertificateContext or ServerCertificateSelectionCallback.
                ServerCertificate = serverCertificate
            }
        };

        // Initialize, configure the listener and start listening.
        await using var listener = await QuicListener.ListenAsync(new QuicListenerOptions()
        {
            // Listening endpoint, port 0 means any port.
            ListenEndPoint = new IPEndPoint(IPAddress.Loopback, 0),
            // List of all supported application protocols by this listener.
            ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 },
            // Callback to provide options for the incoming connections, it gets once called per each of them.
            ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(serverConnectionOptions)
        });

        async ValueTask RunServer()
        {
            // Accept and process the connections.
            //while (isRunning)
            {
                // Accept will propagate any exceptions that occurred during the connection establishment,
                // including exceptions thrown from ConnectionOptionsCallback, caused by invalid QuicServerConnectionOptions or TLS handshake failures.
                await using var serverConnection = await listener.AcceptConnectionAsync();
                using var stream = await serverConnection.AcceptInboundStreamAsync();
                while (true)
                {
                    try
                    {
                        await stream.WriteAsync(new byte[10]);
                        await Task.Delay(100);
                    }
                    catch (QuicException ex) when (ex.QuicError == QuicError.StreamAborted)
                    {
                        Console.WriteLine($"Stream {stream} aborted: {ex.Message}");
                        break;
                    }
                }
            }
        }
        var serverTask = RunServer();

        var clientConnectionOptions = new QuicClientConnectionOptions()
        {
            RemoteEndPoint = listener.LocalEndPoint,
            DefaultStreamErrorCode = 321,
            DefaultCloseErrorCode = 654,
            MaxInboundBidirectionalStreams = 1,
            ClientAuthenticationOptions = new SslClientAuthenticationOptions()
            {
                ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 },
                RemoteCertificateValidationCallback = delegate { return true; }
            },
        };

        var connection = await QuicConnection.ConnectAsync(clientConnectionOptions);
        //var serverConnection = await listener.AcceptConnectionAsync();
        Console.WriteLine($"Connected {connection.LocalEndPoint} --> {connection.RemoteEndPoint}");
        var outgoingStream = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional);
        await outgoingStream.WriteAsync(new byte[1]);

        var buffer = new byte[100];
        await outgoingStream.ReadAsync(buffer);
        outgoingStream.Dispose();

        //await WorkWithStream(outgoingStream);
        /*while (isRunning)
        {
            var incomingStream = await connection.AcceptInboundStreamAsync();
        }*/
        await connection.CloseAsync(789);
        await connection.DisposeAsync();

        await serverTask;

        await Task.Delay(2000);
    }
}


/*var listener = await QuicListener.ListenAsync(new QuicListenerOptions() {
    ListenEndPoint = new IPEndPoint(IPAddress.Loopback, 0),
    ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 },
    ConnectionOptionsCallback = (_, _, _) =>
    {
        var serverOptions = new QuicServerConnectionOptions()
        {
            DefaultStreamErrorCode = 12345,
            DefaultCloseErrorCode = 123456,
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
    DefaultCloseErrorCode = 654321,
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
Thread.Sleep(TimeSpan.FromSeconds(10));*/

internal sealed class QuicEventListener : EventListener
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
        var sb = new StringBuilder().Append($"{eventData.TimeStamp:HH:mm:ss.fffffff}[{eventData.EventName}] ");
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