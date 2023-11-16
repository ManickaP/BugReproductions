// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Mime;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

Console.WriteLine("Hello, World!");

SocketsHttpHandler handler = new SocketsHttpHandler();
handler.SslOptions.RemoteCertificateValidationCallback = delegate { return true; };
handler.Proxy = new WebProxy("https://localhost:8080");
HttpClient client = new HttpClient(handler);

await client.GetAsync("https://httpbin.org/");

X509Certificate2 cert = null!;
HashSet<string> blockedServers = new HashSet<string>()
{
    "localhost"
};

await using var listener = await QuicListener.ListenAsync(new QuicListenerOptions
{
    ApplicationProtocols = new List<SslApplicationProtocol>
    {
        new SslApplicationProtocol("test")
    },
    ListenEndPoint = IPEndPoint.Parse("127.0.0.1:19999"),
    ConnectionOptionsCallback = (con, hello, token) =>
    {
        if (blockedServers.Contains(hello.ServerName))
        {
            throw new ArgumentException($"Connection attempt from forbidden server: '{hello.ServerName}'.", nameof(hello));
        }
        
        return ValueTask.FromResult(new QuicServerConnectionOptions
        {
            DefaultStreamErrorCode = 123456,
            DefaultCloseErrorCode = 654321,
            ServerAuthenticationOptions = new SslServerAuthenticationOptions
            {
                ApplicationProtocols = new List<SslApplicationProtocol>
                {
                    new SslApplicationProtocol("test")
                },
                ServerCertificate = cert,
                RemoteCertificateValidationCallback = (sender, chain, certificate, errors) => true
            },
        });
    },
});

try
{
    await using var connection = await listener.AcceptConnectionAsync();
}
catch (QuicException ex) when (ex is { QuicError: QuicError.CallbackError, InnerException: ArgumentException })
{
    Console.WriteLine($"Blocked connection attempt from forbidden server: {ex.InnerException.Message}");
}