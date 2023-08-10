using System;
using System.Net;
using System.Net.Quic;
using System.Net.Quic.Implementations;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

// See https://aka.ms/new-console-template for more information
Console.WriteLine(QuicImplementationProviders.MsQuic.IsSupported);
Console.WriteLine(Environment.ProcessId);
Console.WriteLine("Hello, World!");

using SemaphoreSlim serverSem = new SemaphoreSlim(0);
using SemaphoreSlim clientSem = new SemaphoreSlim(0);
var qt = new QuicTest();
await qt.RunClientServer(
    serverFunction: async connection =>
    {
        // Establish stream, send the payload based on the input and synchronize with the peer.
        await using QuicStream stream = await connection.AcceptStreamAsync();
        await stream.WriteAsync(new byte[1024]);
        serverSem.Release();
        await clientSem.WaitAsync();

        Console.WriteLine($"Server read {await ReadAsync(stream)} bytes, read is {(stream.ReadsCompleted ? "close" : "open")}");
        await connection.CloseAsync(124);
        Console.WriteLine($"Server read {await ReadAsync(stream)} bytes, read is {(stream.ReadsCompleted ? "close" : "open")}");

    },
    clientFunction: async connection =>
    {
        // Establish stream, send the payload based on the input and synchronize with the peer.
        await using QuicStream stream = connection.OpenBidirectionalStream();
        await stream.WriteAsync(new byte[1024]);
        clientSem.Release();
        await serverSem.WaitAsync();

        while (!stream.ReadsCompleted)
        {
            await Task.Delay(2_000);
            Console.WriteLine($"Client read {await ReadAsync(stream)} bytes, read is {(stream.ReadsCompleted ? "close" : "open")}");
        }
    });

async ValueTask<int?> ReadAsync(QuicStream stream)
{
    try
    {
        return await stream.ReadAsync(new byte[64]);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"POOOP: {ex}");
    }
    return null;
}

public class QuicTest
{
    private static readonly byte[] s_ping = Encoding.UTF8.GetBytes("PING");
    private static readonly byte[] s_pong = Encoding.UTF8.GetBytes("PONG");

    public static QuicImplementationProvider ImplementationProvider { get; } = QuicImplementationProviders.MsQuic;
    public static bool IsSupported => ImplementationProvider.IsSupported;

    public static SslApplicationProtocol ApplicationProtocol { get; } = new SslApplicationProtocol("quictest");


