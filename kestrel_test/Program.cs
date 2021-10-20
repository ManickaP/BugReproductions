using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace kestrel_test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var listener = new HttpEventListener();
            CreateHostBuilder(args).Build().Run();
            listener.Dispose();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseKestrel(kestrel =>
                    {
                        kestrel.ListenAnyIP(5001, listenOptions =>
                        {
                            using (RSA rsa = RSA.Create())
                            {
                                var certReq = new CertificateRequest("CN=contoso.com", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                                certReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
                                certReq.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));
                                certReq.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
                                X509Certificate2 cert = certReq.CreateSelfSigned(DateTimeOffset.UtcNow.AddMonths(-1), DateTimeOffset.UtcNow.AddMonths(1));
                                if (OperatingSystem.IsWindows())
                                {
                                    cert = new X509Certificate2(cert.Export(X509ContentType.Pfx));
                                }
                                listenOptions.UseHttps(cert);
                            }
                            listenOptions.Protocols = HttpProtocols.Http3;
                        });
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Trace);
                });
    }



    internal sealed class HttpEventListener : EventListener
    {

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "Private.InternalDiagnostics.System.Net.Quic")
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
}
