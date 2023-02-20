using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Tracing;
using System.Text;
using System.Net.Mail;
using System.IO;
using System.Net.Quic;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Management;
using System.Security.Principal;
using System.Runtime.Versioning;

#pragma warning disable CA2252

namespace playground
{
    class Program
    {
        public static async Task Main() {
            using var testListener = new HttpEventListener();

            foreach (var x in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
                Console.WriteLine($"{x} - {x.AddressFamily} {Dns.GetHostName()}");
            }

            string certificatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "testservereku.contoso.com.pfx");
            X509Certificate2 serverCertificate = new X509Certificate2(File.ReadAllBytes(certificatePath), "testcertificate", X509KeyStorageFlags.Exportable);

            var ll = await QuicListener.ListenAsync(new QuicListenerOptions(){
                ListenEndPoint = new IPEndPoint(IPAddress.Loopback, 0),
                ApplicationProtocols = new List<SslApplicationProtocol>() { new SslApplicationProtocol("h3") },
                ConnectionOptionsCallback = (_, _, _) => {
                    Console.WriteLine("a");
                    return ValueTask.FromResult(new QuicServerConnectionOptions() {
                        DefaultStreamErrorCode = 456,
                        DefaultCloseErrorCode = 123,
                        ServerAuthenticationOptions = new SslServerAuthenticationOptions() {
                            ApplicationProtocols = new List<SslApplicationProtocol>() { new SslApplicationProtocol("h3") },
                            ServerCertificate = serverCertificate
                        }
                    });
                }
            });

            var vt = ll.AcceptConnectionAsync();
            Console.WriteLine("b");

            var address = Dns.GetHostEntry(Dns.GetHostName()).AddressList[4];
            Console.WriteLine($"Connecting to {address}");
            var c = await QuicConnection.ConnectAsync(new QuicClientConnectionOptions() {
                RemoteEndPoint = new IPEndPoint(address, ll.LocalEndPoint.Port),//ll.LocalEndPoint,//new DnsEndPoint(Dns.GetHostName(), ll.LocalEndPoint.Port),
                    DefaultStreamErrorCode = 456,
                    DefaultCloseErrorCode = 123,
                    ClientAuthenticationOptions = new SslClientAuthenticationOptions() {
                        RemoteCertificateValidationCallback = (_,_,_,_) => true,
                        ApplicationProtocols = new List<SslApplicationProtocol>() { new SslApplicationProtocol("h3") },
                    }
            });
            Console.WriteLine("c");
            await vt;
            Console.WriteLine("d");
        }
        public static void Main32(string[] args)
        {
            /*string userName = "";
            var mos = new ManagementObjectSearcher("root\\cimv2", "select UserName from Win32_ComputerSystem");
            foreach (ManagementObject mo in mos.Get())
            {
                var x = mo["UserName"].ToString();
                var y = new NTAccount(x.Substring(x.LastIndexOf('\\')+1));
                var z = y.Translate(typeof(SecurityIdentifier));
            }*/
            string path = @"C:\Windows\System32\cmd.exe";
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
            Console.WriteLine(info.FileVersion);
            path = @"C:\Users\mapichov\repositories\cmd.exe";
            info = FileVersionInfo.GetVersionInfo(path);
            Console.WriteLine(info.FileVersion);
            /*_ = Parallel.For(0, 5, r =>
            {
               Console.WriteLine(r);
            });*/
        }
        private static X509Certificate2 GetCertificate() =>
            new X509Certificate2(
                Convert.FromBase64String("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"),
                "testcertificate",
                X509KeyStorageFlags.DefaultKeySet);
        private static NetworkStream _client = null!, _server = null!;
        private static readonly byte[] _buffer = new byte[1];
        private static readonly SslServerAuthenticationOptions _options = new SslServerAuthenticationOptions
        {
            ServerCertificateContext = SslStreamCertificateContext.Create(GetCertificate(), null),
        };
        public static async Task Main31()
        {
            using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            listener.Listen(1);

            //var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Connect(listener.LocalEndPoint);

            //_client = new NetworkStream(socket, ownsSocket: true);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i <= 1_000; ++i)
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(listener.LocalEndPoint!);
                _server = new NetworkStream(listener.Accept(), ownsSocket: true);
                _client = new NetworkStream(socket, ownsSocket: true);

                using var client = new SslStream(_client, leaveInnerStreamOpen: true, delegate { return true; });
                using var server = new SslStream(_server, leaveInnerStreamOpen: true, delegate { return true; });

                await Task.WhenAll(
                    client.AuthenticateAsClientAsync("localhost", null, SslProtocols.None, checkCertificateRevocation: false),
                    server.AuthenticateAsServerAsync(_options));

                await client.WriteAsync(_buffer);
                await server.ReadAsync(_buffer);
                await server.WriteAsync(_buffer);
                await client.ReadAsync(_buffer);

                if (i == 0)
                {
                    sw.Stop();
                    Console.WriteLine($"First {sw.Elapsed}");
                    sw.Restart();
                }
                _client.Dispose();
                _server.Dispose();
            }

            sw.Stop();
            Console.WriteLine($"Avg {sw.Elapsed/1000}");

