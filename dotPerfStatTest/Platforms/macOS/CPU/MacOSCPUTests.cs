using System.Reactive;
using System.Runtime.Versioning;
using dotPerfStat;
using Xunit.Abstractions;

namespace dotPerfStatTest;

using dotPerfStat.Platforms.macOS;


[SupportedOSPlatform("macos")]
public class MacOSCPUTests
{
    
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly MacOS_CPU _cpu;

    public MacOSCPUTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _cpu = new MacOS_CPU();
    }

    [SkippableFact]
    public void CorrectNumberOfCores()
    {
        // Assert that we successfully retrieve data
        Assert.True(_cpu.Cores.Count() == Environment.ProcessorCount);
    }
    
    [SkippableFact]
    public void TestGetCPUFrequency()
    {
        List<IEnumerable<IStreamingCorePerfData>> output = new();
        output.Add(_cpu.ManualUpdate());
        Thread.Sleep(1000);
        output.Add(_cpu.ManualUpdate());
        
        // Assert that we successfully retrieve data
        Assert.True(output.Count > 1);
        
        // Assert that we retrieve data for every core in the cpu
        Assert.True(output.ElementAt(0).Count() == _cpu.Cores.Count() && 
                    output.ElementAt(1).Count() == _cpu.Cores.Count());
        
        // Print the data
        foreach (var core in output.ElementAt(1))
        {
            _testOutputHelper.WriteLine(core.ToString());
        }
    }
    
    [SkippableFact]
    public void MultipleUpdates()
    {
        List<IEnumerable<IStreamingCorePerfData>> output = new();
        for (int i = 0; i < 10; i++)
        {
            output.Add(_cpu.ManualUpdate());
            Thread.Sleep(1000);
        }

        // Print the data
        foreach (var core in output.ElementAt(8))
        {
            _testOutputHelper.WriteLine(core.ToString());
        }
    }

    /// <summary>
    /// Tests the latency of performing a single manual update of the CPU performance data.
    /// </summary>
    /// <remarks>
    /// This test measures the time taken for a single call to the <see cref="MacOS_CPU.ManualUpdate"/> method.
    /// It uses the <see cref="HiResSleep.GetTimestamp"/> method to obtain high-resolution timestamps before
    /// and after the manual update. The measured latency is asserted to be below 5 milliseconds.
    /// </remarks>
    /// <exception cref="Xunit.Sdk.XunitException">
    /// Thrown if the measured latency exceeds the expected threshold of 5 milliseconds.
    /// </exception>
    [SkippableFact]
    public void TestLatencyOneUpdate()
    {
        HiResSleep sw = new();
        DateTime start = sw.GetTimestamp();
        var output = _cpu.ManualUpdate();
        DateTime end = sw.GetTimestamp();
        
        var latency = end - start;
        Assert.True(latency.TotalMilliseconds < 5);
        _testOutputHelper.WriteLine($"Latency of single update: {latency.TotalMilliseconds} ms");
    }

    [SkippableFact]
    public void ConstrainSubscriberLatency()
    {
        List<IEnumerable<IStreamingCorePerfData>> output = new();
        var subscriber = Observer.Create<IEnumerable<IStreamingCorePerfData>>(
            onNext: (data) =>
            {
                output.Add(data);
            });
        
        var subscription = _cpu.SubscribeToAllUpdates(subscriber, 1000);
    }
}