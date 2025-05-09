using System.Reactive;
using dotPerfStat;
using Xunit.Abstractions;

namespace dotPerfStatTest;

public class CPUMonitorTests
{
    private CPUMonitor _monitor;
    private ITestOutputHelper _testOutputHelper;

    public CPUMonitorTests(ITestOutputHelper testOutputHelper)
    {
        _monitor = new CPUMonitor();
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void TestSubscribeToUpdates()
    {
        List<IEnumerable<IStreamingCorePerfData>> output = new();
        var subscriber = Observer.Create<IEnumerable<IStreamingCorePerfData>>(
            onNext: (data) =>
            {
                output.Add(data);
            });
        
        var subscription = _monitor.SubscribeToUpdates(subscriber, 1000);
        Thread.Sleep(10000);
        subscription.Dispose();
        
        // Print the retrieved data
        foreach (var core in output.ElementAt(0))
            _testOutputHelper.WriteLine(core.ToString());
        _testOutputHelper.WriteLine($"Number of updates: {output.Count}");
        
        // Assert that we successfully retrieve data
        Assert.True(output.Count > 0);
    }
}