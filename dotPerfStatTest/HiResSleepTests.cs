using System;
using System.Collections.Generic;
using System.Linq;
using dotPerfStat;
using Xunit;
using Xunit.Abstractions;

namespace dotPerfStatTest;

public class HiResSleepTests
{
    private HiResSleep sw;
    private ITestOutputHelper output;
    public HiResSleepTests(ITestOutputHelper output)
    {
        sw = new HiResSleep();
        this.output = output;
    }
    
    [Fact]
    public void TestTimestamp()
    {
        DateTime ts = sw.GetTimestamp();
        DateTime now = DateTime.UtcNow;
        TimeSpan skew = now - ts.ToUniversalTime();
        output.WriteLine($"Timestamp: {ts:O}");
        output.WriteLine($"System UTC Now: {now:O}");
        output.WriteLine($"Skew: {skew.TotalMilliseconds} ms");
        Assert.True(ts.Date != DateTime.MinValue, 
            $"GetTimestamp returned invalid date: {ts:O}");
    }
    
    [Fact]
    public void TestSleep()
    {
        const int milliseconds = 1000;
        DateTime start = sw.GetTimestamp();
        sw.Sleep(milliseconds);
        DateTime end = sw.GetTimestamp();
        TimeSpan elapsed = end - start;
        output.WriteLine($"Start:     {start:O}");
        output.WriteLine($"End:       {end:O}");
        output.WriteLine($"Elapsed:   {elapsed.TotalMilliseconds} ms");
        output.WriteLine($"Expected:  >= {milliseconds} ms");
        Assert.True(elapsed >= TimeSpan.FromMilliseconds(milliseconds),
            $"Sleep too short: {elapsed.TotalMilliseconds} ms elapsed, expected at least {milliseconds} ms");
    }

    [Fact]
    public void TestSleepPrecision()
    {
        const int milliseconds = 1000;
        const double maxDriftMs = 1.0;
        const int iterations = 5;
        var drifts = new List<double>(iterations);
        
        for (int i = 1; i <= iterations; i++)
        {
            DateTime start = sw.GetTimestamp();
            sw.Sleep(milliseconds);
            DateTime end = sw.GetTimestamp();
            double elapsedMs = (end - start).TotalMilliseconds;
            double driftMs = elapsedMs - milliseconds;
            drifts.Add(driftMs);
            output.WriteLine($"Run {i}: Elapsed={elapsedMs:F4} ms, Drift={driftMs:F4} ms");
        }
        
        double maxObservedDrift = drifts.Max();
        double minObservedDrift = drifts.Min();
        double avgDrift = drifts.Average();
        output.WriteLine($"Drift summary: min={minObservedDrift:F4} ms, max={maxObservedDrift:F4} ms, avg={avgDrift:F4} ms");
        
        Assert.All(drifts, drift =>
            Assert.InRange(drift, 0, maxDriftMs));
    }
}