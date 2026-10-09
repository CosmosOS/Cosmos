// This code is licensed under MIT license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Memory.VAS;



namespace Cosmos.Kernel.Core.Scheduler;

internal partial class ProcessSignals
{

    public const uint SstackSize = 1024 * 4;

    internal ProcessSignals(Process process)
    {
        _process = process;
    }

    private nuint?[] Handels = new nuint?[31];
    private readonly Process _process;

    public void RegisterHandel(ushort signal, nuint func)
    {
        Handels[signal] = func;
    }

    public void Send(ushort signal)
    {
        nuint? handel = Handels[signal];
        ProcessSignalConfig config = ProcessSignalConfigs.Configs[signal];
        if (config.KillProcess)
        {
            using (InternalCpu.DisableInterruptsScope())
            {
                foreach (SchedulerThread thread in _process.Threads)
                {
                    thread.State = SchedulerThreadState.Dead;
                }
            }
        }
        if (handel != null)
        {
            nuint entryPoint = handel.Value;
            SchedulerThread thread = new SchedulerThread
            {
                Id = SchedulerManager.AllocateThreadId(),
                CpuId = 0,
                State = SchedulerThreadState.Created,
                Flags = SchedulerThreadFlags.NativeProcess
            };

            byte ring = _process.Ring;
            AddressSpace? userSpace = ring == 3 ? _process.AddressSpace : null;

#if ARCH_X64
            // Ring-3 paths ignore codeSegment (ThreadContext picks 0x1B); pass
            // 0 so InitializeStack forwards it as a sentinel. Ring-0 keeps the
            // current kernel CS for the manual RSP+jmp exit path.
            ushort cs = ring == 3 ? (ushort)0 : (ushort)GetCurrentCodeSelector();
            thread.InitializeStack(entryPoint, cs, stackSize: SstackSize, ring: ring, userSpace: userSpace, userProcess: _process);
#elif ARCH_ARM64
            // ARM64: no code selector needed, use 0.
            thread.InitializeStack(entryPoint, 0, stackSize: SstackSize, ring: ring, userSpace: userSpace, userProcess: _process);
#endif

            _process.StartThread(thread);
        }

        if (config.KillProcess)
        {
            _process.Kill(1);
        }
    }


#if ARCH_X64
    // replace this with something better
    [LibraryImport("*", EntryPoint = "_native_x64_get_code_selector")]
    [SuppressGCTransition]
    public static partial ulong GetCurrentCodeSelector();
#endif
}



internal readonly struct ProcessSignalConfig
{
    public readonly ulong TimeOut { get; init; }
    public readonly bool KillProcess { get; init; }
}

internal class ProcessSignalConfigs
{

    /// <summary>Hangup detected on controlling terminal.</summary>
    public const int SIGHUP = 1;

    /// <summary>Interrupt from keyboard (Ctrl+C).</summary>
    public const int SIGINT = 2;

    /// <summary>Quit from keyboard.</summary>
    public const int SIGQUIT = 3;

    /// <summary>Illegal instruction.</summary>
    public const int SIGILL = 4;

    /// <summary>Trace or breakpoint trap.</summary>
    public const int SIGTRAP = 5;

    /// <summary>Process abort signal.</summary>
    public const int SIGABRT = 6;

    /// <summary>Bus error.</summary>
    public const int SIGBUS = 7;

    /// <summary>Floating-point exception.</summary>
    public const int SIGFPE = 8;

    /// <summary>
    /// Immediately terminates the process.
    /// This signal cannot be caught, blocked, or ignored.
    /// </summary>
    public const int SIGKILL = 9;

    /// <summary>User-defined signal 1.</summary>
    public const int SIGUSR1 = 10;

    /// <summary>Invalid memory reference (segmentation fault).</summary>
    public const int SIGSEGV = 11;

    /// <summary>User-defined signal 2.</summary>
    public const int SIGUSR2 = 12;

    /// <summary>Write on a pipe with no readers.</summary>
    public const int SIGPIPE = 13;

    /// <summary>Alarm clock signal.</summary>
    public const int SIGALRM = 14;

    /// <summary>
    /// Requests graceful process termination.
    /// Applications should handle this signal to perform cleanup.
    /// </summary>
    public const int SIGTERM = 15;

    /// <summary>Stack fault (Linux-specific).</summary>
    public const int SIGSTKFLT = 16;

    /// <summary>Child process has stopped or exited.</summary>
    public const int SIGCHLD = 17;

    /// <summary>Continue execution if stopped.</summary>
    public const int SIGCONT = 18;

    /// <summary>
    /// Stops the process.
    /// This signal cannot be caught, blocked, or ignored.
    /// </summary>
    public const int SIGSTOP = 19;

    /// <summary>Terminal stop signal (Ctrl+Z).</summary>
    public const int SIGTSTP = 20;

    /// <summary>Background process attempted terminal input.</summary>
    public const int SIGTTIN = 21;

    /// <summary>Background process attempted terminal output.</summary>
    public const int SIGTTOU = 22;

    /// <summary>Urgent condition on a socket.</summary>
    public const int SIGURG = 23;

    /// <summary>CPU time limit exceeded.</summary>
    public const int SIGXCPU = 24;

    /// <summary>File size limit exceeded.</summary>
    public const int SIGXFSZ = 25;

    /// <summary>Virtual timer expired.</summary>
    public const int SIGVTALRM = 26;

    /// <summary>Profiling timer expired.</summary>
    public const int SIGPROF = 27;

    /// <summary>Terminal window size changed.</summary>
    public const int SIGWINCH = 28;

    /// <summary>I/O is now possible.</summary>
    public const int SIGIO = 29;

    /// <summary>
    /// Alias for <see cref="SIGIO"/>.
    /// </summary>
    public const int SIGPOLL = SIGIO;

    /// <summary>Power failure.</summary>
    public const int SIGPWR = 30;

    /// <summary>Bad system call.</summary>
    public const int SIGSYS = 31;

    public static readonly ProcessSignalConfig[] Configs =
    [
        new ProcessSignalConfig()
        {
            TimeOut = 99999,
            KillProcess = true,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 30,
            KillProcess = true,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 30,
            KillProcess = true,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 30,
            KillProcess = true,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 30,
            KillProcess = true,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        },
        new ProcessSignalConfig()
        {
            TimeOut = 0,
            KillProcess = false,
        }
    ];

}
