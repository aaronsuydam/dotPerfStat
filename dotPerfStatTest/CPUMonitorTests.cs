using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using dotPerfStat.Services;
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
    public async Task TestSubscribeToUpdates()
    {
        // Wrap the subscription in an observable
        var observable = Observable.Create<IEnumerable<IStreamingCorePerfData>>(observer =>
            _monitor.SubscribeToUpdates(observer, 1000));

        // Await exactly two updates
        var updates = await observable
            .Take(3)        // take exactly two emissions
            .ToArray()      // buffer into array
            .ToTask();      // convert to Task

        // Log the second update
        foreach (var core in updates[2])
            _testOutputHelper.WriteLine(core.ToString());
        _testOutputHelper.WriteLine($"Number of updates: {updates.Length}");

        // Assert exactly two updates were received
        Assert.Equal(3, updates.Length);
    }
}