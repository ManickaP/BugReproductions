// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
var client = new HttpClient();
Console.WriteLine(client.GetType().Assembly.Location);