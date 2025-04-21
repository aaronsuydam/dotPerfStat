using dotPerfStat.PlatformInvoke;
using dotPerfStat.Types;

namespace dotPerfStat;


public class MacCPUMetadata : ICPUMetadata
{
    public string BrandName { get; private set; }
}

public class MacCPUCoreMetadata : ICPUCoreMetadata
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

public class MacOS_CPU : IMacCPU
{
    public List<ICPUCore> Cores { get; }
    public ICPUCoreMetadata ArchitectureInformation { get; }
    public MacCPUType ArchitectureFlavor { get; }

    public MacOS_CPU()
    {
        Cores = new List<ICPUCore>();
        ArchitectureInformation = getCPUArchitectureInformation();
        
    }

    private ICPUCoreMetadata getCPUArchitectureInformation()
    {
        MacCPUType arch = __getMacCPUType();
        
        var num_cpus = SYSCTL_BY_NAME.GetSysctlByName<Int32>("hw.ncpu");
        var cpu_info = SYSCTL_BY_NAME.GetSysctlByName<Int32>("hw.cpufrequency");
        var cpu_arch = SYSCTL_BY_NAME.GetSysctlByName<Int32>("hw.cputype");
        return null;
    }
    
    private MacCPUType __getMacCPUType()
    {
        return MacCPUType.AppleSilicon;
    }
}