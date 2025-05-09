namespace dotPerfStat;

using System.Diagnostics;

public class HiResSleep
{
    private readonly Stopwatch sw;
    private readonly DateTime start;
    private double ticks = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;
    
    public HiResSleep()
    {
        sw = Stopwatch.StartNew();
        start = DateTime.Now;
    }
    
    public void Sleep(u32 milliseconds)
    {
        // Capture the current high-res counter at invocation
        long startCounter = Stopwatch.GetTimestamp();
        // Compute how many raw counter ticks correspond to the requested milliseconds
        long ticksToWait = (long)(Stopwatch.Frequency * (milliseconds / 1000.0));
        long targetCounter = startCounter + ticksToWait;
    
        // Busy-spin until we reach or exceed that counter value
        while (Stopwatch.GetTimestamp() < targetCounter)
        {
            Thread.SpinWait(1);
        }
    }

    public DateTime GetTimestamp()
    {
        return start.AddTicks((long)(sw.ElapsedTicks * ticks));
    }
}