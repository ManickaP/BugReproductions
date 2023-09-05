// See https://aka.ms/new-console-template for more information
using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Mail;
using System.Text;

Console.WriteLine("Hello, World!");

using var nel = new NetEventListener();

using var mailMessage = new MailMessage();
mailMessage.From = new MailAddress("systems@xxxxxxx.com.cn", "FromName");
mailMessage.To.Add("receiver@xxxxxx.com.cn");
mailMessage.Subject = "Test Subject";
mailMessage.Body = "Test Body";

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
using var smtpClient = new SmtpClient("smtp.office365.com", 587);
smtpClient.Credentials = new NetworkCredential("systems@xxxxxx.com.cn", "Password");
smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
smtpClient.EnableSsl = true;
smtpClient.Timeout = 5000;
smtpClient.SendCompleted += (sender, e) =>
{
    Console.WriteLine("Complete");
    Console.WriteLine(e.Error?.Message ?? string.Empty);
};
Console.WriteLine("Start");
await smtpClient.SendMailAsync(mailMessage);
Console.WriteLine("End");

internal sealed class NetEventListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name.StartsWith("Private.InternalDiagnostics.System.Net") ||
            eventSource.Name.StartsWith("System.Net"))
            EnableEvents(eventSource, EventLevel.LogAlways);
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        var sb = new StringBuilder().Append($"{eventData.TimeStamp:HH:mm:ss.fffffff}[{eventData.EventName}] ");
        for (int i = 0; i < eventData.Payload?.Count; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(eventData.PayloadNames?[i]).Append(": ").Append(eventData.Payload[i]);
        }
        Console.WriteLine(sb.ToString());
    }
}