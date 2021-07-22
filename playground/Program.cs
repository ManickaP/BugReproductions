using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
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

namespace playground
{
    class Program
    {
        public static async Task Main()
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
}