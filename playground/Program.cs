using System;
using System.Runtime.InteropServices;

namespace playground
{
    class Program
    {
        static unsafe void Main(string[] args)
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