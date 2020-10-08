using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace rent_buffer_loh
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(Environment.ProcessId);
            await Task.Delay(TimeSpan.FromSeconds(30));
            const int length = 81920;
            while (true) {
                Console.WriteLine($"Renting {length}");
                var buffik = ArrayPool<byte>.Shared.Rent(length);
                Console.WriteLine($"Got {buffik.Length}");
                /*var cnt = await ProcessBuffer(buffik);
                Console.WriteLine($"Read {cnt}");*/
                await Task.Delay(TimeSpan.FromSeconds(10));
                ArrayPool<byte>.Shared.Return(buffik);
                Console.WriteLine($"Returned {buffik.Length}");
                await Task.Delay(TimeSpan.FromSeconds(10));
                //GC.Collect();
            }
        }

        static async Task<int> ProcessBuffer(byte[] buffik)
        {
            using var file = new FileStream("/home/manicka/Documents/furtik_h2.txt", FileMode.Open, FileAccess.Read);
            return await file.ReadAsync(buffik, 0, buffik.Length);
        }
    }
}
