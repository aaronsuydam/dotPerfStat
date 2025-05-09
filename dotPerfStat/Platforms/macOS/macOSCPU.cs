using System.Reactive;
using System.Reactive.Disposables;

namespace dotPerfStat.Platforms.macOS;

using System.Reactive.Linq;
using System.Runtime.Versioning;
using PlatformInvoke;
using Types;
using Interfaces.CPU;

public interface IMacCPU : ICPU
{
    public MacCPUType ArchitectureFlavor { get; }
}

public enum MacCPUType
{
    Intel,
    AppleSilicon
}

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
            macOS_CPUCore new_core = new macOS_CPUCore((u8)i);
            Cores.Add(new_core);
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

    public CompositeDisposable SubscribeToAllUpdates(IObserver<IList<IStreamingCorePerfData>> observer, u32 updateFrequencyMs = 1000)
    {
        List<IDisposable> subscriptions = new();
        foreach (var core in Cores)
        {
            subscriptions.Add(
                core.Subscribe(Observer.Create<IStreamingCorePerfData>(_ => { }), updateFrequencyMs)
                );
        }
        var zipped = Observable.Zip(Cores.Select(core => core.PerformanceData));
        var all_subscription = zipped.Subscribe(observer);
        subscriptions.Add(all_subscription);
        return new CompositeDisposable(subscriptions);
    }

    public IDisposable SubscribeToCoreUpdates(
            IObserver<IStreamingCorePerfData> observer, 
            u8 coreNumber,
            u32 updateFrequencyMs = 1000)
    {
        var core = Cores.ElementAt(coreNumber);
        return core.Subscribe(observer, updateFrequencyMs);
    }
    
    public IEnumerable<IStreamingCorePerfData> ManualUpdate()
    {
        List<IStreamingCorePerfData> output = new();
        foreach (var core in Cores)
        {
            output.Add(core.Update());
        }
        return output;
    }

}