// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Mime;
using System.Net.Quic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

Console.WriteLine("Hello, World!");


Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
byte[] message = Encoding.UTF8.GetBytes("Hello world!");
byte[] buffer = new byte[1024];
IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 12346);
//SocketAddress targetAddress = endpoint.Serialize();
SocketAddress receivedAddress = endpoint.Serialize();
SocketAddress address = endpoint.Serialize();


ValueTask<int> receiveTask = server.ReceiveFromAsync(buffer, SocketFlags.None, receivedAddress);
await client.SendToAsync(message, SocketFlags.None, address);
var length = await receiveTask;
Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, length) + " from " + receivedAddress);

Task<SocketReceiveFromResult> receiveTask2 = server.ReceiveFromAsync(buffer, SocketFlags.None, endpoint);
await client.SendToAsync(message, SocketFlags.None, endpoint);
SocketReceiveFromResult result = await receiveTask2;

Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, result.ReceivedBytes) + " from " + result.RemoteEndPoint);


/*IPNetwork ipNet = new IPNetwork(new IPAddress(new byte[] { 127, 0, 0, 0 }), 8);
IPAddress ip1 = new IPAddress(new byte[] { 255, 0, 0, 1 });
IPAddress ip2 = new IPAddress(new byte[] { 127, 0, 0, 10 });
Console.WriteLine($"{ip1} {(ipNet.Contains(ip1) ? "belongs" : "doesn't belong")} to {ipNet}");
Console.WriteLine($"{ip2} {(ipNet.Contains(ip2) ? "belongs" : "doesn't belong")} to {ipNet}");*/

/*
IPNetwork ipNet = IPNetwork.Parse("2a01:110:8012::/96");
IPAddress ip1 = IPAddress.Parse("2a01:110:8012::1742:4244");
IPAddress ip2 = IPAddress.Parse("2a01:110:8012:1010:914e:2451:16ff:ffff");
Console.WriteLine($"{ip1} {(ipNet.Contains(ip1) ? "belongs" : "doesn't belong")} to {ipNet}");
Console.WriteLine($"{ip2} {(ipNet.Contains(ip2) ? "belongs" : "doesn't belong")} to {ipNet}");
*/

using HttpClient httpClient = new HttpClient();

// Handling problems with the server:
try
{
    using HttpResponseMessage response =
        await httpClient.GetAsync("https://testserver", HttpCompletionOption.ResponseHeadersRead);
    using Stream responseStream = await response.Content.ReadAsStreamAsync();
    // Process responseStream ...
}
catch (HttpRequestException e) when (e.HttpRequestError == HttpRequestError.NameResolutionError)
{
    Console.WriteLine($"Unknown host: {e}");
    // --> Try different hostname.
}
catch (HttpRequestException e) when (e.HttpRequestError == HttpRequestError.ConnectionError)
{
    Console.WriteLine($"Server unreachable: {e}");
    // --> Try different server.
}
catch (HttpIOException e) when (e.HttpRequestError == HttpRequestError.InvalidResponse)
{
    Console.WriteLine($"Mangled responses: {e}");
    // --> Block list server.
}

// Handling problems with HTTP version selection:
try
{
    using HttpResponseMessage response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://testserver")
    {
        Version = HttpVersion.Version20,
        VersionPolicy = HttpVersionPolicy.RequestVersionExact
    }, HttpCompletionOption.ResponseHeadersRead);
    using Stream responseStream = await response.Content.ReadAsStreamAsync();
    // Process responseStream ...
}
catch (HttpRequestException e) when (e.HttpRequestError == HttpRequestError.VersionNegotiationError)
{
    Console.WriteLine($"HTTP version is not supported: {e}");
    // Try with different HTTP version.
}

/*SocketsHttpHandler handler = new SocketsHttpHandler();
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
}*/