using System.Diagnostics;
using System.Reactive;
using System.Runtime.Versioning;
using dotPerfStat.Platforms.macOS;

[SupportedOSPlatform("macos")]
public static class Program
{
    public static void Main()
    {
        MacosCPUCore core = new MacosCPUCore(3);

        var million = 1000000;
        var subscriber = Observer.Create<IStreamingCorePerfData>(
            onNext: data => Console.WriteLine($"Core {core.CoreNumber} reports Clockspeed of " + data.Frequency / million + " MHz")
            );
        var subscription = core.Subscribe(subscriber, 1000);
        Thread.Sleep(10000);
        subscription.Dispose();
    }
}