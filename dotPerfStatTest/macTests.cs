using System.Runtime.Versioning;
using System.Reactive;
using System.Reactive.Linq;
using dotPerfStat.PlatformInvoke;
using dotPerfStat.Types;
using LibSystem;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace dotPerfStat.Tests
{
    [SupportedOSPlatform("macos")]
    public class MacCPUCoreTests : IDisposable
    {
        private readonly MacCPUCore _core;
        private ITestOutputHelper _testOutputHelper;

        public MacCPUCoreTests(ITestOutputHelper testOutputHelper)
        {
            int rc = 0;
            rc = KPCNative.kpc_set_counting(KPCNative.KPC_CLASS_FIXED_MASK);
            if (rc != 0)
                throw new InvalidOperationException(rc.ToString());
            rc = KPCNative.kpc_force_all_ctrs_set(NativeMethods.mach_task_self(), 1);
            if  (rc != 0)
                throw new InvalidOperationException(rc.ToString());

            _testOutputHelper = testOutputHelper;

            // Arrange: test Core 0 (you may wish to parametrize for other cores)
            _core = new MacCPUCore(0);
        }

        public void Dispose()
        {
            // no unmanaged cleanup needed here
        }

        [Fact]
        public void Constructor_SetsCoreNumber()
        {
            // Act & Assert
            Assert.Equal((byte)0, _core.CoreNumber);
        }
        
        [Fact]
        public void MonitoringLoopUpdatesValues()
        {
            var initial = _core.Update();
            System.Threading.Thread.Sleep(1001);
            var updated = _core.Update();
            string output_inital = "Initial Frequency " + initial.Frequency.ToString();
            string output_updated = "Updated Frequency " + updated.Frequency.ToString();
            _testOutputHelper.WriteLine(output_inital);
            _testOutputHelper.WriteLine(output_updated);
        
            Assert.NotEqual(initial.Timestamp, updated.Timestamp);
        }
        
        // [Fact]
        // public void Update_PopulatesUtilization()
        // {
        //     // Act: call twice to allow delta computation
        //     _core.Update();
        //     _core.Update();
        //
        //     // Assert: percents in [0,100]
        //     Assert.InRange((long)_core.UtilizationPercent, 0L, 100L);
        //     Assert.InRange((long)_core.UtilizationPercentUser,  0L, 100L);
        //     Assert.InRange((long)_core.UtilizationPercentKernel,0L, 100L);
        // }
        //
        // [Fact]
        // public void Update_IncreasesCyclesOverTime()
        // {
        //     // Act
        //     _core.Update();
        //     var first = _core.Cycles;
        //     System.Threading.Thread.Sleep(50);
        //     _core.Update();
        //     var second = _core.Cycles;
        //
        //     // Assert: cycles should be non-decreasing and should increase by at least a few thousand
        //     Assert.True(second >= first, "Cycles should never go backwards");
        //     Assert.True(second - first > 1_000, "Expected at least some cycles to elapse");
        // }
    }

    
}