using System.Runtime.CompilerServices;

namespace dotPerfStat.PlatformInvoke;

using System.Runtime.InteropServices;

public partial class SYSCTL_BY_NAME
{
    [UnmanagedCallConv( CallConvs = new Type[] { typeof( CallConvCdecl ) })]
    [LibraryImport("libc", EntryPoint = "sysctlbyname")]
    private static partial int sysctlbyname(
        [MarshalAs(UnmanagedType.LPStr)] string name, 
        IntPtr oldp, 
        ref IntPtr oldlen_p, 
        IntPtr newlen, 
        IntPtr newlen_p);
    
    
    // Start Here!
    public static string GetSysctlByName(string name)
    {
        // First we have to get the size of the field we are querying
        IntPtr oldlen_p = IntPtr.Zero;
        int rc = sysctlbyname(name, IntPtr.Zero, ref oldlen_p, IntPtr.Zero, IntPtr.Zero);
        
        // Once we have the size, read the field for real. 
        
    }
}