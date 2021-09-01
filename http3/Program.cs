// See https://aka.ms/new-console-template for more information
using System.Net;

Console.WriteLine("Hello, World!");
var client = new HttpClient()
{
    DefaultRequestVersion = HttpVersion.Version30,
    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
};
var resp = await client.GetAsync("https://cloudflare-quic.com/");
Console.WriteLine($"status: {resp.StatusCode}, version: {resp.Version}");
