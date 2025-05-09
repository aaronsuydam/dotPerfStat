using System.Reactive.Disposables;

namespace dotPerfStat.Interfaces.CPU;

/**
 *  This file defines interfaces for top-level CPU types. Core interfaces are defined in ICPUCore.cs
 */

public interface ICPU
{
    public List<ICPUCore> Cores { get; }
    
    public ICPUMetadata CPUArchInfo { get; }
    
    public ICPUCoreMetadata CoreArchInfo { get; }

    public IEnumerable<IStreamingCorePerfData> ManualUpdate();
    
    public CompositeDisposable SubscribeToAllUpdates(IObserver<IList<IStreamingCorePerfData>> observer,
        u32 updateFrequencyMs);
    public IDisposable SubscribeToCoreUpdates(IObserver<IStreamingCorePerfData> observer, u8 coreNumber,
        u32 updateFrequencyMs);
}

public interface ICPUMetadata
{
    // The model name of the CPU
    public string BrandName { get; }
    
    public i8 num_cores { get; }
}

