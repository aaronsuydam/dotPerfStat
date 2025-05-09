using System.Diagnostics;
using System.Reactive;
using System.Runtime.Versioning;
using dotPerfStat.Types;

[SupportedOSPlatform("macos")]
public static class Program
{
    public static void Main()
    {
        macOS_CPUCore core = new macOS_CPUCore(3);

        var million = 1000000;
        var subscriber = Observer.Create<IStreamingCorePerfData>(
            onNext: data => Console.WriteLine($"Core {core.CoreNumber} reports Clockspeed of " + data.Frequency / million + " MHz")
            );
        var subscription = core.Subscribe(subscriber, 1000);
        Thread.Sleep(10000);
        subscription.Dispose();
    }
}