            _client.Dispose();
            _server.Dispose();
        }
        public static void Main30()
        {
            var serverAuthentication = new NegotiateAuthentication(new NegotiateAuthenticationServerOptions { });
            var clientAuthentication = new NegotiateAuthentication(
                new NegotiateAuthenticationClientOptions
                {
                    Package = "Negotiate",
                    Credential = CredentialCache.DefaultNetworkCredentials,
                    TargetName = "HTTP/localhost",
                    RequiredProtectionLevel = ProtectionLevel.Sign
                });

            string? serverBlob = null;
            while (!clientAuthentication.IsAuthenticated)
            {
                // Client produces the authentication challenge, or response to server's challenge
                string? clientBlob = clientAuthentication.GetOutgoingBlob(serverBlob, out var clientStatusCode);
                if (clientStatusCode == NegotiateAuthenticationStatusCode.ContinueNeeded)
                {
                    // Send the client blob to the server; this would normally happen over a network
                    Console.WriteLine($"C: {clientBlob}");
                    serverBlob = serverAuthentication.GetOutgoingBlob(clientBlob, out var serverStatusCode);
                    if (serverStatusCode != NegotiateAuthenticationStatusCode.Completed &&
                        serverStatusCode != NegotiateAuthenticationStatusCode.ContinueNeeded)
                    {
                        Console.WriteLine($"Server authentication failed with status code {serverStatusCode}");
                        break;
                    }
                    Console.WriteLine($"S: {serverBlob}");
                }
                else
                {
                    Console.WriteLine(
                        clientStatusCode == NegotiateAuthenticationStatusCode.Completed ?
                        "Successfully authenticated" :
                        $"Authentication failed with status code {clientStatusCode}");
                    break;
                }
            }
        }
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        public static async Task Main29()
        {
            using var httpListener = new HttpEventListener();

            string certificatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "testservereku.contoso.com.pfx");
            X509Certificate2 serverCertificate = new X509Certificate2(File.ReadAllBytes(certificatePath), "testcertificate", X509KeyStorageFlags.Exportable);
            var listener = await QuicListener.ListenAsync(new QuicListenerOptions
            {
                ListenEndPoint = new IPEndPoint(IPAddress.Loopback, 5001),
                ApplicationProtocols = new List<SslApplicationProtocol>() { new SslApplicationProtocol("h3") },
                ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(new QuicServerConnectionOptions()
                {
                    DefaultStreamErrorCode = 456,
                    DefaultCloseErrorCode = 123,
                    ServerAuthenticationOptions = new SslServerAuthenticationOptions()
                    {
                        ApplicationProtocols = new List<SslApplicationProtocol>() { new SslApplicationProtocol("h3") },
                        ServerCertificate = serverCertificate
                    }
                })
            });
            var connection = listener.AcceptConnectionAsync();
            Console.ReadLine();
        }
        public static async Task Main28()
        {
            /*var client = new HttpClient(new HttpClientHandler());
            var request = new HttpRequestMessage(HttpMethod.Post, "https://httpbin.org/anything")
            {
                Content = new MyContent("Ahoooj!")
            };
            var response = await client.SendAsync(request);
            Console.WriteLine(request);
            Console.WriteLine(response);*/
            var x = new MyContent("Ahoooj! 2");
            /*x.CopyTo
            x.CopyToAsync*/
            //await x.LoadIntoBufferAsync();
            await x.CopyToAsync(Stream.Null);
            await x.CopyToAsync(Stream.Null);
            //var a = await x.ReadAsByteArrayAsync();
            //var d = await x.ReadAsStringAsync();
            var c = await x.ReadAsStreamAsync();
            var b = x.ReadAsStream();
            var c2 = await x.ReadAsStreamAsync();
            var c3 = await x.ReadAsStreamAsync();
            //var stream = await x.ReadAsStreamAsync(); //true
            //await x.CopyToAsync(Stream.Null); //false
            //await x.LoadIntoBufferAsync(); //true
            //var y = await x.ReadAsStringAsync(); //true
            //x.CopyTo(Stream.Null, null, default);
        }

        public static void Main27()
        {
            var tcs = new TaskCompletionSource();
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            token.UnsafeRegister((_,_) => Console.WriteLine("a"), null);
            token.UnsafeRegister((_,_) => Console.WriteLine("b"), null);
            cts.Cancel();
            //tcs.TrySetCanceled(token);
            tcs.TrySetException(new OperationCanceledException(token));
            var x = tcs.Task.Exception;
        }

        public static async Task Main26()
        {
            const string url = "https://httpbin.org/image/jpeg";
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(url);
            Console.WriteLine($"Content length: {response.Content.Headers.ContentLength}");

            await using var stream = await httpClient.GetStreamAsync(url);
            Console.WriteLine($"Stream type: {stream.GetType()}");
            Console.WriteLine($"Stream can seek: {stream.CanSeek}");
            Console.WriteLine($"Stream length: {stream.Length}");
        }

        public static void Main25()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var test = new TestStruct()
            {
                Value = 100
            };
            cancellationToken.UnsafeRegister(static (obj, cancellationToken) =>
            {
                var instance = (TestStruct)obj!;
                Console.WriteLine($"Callback({instance.Value})");
                instance.Value = 200;
            }, test);
            test.Value = 300;
            Console.WriteLine($"Before({test.Value})");
            cts.Cancel();
            Console.WriteLine($"After({test.Value})");
        }
        public static async Task Main24()
        {
            var test = new Test();
            await test.M();
        }

        public static async Task Main23() {
            var r = new ResettableValueTaskSource();
            r.TrySetResult();
            if (r.TryGetValueTask(out ValueTask vt))
            {
                await vt;
                Console.WriteLine("a");
            }
            await Main21();
        }
        static async Task Main22() {
            var channel = Channel.CreateUnbounded<int>(new UnboundedChannelOptions() {
                SingleWriter = true
            });

            var producer = Task.Run(async () => {
                foreach (var i in Enumerable.Range(0, 1_000_000)) {
                    if (!channel.Writer.TryWrite(i)) {
                        Console.WriteLine("Booo");
                        break;
                    }
                    await Task.CompletedTask;
                    //await Task.Yield();
                }
                if (!channel.Writer.TryComplete()) {
                    Console.WriteLine("Mega Booo");
                }
                Console.WriteLine("Completed");
            });

            var consumers = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>{
                await Task.Delay(TimeSpan.FromSeconds(5));
                List<int> threadData = new List<int>();
                while (!channel.Reader.Completion.IsCompleted) {
                    await channel.Reader.WaitToReadAsync();
                    if (!channel.Reader.TryRead(out int x)) {
                        continue;
                    }
                    threadData.Add(x);
                    //Console.WriteLine($"Thread {i} consumed: {x}");
                }
                await channel.Reader.Completion;
                Console.WriteLine($"Thread {i} consumed in total: {threadData.Count}");
            }));
            await Task.WhenAll(producer, Task.WhenAll(consumers));
            Console.WriteLine(await channel.Reader.WaitToReadAsync());
        }
        static async Task Main21() {
            var rvt = new ResettableValueTaskSource();
            Console.WriteLine("1 got: " + rvt.TryGetValueTask(out ValueTask vt1));
            Console.WriteLine("2 got: " + rvt.TryGetValueTask(out ValueTask vt2));
            ValueTask vt3 = rvt.GetFinalTask();
            Console.WriteLine("3 got final: ");
            ValueTask vt4 = rvt.GetFinalTask();
            Console.WriteLine("4 got final: ");
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine("3 completed: " + vt3.IsCompleted);
            Console.WriteLine("4 completed: " + vt4.IsCompleted);
            Console.WriteLine("set: " + rvt.TrySetResult());
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine("3 completed: " + vt3.IsCompleted);
            Console.WriteLine("4 completed: " + vt4.IsCompleted);
            await vt1;
            Console.WriteLine("vt1 awaited");
            await vt2;
            Console.WriteLine("vt2 awaited");
            Console.WriteLine("1 got: " + rvt.TryGetValueTask(out vt1));
            Console.WriteLine("2 got: " + rvt.TryGetValueTask(out vt2));
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine("3 completed: " + vt3.IsCompleted);
            Console.WriteLine("4 completed: " + vt4.IsCompleted);
            Console.WriteLine("set: " + rvt.TrySetResult());
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine("3 completed: " + vt3.IsCompleted);
            Console.WriteLine("4 completed: " + vt4.IsCompleted);
            await vt1;
            Console.WriteLine("vt1 awaited");
            await vt2;
            Console.WriteLine("vt2 awaited");
            Console.WriteLine("1 got: " + rvt.TryGetValueTask(out vt1));
            Console.WriteLine("2 got: " + rvt.TryGetValueTask(out vt2));
            ValueTask vt5 = rvt.GetFinalTask();
            Console.WriteLine("5 got final: ");
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine("3 completed: " + vt3.IsCompleted);
            Console.WriteLine("4 completed: " + vt4.IsCompleted);
            Console.WriteLine("5 completed: " + vt5.IsCompleted);
            Console.WriteLine("set: " + rvt.TrySetResult());
            Console.WriteLine("set: " + rvt.TrySetResult(true));
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine("3 completed: " + vt3.IsCompleted);
            Console.WriteLine("4 completed: " + vt4.IsCompleted);
            Console.WriteLine("5 completed: " + vt5.IsCompleted);
            await vt1;
            Console.WriteLine("vt1 awaited");
            await vt2;
            Console.WriteLine("vt2 awaited");
            Console.WriteLine("set ex: " + rvt.TrySetException(new Exception(), true));
            Console.WriteLine("set: " + rvt.TrySetResult());
            Console.WriteLine("set: " + rvt.TrySetResult());
            Console.WriteLine("1 got: " + rvt.TryGetValueTask(out vt1));
            Console.WriteLine("2 got: " + rvt.TryGetValueTask(out vt2));
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine("3 completed: " + vt3.IsCompleted);
            Console.WriteLine("4 completed: " + vt4.IsCompleted);
            Console.WriteLine("5 completed: " + vt5.IsCompleted);
            ValueTask vt6 = rvt.GetFinalTask();
            Console.WriteLine("6 got final: ");
            Console.WriteLine("7 got: " + rvt.TryGetValueTask(out ValueTask vt7));
            Console.WriteLine("8 got: " + rvt.TryGetValueTask(out ValueTask vt8));
            Console.WriteLine("6 completed: " + vt6.IsCompleted);
            Console.WriteLine("7 completed: " + vt7.IsCompleted);
            Console.WriteLine("8 completed: " + vt8.IsCompleted);
            try
            {
                await vt1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught: " + ex);
            }
            Console.WriteLine(vt2.IsCompletedSuccessfully);
            try
            {
                await vt2;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught: " + ex);
            }
            try
            {
                await vt3;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught: " + ex);
            }
            try
            {
                await vt4;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught: " + ex);
            }
            try
            {
                await vt5;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught: " + ex);
            }
            try
            {
                await vt6;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught: " + ex);
            }
            try
            {
                await vt7;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught: " + ex);
            }
            try
            {
                await vt8;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught: " + ex);
            }
        }

        static void Main20() {
            var dic = new Dictionary<int, object>();
            var sw = new Stopwatch();

            foreach (var x in Enumerable.Range(0, 200_000)) {
                dic.Add(x, new object());
            }
            sw.Reset();
            sw.Start();
            for (int i = Int32.MaxValue; i >= Int32.MaxValue - 100; --i) {
                dic.Add(i, new object());
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            dic.Clear();

            foreach (var x in Enumerable.Range(0, 200_000)) {
                dic.Add(x, new object());
            }
            sw.Reset();
            sw.Start();
            for (int i = Int32.MaxValue; i >= Int32.MaxValue - 100; --i) {
                dic.Add(i, new object());
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            dic.Clear();

            foreach (var x in Enumerable.Range(0, 2_000_000)) {
                dic.Add(x, new object());
            }
            sw.Reset();
            sw.Start();
            for (int i = Int32.MaxValue; i >= Int32.MaxValue - 100; --i) {
                dic.Add(i, new object());
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            dic.Clear();
        }

        private static async ValueTask FinishHandshakeAsync(TaskCompletionSource tcs) {
            Console.WriteLine("a");
            await Task.Yield();
            Console.WriteLine("b");
            await Task.Delay(1000);
            Console.WriteLine("c");
            await tcs.Task;
            Console.WriteLine("d");
        }
        static unsafe void Main19() {
            var tcs = new TaskCompletionSource();
            var task = FinishHandshakeAsync(tcs);
            Thread.Sleep(5000);
            Console.WriteLine(task.IsCompleted);
            tcs.TrySetResult();
            Console.WriteLine(task.IsCompleted);
            Thread.Sleep(5000);
            Console.WriteLine(task.IsCompleted);
        }
        static unsafe void Main18() {
            String str = "foobar";
            Console.WriteLine("As char*: ");
            fixed (char* ptr1 = str) {
                for (int i = 0; i <= str.Length; ++i) {
                    Console.WriteLine($"{ptr1[i]} = {(int)ptr1[i]}");
                }
            }
            Console.WriteLine("As sbyte*: ");
            fixed (char* ptr2 = str) {
                sbyte* ptr3 = (sbyte*)ptr2;
                for (int i = 0; i < str.Length * 2; ++i) {
                    Console.WriteLine($"{ptr3[i]} = {(int)ptr3[i]}");
                }
            }
            Console.WriteLine("As marshalled IntPtr: ");
            IntPtr ptr4 = Marshal.StringToCoTaskMemUTF8(str);
            sbyte* ptr5 = (sbyte*)ptr4;
            for (int i = 0; i <= str.Length; ++i) {
                Console.WriteLine($"{ptr5[i]} = {(int)ptr5[i]}");
            }
            Marshal.FreeCoTaskMem(ptr4);
        }
        static void Main17(string[] args)
        {
            ProcessStartInfo psi = new ProcessStartInfo("ps", $"-Q");
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            Process? process;

            try
            {
                process = Process.Start(psi);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception while trying to run 'ps' command", ex);
            }

            if (process == null)
            {
                throw new Exception("Could not create process 'ps'");
            }

            try
            {
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                if (process.ExitCode != 0)
                {
                    throw new Exception($"Process 'ps' returned exit code {process.ExitCode}");
                }

                using StringReader sr = new StringReader(output);

                while (true)
                {
                    string? line = sr.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    Console.WriteLine(line);
                }
            }
            finally
            {
                process.Dispose();
            }
        }

        static async Task Main16(string[] args)
        {
            var client = new HttpClient(new SocketsHttpHandler()
            {
                SslOptions = new SslClientAuthenticationOptions()
                {
                    LocalCertificateSelectionCallback = (object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate? remoteCertificate, string[] acceptableIssuers) =>
                    {
                        Console.WriteLine("Client is selecting a local certificate.");
                        return new X509Certificate(new byte[0]);
                    }
                }
            });
            Console.WriteLine(await client.GetAsync("https://github.com/dotnet/runtime/"));
        }

        static void Main15(string[] args)
        {
            var x = new ResettableCompletionSource<uint>();
            Console.WriteLine(x.ToString());
        }
        internal sealed class ResettableCompletionSource<T> : IValueTaskSource<T>, IValueTaskSource
        {
            private ManualResetValueTaskSourceCore<T> _valueTaskSource;

            public ResettableCompletionSource()
            {
                _valueTaskSource.RunContinuationsAsynchronously = true;
            }

            public ValueTask<T> GetValueTask()
            {
                return new ValueTask<T>(this, _valueTaskSource.Version);
            }

            public ValueTask GetTypelessValueTask()
            {
                return new ValueTask(this, _valueTaskSource.Version);
            }

            public ValueTaskSourceStatus GetStatus(short token)
            {
                return _valueTaskSource.GetStatus(token);
            }

            public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            {
                _valueTaskSource.OnCompleted(continuation, state, token, flags);
            }

            public void Complete(T result)
            {
                _valueTaskSource.SetResult(result);
            }

            public void CompleteException(Exception ex)
            {
                _valueTaskSource.SetException(ex);
            }

            public T GetResult(short token)
            {
                bool isValid = token == _valueTaskSource.Version;
                try
                {
                    return _valueTaskSource.GetResult(token);
                }
                finally
                {
                    if (isValid)
                    {
                        _valueTaskSource.Reset();
                    }
                }
            }

            void IValueTaskSource.GetResult(short token)
            {
                bool isValid = token == _valueTaskSource.Version;
                try
                {
                    _valueTaskSource.GetResult(token);
                }
                finally
                {
                    if (isValid)
                    {
                        _valueTaskSource.Reset();
                    }
                }
            }

            public override string ToString()
            {
                var t = _valueTaskSource.GetType();
                var _completed = t.GetField("_completed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_valueTaskSource);
                var _result = t.GetField("_result", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_valueTaskSource);
                var _error = t.GetField("_error", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_valueTaskSource);
                return $"VTS {_valueTaskSource.Version} completed={_completed}, result={_result}, error={_error}";
            }
        }
        static async Task Main14(string[] args)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("https://google.com");
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine(responseContent);
            }
        }

        static void Main13(string[] args)
        {
            // display name accepts a name with \
            var ok = new MailAddress("foo@foo.com", "Foo \\ Bar");
            Console.WriteLine(ok.ToString());

            try
            {
                // however, the generated address fails to round-trip
                var fail1 = new MailAddress(ok.ToString());
            }
            catch (FormatException e)
            {
                Console.WriteLine($"Fail1: {e}");
            }
            try
            {
                // parsing an address from a string also fails for the same reason
                var fail2 = new MailAddress("\"Foo \\ Bar\" <foo@foo.com>");
            }
            catch (FormatException e)
            {
                Console.WriteLine($"Fail2: {e.Message}");
            }
        }

        public static async Task Main12()
        {
            using var _ =  new HttpEventListener();
            using HttpClient client = new HttpClient()
            {
                DefaultRequestVersion = HttpVersion.Version30,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
            };
            for (int i = 0; i < 2; ++i)
            {
                try
                {
                    using HttpResponseMessage response = await client.GetAsync("https://cloudflare-quic.com/");
                    Console.WriteLine($"====={i}=====");
                    Console.WriteLine(response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"====={i}=====");
                    Console.WriteLine(ex);
                }
            }
        }

        internal sealed class HttpEventListener : EventListener
        {
            protected override void OnEventSourceCreated(EventSource eventSource)
            {
                if (eventSource.Name == "Private.InternalDiagnostics.System.Net.Http" || eventSource.Name == "Private.InternalDiagnostics.System.Net.Quic")
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

        public static void Main11()
        {
            Console.WriteLine("Exists Certs Name and Location");
            Console.WriteLine("------ ----- -------------------------");

            foreach (StoreLocation storeLocation in (StoreLocation[])
                Enum.GetValues(typeof(StoreLocation)))
            {
                foreach (StoreName storeName in (StoreName[])
                    Enum.GetValues(typeof(StoreName)))
                {
                    X509Store store = new X509Store(storeName, storeLocation);

                    try
                    {
                        store.Open(OpenFlags.OpenExistingOnly);
                        Console.WriteLine("Yes    {0,4}  {1}, {2}",
                            store.Certificates.Count, store.Name, store.Location);
                    }
                    catch (CryptographicException)
                    {
                        Console.WriteLine("No           {0}, {1}",
                            store.Name, store.Location);
                    }
                }
                Console.WriteLine();
            }
        }


        private static string TraceId = null!;
        public static void Main10(string[] args)
        {
            Console.WriteLine(TraceId);
            TraceId = "";
            Console.WriteLine(TraceId);
            TraceId = "TraceId";
            Console.WriteLine(TraceId);
        }
        public static async Task Main9(string[] args)
        {
            using var client = new HttpClient();
            var message = new HttpRequestMessage(HttpMethod.Get, "http://github.com/dotnet/runtime/issues/42856");
            message.Headers.Add("Cookie", "test1=1;test2=2;test3=3");
            Console.WriteLine(message);
            using var response = await client.SendAsync(message);
            Console.WriteLine(response);
        }

        public static void Main8(string[] args)
        {
            Console.WriteLine(Environment.Version);
            Console.WriteLine(Dns.GetHostName());
            foreach (var x in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                Console.WriteLine(x);
            }
            Console.WriteLine();
            foreach (var x in Dns.GetHostEntry("localhost").AddressList)
            {
                Console.WriteLine(x);
            }
        }

        static async Task Main7(string[] args)
        {
            ValueTask x = default;
            Console.WriteLine(x.IsCompletedSuccessfully);
            await x.ConfigureAwait(false);
            Console.WriteLine(x.IsCompletedSuccessfully);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static async Task GetCallingAssemblyFromAsyncMethod()
        {
            var returnedAssembly = await DoSomethingAsync();

            Console.WriteLine(returnedAssembly.ToString());
            var expectedAssembly = typeof(Program).Assembly;
            Console.WriteLine(expectedAssembly.Equals(returnedAssembly));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static async Task<Assembly> DoSomethingAsync()
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            await Task.Delay(50);
            return callingAssembly;
        }

        static void Main6(string[] args)
        {
            GetCallingAssemblyFromAsyncMethod().GetAwaiter().GetResult();
        }

        static async Task Main5(string[] args)
        {
            var sw = Stopwatch.StartNew();
            var tcs = new TaskCompletionSource();

            Console.WriteLine($"{sw.Elapsed}");

            //Console.WriteLine($"{sw.Elapsed} TrySetException {tcs.TrySetException(new Exception())}");
            var tasks = new Task[3];
            var ctss = new CancellationTokenSource[3];
            for (int i = 0; i < tasks.Length; ++i)
            {
                var j = i;
                var cts = (ctss[i] = new CancellationTokenSource());
                tasks[i] = Task.Run(async () =>
                {
                    Console.WriteLine($"{sw.Elapsed} Before await {j}");
                    await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);
                    Console.WriteLine($"{sw.Elapsed} After await {j}");
                });
            }
            ctss[0].Cancel();
            //Console.WriteLine($"{sw.Elapsed} TrySetException {tcs.TrySetException(new Exception())}");
            await Task.Delay(10_000).ConfigureAwait(false);
            Console.WriteLine($"{sw.Elapsed} TrySetResult {tcs.TrySetResult()}");
            Console.WriteLine($"{sw.Elapsed} TrySetResult {tcs.TrySetResult()}");
            Console.WriteLine($"{sw.Elapsed} TrySetException {tcs.TrySetException(new Exception())}");
            await Task.WhenAll(tasks);
        }
        static async Task Main4(string[] args)
        {
            var cts = new CancellationTokenSource();
            var cw = new CreditWaiter(cts.Token);
            var sw = Stopwatch.StartNew();

            Console.WriteLine($"{sw.Elapsed} TrySetResult {cw.TrySetResult(1)}");
            Console.WriteLine($"{sw.Elapsed} TrySetResult {cw.TrySetResult(1)}");
            cw.ResetForAwait(cts.Token);

            var tasks = new Task[3];
            for (int i = 0; i < tasks.Length; ++i)
            {
                var j = i;
                tasks[i] = Task.Run(async () =>
                {
                    Console.WriteLine($"{sw.Elapsed} Before await {j}");
                    Console.WriteLine(await cw.AsValueTask().ConfigureAwait(false));
                    Console.WriteLine($"{sw.Elapsed} After await {j}");
                });
            }
            await Task.Delay(10_000).ConfigureAwait(false);
            Console.WriteLine($"{sw.Elapsed} TrySetResult {cw.TrySetResult(1)}");
            cw.ResetForAwait(cts.Token);
            Console.WriteLine($"{sw.Elapsed} TrySetResult {cw.TrySetResult(1)}");

            cts.Cancel();
            Console.WriteLine($"{sw.Elapsed} Cancel {cw.Amount}");
            await Task.WhenAll(tasks);
        }

        static async Task Main3(string[] args)
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            var _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine($"AA {semaphore.CurrentCount}");
                        await Task.Delay(5000);
                        Console.WriteLine($"BB {semaphore.CurrentCount}");
                        semaphore.Release(Int32.MaxValue);
                        Console.WriteLine($"CC {semaphore.CurrentCount}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            });
            while (true)
            {
                Console.WriteLine($"A {semaphore.CurrentCount}");
                await semaphore.WaitAsync();
                Console.WriteLine($"B {semaphore.CurrentCount}");
                await Task.Delay(500);
                Console.WriteLine($"C {semaphore.CurrentCount}");
            }
        }
        static async Task Main2(string[] args)
        {
            using var client = new HttpClient();
            using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Length"));
            Console.WriteLine(response);
            Console.WriteLine(response.Content.Headers.ContentLength);
        }
        static unsafe void Main1(string[] args)
        {
            Console.WriteLine(new SafeMsQuicConnectionHandle().DangerousGetHandle());
            byte* array = stackalloc byte[10];

            Console.WriteLine(new IntPtr(array));
        }
    }

    /// <summary>
    /// A resettable completion source which can be completed multiple times.
    /// Used to make methods async between completed events and their associated async method.
    /// </summary>
    internal class ResettableCompletionSource<T> : IValueTaskSource<T>, IValueTaskSource
    {
        private ManualResetValueTaskSourceCore<T> _valueTaskSource;

        public ResettableCompletionSource()
        {
            _valueTaskSource.RunContinuationsAsynchronously = true;
        }

        public ValueTask<T> GetValueTask()
        {
            return new ValueTask<T>(this, _valueTaskSource.Version);
        }

        public ValueTask GetTypelessValueTask()
        {
            return new ValueTask(this, _valueTaskSource.Version);
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            return _valueTaskSource.GetStatus(token);
        }

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            _valueTaskSource.OnCompleted(continuation, state, token, flags);
        }

        public void Complete(T result)
        {
            _valueTaskSource.SetResult(result);
        }

        public void CompleteException(Exception ex)
        {
            _valueTaskSource.SetException(ex);
        }

        public T GetResult(short token)
        {
            bool isValid = token == _valueTaskSource.Version;
            try
            {
                return _valueTaskSource.GetResult(token);
            }
            finally
            {
                if (isValid)
                {
                    _valueTaskSource.Reset();
                }
            }
        }

        void IValueTaskSource.GetResult(short token)
        {
            bool isValid = token == _valueTaskSource.Version;
            try
            {
                _valueTaskSource.GetResult(token);
            }
            finally
            {
                if (isValid)
                {
                    _valueTaskSource.Reset();
                }
            }
        }
    }

    /// <summary>Represents a waiter for credit.</summary>
    internal sealed class CreditWaiter : IValueTaskSource<int>
    {
        // State for the implementation of the CreditWaiter. Note that neither _cancellationToken nor
        // _registration are zero'd out upon completion, because they're used for synchronization
        // between successful completion and cancellation.  This means an instance may end up
        // referencing the underlying CancellationTokenSource even after the await operation has completed.

        /// <summary>Cancellation token for the current wait operation.</summary>
        private CancellationToken _cancellationToken;
        /// <summary>Cancellation registration for the current wait operation.</summary>
        private CancellationTokenRegistration _registration;
        /// <summary><see cref="IValueTaskSource"/> implementation.</summary>
        private ManualResetValueTaskSourceCore<int> _source;

        // State carried with the waiter for the consumer to use; these aren't used at all in the implementation.

        /// <summary>Amount of credit desired by this waiter.</summary>
        public int Amount = 0;
        /// <summary>Next waiter in a list of waiters.</summary>
        public CreditWaiter? Next = null;

        /// <summary>Initializes a waiter for a credit wait operation.</summary>
        /// <param name="cancellationToken">The cancellation token for this wait operation.</param>
        public CreditWaiter(CancellationToken cancellationToken)
        {
            _source.RunContinuationsAsynchronously = true;
            RegisterCancellation(cancellationToken);
        }

        /// <summary>Re-initializes a waiter for a credit wait operation.</summary>
        /// <param name="cancellationToken">The cancellation token for this wait operation.</param>
        public void ResetForAwait(CancellationToken cancellationToken)
        {
            _source.Reset();
            RegisterCancellation(cancellationToken);
        }

        /// <summary>Registers with the cancellation token to transition the source to a canceled state.</summary>
        /// <param name="cancellationToken">The cancellation token with which to register.</param>
        private void RegisterCancellation(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _registration = cancellationToken.UnsafeRegister(static (s, cancellationToken) =>
            {
                // The callback will only fire if cancellation owns the right to complete the instance.
                ((CreditWaiter)s!)._source.SetException(ExceptionDispatchInfo.SetCurrentStackTrace(new OperationCanceledException(cancellationToken)));
            }, this);
        }

        /// <summary>Wraps the instance as a <see cref="ValueTask{TResult}"/> to make it awaitable.</summary>
        public ValueTask<int> AsValueTask() => new ValueTask<int>(this, _source.Version);

        /// <summary>Completes the instance with the specified result.</summary>
        /// <param name="result">The result value.</param>
        /// <returns>true if the instance was successfully completed; false if it was or is being canceled.</returns>
        public bool TrySetResult(int result)
        {
            if (UnregisterAndOwnCompletion())
            {
                _source.SetResult(result);
                return true;
            }

            return false;
        }

        /// <summary>Disposes the instance, failing any outstanding wait.</summary>
        public void Dispose()
        {
            if (UnregisterAndOwnCompletion())
            {
                _source.SetException(ExceptionDispatchInfo.SetCurrentStackTrace(new ObjectDisposedException("Picik")));
            }
        }

        /// <summary>Unregisters the cancellation callback.</summary>
        /// <returns>true if the non-cancellation caller has the right to complete the instance; false if the instance was or is being completed by cancellation.</returns>
        private bool UnregisterAndOwnCompletion() =>
            // Unregister the cancellation callback.  If Unregister returns true, then the cancellation callback was successfully removed,
            // meaning it hasn't run and won't ever run.  If it returns false, a) cancellation already occurred or is occurring and thus
            // the callback couldn't be removed, b) cancellation occurred prior to the UnsafeRegister call such that _registration was
            // set to a default value (or hasn't been set yet), or c) a default CancellationToken was used.  (a) and (b) are effectively
            // the same, and (c) can be checked via CanBeCanceled.
            _registration.Unregister() || !_cancellationToken.CanBeCanceled;

        int IValueTaskSource<int>.GetResult(short token) =>
            _source.GetResult(token);
        ValueTaskSourceStatus IValueTaskSource<int>.GetStatus(short token) =>
            _source.GetStatus(token);
        void IValueTaskSource<int>.OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) =>
            _source.OnCompleted(continuation, state, token, flags);
    }


    internal sealed class SafeMsQuicConnectionHandle : SafeHandle
    {
        public override bool IsInvalid => handle == IntPtr.Zero;

        public SafeMsQuicConnectionHandle()
            : base(IntPtr.Zero, ownsHandle: true)
        { }

        public SafeMsQuicConnectionHandle(IntPtr connectionHandle)
            : this()
        {
            SetHandle(connectionHandle);
        }

        protected override bool ReleaseHandle()
        {
            return true;
        }
    }


    internal class ResettableValueTaskSource : IValueTaskSource
    {
        // None -> [TryGetValueTask] -> Awaiting -> [TrySetResult|TrySetException(final: false)] -> Ready -> [GetResult] -> None
        // None -> [TrySetResult|TrySetException(final: false)] -> Ready -> [TryGetValueTask] -> [GetResult] -> None
        // None|Awaiting -> [TrySetResult|TrySetException(final: true)] -> Final(never leaves this state)
        private enum State
        {
            None,
            Awaiting,
            Ready,
            Completed
        }

        private State _state;
        private ManualResetValueTaskSourceCore<bool> _valueTaskSource;
        private CancellationTokenRegistration _cancellationRegistration;
        private GCHandle _keepAlive;
        private FinalValueTaskSource _finalTaskSource;

        public ResettableValueTaskSource(bool runContinuationsAsynchronously = true)
        {
            _state = State.None;
            _valueTaskSource = new ManualResetValueTaskSourceCore<bool>() { RunContinuationsAsynchronously = runContinuationsAsynchronously };
            _cancellationRegistration = default;
            _keepAlive = default;

            _finalTaskSource = new FinalValueTaskSource(runContinuationsAsynchronously);

            // Reset the value task source so that its version doesn't overlap with the final task source.
            if (_valueTaskSource.Version == _finalTaskSource.Version)
            {
                _valueTaskSource.Reset();
            }
        }

        public bool IsCompleted
        {
            get
            {
                lock (this)
                {
                    return _state == State.Completed;
                }
            }
        }

        public bool TryGetValueTask(out ValueTask valueTask, object? keepAlive = null, CancellationToken cancellationToken = default)
        {
            lock (this)
            {
                // Cancellation might kick off synchronously, re-entering the lock and changing the state to completed.
                if (_state == State.None)
                {
                    // Register cancellation if the token can be cancelled and the task is not completed yet.
                    if (cancellationToken.CanBeCanceled)
                    {
                        _cancellationRegistration = cancellationToken.UnsafeRegister(static (obj, cancellationToken) =>
                        {
                            var parent = (ResettableValueTaskSource)obj!;
                            parent.TrySetException(new OperationCanceledException(cancellationToken));
                        }, this);
                    }
                }

                var state = _state;

                // None: prepare for the actual operation happening and transition to Awaiting.
                if (state == State.None)
                {
                    // Root the object to keep alive if requested.
                    if (keepAlive is not null)
                    {
                        Debug.Assert(!_keepAlive.IsAllocated);
                        _keepAlive = GCHandle.Alloc(keepAlive);
                    }

                    _state = State.Awaiting;
                }
                // None, Completed, Final: return the current task.
                if (state == State.None ||
                    state == State.Ready ||
                    state == State.Completed)
                {
                    valueTask = new ValueTask(this, _valueTaskSource.Version);
                    return true;
                }

                // Awaiting: forbidden concurrent call.
                valueTask = default;
                return false;
            }
        }

        private bool TryComplete(Exception? exception, bool final)
        {
            Debug.Assert(Monitor.IsEntered(this));
            try
            {
                var state = _state;

                // None,Awaiting: clean up and finish the task source.
                if (state == State.Awaiting ||
                    state == State.None)
                {
                    // Dispose the cancellation if registered.
                    _cancellationRegistration.Dispose();
                    _cancellationRegistration = default;

                    // Unblock the current task source and in case of a final also the final task source.
                    if (exception is not null)
                    {
                        _valueTaskSource.SetException(exception);
                    }
                    else
                    {
                        _valueTaskSource.SetResult(final);
                    }
                    if (final)
                    {
                        _finalTaskSource.TryComplete();
                        _finalTaskSource.TrySignal(out _);
                    }

                    _state = final ? State.Completed : State.Ready;
                    return true;
                }

                // Final: remember the first final result to set it once the current non-final result gets retrieved.
                if (final)
                {
                    return _finalTaskSource.TryComplete(exception);
                }

                return false;
            }
            finally
            {
                // Un-root the the kept alive object in all cases.
                if (_keepAlive.IsAllocated)
                {
                    _keepAlive.Free();
                }
            }
        }

        public bool TrySetResult(bool final = false)
        {
            lock (this)
            {
                return TryComplete(null, final);
            }
        }

        public bool TrySetException(Exception exception, bool final = false)
        {
            // Set up the exception stack strace for the caller.
            exception = exception.StackTrace is null ? ExceptionDispatchInfo.SetCurrentStackTrace(exception) : exception;
            lock (this)
            {
                return TryComplete(exception, final);
            }
        }

        private void GetResult(short token)
        {
            try
            {
                _valueTaskSource.GetResult(token);
            }
            finally
            {
                lock (this)
                {
                    var state = _state;

                    if (state == State.Ready)
                    {
                        // Reset the value task source so that its version doesn't overlap with the final task source.
                        _valueTaskSource.Reset();
                        if (_valueTaskSource.Version == _finalTaskSource.Version)
                        {
                            _valueTaskSource.Reset();
                        }

                        if (_finalTaskSource.TrySignal(out var exception))
                        {
                            if (exception is not null)
                            {
                                _valueTaskSource.SetException(exception);
                            }
                            else
                            {
                                _valueTaskSource.SetResult(true);
                            }
                            _state = State.Completed;
                        }
                        else
                        {
                            _state = State.None;
                        }
                    }
                }
            }
        }

        public ValueTask GetFinalTask() => new ValueTask(this, _finalTaskSource.Version);

        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token)
        {
            if (token == _finalTaskSource.Version)
            {
                return _finalTaskSource.GetStatus(token);

            }
            else
            {
                return _valueTaskSource.GetStatus(token);
            }
        }

        void IValueTaskSource.OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            if (token == _finalTaskSource.Version)
            {
                _finalTaskSource.OnCompleted(continuation, state, token, flags);

            }
            else
            {
                _valueTaskSource.OnCompleted(continuation, state, token, flags);
            }
        }

        void IValueTaskSource.GetResult(short token)
        {
            if (token == _finalTaskSource.Version)
            {
                _finalTaskSource.GetResult(token);

            }
            else
            {
                GetResult(token);
            }
        }

        private struct FinalValueTaskSource : IValueTaskSource
        {
            private ManualResetValueTaskSourceCore<bool> _valueTaskSource;
            private bool _isCompleted;
            private Exception? _exception;

            public FinalValueTaskSource(bool runContinuationsAsynchronously = true)
            {
                _valueTaskSource = new ManualResetValueTaskSourceCore<bool>() { RunContinuationsAsynchronously = runContinuationsAsynchronously };
                _isCompleted = false;
                _exception = null;
            }

            public short Version => _valueTaskSource.Version;

            public bool TryComplete(Exception? exception = null)
            {
                if (_isCompleted)
                {
                    return false;
                }

                _exception = exception;
                _isCompleted = true;
                return true;
            }

            public bool TrySignal(out Exception? exception)
            {
                if (!_isCompleted)
                {
                    exception = default;
                    return false;
                }

                if (_exception is not null)
                {
                    _valueTaskSource.SetException(_exception);
                }
                else
                {
                    _valueTaskSource.SetResult(true);
                }

                exception = _exception;
                return true;
            }

            public ValueTaskSourceStatus GetStatus(short token)
                => _valueTaskSource.GetStatus(_valueTaskSource.Version);

            public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
                => _valueTaskSource.OnCompleted(continuation, state, token, flags);

            public void GetResult(short token)
                => _valueTaskSource.GetResult(_valueTaskSource.Version);
        }
    }
}


