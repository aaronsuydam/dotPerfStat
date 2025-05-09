using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using Xunit.Abstractions;

namespace dotPerfStatTest;

using dotPerfStat.PlatformInvoke;
using Xunit;

[SupportedOSPlatform("macos")]
public class SysctlTests(ITestOutputHelper testOutputHelper)
{
    [SkippableFact]
    public void SanityCheckNumCPUs()
    {
        var retval = SYSCTL_BY_NAME.GetSysctlByName<Int32>("hw.ncpu");
        Assert.True(retval > 0);
        testOutputHelper.WriteLine("Sanity check returned: " + retval);
    }
}