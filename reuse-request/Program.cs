// See https://aka.ms/new-console-template for more information
using System.Text;

Console.WriteLine("Hello, World!");

string xmlData = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://learnwebservices.com/services/tempconverter\"><soapenv:Header/><soapenv:Body><tem:FahrenheitToCelsiusRequest><tem:TemperatureInFahrenheit>30</tem:TemperatureInFahrenheit></tem:FahrenheitToCelsiusRequest></soapenv:Body></soapenv:Envelope>";
var request = new HttpRequestMessage(HttpMethod.Post, "https://www.learnwebservices.com/services/tempconverter")
{
    Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(xmlData)))
};
for (int i = 1; i <= 5; i++)
{
    DateTime start = DateTime.Now;

    Console.WriteLine($"Request {i} BEGIN");
    HttpMessageInvoker client = new HttpMessageInvoker(new HttpClientHandler());
    var stream = request.Content.ReadAsStream();
    Console.WriteLine(stream.CanSeek);
    var res = await client.SendAsync(request, CancellationToken.None);
    var xml = await res.Content.ReadAsStringAsync();
    Console.WriteLine($"Request {i} END - {(DateTime.Now - start).TotalSeconds} seconds");
    Console.WriteLine(xml);
}
