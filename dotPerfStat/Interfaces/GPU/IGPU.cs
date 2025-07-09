using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace dotPerfStat.Interfaces.GPU
{
    /// <summary>
    /// Important note, IGPU here stands for the interface type corresponding to the GPU, not an Integrated GPU.
    /// </summary>
    public interface IGPU
    {
        public string Name { get; }
        public string Vendor { get; }
        public string DeviceId { get; }

        public IEnumerable<IStreamingGPUPerfStats> ManualUpdate();

        public CompositeDisposable SubscribeToAllUpdates(IObserver<IList<IStreamingGPUPerfStats>> observer,
            u32 updateFrequencyMs);
        public IDisposable SubscribeToCoreUpdates(IObserver<IStreamingGPUPerfStats> observer, u8 coreNumber,
            u32 updateFrequencyMs);
    }


    public interface IStreamingGPUPerfStats
    {
        public float Utilization { get; }
    }
}
