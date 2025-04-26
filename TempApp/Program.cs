using System.Diagnostics;


public static class Program
{
    public static void Main()
    {
        var _userUtilization   = new PerformanceCounter("Processor Information", "% User Time", "0,0");

    }
}