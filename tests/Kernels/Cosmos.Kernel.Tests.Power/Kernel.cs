using System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Power;

// Validates the Cosmos.Kernel.System.Power surface end-to-end. Two of the
// tests (Reboot, Shutdown) actually exit QEMU, so the test runner walks the
// suite across multiple boots: it sets `skip=N` on the Limine cmdline so the
// kernel knows which destructive test already fired in a previous boot and
// jumps to the next one. After both have fired the final boot finalises the
// suite cleanly with TR.Finish() + TR.Complete().
public class Kernel : Sys.Kernel
{
    protected override void BeforeRun()
    {
        int skip = TR.GetSkipCount();

        Log.WriteString("[Power Tests] BeforeRun() reached, skip=");
        Log.WriteNumber((uint)skip);
        Log.WriteString("\n");

        TR.Start("Power Tests", expectedTests: 4);

        // The ring's own answer to "did the platform wire up power
        // management". The initializer builds the CPU and power operations
        // together from non-nullable factories, so this one fact stands for
        // both: it can only read true once that initializer has run.
        TR.Run("Power_IsSupported_OnThisPlatform", () =>
        {
            Assert.True(Sys.Power.IsSupported, "the platform initializer should have published power operations");
        });

        TR.Run("Power_Halt_Callable", () =>
        {
            Action halt = Sys.Power.Halt;
            Assert.NotNull(halt);
        });

        // Test #3: Reboot. Fires on skip=0, replays as already-passed otherwise.
        if (skip == 0)
        {
            Log.WriteString("[Power Tests] About to invoke Power.Reboot(), QEMU should exit\n");
            TR.RunDestructive(
                "Reboot_FiresAndExits",
                () => Sys.Power.Reboot(),
                "Power.Reboot() returned without rebooting");
            // Only reached if Reboot didn't fire.
            TR.Finish();
            Log.WriteString("[Power Tests] FATAL: reached post-Reboot epilogue\n");
            return;
        }
        TR.Run("Reboot_FiresAndExits", () => { });

        // Test #4: Shutdown. Fires on skip=1, replays as already-passed otherwise.
        if (skip == 1)
        {
            Log.WriteString("[Power Tests] About to invoke Power.Shutdown(), QEMU should exit\n");
            TR.RunDestructive(
                "Shutdown_FiresAndExits",
                () => Sys.Power.Shutdown(),
                "Power.Shutdown() returned without powering off");
            // Only reached if Shutdown didn't fire.
            TR.Finish();
            Log.WriteString("[Power Tests] FATAL: reached post-Shutdown epilogue\n");
            return;
        }
        TR.Run("Shutdown_FiresAndExits", () => { });

        // skip >= 2: both destructive tests already fired in earlier boots.
        // Wrap up the suite cleanly so the runner sees a TestSuiteEnd.
        TR.Finish();
    }

    protected override void Run()
    {
        Stop();
    }

    protected override void AfterRun()
    {
        TR.Complete();
        Sys.Power.Halt();
    }
}
