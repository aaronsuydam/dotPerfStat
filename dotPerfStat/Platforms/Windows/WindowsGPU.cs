using dotPerfStat.Interfaces.GPU;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace dotPerfStat.Platforms.Windows
{

    public class StreamingGPUPerfStats : IStreamingGPUPerfStats
    {
        public float Utilization { get; private set; }
        public StreamingGPUPerfStats(float utilization)
        {
            Utilization = utilization;
        }
    }

    public class WindowsGPU : IGPU
    {
        public string Name { get; private set; }
        public string Vendor { get; private set; }
        public string DeviceId { get; private set; }

        private GPUCounterCache gpuCounterCache = new GPUCounterCache();

        [SupportedOSPlatform("windows")]
        public WindowsGPU(string name, string vendor, string deviceId)
        {
            var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_VideoController");

            var inst = searcher.Get().Cast<System.Management.ManagementObject>().FirstOrDefault();

            try
            {
                this.Name= inst?["Name"]?.ToString() ?? name;
                this.Vendor = inst?["AdapterCompatibility"]?.ToString() ?? vendor;
                this.DeviceId = inst?["PNPDeviceID"]?.ToString() ?? deviceId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize GPU performance counter for {name}. Ensure the GPU is supported and the Performance Counters are available.", ex);
            }
        }

        public IEnumerable<IStreamingGPUPerfStats> ManualUpdate()
        {
            StreamingGPUPerfStats stats = new(gpuCounterCache.GetTotal3DEngineUsage());
            List<IStreamingGPUPerfStats> statsList = new() { stats };
            return statsList;
        }

        CompositeDisposable IGPU.SubscribeToAllUpdates(IObserver<IList<IStreamingGPUPerfStats>> observer, uint updateFrequencyMs)
        {
            throw new NotImplementedException();
        }

        IDisposable IGPU.SubscribeToCoreUpdates(IObserver<IStreamingGPUPerfStats> observer, byte coreNumber, uint updateFrequencyMs)
        {
            throw new NotImplementedException();
        }
    }
}
