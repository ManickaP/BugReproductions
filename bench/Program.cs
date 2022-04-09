using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace bench
{
    // It is very easy to use BenchmarkDotNet. You should just create a class
    public class IntroBasic
    {
        private QuicStream _stream = new QuicStream();

        // And define a method with the Benchmark attribute
        //[Benchmark]
        public void Bla() => Do();

        // You can write a description for your method.
        //[Benchmark]
        public void BlaAsync() => DoAsync().GetAwaiter().GetResult();

        private void Do()
        {
            Thread.Sleep(1);
        }
        private async Task DoAsync()
        {
            Thread.Sleep(1);
        }
        //[Benchmark]
        public void ConditionalRef()
        {
            int a = 0;
            int b = 0;
            bool x = true;

            (x ? ref a: ref b) = 5;
        }
        //[Benchmark]
        public void IfElse()
        {
            int a = 0;
            int b = 0;
            bool x = true;

            if (x)
                a = 5;
            else
                b = 5;
        }

        [Benchmark(Baseline = true)]
        public string TraceId() => $"Logging {_stream._traceId}";
        [Benchmark()]
        public string Handle() => $"Logging {_stream._handle}";
    }

    class Program
    {
        static void Main(string[] args)
            => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }

    internal sealed class QuicStream
    {
        public SafeMsQuicStreamHandle _handle;
        public string _traceId;

        public QuicStream()
        {
            _handle = new SafeMsQuicStreamHandle(1024);
            _traceId = _handle.ToString();
        }
    }

    internal sealed class SafeMsQuicStreamHandle : MsQuicSafeHandle
    {
        public SafeMsQuicStreamHandle(int handle)
            : base(handle, "strm")
        { }
    }
    internal abstract class MsQuicSafeHandle
    {
        private readonly string _traceId;

        protected MsQuicSafeHandle(int handle, string prefix)
        {
            _traceId = $"[{prefix}][0x{handle:X11}]";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => _traceId;
    }
}
