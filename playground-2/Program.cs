using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

string certificatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "testservereku.contoso.com.pfx");
X509Certificate2 serverCertificate = new X509Certificate2(File.ReadAllBytes(certificatePath), "testcertificate", X509KeyStorageFlags.Exportable);
IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 5001);

int i = 0;
while (true) {
    var lt = QuicListener.ListenAsync(new QuicListenerOptions() {
                ListenEndPoint = endpoint,
                ApplicationProtocols = new List<SslApplicationProtocol>() { new SslApplicationProtocol("h3") },
                ConnectionOptionsCallback = (_, _, _) => {
                    return ValueTask.FromResult(new QuicServerConnectionOptions() {
                        DefaultStreamErrorCode = 456,
                        DefaultCloseErrorCode = 123,
                        ServerAuthenticationOptions = new SslServerAuthenticationOptions() {
                            ApplicationProtocols = new List<SslApplicationProtocol>() { new SslApplicationProtocol("h3") },
                            ServerCertificate = serverCertificate
                        }
                    });
                }
            }).AsTask();
    var ct = QuicConnection.ConnectAsync(new QuicClientConnectionOptions() {
                DefaultStreamErrorCode = 54321,
                DefaultCloseErrorCode = 654321,
                ClientAuthenticationOptions = new SslClientAuthenticationOptions() {
                    ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 },
                    RemoteCertificateValidationCallback = delegate { return true; }
                },
                RemoteEndPoint = endpoint
            }).AsTask();
    await Task.WhenAll(lt, ct);
    await using var listener = lt.Result;
    await using var connection = ct.Result;
    await listener.AcceptConnectionAsync();
    if (++i % 100 == 0)
        Console.WriteLine(i);
}