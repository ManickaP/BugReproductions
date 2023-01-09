using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore;
using System.Diagnostics.Tracing;
using System.Text;

namespace issue_55193
{
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public class Program
    {
        public static async Task Main()
        {
            var _ = new HttpEventListener();
            string GetAssemblyInfo(Assembly assembly) => $"{assembly.Location}, modified {new FileInfo(assembly.Location).LastWriteTime}";

            Console.WriteLine("       .NET Core: " + GetAssemblyInfo(typeof(object).Assembly));
            Console.WriteLine("    ASP.NET Core: " + GetAssemblyInfo(typeof(WebHost).Assembly));
            Console.WriteLine(" System.Net.Http: " + GetAssemblyInfo(typeof(System.Net.Http.HttpClient).Assembly));

            // Arrange
            var builder = GetHostBuilder()
                .ConfigureWebHost(webHostBuilder =>
                {
                    webHostBuilder
                        .UseKestrel(o =>
                        {
                            o.Listen(IPAddress.Parse("127.0.0.1"), 5005, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http3;
                                listenOptions.UseHttps();
                            });
                        })
                        .Configure(app =>
                        {
                            app.Run(async context =>
                            {
                                await context.Response.WriteAsync("hello, world");
                            });
                        });
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Trace);
                });

            using var host = builder.Build();
            await host.StartAsync();

            await CallHttp3AndHttp1EndpointsAsync(http3Port: 5005, http1Port: 5005);

            await host.StopAsync();
        }

        private static async Task CallHttp3AndHttp1EndpointsAsync(int http3Port, int http1Port)
        {
            var ch = new HttpClientHandler();
            ch.ServerCertificateCustomValidationCallback = (_,_,_,_) =>
            {
                Console.WriteLine("Poop");
                return true;
            };
            // HTTP/3
            using (var client = new HttpClient(ch))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://127.0.0.1:{http3Port}/");
                request.Version = HttpVersion.Version30;
                request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.EnsureSuccessStatusCode();
                Debug.Assert(HttpVersion.Version30 == response.Version);
                var responseText = await response.Content.ReadAsStringAsync();
                Debug.Assert("hello, world" == responseText);
            }

            /*// HTTP/1.1
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using (var client = new HttpClient(httpClientHandler))
            {
                // HTTP/1.1
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://127.0.0.1:{http1Port}/");

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.EnsureSuccessStatusCode();
                Debug.Assert(HttpVersion.Version11 == response.Version);
                var responseText = await response.Content.ReadAsStringAsync();
                Debug.Assert("hello, world" == responseText);
            }*/
        }

        public static IHostBuilder GetHostBuilder(long? maxReadBufferSize = null)
        {
            return new HostBuilder()
                .ConfigureWebHost(webHostBuilder =>
                {
                    webHostBuilder
                        .UseQuic(options =>
                        {
                            options.MaxReadBufferSize = maxReadBufferSize;
                            //options.Alpn = "h3-29";
                        });
                });
        }
    }



    internal sealed class HttpEventListener : EventListener
    {

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "Private.InternalDiagnostics.System.Net.Http" ||
                eventSource.Name == "Private.InternalDiagnostics.System.Net.Quic")
                EnableEvents(eventSource, EventLevel.LogAlways);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            var sb = new StringBuilder().Append($"{eventData.TimeStamp:HH:mm:ss.fffffff}[{eventData.EventSource.Name}][{eventData.EventName}] ");
            for (int i = 0; i < eventData.Payload?.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(eventData.PayloadNames?[i]).Append(": ").Append(eventData.Payload[i]);
            }
            //Console.WriteLine(sb.ToString());
        }
    }

}
