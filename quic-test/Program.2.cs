using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

internal partial class Program
{
    private static async Task Main(string[] args)
    {
        //using var quicListener = new QuicEventListener();

        string certificatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "testservereku.contoso.com.pfx");
        X509Certificate2 serverCertificate = new X509Certificate2(File.ReadAllBytes(certificatePath), "testcertificate", X509KeyStorageFlags.Exportable);

        await using var listener = await QuicListener.ListenAsync(new QuicListenerOptions()
        {
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
                    },
                    MaxInboundBidirectionalStreams = 1,
                    MaxInboundUnidirectionalStreams = 1
                };
                return ValueTask.FromResult(serverOptions);
            }
        });

        await using var clientConnection = await QuicConnection.ConnectAsync(new QuicClientConnectionOptions()
        {
            DefaultStreamErrorCode = 54321,
            DefaultCloseErrorCode = 654321,
            ClientAuthenticationOptions = new SslClientAuthenticationOptions()
            {
                ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 },
                RemoteCertificateValidationCallback = delegate { return true; }
            },
            RemoteEndPoint = listener.LocalEndPoint
        });
        await using var serverConnection = await listener.AcceptConnectionAsync();

        await using var localStream = await clientConnection.OpenOutboundStreamAsync(QuicStreamType.Unidirectional);
        await localStream.WriteAsync(new byte[64*1024-1], completeWrites: true);
        //await using var remoteStream = await serverConnection.AcceptInboundStreamAsync();
        //await localStream.ReadAsync(new byte[1]);

        // Stream flow control should prevent a second stream to be opened since the
        // data buffered on remoteStream has not been consumed yet. Today, it doesn't hang.
        // It allows a client to open a new stream and buffer more data on the server.
        while (true)
        {
            await using var localStream2 = await clientConnection.OpenOutboundStreamAsync(QuicStreamType.Unidirectional);
            await localStream2.WriteAsync(new byte[64*1024-1], completeWrites: true);
            GC.Collect();
            Console.WriteLine($"Memory {GC.GetTotalAllocatedBytes()} for PID {Environment.ProcessId}");
            Thread.Sleep(100);
        }
    }
}