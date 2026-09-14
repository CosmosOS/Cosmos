using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Cpu;

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
    /// Starts the kernel lifecycle.
    /// Called by the generated entry point.
    /// </summary>
    public virtual void Start()
    {
        Serial.WriteString("[Kernel] Starting kernel...\n");

        Serial.WriteString("[Kernel] Calling OnBoot()...\n");
        OnBoot();

        if (InterruptManager.IsEnabled)
        {
            Serial.WriteString("[Kernel] Enabling interrupts...\n");
            InternalCpu.EnableInterrupts();
        }

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
    /// Called once during boot, before BeforeRun().
    /// Override to customize system initialization.
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
