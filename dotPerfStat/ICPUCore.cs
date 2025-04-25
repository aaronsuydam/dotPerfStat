namespace dotPerfStat.Types;


public interface ICPU
{
    public List<ICPUCore> Cores { get; }
    
    public ICPUMetadata CPUArchInfo { get; }
    
    public ICPUCoreMetadata CoreArchInfo { get; }
}

public interface ICPUMetadata
{
    // The model name of the CPU
    public string BrandName { get; }
    
    public i8 num_cores { get; }
}

public interface IMacCPU : ICPU
{
    public MacCPUType ArchitectureFlavor { get; }
}

public interface ICPUCore
{
    public u8 CoreNumber { get; }
   
    public IObservable<IStreamingCorePerfData> PerformanceData { get; }

    public ICPUCoreMetadata ArchitectureInformation { get; }
    
    public IDisposable Subscribe(IObserver<IStreamingCorePerfData> observer);
}

public interface IStreamingCorePerfData
{
    public u64 Frequency { get; }
    public u128 Cycles { get; }
    public u64 UtilizationPercent { get; }
    public u64 UtilizationPercentUser { get; }
    public u64 UtilizationPercentKernel { get; }
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

public enum MacCPUType
{
    Intel,
    AppleSilicon
}