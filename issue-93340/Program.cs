// See https://aka.ms/new-console-template for more information
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Text;

using var x = new HttpEventListener();

HttpListener listener = new HttpListener();
string prefix = "http://localhost:1234/";
listener.Prefixes.Add(prefix);
listener.Start();

var credentialPlugin = new CredentialPlugin();

Task httpHandlerTask = HandleHttpRequests(listener);

using HttpClientHandler httpClientHandler = new()
{
    //PreAuthenticate = true,
    Credentials = credentialPlugin
};
using HttpClient httpClient = new(httpClientHandler);

await SendRequest(httpClient, prefix);
await SendRequest(httpClient, prefix + "api1/ABC");
await SendRequest(httpClient, prefix + "api2/123");

credentialPlugin.RefreshToken();

await SendRequest(httpClient, prefix + "api1/DEF");
// Try another time, just in case the cred cache is updated after the first failed attempt
await SendRequest(httpClient, prefix + "api1/DEF");

await SendRequest(httpClient, prefix + "api2/456");

listener.Stop();
await httpHandlerTask;

async Task SendRequest(HttpClient client, string url)
{
    using var response = await httpClient.GetAsync(url);
    Console.WriteLine($"Client: response code {response.StatusCode} from {url}");
}

async Task HandleHttpRequests(HttpListener listener)
{
    while (true)
    {
        try
        {
            var request = await listener.GetContextAsync();
            var authenticationHeader = request.Request.Headers.Get("Authorization");
            if (string.IsNullOrEmpty(authenticationHeader))
            {
                Console.WriteLine("Server: unauthenticated request to " + request.Request.RawUrl);
                request.Response.AddHeader("WWW-Authenticate", "basic");
                request.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                var auth = request.Request.Headers["Authorization"];
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(auth.Replace("Basic", "", StringComparison.OrdinalIgnoreCase)));
                if (decoded == $"{credentialPlugin.UserName}:{credentialPlugin.Password}")
                {
                    Console.WriteLine($"Server: authenticated request ({decoded}) to {request.Request.RawUrl}");
                    request.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    Console.WriteLine($"Server: wrong username/password ({decoded}) request to " + request.Request.RawUrl);
                    request.Response.AddHeader("WWW-Authenticate", "basic");
                    request.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
            }
            request.Response.Close();
        }
        catch
        {
            // this is how http listener "gracefully" stops?
            return;
        }
    }
}

class CredentialPlugin : ICredentials
{
    public CredentialPlugin()
    {
        UserName = "username";
        counter = 0;
        Password = "password";
    }

    private int counter;
    public string UserName {get; private set;}
    public string Password { get;private set;}

    // pretend this comes from an OAuth2 service
    public void RefreshToken()
    {
        counter++;
        Password = "password" + counter;
        Console.WriteLine($"Credential Plugin: Changed to '{UserName}:{Password}'");
    }

    NetworkCredential? ICredentials.GetCredential(Uri uri, string authType)
    {
        Console.WriteLine($"Credential Plugin: returning credential '{UserName}:{Password}'");
        return new NetworkCredential(UserName, Password);
    }
}

internal sealed class HttpEventListener : EventListener
{
    public static string[] NetworkingEvents => new[]
    {
        "System.Net.Http",
        "System.Net.NameResolution",
        "System.Net.Sockets",
        "System.Net.Security",
        "System.Net.TestLogging",
        "Private.InternalDiagnostics.System.Net.Http",
        "Private.InternalDiagnostics.System.Net.NameResolution",
        "Private.InternalDiagnostics.System.Net.Sockets",
        "Private.InternalDiagnostics.System.Net.Security",
        "Private.InternalDiagnostics.System.Net.Quic",
        "Private.InternalDiagnostics.System.Net.Http.WinHttpHandler",
        "Private.InternalDiagnostics.System.Net.HttpListener",
        "Private.InternalDiagnostics.System.Net.Mail",
        "Private.InternalDiagnostics.System.Net.NetworkInformation",
        "Private.InternalDiagnostics.System.Net.Primitives",
        "Private.InternalDiagnostics.System.Net.Requests",
    };

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (NetworkingEvents.Contains(eventSource.Name))
            EnableEvents(eventSource, EventLevel.LogAlways);
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        var sb = new StringBuilder().Append($"{eventData.TimeStamp:HH:mm:ss.fffffff}[{eventData.EventName}] ");
        for (int i = 0; i < eventData.Payload?.Count; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(eventData.PayloadNames?[i]).Append(": ").Append(eventData.Payload[i]);
        }
        Console.WriteLine(sb.ToString());
    }
}