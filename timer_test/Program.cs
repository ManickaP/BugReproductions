// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

int _removeStalePoolsIsRunning = 0;

void RemoveStalePools()
{
    Console.WriteLine($"RemoveStalePools[{Thread.CurrentThread.ManagedThreadId:00000000}] called");
    // Check whether the method is not already running and prevent parallel execution.
    if (Interlocked.CompareExchange(ref _removeStalePoolsIsRunning, 1, 0) != 0)
    {
        Console.WriteLine($"RemoveStalePools[{Thread.CurrentThread.ManagedThreadId:00000000}] bailed");
        return;
    }
    Console.WriteLine($"RemoveStalePools[{Thread.CurrentThread.ManagedThreadId:00000000}] entered");

    try
    {
        Thread.Sleep(3000);
    }
    finally
    {
        Console.WriteLine($"RemoveStalePools[{Thread.CurrentThread.ManagedThreadId:00000000}] back to 0");
        // Make sure the guard value gets always reset back to 0 and that it's visible to other threads.
        Volatile.Write(ref _removeStalePoolsIsRunning, 0);
    }
}

Timer _cleaningTimer = new Timer((_) => RemoveStalePools(), null, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000));
Thread.Sleep(TimeSpan.FromSeconds(15));