using System;


class Program
{
    private InitTaskCompletionSource _itcs = new InitTaskCompletionSource();

    public ValueTask Test(Action action) {
        if (_itcs.TryInitialize(out var task)) {
            action();
        }
        return task;
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine($"Hello World! {Environment.ProcessId}");
        var p = new Program();
        var tasks = new Task[10_000];
        for (int i = 0; i < tasks.Length; ++i) {
            var j = i;
            tasks[i] = Task.Run(async () => {
                await Task.Yield();
                await p.Test(() => Console.WriteLine($"Winner {j}"));
            });
        }

        p._itcs.Set();
        await Task.WhenAll(tasks);
    }
}


public struct InitTaskCompletionSource {

    private TaskCompletionSource? _taskCompletionSource = null;

    public InitTaskCompletionSource() {
    }

    public bool TryInitialize(out ValueTask valueTask) {
        // Firstly checks if the _taskCompletionSource has been initialized to minimize number of wastefully created instances.
        if (_taskCompletionSource is not null) {
            //Console.WriteLine("fast return");
            valueTask = new ValueTask(_taskCompletionSource.Task);
            return false;
        }
        // If we're the first here and we will return true.
        var original = Interlocked.CompareExchange(ref _taskCompletionSource, new TaskCompletionSource(), null);
        if (original is not null) {
            Console.WriteLine("interlocked return");
            valueTask = new ValueTask(_taskCompletionSource.Task);
            return false;
        }

        Console.WriteLine("winner");
        valueTask = new ValueTask(_taskCompletionSource.Task);
        return true;
    }
    public void Set() {
        while (_taskCompletionSource is null) {
            Thread.Sleep(1);
        }
        _taskCompletionSource!.TrySetResult();
    }
}
