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

namespace playground
{
    class Program
    {
        public static async Task Main()
        {
            var test = new Test();
            await test.M();
        }

        public static async Task Main23() {
            var r = new ResettableValueTaskSource<int>();
            r.TrySetResult(5);
            if (r.TryGetValueTask(out ValueTask<int> vt))
            {
                Console.WriteLine(await vt);
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
            var rvt = new ResettableValueTaskSource<int>();
            Console.WriteLine("1 got: " + rvt.TryGetValueTask(out ValueTask<int> vt1));
            Console.WriteLine("2 got: " + rvt.TryGetValueTask(out ValueTask<int> vt2));
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine("set 100: " + rvt.TrySetResult(100));
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine(await vt1);
            Console.WriteLine(await vt2);
            Console.WriteLine("1 got: " + rvt.TryGetValueTask(out vt1));
            Console.WriteLine("2 got: " + rvt.TryGetValueTask(out vt2));
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine("set 100: " + rvt.TrySetResult(100));
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine(await vt1);
            Console.WriteLine(await vt2);
            Console.WriteLine("1 got: " + rvt.TryGetValueTask(out vt1));
            Console.WriteLine("2 got: " + rvt.TryGetValueTask(out vt2));
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine("set 100: " + rvt.TrySetResult(100));
            Console.WriteLine("set 200: " + rvt.TrySetResult(200));
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine(await vt1);
            Console.WriteLine(await vt2);
            Console.WriteLine("set ex: " + rvt.TrySetException(new Exception(), true));
            Console.WriteLine("set 50: " + rvt.TrySetResult(50));
            Console.WriteLine("set 500: " + rvt.TrySetResult(500));
            Console.WriteLine("1 got: " + rvt.TryGetValueTask(out vt1));
            Console.WriteLine("2 got: " + rvt.TryGetValueTask(out vt2));
            Console.WriteLine("1 completed: " + vt1.IsCompleted);
            Console.WriteLine("2 completed: " + vt2.IsCompleted);
            Console.WriteLine("3 got: " + rvt.TryGetValueTask(out ValueTask vt3));
            Console.WriteLine("4 got: " + rvt.TryGetValueTask(out ValueTask vt4));
            Console.WriteLine("3 completed: " + vt3.IsCompleted);
            Console.WriteLine("4 completed: " + vt4.IsCompleted);
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
                await vt1;
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
                        return null;
                    }
                }
            });
            Console.WriteLine(await client.GetAsync("https://github.com/dotnet/runtime/"));
        }

        static async Task Main15(string[] args)
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

        public static async Task Main11()
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
        public static async Task Main10(string[] args)
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
        public int Amount;
        /// <summary>Next waiter in a list of waiters.</summary>
        public CreditWaiter? Next;

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


    internal class ResettableValueTaskSource<T> : IValueTaskSource<T>, IValueTaskSource
    {
        private static readonly TaskCompletionSource<T> s_completionSentinel = new TaskCompletionSource<T>();
        // None -> [TryGetValueTask] -> Awaiting -> [TrySetResult|TrySetException(final: false)] -> Completed -> [GetResult] -> None
        // None -> [TrySetResult|TrySetException(final: false)] -> Completed -> [GetResult] -> None
        // None|Awaiting -> [TrySetResult|TrySetException(final: true)] -> Final(never leaves this state)
        private const int StateNone = 0;
        private const int StateAwaiting = 1;
        private const int StateCompleted = 2;
        private const int StateFinal = 3;
        private int _state = StateNone;
        private ManualResetValueTaskSourceCore<T> _valueTaskSource;
        private CancellationTokenRegistration _cancellationRegistration;
        private TaskCompletionSource<T>? _taskCompletionSource;

        public ResettableValueTaskSource(bool runContinuationsAsynchronously = true)
        {
            _valueTaskSource = new ManualResetValueTaskSourceCore<T>() { RunContinuationsAsynchronously = runContinuationsAsynchronously };
            _cancellationRegistration = default;
            _taskCompletionSource = default;
        }

        private bool TryGetValueTask<TValueTask>(Func<ResettableValueTaskSource<T>, TValueTask> createValueTask, out TValueTask valueTask, CancellationToken cancellationToken = default)
            where TValueTask : struct
        {
            // None -> Awaiting
            int state = Interlocked.CompareExchange(ref _state, StateAwaiting, StateNone);
            if (state == StateNone || state == StateCompleted)
            {
                valueTask = createValueTask(this);

                // Register cancellation if the token can be cancelled and the task is not completed yet.
                if (cancellationToken.CanBeCanceled && state == StateNone)
                {
                    _cancellationRegistration = cancellationToken.UnsafeRegister(static (obj, cancellationToken) =>
                    {
                        var parent = (ResettableValueTaskSource<T>)obj!;
                        parent.TrySetException(new OperationCanceledException(cancellationToken));
                    }, this);
                }
                return true;
            }
            if (state == StateFinal)
            {
                // The task never gets reset once it reaches the final state, thus a ValueTask can be returned repeatedly.
                valueTask = createValueTask(this);
                return true;
            }

            valueTask = default;
            return false;
        }

        public bool TryGetValueTask(out ValueTask<T> valueTask, CancellationToken cancellationToken = default)
            => TryGetValueTask(static parent => new ValueTask<T>(parent, parent._valueTaskSource.Version), out valueTask, cancellationToken);

        public bool TryGetValueTask(out ValueTask valueTask, CancellationToken cancellationToken = default)
            => TryGetValueTask(static parent => new ValueTask(parent, parent._valueTaskSource.Version), out valueTask, cancellationToken);

        public bool TrySetResult(T item, bool final = false)
        {
            // None|Awaiting -> Completed|Final
            // Completion when final is true means that the task will never get reset.
            if (Interlocked.CompareExchange(ref _state, final ? StateFinal : StateCompleted, StateAwaiting) == StateAwaiting ||
                Interlocked.CompareExchange(ref _state, final ? StateFinal : StateCompleted, StateNone) == StateNone)
            {
                _cancellationRegistration.Dispose();
                _cancellationRegistration = default;
                _valueTaskSource.SetResult(item);
                if (final)
                {
                    var taskCompletionSource = Interlocked.CompareExchange(ref _taskCompletionSource, s_completionSentinel, null);
                    if (taskCompletionSource is not null && taskCompletionSource != s_completionSentinel)
                    {
                        taskCompletionSource.SetResult(item);
                    }
                }
                return true;
            }

            return false;
        }

        public bool TrySetException(Exception exception, bool final = false)
        {
            // None|Awaiting -> Completed|Final
            // Completion when final is true means that the task will never get reset.
            if (Interlocked.CompareExchange(ref _state, final ? StateFinal : StateCompleted, StateAwaiting) == StateAwaiting ||
                Interlocked.CompareExchange(ref _state, final ? StateFinal : StateCompleted, StateNone) == StateNone)
            {
                _cancellationRegistration.Dispose();
                _cancellationRegistration = default;
                _valueTaskSource.SetException(exception.StackTrace is null ? ExceptionDispatchInfo.SetCurrentStackTrace(exception) : exception);
                if (final)
                {
                    var taskCompletionSource = Interlocked.CompareExchange(ref _taskCompletionSource, s_completionSentinel, null);
                    if (taskCompletionSource is not null && taskCompletionSource != s_completionSentinel)
                    {
                        taskCompletionSource.SetException(exception.StackTrace is null ? ExceptionDispatchInfo.SetCurrentStackTrace(exception) : exception);
                    }
                }
                return true;
            }

            return false;
        }

        private T GetResult(short token)
        {
            try
            {
                return _valueTaskSource.GetResult(token);
            }
            finally
            {
                // Completed -> None
                // Either reset if the task has been completed non-finally or keep it as-is if final, no other states are possible here.
                if (Interlocked.CompareExchange(ref _state, StateNone, StateCompleted) == StateCompleted)
                {
                    _valueTaskSource.Reset();
                }
            }
        }

        public ValueTask<T> GetFinalTask()
        {
            // Avoid allocating tcs in case we have already finished.
            if (Volatile.Read(ref _state) == StateFinal)
            {
                return new ValueTask<T>(this, _valueTaskSource.Version);
            }

            // Fast path for already allocated tcs.
            var taskCompletionSource = Volatile.Read(ref _taskCompletionSource);
            if (taskCompletionSource is not null)
            {
                if (taskCompletionSource == s_completionSentinel)
                {
                    return new ValueTask<T>(this, _valueTaskSource.Version);
                }
                return new ValueTask<T>(taskCompletionSource.Task);
            }

            // We might race here so make sure only one shared instance of tcs is created.
            taskCompletionSource = Interlocked.CompareExchange(ref _taskCompletionSource, new TaskCompletionSource<T>(_valueTaskSource.RunContinuationsAsynchronously), null);
            if (taskCompletionSource == s_completionSentinel)
            {
                return new ValueTask<T>(this, _valueTaskSource.Version);
            }
            return new ValueTask<T>(_taskCompletionSource.Task);
        }

        ValueTaskSourceStatus IValueTaskSource<T>.GetStatus(short token)
            => _valueTaskSource.GetStatus(token);

        void IValueTaskSource<T>.OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => _valueTaskSource.OnCompleted(continuation, state, token, flags);

        T IValueTaskSource<T>.GetResult(short token)
            => GetResult(token);

        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token)
            => _valueTaskSource.GetStatus(token);

        void IValueTaskSource.OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => _valueTaskSource.OnCompleted(continuation, state, token, flags);

        void IValueTaskSource.GetResult(short token)
            => GetResult(token);
    }
}


public class Test
{
    WeakReference<TaskCompletionSource> wr;
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
        wr = new WeakReference<TaskCompletionSource>(new TaskCompletionSource());
        var t = TCS;
        var task = //TestMethod(TCS);
                   Task.Run(() => TestMethod(t));
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
            TCS.SetResult();
        }
        await task;
    }

    private async Task TestMethod(TaskCompletionSource tcs)
    {
        await Task.Yield();
        var foo = new Foo(tcs);
        await foo.PoopAsync(); //the same thing with manual DisposeAsync
        Console.WriteLine(foo);
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