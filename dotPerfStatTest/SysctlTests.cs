using System.Diagnostics;
using System.Globalization;
using Xunit.Abstractions;

namespace dotPerfStatTest;

using dotPerfStat.PlatformInvoke;
using Xunit;

public class SysctlTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void SanityCheckNumCPUs()
    {
        var retval = SYSCTL_BY_NAME.GetSysctlByName<Int32>("hw.ncpu");
        Assert.True(retval > 0);
        testOutputHelper.WriteLine("Sanity check returned: " + retval);
    }
}