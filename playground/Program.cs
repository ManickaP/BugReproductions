using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace playground
{
    class Program
    {
        static async Task Main(string[] args)
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