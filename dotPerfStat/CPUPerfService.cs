using System.Runtime.InteropServices;
using dotPerfStat.Interfaces.CPU;
using dotPerfStat.Platforms.macOS;

namespace dotPerfStat
{
    
    public class CPUMonitor
    {

        private ICPU _internalCPU;
        
        
        public static ICPU CPUFactory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new MacOS_CPU();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsCPU();
            }
            else 
                throw new NotImplementedException("Your platform is not supported. Please open an issue on GitHub.");
        }

        public CPUMonitor()
        {
            _internalCPU = CPUFactory();
        }

        public IDisposable SubscribeToUpdates(IObserver<IList<IStreamingCorePerfData>> observer, u32 updateFrequencyMs)
        {
            return _internalCPU.SubscribeToAllUpdates(observer, updateFrequencyMs);
        }
    }
}
