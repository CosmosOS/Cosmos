using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Cpu;
using Cosmos.Kernel.HAL.DriverKit.Engine;

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
    /// Starts the kernel lifecycle. Called by the generated entry point.
    /// Interrupts are enabled and the driver stage runs first, returning
    /// once every node, the children a bus driver published from its probe
    /// included, has been offered, so <see cref="OnBoot"/> and everything
    /// after it see the devices the kernel's drivers bound; a kernel that
    /// overrides this method owns that whole sequence.
    /// </summary>
    public virtual void Start()
    {
        Serial.WriteString("[Kernel] Starting kernel...\n");

        if (InterruptManager.IsEnabled)
        {
            Serial.WriteString("[Kernel] Enabling interrupts...\n");
            InternalCpu.EnableInterrupts();
        }

        // The driver stage: offers every node published so far to the
        // drivers in the manifest, children included, and returns once
        // every one of them has been offered.
        Serial.WriteString("[Kernel] Starting drivers...\n");
        DriverEngine.Start();

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
    /// Called once during boot, before BeforeRun(), with interrupts enabled
    /// and the driver stage complete. Override to customize system
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
