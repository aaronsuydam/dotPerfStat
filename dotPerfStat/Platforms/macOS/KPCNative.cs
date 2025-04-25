using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace dotPerfStat.PlatformInvoke;

public static partial class KPCNative
{
    private const string Kperf = "/System/Library/PrivateFrameworks/kperf.framework/kperf";
    
    /* constants */
    public const uint KPC_CLASS_FIXED_MASK = 0x01;
    public const uint KPC_CLASS_CONFIGURABLE_MASK = 0x02;
    public const uint KPC_CLASS_FIXED = 0;

    /* --- sysctl wrappers --- */
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(Kperf, EntryPoint = "kpc_set_counting", SetLastError = true)]
    public static partial int kpc_set_counting(uint classes);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(Kperf, EntryPoint = "kpc_force_all_ctrs_set", SetLastError = true)]
    public static partial int kpc_force_all_ctrs_set(IntPtr task, int enable);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(Kperf, EntryPoint = "kpc_get_counter_count", SetLastError = true)]
    public static partial int kpc_get_counter_count(uint classes);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(Kperf, EntryPoint = "kpc_get_cpu_counters", SetLastError = true)]
    public static partial int kpc_get_cpu_counters([MarshalAs(UnmanagedType.Bool)] bool allCpus, uint classes, out  uint curCpu, [Out] ulong[] buf);
}