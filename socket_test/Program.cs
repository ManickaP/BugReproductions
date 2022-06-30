using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace socket_test
{
class Program
{
    static async Task Main(string[] args)
    {

        Console.WriteLine("{0:X8}",
            unchecked(
                (int)(5023) <= 0 ? ((int)(5023)) : ((int)(((5023) & 0x0000FFFF) | (7 << 16) | 0x80000000))
                )
        );
        await Task.WhenAll(RunServer(), RunClient());
    }

    static readonly TaskCompletionSource<IPEndPoint> serverEndpoint = new TaskCompletionSource<IPEndPoint>();
    static readonly TaskCompletionSource clientWrite = new TaskCompletionSource();

    static async Task RunClient()
    {
        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        socket.NoDelay = true;
        var endpoint = await serverEndpoint.Task;
        Console.WriteLine("Client connecting to: " + endpoint);
        socket.Connect(endpoint);
        Console.WriteLine("Client connected to: " + socket.RemoteEndPoint);
        var stream = new NetworkStream(socket, ownsSocket: true);
        for (int i = 0; i < 1_000; ++i)
        {
            await stream.WriteAsync(UTF8Encoding.UTF8.GetBytes("Ahoj"));
        }
        clientWrite.SetResult();
        /*try
        {
            // Read will throw "Connection reset by peer".
            await stream.ReadAsync(new byte[10]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }*/
        socket.Shutdown(SocketShutdown.Send);
        stream.Dispose();
    }
    static async Task RunServer()
    {
        var listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        listenSocket.Listen();
        serverEndpoint.SetResult(listenSocket.LocalEndPoint as IPEndPoint);
        Console.WriteLine("Server listening on: " + listenSocket.LocalEndPoint);
        var socket = await listenSocket.AcceptAsync().ConfigureAwait(false);
        socket.NoDelay = true;
        // To force RST when calling Close.
        socket.LingerState = new LingerOption(true, 0);
        var stream = new NetworkStream(socket, ownsSocket: true);
        var buffer = new byte[100];
        int readBytes = 0;
        do
        {
            readBytes = await stream.ReadAsync(buffer);
            Console.WriteLine($"Server({readBytes}):" + UTF8Encoding.UTF8.GetString(buffer, 0, readBytes));
            await clientWrite.Task;
            socket.Close(0);
        } while (false);//(readBytes > 0);
    }
}
}
