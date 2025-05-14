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
        public i8 CoreNumber { get; set; } = -1;
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
            return (CoreNumber == -1);
        }

        public override string ToString()
        {
            return $"Core: {CoreNumber}\n" +
                   $"   Timestamp: {Timestamp}\n" +
                   $"   Frequency (MHz): {(Frequency/(f32)1000000)}\n" +
                   $"   Cycles: {Cycles/1000000000}\n " +
                   $"   Utilization (total): {UtilizationPercent}%\n" +
                   $"   Utilization (user): {UtilizationPercentUser}%\n" +
                   $"   Utilization (kernel): {UtilizationPercentKernel}%\n";
        }
    }
    
    

}