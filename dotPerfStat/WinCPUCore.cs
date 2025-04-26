using System.Diagnostics;
using System.Management;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Versioning;
using dotPerfStat.Types;

namespace dotPerfStat;

[SupportedOSPlatform("windows")]
public class WinCPUCore : ICPUCore
{
    private readonly Subject<IStreamingCorePerfData> _subject = new();
    public IObservable<IStreamingCorePerfData> PerformanceData => _subject.AsObservable();
        
    public ICPUCoreMetadata ArchitectureInformation { get; }

    public u8 CoreNumber { get; } = 0;

    private PerformanceCounter _frequency;
    private PerformanceCounter _utilization;
    private PerformanceCounter _userUtilization;
    private PerformanceCounter _kernelUtilization;
    private u64 max_freq = 0;
    private HiResSleep sw = new();
    
    private Task? monitoringLoop = null;

    /**
     * Currently only supports single-socket systems
     */
    public WinCPUCore(u8 coreNumber)
    {
        CoreNumber = coreNumber;

        string counter_core_id = "0," + CoreNumber.ToString();
        
        // Retrieve the maximum clock speed of the cpu from WMI and store it
        // Create a ManagementObjectSearcher to query Win32_Processor
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("select MaxClockSpeed, Name from Win32_Processor");

        foreach (ManagementObject obj in searcher.Get())
        {
            Debug.WriteLine("Processor Name: " + obj["Name"]);
            Debug.WriteLine("Max Clock Speed: " + obj["MaxClockSpeed"] + " MHz");
            Debug.WriteLine("---------------------------------------");
            max_freq = (u32)obj["MaxClockSpeed"];
        }

        _frequency = new PerformanceCounter("Processor Information", "% Processor Performance", counter_core_id);
        _utilization = new PerformanceCounter("Processor Information", "% Processor Time", counter_core_id);
        _userUtilization   = new PerformanceCounter("Processor Information", "% User Time", counter_core_id);
        _kernelUtilization = new PerformanceCounter("Processor Information", "% Privileged Time", counter_core_id);
    }

    public IDisposable Subscribe(IObserver<IStreamingCorePerfData> observer, u16 update_frequency_ms = 1000)
    {
        if (monitoringLoop == null)
        {
            monitoringLoop = new Task(() =>
            {
                while (true)
                {
                    var data = Update();
                    _subject.OnNext(data);
                    sw.Sleep(update_frequency_ms);
                }
            });
            monitoringLoop.Start();
        }
        return _subject.Subscribe(observer);
    }

    public StreamingCorePerfData Update()
    {
        StreamingCorePerfData newData = new StreamingCorePerfData(sw.GetTimestamp());
        var perf = (UInt64)_frequency.NextValue();
        newData.Frequency = perf * max_freq * 10 * 1000;
        newData.UtilizationPercent = (u64)_utilization.NextValue();
        newData.UtilizationPercentUser = (u64)_userUtilization.NextValue();
        newData.UtilizationPercentKernel = (u64)_kernelUtilization.NextValue();
        return newData;
    }
       
}
