using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

using var testListener = new HttpEventListener();

string certificatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "testservereku.contoso.com.pfx");
X509Certificate2 serverCertificate = new X509Certificate2(File.ReadAllBytes(certificatePath), "testcertificate", X509KeyStorageFlags.Exportable);
IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 5001);

int i = 0;
while (false) {
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

static Task<HttpResponseMessage> HttpReq(CookieContainer cookieContainer)
{
    string url = "https://arduino.ua/";

    // Now create a client handler which uses that proxy
    var httpClientHandler = new HttpClientHandler
    {
        CookieContainer = cookieContainer,
        UseCookies = true
    };

    httpClientHandler.AllowAutoRedirect = true;

    var client = new HttpClient(
        handler: httpClientHandler,
        disposeHandler: true
        );


    client.Timeout = TimeSpan.FromSeconds(3000);
    // add default headers
    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/javascript, text/html, application/xml, text/xml, */*");
    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0");
    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "uk-UA,uk;q=0.8,en-US;q=0.5,en;q=0.3");


    var response = client.GetAsync(url);
    return response;

}

var cookieContainer = new CookieContainer();

Uri uri = new Uri("https://arduino.ua");
System.DateTime today = System.DateTime.Now;
System.DateTime Expires = today.AddYears(1);

Cookie cookie = new Cookie();
cookie.Expires = Expires;
cookie.HttpOnly = false;
cookie.Value = "nnnnn";
cookie.Name = "FBeID";
cookie.Domain = "arduino.ua";

cookieContainer.Add(uri, cookie);

var response = HttpReq(cookieContainer);
response.Result.Content.ReadAsStringAsync().Wait();

IEnumerable<Cookie> responseCookies = cookieContainer.GetCookies(uri).Cast<Cookie>();
foreach (Cookie new_cookie in responseCookies)
{
    Console.WriteLine(new_cookie.Name + "=" + new_cookie.Value + "; " + new_cookie.Domain + "; " + new_cookie.Path);
}