// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Net;

using var listener = new HttpEventListener();

Console.WriteLine("Hello, World!");

var client = new HttpClient(new SocketsHttpHandler() {
    ActivityHeadersPropagator= DistributedContextPropagator.CreatePassThroughPropagator()
});
using Activity parent = new Activity("parent");
parent.SetIdFormat(ActivityIdFormat.Hierarchical);
parent.Start();
var resp = await client.GetAsync("https://motherfuckingwebsite.com/");
Console.WriteLine($"status: {resp.StatusCode}, version: {resp.Version}");