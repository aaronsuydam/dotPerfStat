using System.Reactive.Linq;
using System.Runtime.Versioning;
using dotPerfStat.PlatformInvoke;
using dotPerfStat.Types;

namespace dotPerfStat;


public class MacCPUMetadata : ICPUMetadata
{
    public string BrandName { get; internal set; }
    public MacCPUType ArchitectureFlavor { get; internal set; }
    public i8 num_cores { get; internal set; }
}

public class MacCPUCoreMetadata : ICPUCoreMetadata
{
    public u32 L1ICacheSize { get; internal set; }
    public u32 L1DCacheSize { get; internal set; }
    public u32 L2CacheSize { get; internal set; }
    public u64 MaxHWFrequency { get; internal set; }
    public u64 MinHWFrequency { get; internal set; }
    public CoreType CoreType { get; internal set; }
    public bool SupportsSMT { get; internal set; }
    public bool L2CacheIsShared { get; internal set; }
}

[SupportedOSPlatform("macos")]
public class MacOS_CPU : IMacCPU
{
    public List<ICPUCore> Cores { get; }
    public ICPUMetadata CPUArchInfo { get; }
    public ICPUCoreMetadata CoreArchInfo { get; }
    public ICPUCoreMetadata ArchitectureInformation { get; }
    public MacCPUType ArchitectureFlavor { get; }

    public MacOS_CPU()
    {
        Cores = new List<ICPUCore>();
        CPUArchInfo = getCPUArchitectureInformation();

        for (int i = 0; i < CPUArchInfo.num_cores; i++)
        {
            MacCPUCore new_core = new MacCPUCore((u8)i);
        }
    }

    private ICPUMetadata getCPUArchitectureInformation()
    {
        MacCPUMetadata cpu_info = new MacCPUMetadata();
        cpu_info.BrandName = SYSCTL_BY_NAME.GetSysctlByName<String>("hw.model");
        
        MacCPUType arch = __getMacCPUType();
        cpu_info.ArchitectureFlavor = arch;
        
        var num_cpus = SYSCTL_BY_NAME.GetSysctlByName<Int32>("hw.ncpu");
        cpu_info.num_cores = (i8)num_cpus;

        return cpu_info;
    }
    
    private MacCPUType __getMacCPUType()
    {
        return MacCPUType.AppleSilicon;
    }

    public IDisposable SubscribeAllCores(IObserver<IList<IStreamingCorePerfData>> observer)
    {
        var zipped = Observable.Zip(Cores.Select(core => core.PerformanceData));
        return zipped.Subscribe(observer);
    }

    public IDisposable Subscribe(IObserver<IStreamingCorePerfData> observer, u8 coreNumber)
    {
        var core = Cores.ElementAt(coreNumber);
        return core.Subscribe(observer);
    }
}