public class Test
{
    WeakReference<TaskCompletionSource> wr = new WeakReference<TaskCompletionSource>(new TaskCompletionSource());
    private TaskCompletionSource? TCS => wr.TryGetTarget(out var tcs) ? tcs : null;

    public async Task M1()
    {
        var tcs = new TaskCompletionSource();
        var task = Task.Run(async() => {
            var foo = new Foo(tcs);
            await foo.PoopAsync();
            Console.WriteLine(foo);
        });
        GC.Collect();
        tcs.SetResult();
        await task;
    }

    public async Task M()
    {
        //var t = TCS;
        var task = //TestMethod(TCS);
                   Task.Run(() => TestMethod(TCS ?? throw new ArgumentNullException()));
        //GC.Collect(); doesn't seem to change anything
        Console.WriteLine(Environment.ProcessId);
        await Task.Delay(TimeSpan.FromSeconds(20)); // So that SetWr gets called
        GC.Collect();
        if (!wr.TryGetTarget(out var x))
        {
            Console.WriteLine("Disapeared");
        }
        else
        {
            Console.WriteLine("Alive");
            TCS?.SetResult();
        }
        await task;
    }

    private async Task TestMethod(TaskCompletionSource tcs)
    {
        await Task.Yield();
        var foo = new Foo(tcs);
        Console.WriteLine(foo);
        var gcHandle = GCHandle.Alloc(tcs, GCHandleType.Normal);
        await foo.PoopAsync(); //the same thing with manual DisposeAsync
        Console.WriteLine(foo);
        gcHandle.Free();
    }
}

public class Foo : IAsyncDisposable
{
    private TaskCompletionSource _tcs;
    public Foo(TaskCompletionSource tcs)
    {
        _tcs = tcs;
    }

    public async ValueTask DisposeAsync()
    {
        await _tcs.Task;
    }
    public async Task PoopAsync()
    {
        await _tcs.Task;
    }
}

public struct TestStruct
{
    public int Value;
}

public class MyContent : HttpContent
{
    private readonly string _data;
    public MyContent(string data) {
        _data = data;
    }
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        => stream.WriteAsync(Encoding.UTF8.GetBytes(_data)).AsTask();

    /*protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        => stream.WriteAsync(Encoding.UTF8.GetBytes(_data), cancellationToken).AsTask();*/

    protected override bool TryComputeLength(out long length) {
        length = 0;
        return false;
    }

    protected override Stream CreateContentReadStream(CancellationToken cancellationToken)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(_data));
    }
    protected override Task<Stream> CreateContentReadStreamAsync()
    {
        return Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(_data)));
    }
    protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
    {
        return base.CreateContentReadStreamAsync(cancellationToken);
    }
    protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        base.SerializeToStream(stream, context, cancellationToken);
    }
}