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
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public u64 Frequency { get; set; } = 0;
        public u128 Cycles { get; set; } = 0;
        public u64 UtilizationPercent { get; set; } = 0;
        public u64 UtilizationPercentUser { get; set; } = 0;
        public u64 UtilizationPercentKernel { get; set; } = 0;
    }
    
    [SupportedOSPlatform("windows")]
    public class WinCPUCore : ICPUCore
    {
        private readonly Subject<IStreamingCorePerfData> _subject;
        public IObservable<IStreamingCorePerfData> PerformanceData => _subject.AsObservable();
        
        public ICPUCoreMetadata ArchitectureInformation { get; } = null;
        public IDisposable Subscribe(IObserver<IStreamingCorePerfData> observer)
        {
            throw new NotImplementedException();
        }

        public u8 CoreNumber { get; } = 0;

        private PerformanceCounter _frequency;
        private PerformanceCounter _utilization;

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
            StreamingCorePerfData newData = new StreamingCorePerfData();
            newData.Frequency = (UInt64)_frequency.NextValue();
            newData.UtilizationPercent = (u64)_utilization.NextValue();
            _subject.OnNext(newData);
        }
       
    }
    
    [SupportedOSPlatform("macos")]
    public partial class MacCPUCore : ICPUCore
    {
        public u8 CoreNumber { get; internal set; }

        private readonly Subject<IStreamingCorePerfData> _subject = new();
        public IObservable<IStreamingCorePerfData> PerformanceData => _subject.AsObservable();
        
        public ICPUCoreMetadata ArchitectureInformation { get; }

        private Task? monitoringTask = null;
        
        public MacCPUCore(u8 coreNumber)
        {
            CoreNumber = coreNumber;
            
        }

        // Subscribe to the data source.
        public IDisposable Subscribe(IObserver<IStreamingCorePerfData> observer)
        {
            if (monitoringTask == null)
            {
                monitoringTask = new Task(() =>
                {
                    while (true)
                    {
                        var data = MonitoringLoopIteration();
                        _subject.OnNext(data);
                        Thread.Sleep(1000);
                    }
                });
                monitoringTask.Start();
            }
            return _subject.Subscribe(observer);
        }

        private StreamingCorePerfData MonitoringLoopIteration()
        {
            StreamingCorePerfData newData = new();
            // First, ask how many fixed-function counters the kernel supports
            uint nCtrs = (uint)KPCNative.kpc_get_counter_count(KPCNative.KPC_CLASS_FIXED_MASK);
            if (nCtrs == 0)
                throw new InvalidOperationException("No fixed counters available");

            // Allocate an array large enough for all fixed counters
            ulong[] buf = new ulong[nCtrs];

            // Fetch exactly that many counters into our buffer
            i32 rc = KPCNative.kpc_get_cpu_counters(true, KPCNative.KPC_CLASS_FIXED_MASK, out nCtrs, buf);
            if (rc != 0)
            {
                throw new InvalidOperationException(rc.ToString());
            }
            else
            {
                newData.Frequency = (UInt64)buf[0];
                newData.UtilizationPercent = (u64)buf[1];
            }
            return newData;
        }
        
        public StreamingCorePerfData Update()
        {
            return MonitoringLoopIteration();
        }
        
    }
}