using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var client = new HttpClient(new SocketsHttpHandler()
{
    SslOptions = new SslClientAuthenticationOptions()
    {
        RemoteCertificateValidationCallback = delegate { return true; }
    }
})
{
    DefaultRequestVersion = HttpVersion.Version30,
    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
};

using var response = await client.GetAsync("https://localhost:5000/stream", HttpCompletionOption.ResponseHeadersRead);
using var stream = response.Content.ReadAsStream();
while (true)
{
    await Task.Delay(100);
    if (Random.Shared.Next(3) == 0)
    {
        var buff = new byte[64*1024];
        var length = await stream.ReadAsync(buff);
        if (length == 0)
        {
            break;
        }
        Console.WriteLine(Convert.ToHexString(buff, 0, length));
    }
    var b = stream.ReadByte();
    if (b == -1)
    {
        break;
    }
    else
    {
        Console.WriteLine($"0x{b:X2}");
    }
}

/*Console.WriteLine("\r\nExists Certs Name and Location");
Console.WriteLine("------ ----- -------------------------");

foreach (StoreLocation storeLocation in (StoreLocation[])Enum.GetValues(typeof(StoreLocation)))
{
    foreach (StoreName storeName in (StoreName[])Enum.GetValues(typeof(StoreName)))
    {
        X509Store store = new X509Store(storeName, storeLocation);

        try
        {
            store.Open(OpenFlags.OpenExistingOnly);

            Console.WriteLine("Yes    {0,4}  {1}, {2}", store.Certificates.Count, store.Name, store.Location);


            foreach (var cert in store.Certificates)
            {
                Console.WriteLine("Cert         {0}, {1}", cert.FriendlyName, cert.SubjectName.Name);
            }
        }
        catch (CryptographicException)
        {
            Console.WriteLine("No           {0}, {1}", store.Name, store.Location);
        }
    }
    Console.WriteLine();
}*/

/*//inputstring
var queryString = "_return_fields%2b=extattrs&name%3a=somename.somedomain.local";
Console.WriteLine($"Query string input: {queryString}");
//parse
var nameValues = HttpUtility.ParseQueryString(queryString);

//show paring decodes the name part
foreach (var key in nameValues.AllKeys)
{
    Console.WriteLine($"Key: {key} => Value: {nameValues[key]}");
}

//call tostring to make the namevalues to query string

Console.WriteLine($"Query string output: {nameValues.ToString()}");*/

/*FtpWebRequest myWebRequest = (FtpWebRequest)WebRequest.Create("ftp://ftp.dlptest.com/\r \nDELE test");
myWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;
myWebRequest.UseBinary = true;
myWebRequest.Credentials = new NetworkCredential("dlpuser", "rNrKYTX9g7z3RgJRmxWuGHbeu");

FtpWebResponse ftpWebResponse = (FtpWebResponse)myWebRequest.GetResponse();
StreamReader streamReader = new StreamReader(ftpWebResponse.GetResponseStream());
Console.WriteLine(streamReader.ReadToEnd());

streamReader.Close();
ftpWebResponse.Close();
var uri = new Uri("ftp://ftp.dlptest.com/\r \nDELE test");*/

/*var x = new X()
{
    Y = new Y()
    {
        Value = 6
    }
};
Console.WriteLine(x.Y.Value);
x.Y.Value = 7;
Console.WriteLine(x.Y.Value);
ref Y y = ref x.Y;
y.Value = 10;
Console.WriteLine(x.Y.Value);
x.Y = y;
Console.WriteLine(x.Y.Value);
x.Y = new Y() {
    Value = x.Y.Value + 1
};
Console.WriteLine(x.Y.Value);
x.Y = x.Y with { Value = x.Y.Value + 1 };
Console.WriteLine(x.Y.Value);

public class X
{
    private Y y;
    public ref Y Y { get { return ref y;} }
}
public struct Y
{
    public int Value { get; set; } = 5;
    public Y()
    {}
}*/

/*//var x = await client.PostAsync("http://localhost:5000", new MyContent());
//var x = await client.GetAsync("http://localhost:5000/");
//var str = await x.Content.ReadAsStringAsync();

var client = new HttpClient();
HttpResponseMessage x = await client.GetAsync("http://localhost:5000/sendBytes?length=4097", HttpCompletionOption.ResponseHeadersRead);
Console.WriteLine(x);
var stream = await x.Content.ReadAsStreamAsync();
Console.WriteLine(stream.GetType());

stream = await client.GetStreamAsync("http://localhost:5000/sendBytes?length=4097");
Console.WriteLine(stream.GetType());

var invoker = new HttpMessageInvoker(new HttpClientHandler());
x = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:5000/sendBytes?length=4097"), default);
Console.WriteLine(x);
stream = await x.Content.ReadAsStreamAsync();
Console.WriteLine(stream.GetType());


class MyContent : HttpContent
{
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        await stream.WriteAsync(new byte[4097]);
        await Task.Delay(5_000);
        throw new Exception("Manicka");
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 4097;
        return true;
    }
}*/

/*unsafe {
    void* pointer = NativeMemory.AllocZeroed(5);
    Console.WriteLine("p :" + (nint)pointer);
    void* original = (void*)Interlocked.Exchange(ref *(nint*)&pointer, 0);
    Console.WriteLine("o1:" + (nint)original);
    Console.WriteLine("p1:" + (nint)pointer);
    original = (void*)Interlocked.Exchange(ref *(nint*)&pointer, IntPtr.Zero);
    Console.WriteLine("o2:" + (nint)original);
    Console.WriteLine("p2:" + (nint)pointer);
}*/

/*var request = (HttpWebRequest)WebRequest.Create("https://httpbin.org/post"); // exact URL is irrelevant because request never gets that far

request.AllowWriteStreamBuffering = false;
request.Method = "POST";

using (var requestStream = request.GetRequestStream())
{
    byte[] buffer = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");

    for (int megabyte = 0; megabyte < 2; megabyte++)
        requestStream.Write(buffer, 0, buffer.Length);
}
Console.WriteLine(new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd());*/
/*
Console.WriteLine(await new HttpClient().GetAsync("https://httpstat.us/400"));

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
    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/javascript, text/html, application/xml, text/xml");
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
*/