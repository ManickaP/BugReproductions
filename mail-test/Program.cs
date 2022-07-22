using System.Diagnostics;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

using (ISmtpClient smtp = new SmtpClient())
{
    smtp.Connect("***", 465, SecureSocketOptions.SslOnConnect);
    smtp.Authenticate("***", "***");
    var sw = Stopwatch.StartNew();
    for (int i = 0; i < 9; i++)
    {
        using (MimeMessage mm = new MimeMessage())
        {
            mm.From.Add(MailboxAddress.Parse("***"));
            mm.To.Add(MailboxAddress.Parse($"EmailTester{i}@mailinator.com"));
            mm.Subject = $"Email sent at {DateTime.Now.ToString("HH: mm:ss.ff")} ";
            mm.Body = new TextPart($"<h1>Performance Test Email </h1>. The time is now: {DateTime.Now.ToString("HH: mm:ss.ff")}. <br />Thank you for reading this");
            smtp.Send(mm);
        }
        Console.WriteLine(sw.Elapsed);
    }
    Console.WriteLine($"Total for 9 mails {sw.Elapsed}");

    smtp.Disconnect(true);
}
