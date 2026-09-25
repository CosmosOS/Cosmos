using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.System;

/// <summary>
/// Base class for all Cosmos user kernels.
/// Provides the BeforeRun/Run/AfterRun lifecycle pattern.
/// </summary>
public abstract partial class Kernel
{
    /// <summary>
    /// True once BeforeRun has completed and the Run loop is active.
    /// </summary>
    protected bool Started { get; private set; }

    /// <summary>
    /// True once <see cref="Stop"/> has been called. The Run loop exits after
    /// the current <see cref="Run"/> returns; call <see cref="Stop"/> to end
    /// the kernel, this is the read side of it.
    /// </summary>
    protected bool Stopped { get; private set; }

    /// <summary>
    /// Constructs a new Kernel instance.
    /// </summary>
    public Kernel()
    {
        Serial.WriteString("[Kernel] Constructing Cosmos.Kernel.System.Kernel instance\n");
    }

    /// <summary>
    /// Runs the kernel lifecycle: <see cref="OnBoot"/>, <see cref="BeforeRun"/>,
    /// the <see cref="Run"/> loop, then <see cref="AfterRun"/>. Called by
    /// <see cref="Global.StartKernel"/> once interrupts are enabled and USB
    /// hot-plug is started, so an override that replaces this lifecycle
    /// keeps both.
    /// </summary>
    public virtual void Start()
    {
        Serial.WriteString("[Kernel] Starting kernel...\n");

        Serial.WriteString("[Kernel] Calling OnBoot()...\n");
        OnBoot();

        EarlyGop.Enabled = false;

        Serial.WriteString("[Kernel] Calling BeforeRun()...\n");
        BeforeRun();

        Started = true;

        Serial.WriteString("[Kernel] Entering main loop...\n");
        while (!Stopped)
        {
            Serial.WriteString("[Kernel] Calling Run()...\n");
            Run();
            Serial.WriteString("[Kernel] Run() returned\n");
        }

        Serial.WriteString("[Kernel] Main loop exited, calling AfterRun()...\n");
        AfterRun();

        // Halt the CPU to prevent returning to NativeAOT shutdown sequence
        // The shutdown code tries to allocate memory which fails in kernel environment
        Serial.WriteString("[Kernel] Halting CPU...\n");
        while (true)
        {
            InternalCpu.Halt();
        }
    }

    /// <summary>
    /// Called once during boot, before BeforeRun(). Interrupts are already
    /// enabled (unless the Interrupts switch is off), and USB hot-plug is
    /// already started where it could start. Override to customize system
    /// initialization.
    /// </summary>
    protected virtual void OnBoot()
    {
        Global.Initialize();
    }

    /// <summary>
    /// Called once before the main loop starts.
    /// Override to perform one-time setup.
    /// </summary>
    protected virtual void BeforeRun()
    {
    }

    /// <summary>
    /// Called repeatedly in the main loop.
    /// Override to implement your kernel's main logic.
    /// </summary>
    protected abstract void Run();

    /// <summary>
    /// Called once after the main loop exits.
    /// Override to perform cleanup.
    /// </summary>
    protected virtual void AfterRun()
    {
    }

    /// <summary>
    /// Signals the kernel to stop the main loop.
    /// </summary>
    public void Stop()
    {
        Stopped = true;
    }
}
