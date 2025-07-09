using dotPerfStat.Platforms.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace dotPerfStatTest.Platforms.Windows
{
    public class WindowsGPUTests
    {
        [Fact]
        [SupportedOSPlatform("windows")]
        public void TemporaryTestMVP()
        {
            WindowsGPU gpu = new WindowsGPU("Test GPU", "Test Vendor", "Test DeviceId");
            var result = gpu.ManualUpdate();
            Assert.NotNull(result);
            Debug.WriteLine($"GPU Name: {gpu.Name}, Vendor: {gpu.Vendor}, DeviceId: {gpu.DeviceId}");
            foreach (var stats in result)
            {
                Debug.WriteLine($"GPU Utilization: {stats}");
            }
        }
    }
}
