using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Versioning;
using dotPerfStat.Interfaces.CPU;
using dotPerfStat.PlatformInvoke;
using LibSystem;

namespace dotPerfStat.Platforms.macOS;

[SupportedOSPlatform("macos")]
public partial class MacosCPUCore : ICPUCore
{
    public u8 CoreNumber { get; internal set; }

    private readonly BehaviorSubject<IStreamingCorePerfData> _subject = new(new StreamingCorePerfData(DateTime.Now));
    public IObservable<IStreamingCorePerfData> PerformanceData => _subject.AsObservable();
    
    public ICPUCoreMetadata ArchitectureInformation { get; }

    private Task? _monitoringTask = null;
    private u32 _updateFrequencyMs = 1000;
    private CPULoadInfo _currentTicks = new();
    private readonly HiResSleep _sw;
    
    public MacosCPUCore(u8 coreNumber)
    {
        CoreNumber = coreNumber;
        _sw = new HiResSleep();
    }

    // Subscribe to the data source.
    public IDisposable Subscribe(IObserver<IStreamingCorePerfData> observer, u32 updateFrequencyMs = 1000)
    {
        this._updateFrequencyMs = updateFrequencyMs;
        if (_monitoringTask == null)
        {
            _monitoringTask = new Task(() =>
            {
                while (true)
                {
                    try
                    {
                        var data = MonitoringLoopIteration();
                        _subject.OnNext(data);
                    }
                    catch (Exception ex)
                    {
                        _subject.OnError(ex);
                        break;
                    }
                    _sw.Sleep(updateFrequencyMs);
                }
            });
            _monitoringTask.Start();
        }
        return _subject.Subscribe(observer);
    }

    private StreamingCorePerfData MonitoringLoopIteration()
    {
        StreamingCorePerfData newData = new(_sw.GetTimestamp());
        // First, ask how many fixed-function counters the kernel supports
        u32 nCtrs = (u32)KPCNative.kpc_get_counter_count(KPCNative.KPC_CLASS_FIXED_MASK);
        if (nCtrs == 0)
            throw new InvalidOperationException("No fixed counters available");
        
        int totalCores = Environment.ProcessorCount;
        u64[] data = new u64[nCtrs * totalCores];
        i32 rc = KPCNative.kpc_get_cpu_counters(true, KPCNative.KPC_CLASS_FIXED_MASK, out _, data);
        
        if (rc != 0)
            throw new InvalidOperationException($"kpc_get_cpu_counters failed: {rc}");
        
        newData.Cycles = data[this.CoreNumber * nCtrs + 0];
        bool can_calc_freq = _subject.Value.IsEmpty();
        if (!can_calc_freq)
        {
            u128 old_cycles = _subject.Value.Cycles;
            u128 delta_cycles = newData.Cycles - old_cycles;
            f64 elapsed_seconds = (newData.Timestamp.Subtract(_subject.Value.Timestamp).TotalSeconds);
            f32 frequency_hz = (f32)((f32)delta_cycles / elapsed_seconds);
            newData.Frequency = frequency_hz;
        }

        
        // LoadInfo contains Ticks. We need to math to convert them to percentages.
        var loadInfo = NativeMethods.GetHostProcessorInfo(this.CoreNumber);
        if (!_currentTicks.IsEmpty())
        {
            CPULoadInfo elapsed_ticks = new();
            elapsed_ticks.Idle = loadInfo.Idle - _currentTicks.Idle;
            elapsed_ticks.Nice = loadInfo.Nice - _currentTicks.Nice;
            elapsed_ticks.System = loadInfo.System - _currentTicks.System;
            elapsed_ticks.User = loadInfo.User - _currentTicks.User;
            var total_ticks = elapsed_ticks.Idle + elapsed_ticks.Nice + elapsed_ticks.System + elapsed_ticks.User;
        
            float system_div_total = (f32) elapsed_ticks.System / (f32) total_ticks;
            newData.UtilizationPercentKernel = (u32)(system_div_total * 100);
            float user_div_total = (f32) elapsed_ticks.User / (f32) total_ticks;
            newData.UtilizationPercentUser = (u32)(user_div_total * 100);
            float nice_perc = (u32) (((f32) elapsed_ticks.Nice / (f32) total_ticks) * 100);
            newData.UtilizationPercent = (u64)(newData.UtilizationPercentKernel + newData.UtilizationPercentUser);
        }
        _currentTicks = loadInfo;
        
        return newData;
    }
    
    public StreamingCorePerfData Update()
    {
        try
        {
            var updated_values = MonitoringLoopIteration();
            _subject.OnNext(updated_values);
            return updated_values;
        }
        catch (Exception ex)
        {
            _subject.OnError(ex);
            throw;
        }
    }

    public void SetUpdateFrequency(u32 updateFrequencyMs)
    {
        this._updateFrequencyMs = updateFrequencyMs;
    }
}