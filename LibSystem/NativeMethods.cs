namespace LibSystem;

using System.Runtime.InteropServices;

public static partial class NativeMethods
{
    [LibraryImport("libSystem.B.dylib", EntryPoint = "mach_task_self")]
    public static partial IntPtr mach_task_self();
}