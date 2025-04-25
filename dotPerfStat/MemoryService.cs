using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Dispatching;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiveChartsCore;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace PowerTaskMan.Performance
{
    public class MemoryStats
    {
        public int UsedMemoryMB { get; set; }
        public int TotalMemoryMB { get; set; }
        public int AvailableMemoryMB { get; set; }
        public int UsedMemoryPercent { get; set; }
    }

}

namespace PowerTaskMan.Services
{
    public partial class MemoryService
    {
        PerformanceCounter availableMemoryPC = new PerformanceCounter("Memory", "Available MBytes");
        PerformanceCounter readCounter = new PerformanceCounter("Memory", "Page Reads/sec");
        PerformanceCounter writeCounter = new PerformanceCounter("Memory", "Page Writes/sec");


        CancellationTokenSource memoryMonitoring =  new();

        private int usedMemoryMB = 0;
        private int totalMemoryMB = 0;
        private int availableMemory = 0;
        private int usedMemoryPercent = 0;
        private int page_reads_sec = 0;
        private int page_writes_sec = 0;

        public MemoryService()
        {
            ObjectQuery query = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection results = searcher.Get();

            // Iterate through each memory module and display the capacity
            ulong totalCapacity = 0;

            foreach (ManagementObject result in results)
            {
                // Capacity is in bytes, so divide by (1024 * 1024 * 1024) to convert to GB
                ulong capacityBytes = (ulong)result["Capacity"];
                totalCapacity += capacityBytes;
                totalMemoryMB = (int)(totalCapacity / (1024 * 1024));

                Console.WriteLine($"Capacity: {capacityBytes / (1024 * 1024 * 1024)} GB");
            }
        }


        private void MemoryStatsMonitor()
        {
            availableMemory = (int)availableMemoryPC.NextValue();
            usedMemoryMB = totalMemoryMB - availableMemory;
            usedMemoryPercent = (int)(((double)usedMemoryMB / (double)totalMemoryMB) * 100);
        }

        public int GetUsedMemoryPercent()
        {
            MemoryStatsMonitor();
            return usedMemoryPercent;
        }

        public int GetUsedMemory()
        {
            MemoryStatsMonitor();
            return usedMemoryMB;
        }

        public int GetTotalMemory()
        {
            return totalMemoryMB;
        }

        public int GetAvailableMemory()
        {
            MemoryStatsMonitor();
            return availableMemory;
        }

        public Performance.MemoryStats GetAllStats()
        {
            MemoryStatsMonitor();
            Performance.MemoryStats stats = new()
            {
                AvailableMemoryMB = availableMemory,
                TotalMemoryMB = totalMemoryMB,
                UsedMemoryMB = usedMemoryMB,
                UsedMemoryPercent = usedMemoryPercent
            };
            return stats;
        }
    }
}
