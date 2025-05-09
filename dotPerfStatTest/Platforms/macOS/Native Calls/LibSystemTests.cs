using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using LibSystem;

namespace dotPerfStatTest;


[SupportedOSPlatform("macos")]
public class LibSystemTests
{
     private const int Trials = 100;

     [SkippableFact]
     public void GetHostProcessorInfo_VariesAcrossCalls()
     {
          // baseline from the public wrapper
          var baseline = NativeMethods.GetHostProcessorInfo(0);

          // sample 9 more times and check for any delta
          var results = Enumerable.Range(1, Trials - 1)
               .Select(_ => {
                    Thread.Sleep(20);
                    return NativeMethods.GetHostProcessorInfo(0);
               });
          
          var changed = results.Any(next =>
               next.User   != baseline.User   ||
               next.System != baseline.System ||
               next.Idle   != baseline.Idle   ||
               next.Nice   != baseline.Nice
          );

          Assert.True(changed, $"Expected at least one differing CPULoadInfo over {Trials} calls");
     }

}