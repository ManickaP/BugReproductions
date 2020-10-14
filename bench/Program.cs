using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace bench
{
    // It is very easy to use BenchmarkDotNet. You should just create a class
    public class IntroBasic
    {
        // And define a method with the Benchmark attribute
        [Benchmark]
        public void Bla() => Do();

        // You can write a description for your method.
        [Benchmark]
        public void BlaAsync() => DoAsync().GetAwaiter().GetResult();

        private void Do()
        {
            Thread.Sleep(1);
        }
        private async Task DoAsync()
        {
            Thread.Sleep(1);
        }
    }

    class Program
    {
        static void Main(string[] args) 
            => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
