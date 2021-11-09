// See https://aka.ms/new-console-template for more information
using System.Net;

using var listener = new HttpEventListener();

Console.WriteLine("Hello, World!");

var client = new HttpClient();
var resp = await client.GetAsync("https://github.com");
Console.WriteLine($"status: {resp.StatusCode}, version: {resp.Version}");

