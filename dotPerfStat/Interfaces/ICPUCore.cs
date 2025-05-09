using dotPerfStat.Types;

namespace dotPerfStat.Interfaces.CPU;

/**
 * This file defines interfaces related to CPU Cores.
 */

public interface ICPUCore
{
    public u8 CoreNumber { get; }
   
    public IObservable<IStreamingCorePerfData> PerformanceData { get; }

    public ICPUCoreMetadata ArchitectureInformation { get; }
    
    public IDisposable Subscribe(IObserver<IStreamingCorePerfData> observer, u32 update_frequency_ms = 1000);
    
    public StreamingCorePerfData Update();
}

public interface ICPUCoreMetadata
{
    public u32 L1ICacheSize { get; }
    public u32 L1DCacheSize { get; }
    public u32 L2CacheSize { get; }
    
    public u64 MaxHWFrequency { get; }
    public u64 MinHWFrequency { get; }
    
    public CoreType CoreType { get; }

    public bool SupportsSMT { get; }
    public bool L2CacheIsShared { get; }
}

public enum CoreType
{
    Performance,
    Standard,
    Efficiency
}

