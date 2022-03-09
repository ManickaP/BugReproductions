using System;
using System.Net.Http;
using System.Threading.Tasks;

Console.WriteLine("Hello World!");

var retryPolicy = HttpPolicyExtensions
  .HandleTransientHttpError()
  .OrResult(message => !message.IsSuccessStatusCode)
  .RetryAsync(3);

using var client = new HttpClient();

while (true) {
    await Task.Delay(1000);
    try {
        Console.WriteLine(await client.GetAsync("http://localhost:5001"));
    } catch (Exception ex) {
        Console.WriteLine(ex);
    }
}