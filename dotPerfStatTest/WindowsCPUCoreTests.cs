namespace dotPerfStatTest;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Runtime.Versioning;
using System.Threading;
using dotPerfStat;
using dotPerfStat.Types;
using Xunit;
using Xunit.Abstractions;

[SupportedOSPlatform("windows")]
public class WindowsCPUCoreTests
{
    private readonly WinCPUCore _core;
    private ITestOutputHelper _testOutputHelper;

    public WindowsCPUCoreTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        // Arrange: test Core 0 (you may wish to parametrize for other cores)
        _core = new WinCPUCore(0);
    }
    
    [Fact]
    public void Constructor_SetsCoreNumber()
    {
        // Act & Assert
        Assert.Equal((byte)0, _core.CoreNumber);
    }
    
    [Fact]
    public void MonitoringLoopUpdatesValues()
    {
        var initial = _core.Update();
        _testOutputHelper.WriteLine(initial.ToString());
        Thread.Sleep(1001);
        var updated = _core.Update();
        _testOutputHelper.WriteLine(updated.ToString());
        Assert.NotEqual(initial.Timestamp, updated.Timestamp);
    }

    [Fact]
    public void MonitoringLoopValuesInRange()
    {
        var initial = _core.Update();
        _testOutputHelper.WriteLine(initial.ToString());
        Thread.Sleep(1001);
        var updated = _core.Update();
        _testOutputHelper.WriteLine(updated.ToString());
        Assert.InRange(updated.UtilizationPercent, (u64)0, (u64)100);
        Assert.InRange(updated.UtilizationPercent, (u64)0, (u64)100);
        Assert.InRange(updated.UtilizationPercent, (u64)0, (u64)100);
        Assert.Equal(updated.UtilizationPercentKernel + updated.UtilizationPercentUser, updated.UtilizationPercent);
    }
    
  
    [Fact]
    public void MonitoringLoopMultipleUpdates()
    {
        const int iterations = 10;
        const double maxMs = 1.0;
        var errors = new List<string>();

        for (int i = 1; i <= iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var data = _core.Update();
            sw.Stop();
            double elapsed = sw.Elapsed.TotalMilliseconds;
            _testOutputHelper.WriteLine(data.ToString());
            if (elapsed > maxMs)
                errors.Add($"Iteration {i} took {elapsed:F4} ms (> {maxMs} ms)");
            Thread.Sleep(1001);
        }

        if (errors.Any())
        {
            foreach (var error in errors)
                _testOutputHelper.WriteLine(error);
        }
    }
    
    
    [Fact]
    public void MonitoringLoopSubscriber()
    {
        List<IStreamingCorePerfData> output = new();
        var subscriber = Observer.Create<IStreamingCorePerfData>(
            onNext: data =>
            {
                _testOutputHelper.WriteLine(data.ToString());
                output.Add(data);
            });
        using var subscription = _core.Subscribe(subscriber, 1000);
        Thread.Sleep(100000);
        subscription.Dispose();

        // Compute inter-timestamp intervals in milliseconds
        var deltas = output
            .Zip(output.Skip(1), (prev, next) => (next.Timestamp - prev.Timestamp).TotalMilliseconds)
            .ToList();

        int total = deltas.Count;
        int slowCount = deltas.Count(d => d > 1001);
        double allowedFraction = 0.02; // 1%
        _testOutputHelper.WriteLine($"Total intervals: {total}, Slow intervals (>1001ms): {slowCount}");

        
        // show the length of the iterations out of range
        for (int i = 0; i < deltas.Count; i++)
        {
            if(deltas[i] > 1001)
                _testOutputHelper.WriteLine($"Iteration {i} took {deltas[i]:F4} ms (> 1001 ms)");
        }
        
        Assert.True(
            slowCount < total * allowedFraction,
            $"Too many slow intervals: {slowCount}/{total} ({(double)slowCount/total:P2}) exceeded 1001ms"
        );
    }
}