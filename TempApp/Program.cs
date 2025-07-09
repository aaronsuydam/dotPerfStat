using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using dotPerfStat.Services;

[SupportedOSPlatform("windows")]
public static class GpuCounterCache
{
    // Cache keyed by the instance name
    private static readonly ConcurrentDictionary<string, PerformanceCounter> _counters
        = new();

    /// <summary>
    /// Get-or-create a cached PerformanceCounter for the given GPU Engine instance.
    /// </summary>
    public static PerformanceCounter GetCounter(string instanceName) =>
        _counters.GetOrAdd(instanceName, name =>
            new PerformanceCounter(
                categoryName: "GPU Engine",
                counterName: "Utilization Percentage",
                instanceName: name,
                readOnly: true
            )
        );

    /// <summary>
    /// Sample total 3D usage by summing all "engtype_3D" instances.
    /// </summary>
    public static float GetTotal3DEngineUsage()
    {
        var category = new PerformanceCounterCategory("GPU Engine");
        var instances = category.GetInstanceNames()
                                .Where(n => n.Contains("engtype_3D"));

        // Use cached counters
        float total = instances
            .Select(name => GetCounter(name).NextValue())
            .Sum();

        return total;
    }
}

// Usage example
class Program
{
    static void Main()
    {
        Console.WriteLine("=== Original GPU Counter Cache Demo ===");
        // First call will create & cache all the counters
        float usage1 = GpuCounterCache.GetTotal3DEngineUsage();
        Console.WriteLine($"3D Usage (sample 1): {usage1:F1}%");

        // Subsequent call reuses the same PerformanceCounter instances
        System.Threading.Thread.Sleep(1000);
        float usage2 = GpuCounterCache.GetTotal3DEngineUsage();
        Console.WriteLine($"3D Usage (sample 2): {usage2:F1}%");

        Console.WriteLine();
        Console.WriteLine("=== New GPUService Demo ===");
        
        try
        {
            // Create GPU monitor using the new service
            var gpuMonitor = new GPUMonitor();
            
            Console.WriteLine($"GPU Name: {gpuMonitor.Name}");
            Console.WriteLine($"GPU Vendor: {gpuMonitor.Vendor}");
            Console.WriteLine($"GPU Device ID: {gpuMonitor.DeviceId}");
            
            // Get current utilization
            float utilization = gpuMonitor.GetUtilization();
            Console.WriteLine($"GPU Utilization: {utilization:F1}%");
            
            // Get all stats
            var allStats = gpuMonitor.GetAllStats();
            Console.WriteLine($"Complete Stats - Name: {allStats.Name}, Vendor: {allStats.Vendor}, Utilization: {allStats.Utilization:F1}%");
            
            // Sample again after a delay
            System.Threading.Thread.Sleep(1000);
            utilization = gpuMonitor.GetUtilization();
            Console.WriteLine($"GPU Utilization (sample 2): {utilization:F1}%");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error using GPUService: {ex.Message}");
        }
    }
}
