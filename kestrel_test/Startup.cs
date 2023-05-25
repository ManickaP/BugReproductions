using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace kestrel_test
{
    public class Startup
    {
        private static readonly Random Random = new Random(123456);

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("/postFormUrlEncodedContent", async context =>
                {
                    using var streamReader = new StreamReader(context.Request.Body, Encoding.UTF8);
                    var requestContent = await streamReader.ReadToEndAsync();
                    await context.Response.WriteAsync(requestContent + requestContent, Encoding.UTF8);
                });
                endpoints.MapGet("/sendBytes", async context =>
                {
                    Console.WriteLine($"Query string: {context.Request.QueryString}");
                    var length = Int32.Parse(context.Request.Query["length"]);
                    Console.WriteLine($"Length: {length}");
                    var byteArray = ArrayPool<byte>.Shared.Rent(length);
                    try
                    {
                        Random.NextBytes(byteArray);
                        await context.Response.WriteAsync(Convert.ToBase64String(byteArray));
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(byteArray);
                    }
                });
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World " + context.Request.Protocol.ToString());
                });
                endpoints.MapGet("/sleepFor", async context =>
                {
                    Console.WriteLine($"Query string: {context.Request.QueryString}");
                    var seconds = Int32.Parse(context.Request.Query["seconds"]);
                    Console.WriteLine($"Seconds: {seconds}");
                    await Task.Delay(TimeSpan.FromSeconds(seconds));
                });
            });
        }
    }
}
