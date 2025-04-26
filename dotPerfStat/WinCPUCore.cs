using System.Diagnostics;
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
    private HiResSleep sw = new();
    
    private Task? monitoringLoop = null;

    /**
     * Currently only supports single-socket systems
     */
    public WinCPUCore(u8 coreNumber)
    {
        CoreNumber = coreNumber;

        string counter_core_id = "0," + CoreNumber.ToString();

        _frequency = new PerformanceCounter("Processor Information", "% Processor Performance", counter_core_id);
        _utilization = new PerformanceCounter("Processor Information", "% Processor Time", counter_core_id);
        _userUtilization   = new PerformanceCounter("Processor", "% User Time", counter_core_id);
        _kernelUtilization = new PerformanceCounter("Processor", "% Privileged Time", counter_core_id);
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
        newData.Frequency = (UInt64)_frequency.NextValue();
        newData.UtilizationPercent = (u64)_utilization.NextValue();
        newData.UtilizationPercentUser = (u64)_userUtilization.NextValue();
        newData.UtilizationPercentKernel = (u64)_kernelUtilization.NextValue();
        return newData;
    }
       
}
