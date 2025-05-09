using System.Reactive.Disposables;
using dotPerfStat.Interfaces.CPU;

namespace dotPerfStat;

public class WindowsCPU : ICPU
{
    public List<ICPUCore> Cores { get; }
    public ICPUMetadata CPUArchInfo { get; }
    public ICPUCoreMetadata CoreArchInfo { get; }
    public IEnumerable<IStreamingCorePerfData> ManualUpdate()
    {
        throw new NotImplementedException();
    }

    public CompositeDisposable SubscribeToAllUpdates(IObserver<IList<IStreamingCorePerfData>> observer, u32 updateFrequencyMs)
    {
        throw new NotImplementedException();
    }

    public IDisposable SubscribeToCoreUpdates(IObserver<IStreamingCorePerfData> observer, u8 coreNumber, u32 updateFrequencyMs)
    {
        throw new NotImplementedException();
    }
}