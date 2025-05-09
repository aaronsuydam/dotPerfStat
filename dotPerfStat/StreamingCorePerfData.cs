using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Versioning;
using dotPerfStat.PlatformInvoke;
using LibSystem;

namespace dotPerfStat
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
    
    

}