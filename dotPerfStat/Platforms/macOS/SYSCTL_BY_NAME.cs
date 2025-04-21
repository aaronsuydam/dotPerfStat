namespace dotPerfStat.PlatformInvoke;
using System.Runtime.CompilerServices;
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
    public static T GetSysctlByName<T>(string name)
    {
        // First we have to get the size of the field we are querying
        IntPtr oldlen_p = IntPtr.Zero;
        int rc = sysctlbyname(name, IntPtr.Zero, ref oldlen_p, IntPtr.Zero, IntPtr.Zero);
        
        // Once we have the size, read the field for real.
        IntPtr oldp = Marshal.AllocHGlobal(oldlen_p.ToInt32());
        rc = sysctlbyname(name, oldp, ref oldlen_p, IntPtr.Zero, IntPtr.Zero);
        if (rc != 0)
        {
            throw new Exception("Sysctl call failed. RC value = " + rc);
        }
        else
        {
            if (typeof(T) == typeof(string))
            {
                var result = Marshal.PtrToStringAnsi(oldp);
                if (result is null)
                {
                    throw new Exception("Sysctl call failed. Result is null");
                }
                Marshal.FreeHGlobal(oldp);
                return (T)(object)result;   
            }
            else
            {
                var result = Marshal.PtrToStructure<T>(oldp);
                Marshal.FreeHGlobal(oldp);
                if (result is null)
                {
                    throw new Exception("Sysctl call failed. Result is null");   
                }

                return result;
            }
        }
    }
}