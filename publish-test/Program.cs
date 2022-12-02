// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

using var client = new HttpClient()
{
    BaseAddress = new Uri("https://httpbin.org")
};
var sw = Stopwatch.StartNew();
var str = await client.GetStringAsync("get");
Console.WriteLine($"Hello, World! {sw.Elapsed} @ {DateTime.Now:mm:ss.fffffff}");
Console.WriteLine(str);