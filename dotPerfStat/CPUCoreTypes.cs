using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Versioning;
using dotPerfStat.PlatformInvoke;
using LibSystem;

namespace dotPerfStat.Types
{

    public class StreamingCorePerfData : IStreamingCorePerfData
    {
        public DateTime Timestamp { get; set; }
        public f32 Frequency { get; set; } = 0;
        public u128 Cycles { get; set; } = 0;
        public u64 UtilizationPercent { get; set; } = 0;
        public u64 UtilizationPercentUser { get; set; } = 0;
        public u64 UtilizationPercentKernel { get; set; } = 0;

        public StreamingCorePerfData(DateTime timestamp)
        {
            Timestamp = timestamp;
        }
        
        public bool IsEmpty()
        {
            return (Frequency == 0 
                    && Cycles == 0 
                    && UtilizationPercent == 0 
                    && UtilizationPercentUser == 0 
                    && UtilizationPercentKernel == 0);
        }

        public override string ToString()
        {
            return $"Timestamp: {Timestamp}\n, Frequency (MHz): {(Frequency/(f32)1000000)}\n, Cycles: {Cycles}\n, Utilization: {UtilizationPercent}%";
        }
    }
    
    [SupportedOSPlatform("windows")]
    public class WinCPUCore : ICPUCore
    {
        private readonly Subject<IStreamingCorePerfData> _subject;
        public IObservable<IStreamingCorePerfData> PerformanceData => _subject.AsObservable();
        
        public ICPUCoreMetadata ArchitectureInformation { get; } = null;
        public IDisposable Subscribe(IObserver<IStreamingCorePerfData> observer, u16 update_frequency_ms = 1000)
        {
            throw new NotImplementedException();
        }

        public IDisposable Subscribe(IObserver<IStreamingCorePerfData> observer)
        {
            throw new NotImplementedException();
        }

        public u8 CoreNumber { get; } = 0;

        private PerformanceCounter _frequency;
        private PerformanceCounter _utilization;
        private HiResSleep sw = new();

        /**
         * Currently only supports single-socket systems
         */
        public WinCPUCore(u8 coreNumber)
        {
            CoreNumber = coreNumber;

            string counter_core_id = "0," + CoreNumber.ToString();

            _frequency = new PerformanceCounter("Processor Information", "% Processor Performance", counter_core_id);
            _utilization = new PerformanceCounter("Processor Information", "% Processor Time", counter_core_id);
        }
        
        public void Update()
        {
            StreamingCorePerfData newData = new StreamingCorePerfData(sw.GetTimestamp());
            newData.Frequency = (UInt64)_frequency.NextValue();
            newData.UtilizationPercent = (u64)_utilization.NextValue();
            _subject.OnNext(newData);
        }
       
    }
    
    [SupportedOSPlatform("macos")]
    public partial class MacCPUCore : ICPUCore
    {
        public u8 CoreNumber { get; internal set; }

        private readonly BehaviorSubject<IStreamingCorePerfData> _subject = new(new StreamingCorePerfData(DateTime.Now));
        public IObservable<IStreamingCorePerfData> PerformanceData => _subject.AsObservable();
        
        public ICPUCoreMetadata ArchitectureInformation { get; }

        private Task? monitoringTask = null;
        private u16 update_frequency_ms = 1000;
        private HiResSleep sw;
        
        public MacCPUCore(u8 coreNumber)
        {
            CoreNumber = coreNumber;
            sw = new HiResSleep();
        }

        // Subscribe to the data source.
        public IDisposable Subscribe(IObserver<IStreamingCorePerfData> observer, u16 update_frequency_ms = 1000)
        {
            this.update_frequency_ms = update_frequency_ms;
            if (monitoringTask == null)
            {
                monitoringTask = new Task(() =>
                {
                    while (true)
                    {
                        var data = MonitoringLoopIteration();
                        _subject.OnNext(data);
                        sw.Sleep(update_frequency_ms);
                    }
                });
                monitoringTask.Start();
            }
            return _subject.Subscribe(observer);
        }

        private StreamingCorePerfData MonitoringLoopIteration()
        {
            StreamingCorePerfData newData = new(sw.GetTimestamp());
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
            if (!_subject.Value.IsEmpty()) // this will only be false for the first invocation
            {
                u128 old_cycles = _subject.Value.Cycles;
                u128 delta_cycles = newData.Cycles - old_cycles;
                f64 elapsed_seconds = (newData.Timestamp.Subtract(_subject.Value.Timestamp).TotalSeconds);
                f32 frequency_hz = (f32)((f32)delta_cycles / elapsed_seconds);
                newData.Frequency = frequency_hz;
            }
            
            return newData;
        }
        
        public StreamingCorePerfData Update()
        {
            var updated_values = MonitoringLoopIteration();
            _subject.OnNext(updated_values);
            return updated_values;
        }
        
    }
}