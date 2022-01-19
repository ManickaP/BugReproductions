using System.Net;
using System.Text;

Console.WriteLine("Hello, World!");

var listener = new HttpListener();
listener.Prefixes.Add("http://*:8080/");
listener.Start();
Console.WriteLine(String.Join(";", listener.Prefixes));
while (true) {
    try {
        var context = await listener.GetContextAsync();
        Console.WriteLine(context.Request);
        await context.Response.OutputStream.WriteAsync(Encoding.ASCII.GetBytes("Hello World!"));
        context.Response.OutputStream.Close();
    } catch (Exception ex) {
        Console.WriteLine(ex);
        break;
    }
}