using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace dotPerfStat.Platforms.Windows
{
    [SupportedOSPlatform("windows")]
    public class GPUCounterCache
    {
        // Cache keyed by the instance name
        private readonly ConcurrentDictionary<string, PerformanceCounter> _counters
            = new();

        public GPUCounterCache()
        {
            InitializeCounters();
        }

        /// <summary>
        /// Initialize all GPU Engine performance counters at startup.
        /// </summary>
        private void InitializeCounters()
        {
            try
            {
                var category = new PerformanceCounterCategory("GPU Engine");
                var instances = category.GetInstanceNames();

                foreach (var instanceName in instances)
                {
                    var counter = new PerformanceCounter(
                        categoryName: "GPU Engine",
                        counterName: "Utilization Percentage",
                        instanceName: instanceName,
                        readOnly: true
                    );
                    _counters.TryAdd(instanceName, counter);
                }
            }
            catch (Exception)
            {
                // Handle cases where GPU Engine category might not be available
            }
        }

        /// <summary>
        /// Get a cached PerformanceCounter for the given GPU Engine instance.
        /// </summary>
        public PerformanceCounter? GetCounter(string instanceName) =>
            _counters.TryGetValue(instanceName, out var counter) ? counter : null;

        /// <summary>
        /// Sample total 3D usage by summing all "engtype_3D" instances.
        /// </summary>
        public float GetTotal3DEngineUsage()
        {
            float total = _counters
                .Where(kvp => kvp.Key.Contains("engtype_3D"))
                .Select(kvp => kvp.Value.NextValue())
                .Sum();

            return total;
        }
    }
}
