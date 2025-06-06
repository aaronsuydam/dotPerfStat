using System.Collections.Concurrent;
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
        var output = new ConcurrentQueue<IEnumerable<IStreamingCorePerfData>>();
        Exception error = null;
        var subscriber = Observer.Create<IEnumerable<IStreamingCorePerfData>>(
            onNext: (data) =>
            {
                output.Enqueue(data);
                _testOutputHelper.WriteLine("Add to output.");
            },
            onError: (err) =>
            {
                error = err;
            });
        
        var subscription = _monitor.SubscribeToUpdates(subscriber, 1000);
        Thread.Sleep(10000);
        subscription.Dispose();

        if(error != null)
            Assert.Fail(error.Message);
        
        // Print the retrieved data
        foreach (var core in output.ElementAt(1))
            _testOutputHelper.WriteLine(core.ToString());
        _testOutputHelper.WriteLine($"Number of updates: {output.Count}");
        
        // Assert that we successfully retrieve data
        Assert.True(output.Count > 0);
    }
}