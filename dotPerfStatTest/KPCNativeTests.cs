namespace dotPerfStatTest;

using System.Runtime.InteropServices;
using dotPerfStat.PlatformInvoke;
using LibSystem;
using Xunit;

public class KPCNativeTests
{
    [Fact]
    public void TestActivateCounters()
    {
        int rc = KPCNative.kpc_force_all_ctrs_set(NativeMethods.mach_task_self(), 1);
        if (rc != 0)
        {
            int err = Marshal.GetLastPInvokeError();
            Console.WriteLine($"Failed to enable fixed counters: {err}");
            string message = Marshal.GetLastPInvokeErrorMessage();
            Console.WriteLine(message);
            throw new InvalidOperationException("Failed to enable fixed counters");
        }
    }
    
    [Fact]
    public void TestGetCPUFrequency()
    {
        try
        {
            int rc = 0;
            rc = KPCNative.kpc_set_counting(KPCNative.KPC_CLASS_FIXED_MASK);
            if (rc != 0)
                throw new InvalidOperationException(rc.ToString());
            rc = KPCNative.kpc_force_all_ctrs_set(NativeMethods.mach_task_self(), 1);
            if  (rc != 0)
                throw new InvalidOperationException(rc.ToString());
            
            // First, ask how many fixed-function counters the kernel supports
            uint nCtrs = (uint)KPCNative.kpc_get_counter_count(KPCNative.KPC_CLASS_FIXED_MASK);
            if (nCtrs == 0)
                throw new InvalidOperationException("No fixed counters available");

            // Allocate an array large enough for all fixed counters
            ulong[] buf = new ulong[nCtrs];

            // Fetch exactly that many counters into our buffer
            rc = KPCNative.kpc_get_cpu_counters(true, KPCNative.KPC_CLASS_FIXED_MASK, out nCtrs, buf);
            if (rc != 0)
            {
                Console.WriteLine($"Failed to get cpu counters");
            }
            else
            {
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
        
    }
}