    public static string certificatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "testservereku.contoso.com.pfx");
    public X509Certificate2 ServerCertificate = new X509Certificate2(File.ReadAllBytes(certificatePath), "testcertificate", X509KeyStorageFlags.Exportable);

    public bool RemoteCertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    public SslServerAuthenticationOptions GetSslServerAuthenticationOptions()
    {
        return new SslServerAuthenticationOptions()
        {
            ApplicationProtocols = new List<SslApplicationProtocol>() { ApplicationProtocol },
            ServerCertificate = ServerCertificate
        };
    }

    public SslClientAuthenticationOptions GetSslClientAuthenticationOptions()
    {
        return new SslClientAuthenticationOptions()
        {
            ApplicationProtocols = new List<SslApplicationProtocol>() { ApplicationProtocol },
            RemoteCertificateValidationCallback = RemoteCertificateValidationCallback,
            TargetHost = "localhost"
        };
    }

    public QuicClientConnectionOptions CreateQuicClientOptions()
    {
        return new QuicClientConnectionOptions()
        {
            ClientAuthenticationOptions = GetSslClientAuthenticationOptions()
        };
    }

    internal QuicConnection CreateQuicConnection(IPEndPoint endpoint)
    {
        return new QuicConnection(ImplementationProvider, endpoint, GetSslClientAuthenticationOptions());
    }

    internal QuicConnection CreateQuicConnection(QuicClientConnectionOptions clientOptions)
    {
        return new QuicConnection(ImplementationProvider, clientOptions);
    }

    internal QuicListenerOptions CreateQuicListenerOptions()
    {
        return new QuicListenerOptions()
        {
            ListenEndPoint = new IPEndPoint(IPAddress.Loopback, 0),
            ServerAuthenticationOptions = GetSslServerAuthenticationOptions()
        };
    }

    internal QuicListener CreateQuicListener(int maxUnidirectionalStreams = 100, int maxBidirectionalStreams = 100)
    {
        var options = CreateQuicListenerOptions();
        options.MaxUnidirectionalStreams = maxUnidirectionalStreams;
        options.MaxBidirectionalStreams = maxBidirectionalStreams;

        return CreateQuicListener(options);
    }

    internal QuicListener CreateQuicListener(IPEndPoint endpoint)
    {
        var options = new QuicListenerOptions()
        {
            ListenEndPoint = endpoint,
            ServerAuthenticationOptions = GetSslServerAuthenticationOptions()
        };
        return CreateQuicListener(options);
    }

    private QuicListener CreateQuicListener(QuicListenerOptions options) => new QuicListener(ImplementationProvider, options);

    internal Task<(QuicConnection, QuicConnection)> CreateConnectedQuicConnection(QuicListener listener) => CreateConnectedQuicConnection(null, listener);
    internal async Task<(QuicConnection, QuicConnection)> CreateConnectedQuicConnection(QuicClientConnectionOptions? clientOptions, QuicListenerOptions listenerOptions)
    {
        using (QuicListener listener = CreateQuicListener(listenerOptions))
        {
            clientOptions ??= new QuicClientConnectionOptions()
            {
                ClientAuthenticationOptions = GetSslClientAuthenticationOptions()
            };
            clientOptions.RemoteEndPoint = listener.ListenEndPoint;
            return await CreateConnectedQuicConnection(clientOptions, listener);
        }
    }

    internal async Task<(QuicConnection, QuicConnection)> CreateConnectedQuicConnection(QuicClientConnectionOptions? clientOptions = null, QuicListener? listener = null)
    {
        int retry = 3;
        int delay = 25;
        bool disposeListener = false;

        if (listener == null)
        {
            listener = CreateQuicListener();
            disposeListener = true;
        }

        clientOptions ??= CreateQuicClientOptions();
        if (clientOptions.RemoteEndPoint == null)
        {
            clientOptions.RemoteEndPoint = listener.ListenEndPoint;
        }

        QuicConnection clientConnection = null;
        ValueTask<QuicConnection> serverTask = listener.AcceptConnectionAsync();
        while (retry > 0)
        {
            clientConnection = CreateQuicConnection(clientOptions);
            retry--;
            try
            {
                await clientConnection.ConnectAsync().ConfigureAwait(false);
                break;
            }
            catch (QuicException ex) when (ex.HResult == (int)SocketError.ConnectionRefused)
            {
                await Task.Delay(delay);
                delay *= 2;

                if (retry == 0)
                {
                    throw ex;
                }
            }
        }

        QuicConnection serverConnection = await serverTask.ConfigureAwait(false);
        if (disposeListener)
        {
            listener.Dispose();
        }

        return (clientConnection, serverTask.Result);
    }

    internal async Task RunClientServer(Func<QuicConnection, Task> clientFunction, Func<QuicConnection, Task> serverFunction, int iterations = 1)
    {
        const long ClientCloseErrorCode = 11111;
        const long ServerCloseErrorCode = 22222;

        using QuicListener listener = CreateQuicListener(CreateQuicListenerOptions());

        using var serverFinished = new SemaphoreSlim(0);
        using var clientFinished = new SemaphoreSlim(0);

        for (int i = 0; i < iterations; ++i)
        {
            (QuicConnection clientConnection, QuicConnection serverConnection) = await CreateConnectedQuicConnection(listener);
            using (clientConnection)
            using (serverConnection)
            {
                await Task.WhenAll(new[]
                {
                    Task.Run(async () =>
                    {
                        await serverFunction(serverConnection);
                        serverFinished.Release();
                        await clientFinished.WaitAsync();
                    }),
                    Task.Run(async () =>
                    {
                        await clientFunction(clientConnection);
                        clientFinished.Release();
                        await serverFinished.WaitAsync();
                    })
                });
                await serverConnection.CloseAsync(ServerCloseErrorCode);
                await clientConnection.CloseAsync(ClientCloseErrorCode);
            }
        }
    }
}