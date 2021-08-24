using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

static partial class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine(Environment.ProcessId);
        var server = CreateWebHostBuilder(5001).Build();
        _ = Task.Factory.StartNew(() => server.Start(), TaskCreationOptions.LongRunning);
        var task = MainClient(args);
        Console.WriteLine(Environment.ProcessId);
        await task;
        Console.ReadKey();
    }

    private static IWebHostBuilder CreateWebHostBuilder(int port)
    {
        return WebHost.CreateDefaultBuilder()
            .SuppressStatusMessages(true)
            .ConfigureLogging((context, logging) =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Trace);
            })
            .UseKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = null;
                options.ListenLocalhost(port);
            })
            .UseStartup<Startup>();
    }
}

public sealed class MyController : Controller
{
    [Route("/")]
    public async Task<int> PostAsync()
    {
        Console.WriteLine("PostAsync ");
        using var sr = new StreamReader(Request.Body);
        var bodyText = await sr.ReadToEndAsync().ConfigureAwait(false);

        Console.WriteLine("PostAsync " + bodyText);
        var payload = JsonConvert.DeserializeObject<Payload>(bodyText);
        return payload.Value;
    }
}

public sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvcCore().AddApplicationPart(Assembly.GetExecutingAssembly());
    }

    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
