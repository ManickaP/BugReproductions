// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
cts.Token.Register(() => throw new Exception("Kill!"));

while (true) {
    await Task.Delay(TimeSpan.FromSeconds(1));
    Console.WriteLine("Poop");
}