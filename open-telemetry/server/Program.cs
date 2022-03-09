var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(opt => opt.ListenAnyIP(5001));
var app = builder.Build();

int counter = 0;

app.MapGet("/", httpContext => {
    if (Interlocked.Increment(ref counter) % 3 == 0) {
        return Task.FromException(new Exception());
    }
    return Task.FromResult($"Hello World! {counter}");
});

app.Run();
