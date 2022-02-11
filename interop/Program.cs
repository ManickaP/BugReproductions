using System;
using System.Buffers;
using System.Diagnostics;
using System.Net.Security;
using System.Runtime.InteropServices;

namespace interop
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using QuicBuffers alpnBuffers = new QuicBuffers();
            var sw = Stopwatch.StartNew();
            alpnBuffers.Initialize((new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 }).ToArray(), x => x.Protocol);
            sw.Stop();
            Console.WriteLine(alpnBuffers.Count + " in " + sw.Elapsed);
            alpnBuffers.Reset();
            sw.Start();
            alpnBuffers.Initialize((new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3, SslApplicationProtocol.Http11 }).ToArray(), x => x.Protocol);
            sw.Stop();
            Console.WriteLine(alpnBuffers.Count + " in " + sw.Elapsed);
        }
    }

    internal unsafe struct QuicBuffers : IDisposable {
        private MemoryHandle[] _handles = Array.Empty<MemoryHandle>();
        private IntPtr _buffers = IntPtr.Zero;
        private int _count = 0;

        public QUIC_BUFFER* Buffers => (QUIC_BUFFER*)_buffers;
        public int Count => _count;

        public void Initialize<T>(T[] inputs, Func<T, ReadOnlyMemory<byte>> toBuffer) {
            // Note that the struct either needs to be freshly created or previously cleaned up with Reset.
            if (_handles.Length < inputs.Length) {
                ArrayPool<MemoryHandle>.Shared.Return(_handles);
                _handles = ArrayPool<MemoryHandle>.Shared.Rent(inputs.Length);
            }
            if (_count < inputs.Length) {
                Marshal.FreeHGlobal(_buffers);
                _buffers = Marshal.AllocHGlobal(sizeof(QUIC_BUFFER) * inputs.Length);
            }
            _count = inputs.Length;

            var buffers = Buffers;
            for (int i = 0; i < _count; ++i) {
                var buffer = toBuffer(inputs[i]);
                var handle = buffer.Pin();

                _handles[i] = handle;
                buffers[i].Buffer = (byte*)handle.Pointer;
                buffers[i].Length = (uint)buffer.Length;
            }
        }

        public void Reset() {
            for (int i = 0; i < _count; ++i) {
                _handles[i].Dispose();
            }
        }

        public void Dispose() {
            Reset();
            ArrayPool<MemoryHandle>.Shared.Return(_handles);
            Marshal.FreeHGlobal(_buffers);
        }
    }

    public unsafe partial struct QUIC_BUFFER
    {
        public uint Length;
        public byte* Buffer;

        public Span<byte> Span => new(Buffer, (int)Length);
    }
}
