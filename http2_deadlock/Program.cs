using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace http2_deadlock
{
    class Program
    {
        static Random random = new Random();
        static async Task Main(string[] args)
        {
            Console.WriteLine(Environment.ProcessId);

            var server = new Http2LoopbackServer();
            var tcs = default(TaskCompletionSource<Http2LoopbackConnection>);
            
            async Task<Http2LoopbackConnection> Accept()
            {
                tcs = new TaskCompletionSource<Http2LoopbackConnection>();
                var connection = await server.AcceptConnectionAsync();
                await connection.ReadAndSendSettingsAsync(TimeSpan.FromSeconds(10), new SettingsEntry()
                {
                    SettingId = SettingId.InitialWindowSize,
                    Value = Int32.MaxValue
                }).ConfigureAwait(false);
                await connection.WriteFrameAsync(new WindowUpdateFrame(Int32.MaxValue / 2, 0));
                tcs.SetResult(connection);
                return connection;
            }

            var t = Task.Run(async () => {
                var connection = await Accept();
                while (true)
                {
                    try
                    {
                        await connection.HandleRequestAsync();
                    }
                    catch (Exception ex) when (ex.Message == "Got RST")
                    {
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Server: " + ex);
                        //connection = await Accept();
                    }
                }
            });
            
            var t2 = Task.Run(async () => {
                SpinWait.SpinUntil(() => tcs != null);
                while (true) {
                    var connection = await tcs.Task;
                    await Task.Delay(random.Next(1000, 5000));
                    try
                    {
                        await connection.SendSettingsAsync(TimeSpan.FromSeconds(10), new SettingsEntry[] 
                        {
                            new SettingsEntry()
                            {
                                SettingId = SettingId.InitialWindowSize,
                                Value = Int32.MaxValue
                            }
                        }, false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Settings: " + ex);
                        //connection = await Accept();
                    }
                }
            });
            await Task.WhenAll(t, t2, SendRequests());
        }

        static async Task SendRequests()
        {
            using var client = new HttpClient()
            {
                DefaultRequestVersion = HttpVersion.Version20,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
                Timeout = Timeout.InfiniteTimeSpan
            };

            byte[] buffer = new byte[1 << 5];
            random.NextBytes(buffer);

            while (true)
            {
                try {
                    int sent = 0;
                    await client.PostAsync("http://localhost:5001", new StreamContent(new DelegateStream(canReadFunc: () => true, readFunc: (array, offset, count) => {
                        if (random.Next(1000) < 2)
                        {
                            throw new Exception("Pingu");
                        }
                        var copy = Math.Min(buffer.Length - sent, count);
                        if (copy == 0)
                        {
                            return 0;
                        }
                        Buffer.BlockCopy(buffer, sent, array, offset, copy);
                        sent += copy;
                        return copy;
                    })));
                }
                catch (Exception ex) when (ex.Message == "Pingu")
                {
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }
    }
}
