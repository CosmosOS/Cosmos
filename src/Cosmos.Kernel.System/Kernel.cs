// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.System.Graphics;

namespace Cosmos.Kernel.System;

/// <summary>
/// Base class for all Cosmos user kernels.
/// Provides the <see cref="BeforeRun"/>/<see cref="Run"/>/<see cref="AfterRun"/> lifecycle pattern.
/// </summary>
public abstract partial class Kernel
{
    /// <summary>
    /// The running kernel: the instance the generated entry point registered
    /// before calling its <see cref="Start"/>, or <see langword="null"/>
    /// before then. Code that runs from <see cref="Start"/> onwards,
    /// <see cref="OnBoot"/>, <see cref="BeforeRun"/> and <see cref="Run"/>
    /// included, always sees one, so static code with no instance at hand
    /// reaches the kernel here, for example to call <see cref="Stop"/>. The
    /// kernel's own constructor runs before the registration and sees
    /// <see langword="null"/>.
    /// </summary>
    public static Kernel? Current { get; internal set; }

    /// <summary>
    /// True once <see cref="BeforeRun"/> has completed and the <see cref="Run"/> loop is active.
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
    /// Called once during boot, before <see cref="BeforeRun"/>, with interrupts enabled
    /// and the driver stage complete. The default brings up the graphical
    /// <see cref="KernelConsole"/>, which is what makes <c>Console.WriteLine</c>
    /// draw to the screen; every other subsystem is already up by this point,
    /// since the library initializer wires the managers to the HAL before any
    /// managed code runs. Override to customize system initialization.
    /// </summary>
    protected virtual void OnBoot()
    {
        Serial.WriteString("[Kernel] OnBoot() called\n");

        if (Core.CosmosFeatures.GraphicsEnabled)
        {
            Serial.WriteString("[Kernel] Initializing KernelConsole...\n");
            if (KernelConsole.Initialize())
            {
                Serial.WriteString("[Kernel] KernelConsole initialized: ");
                Serial.WriteNumber((ulong)KernelConsole.Default.Cols);
                Serial.WriteString("x");
                Serial.WriteNumber((ulong)KernelConsole.Default.Rows);
                Serial.WriteString(" chars\n");
            }
            else
            {
                Serial.WriteString("[Kernel] WARNING: KernelConsole initialization failed!\n");
            }
        }
        else
        {
            Serial.WriteString("[Kernel] Graphics disabled via feature switch.\n");
        }
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
    public void Stop() => Stopped = true;
}
