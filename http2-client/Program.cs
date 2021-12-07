using System.Net;
using System.Net.Security;

var httpClient = new HttpClient(new SocketsHttpHandler()
{
    SslOptions = new SslClientAuthenticationOptions()
    {
        RemoteCertificateValidationCallback = delegate { return true; }
    }
})
{
    DefaultRequestVersion = HttpVersion.Version20,
    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
    BaseAddress = new Uri("https://localhost:9559")
};

Console.WriteLine(await httpClient.GetAsync("/").ConfigureAwait(false));