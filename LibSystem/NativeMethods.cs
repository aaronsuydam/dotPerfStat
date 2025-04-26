using System;
namespace LibSystem;

using System.Runtime.InteropServices;

public enum _host_proc_info_flavor : u32
{
    ProcessorBasicInfo   = 1,
    ProcessorCpuLoadInfo = 2,
    ProcessorPmRegsInfo  = (i32)0x10000001,
    ProcessorTemperature = (i32)0x10000002
}

public class CPULoadInfo
{
    public i32 User { get; set; } = 0;
    public i32 System { get; set; } = 0;
    public i32 Nice { get; set; } = 0;
    public i32 Idle { get; set; } = 0;

    public bool IsEmpty()
    {
        return (User == 0 && System == 0 && Nice == 0 && Idle == 0);
    }
}

public static partial class NativeMethods
{
    [LibraryImport("libSystem.dylib", EntryPoint = "mach_task_self")]
    public static partial IntPtr mach_task_self();
    
    [LibraryImport("libSystem.dylib", EntryPoint = "mach_host_self")]
    public static partial IntPtr mach_host_self();
    
    [LibraryImport("libSystem.dylib", EntryPoint = "host_processor_info")]
    //  [oai_citation:1‡Apple Developer](https://developer.apple.com/documentation/kernel/1502854-host_processor_info?utm_source=chatgpt.com)
    public static partial int host_processor_info(
        IntPtr host,           // mach_host_self()
        _host_proc_info_flavor flavor,            // PROCESSOR_CPU_LOAD_INFO
        out u32 outCount,     // number of cores
        out IntPtr infoPtr,    // pointer to flat int array
        out u32 infoCount     // total ints returned
    );                         

    [LibraryImport("libSystem.dylib", EntryPoint = "vm_deallocate")]   
    //  [oai_citation:3‡Apple Developer](https://developer.apple.com/documentation/kernel/1585284-vm_deallocate?utm_source=chatgpt.com)
    public static partial int vm_deallocate(
        IntPtr targetTask,     // mach_task_self()
        IntPtr address,        // infoPtr
        u64 sizeInBytes      // infoCount * sizeof(ulong)
    );     
    
    public static CPULoadInfo GetHostProcessorInfo(u8 coreNumber)
    {
        IntPtr host = mach_host_self();
        u32 outCount = 0;
        IntPtr infoPtr;
        u32 infoCount = 0;
        int rc = host_processor_info(mach_host_self(), _host_proc_info_flavor.ProcessorCpuLoadInfo, out outCount, out infoPtr, out infoCount);
        if (rc != 0)
            throw new InvalidOperationException("Invalid Operation, " + rc);
        
        var buf = new i32[infoCount];
        Marshal.Copy(infoPtr, buf, 0, (int)infoCount);

        CPULoadInfo loadInfo = new(); 
        var coreStartIndex = coreNumber * 4;
        loadInfo.User = buf[coreStartIndex];
        loadInfo.System = buf[coreStartIndex + 1];
        loadInfo.Idle = buf[coreStartIndex + 2];
        loadInfo.Nice = buf[coreStartIndex + 3];
        
        return loadInfo;
    }
}