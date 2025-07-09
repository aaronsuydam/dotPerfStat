using System.Runtime.InteropServices;
using dotPerfStat.Interfaces.GPU;
using dotPerfStat.Platforms.Windows;
using System.Reactive.Disposables;

namespace dotPerfStat.Services
{
    
    public class GPUMonitor
    {

        public string Name => _internalGPU.Name;
        public string Vendor => _internalGPU.Vendor;
        public string DeviceId => _internalGPU.DeviceId;

        private IGPU _internalGPU;
        
        
        public static IGPU GPUFactory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // TODO: Implement MacOS_GPU when available
                throw new NotImplementedException("macOS GPU monitoring is not yet implemented. Please open an issue on GitHub.");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsGPU("Unknown", "Unknown", "Unknown");
            }
            else 
                throw new NotImplementedException("Your platform is not supported. Please open an issue on GitHub.");
        }

        public GPUMonitor()
        {
            _internalGPU = GPUFactory();
        }

        public IDisposable SubscribeToUpdates(IObserver<IList<IStreamingGPUPerfStats>> observer, u32 updateFrequencyMs)
        {
            return _internalGPU.SubscribeToAllUpdates(observer, updateFrequencyMs);
        }

        public IEnumerable<IStreamingGPUPerfStats> GetCurrentStats()
        {
            return _internalGPU.ManualUpdate();
        }

        public float GetUtilization()
        {
            var stats = _internalGPU.ManualUpdate().FirstOrDefault();
            return stats?.Utilization ?? 0.0f;
        }

     

    }
}