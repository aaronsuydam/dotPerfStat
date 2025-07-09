// HiResSleep.cs
// Provides high-precision thread sleep with ~1 ms resolution on Windows, Linux, and macOS.
//
// On Windows, uses a high-resolution waitable timer (`CreateWaitableTimerEx` with
// `CREATE_WAITABLE_TIMER_HIGH_RESOLUTION`).
//   Documentation: https://learn.microsoft.com/windows/win32/api/synchapi/nf-synchapi-createwaitabletimerex
//
// On Linux and macOS, uses POSIX `nanosleep` for high-resolution delays.
//   Documentation: https://man7.org/linux/man-pages/man2/nanosleep.2.html
//
// Implementation by OpenAI's ChatGPT (https://openai.com).


using System.Diagnostics;
using System.Runtime.InteropServices;

public class HiResSleep
{
    private readonly bool _isWindows;
    private readonly IntPtr _timer;
    private readonly Stopwatch _sw;
    private readonly DateTime _start;



    private const uint CREATE_WAITABLE_TIMER_HIGH_RESOLUTION = 0x00000002;
    private const uint TIMER_ALL_ACCESS = 0x001F0003;
    private const uint INFINITE = 0xFFFFFFFF;
    private const int EINTR = 4;

    public HiResSleep()
    {
        _sw = Stopwatch.StartNew();
        _start = DateTime.Now;
        
        _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (_isWindows)
        {
            _timer = CreateWaitableTimerEx(
                IntPtr.Zero,
                null,
                CREATE_WAITABLE_TIMER_HIGH_RESOLUTION,
                TIMER_ALL_ACCESS);

            if (_timer == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create high-resolution waitable timer.");
            }
        }
    }

    /// <summary>
    /// Sleeps the current thread for the specified number of milliseconds
    /// with high precision and minimal CPU usage.
    /// </summary>
    /// <param name="milliseconds">Milliseconds to sleep.</param>
    public void Sleep(uint milliseconds)
    {
        if (_isWindows)
        {
            // Negative value indicates relative time in 100-nanosecond intervals
            long dueTime100Ns = -(long)milliseconds * 10_000;
            bool setOk = SetWaitableTimer(
                _timer,
                ref dueTime100Ns,
                0,
                IntPtr.Zero,
                IntPtr.Zero,
                false);

            if (!setOk)
                throw new InvalidOperationException($"SetWaitableTimer failed: {Marshal.GetLastWin32Error()}");

            uint waitResult = WaitForSingleObject(_timer, INFINITE);
            if (waitResult != 0)
                throw new InvalidOperationException($"WaitForSingleObject failed: {waitResult}");
        }
        else
        {
            // Use POSIX nanosleep on Linux and macOS
            Timespec req = new Timespec
            {
                tv_sec = (IntPtr)(milliseconds / 1000),
                tv_nsec = (long)(milliseconds % 1000) * 1_000_000
            };

            // Retry if interrupted by a signal (EINTR)
            while (nanosleep(ref req, IntPtr.Zero) == -1)
            {
                int err = Marshal.GetLastWin32Error();
                if (err != EINTR) break;
            }
        }
    }
    
    /// <summary>
    /// Gets a high-resolution timestamp based on the start time provided in the constructor.
    /// </summary>
    /// <returns>DateTime representing current time with high-resolution accuracy.</returns>
    public DateTime GetTimestamp()
    {
        double ticksPerCount = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;
        long elapsedTicks = (long)(_sw.ElapsedTicks * ticksPerCount);
        return _start.AddTicks(elapsedTicks);
    }

    ~HiResSleep()
    {
        if (_isWindows && _timer != IntPtr.Zero)
        {
            CloseHandle(_timer);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateWaitableTimerEx(
        IntPtr lpTimerAttributes,
        string lpTimerName,
        uint dwFlags,
        uint dwDesiredAccess);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetWaitableTimer(
        IntPtr hTimer,
        ref long pDueTime,
        int lPeriod,
        IntPtr pfnCompletionRoutine,
        IntPtr lpArgToCompletionRoutine,
        bool fResume);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(
        IntPtr hHandle,
        uint dwMilliseconds);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    private struct Timespec
    {
        public IntPtr tv_sec;   // seconds
        public long tv_nsec;    // nanoseconds
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int nanosleep(ref Timespec req, IntPtr rem);
}
