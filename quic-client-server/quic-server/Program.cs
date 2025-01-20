using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

// First, check if QUIC is supported.
if (!QuicListener.IsSupported)
{
    Console.WriteLine("QUIC is not supported, check for presence of libmsquic and support of TLS 1.3.");
    return;
}
else
{
    Console.WriteLine("QUIC is supported.");
}

// Define the server certificate.
string certificatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "testservereku.contoso.com.pfx");
X509Certificate2 serverCertificate = new X509Certificate2(File.ReadAllBytes(certificatePath), "testcertificate", X509KeyStorageFlags.Exportable);

// We want the same configuration for each incoming connection, so we prepare the connection option upfront and reuse them.
// This represents the minimal configuration necessary to accept a connection.
var serverConnectionOptions = new QuicServerConnectionOptions()
{
    MaxInboundUnidirectionalStreams = 10,

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

bool isRunning = true;

async ValueTask RecycleServer()
{
    while (Volatile.Read(ref isRunning))
    {
        GC.Collect();
        // Initialize, configure the listener and start listening.
        await using var listener = await QuicListener.ListenAsync(new QuicListenerOptions()
        {
            // Listening endpoint, port 0 means any port.
            ListenEndPoint = new IPEndPoint(IPAddress.Loopback, 5000),
            // List of all supported application protocols by this listener.
            ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 },
            // Callback to provide options for the incoming connections, it gets once called per each of them.
            ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(serverConnectionOptions)
        });
        await using var serverConnection = await listener.AcceptConnectionAsync();
        await using var stream = await serverConnection.AcceptInboundStreamAsync();
        var buffer = new byte[1024];
        do
        {
            var count = await stream.ReadAsync(buffer);
            //Console.WriteLine($"Read {count} bytes from {stream}: {Encoding.UTF8.GetString(buffer, 0, count)}");
            if (count == 0)
                break;
        } while (true);
    }
}

var serverTask = RecycleServer();

Console.WriteLine($"Press any key to stop the server and clients {Environment.ProcessId}.");
Console.ReadKey();
isRunning = false;
await serverTask;