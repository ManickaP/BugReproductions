// See https://aka.ms/new-console-template for more information
using System.Net.Quic;
using System.Net.Security;
using System.Net;

Console.WriteLine("Hello, World!");

using var cts = new CancellationTokenSource(10_000);

await using var connection = await QuicConnection.ConnectAsync(new QuicClientConnectionOptions {
    RemoteEndPoint = new DnsEndPoint("unfiltered.adguard-dns.com", 853),
    DefaultStreamErrorCode = 0,
    DefaultCloseErrorCode = 0,
    ClientAuthenticationOptions = new SslClientAuthenticationOptions {
        ApplicationProtocols = [new SslApplicationProtocol("doq")]
    }
}, cts.Token);

Console.WriteLine("Connected.");