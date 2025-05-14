using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using System.Reactive;
using System.Runtime.Versioning;
using dotPerfStat;
using dotPerfStat.Platforms.macOS;

[SupportedOSPlatform("macos")]
public static class Program
{
    public static void Main()
    {
        var newmon = new CPUMonitor();
        var output = new ConcurrentQueue<IEnumerable<IStreamingCorePerfData>>();
        Exception error = null;
        var subscriber = Observer.Create<IEnumerable<IStreamingCorePerfData>>(
            onNext: data =>
            {
                output.Enqueue(data);   // ← key: thread-safe enqueue
                Console.WriteLine(data.ElementAt(0).ToString());
            },
            onError: (err) =>
            {
                error = err;
            });
        
        // high-resolution timer for polling
        var hiRes = new HiResSleep();
        // start a dedicated thread to wait for two updates without blocking the monitor's run loop
        var poller = new Thread(() =>
        {
            while (output.Count < 2)
            {
                hiRes.Sleep(1000);
            }
            Console.WriteLine($"Received two updates. Total queued updates: {output.Count}");
        });
        poller.Start();
        newmon.SubscribeToUpdates(subscriber, updateFrequencyMs: 1000);
        // wait for the poller to finish before exiting
        poller.Join();
        
    }
}