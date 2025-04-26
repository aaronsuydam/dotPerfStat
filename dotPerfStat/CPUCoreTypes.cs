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
            return $"Timestamp: {Timestamp}\n, " +
                   $"Frequency (MHz): {(Frequency/(f32)1000000)}\n," +
                   $" Cycles: {Cycles}\n, " +
                   $"Utilization (total): {UtilizationPercent}%\n" +
                   $"Utilization (user): {UtilizationPercentUser}%\n" +
                   $"Utilization (kernel): {UtilizationPercentKernel}%\n";
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
    

}