using System.Text;
using Newtonsoft.Json;

static partial class Program
{
    private const string MimeType = "application/json";
    private static readonly HttpClient s_client = new HttpClient() { Timeout = TimeSpan.FromSeconds(150) };

    public static async Task MainClient(string[] args)
    {
        Console.WriteLine(typeof(HttpClient).Assembly.Location);
        int n;
        if (args.Length < 1 || !int.TryParse(args[0], out n))
        {
            n = 2000;
        }

        var sum = 0;
        var api = $"http://localhost:{5001}/";
        var tasks = new List<Task<int>>(n);
        for (var i = 1; i <= n; i++)
        {
            tasks.Add(SendAsync(api, i));
        }
        foreach (var task in tasks)
        {
            Console.WriteLine("A " + sum);
            sum += await task.ConfigureAwait(false);
        }
        Console.WriteLine(sum);
    }

    private static async Task<int> SendAsync(string api, int value)
    {
        var payload = JsonConvert.SerializeObject(new Payload { Value = value });
        while (true)
        {
            try
            {
                //Console.WriteLine("B " + value);
                var content = new StringContent(payload, Encoding.UTF8, MimeType);
                var response = await s_client.PostAsync(api, content).ConfigureAwait(false);

                Console.WriteLine("C " + value);
                return int.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            catch (Exception ex){
                Console.WriteLine(ex);
            }
        }
    }
}

public class Payload
{
    [JsonProperty("value")]
    public int Value { get; set; }
}

