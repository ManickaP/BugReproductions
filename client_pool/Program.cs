// See https://aka.ms/new-console-template for more information
Console.WriteLine(Environment.ProcessId);

using var client = new HttpClient(new SocketsHttpHandler() {
    PooledConnectionIdleTimeout = TimeSpan.FromMilliseconds(1)
});

var list = File.ReadAllLines("top500Domains.csv").Skip(1).Select(line => "https://" + line.Split(',')[1].Trim('"')).ToArray();
int i = 0;
while (true) {
    try {
        await client.GetAsync(list[i++ % list.Length]);
    } catch {
    }
}