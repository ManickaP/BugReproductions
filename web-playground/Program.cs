using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//app.MapGet("/", () => "Hello World!");
app.MapGet("/", async context => {
    await context.Response.Body.WriteAsync(Encoding.ASCII.GetBytes("Hello World!"));
});

app.Run();
