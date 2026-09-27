// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
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
    /// <see cref="Global.StartKernel"/> once interrupts are enabled, the
    /// driver pass has run and USB hot-plug is started, so an override that
    /// replaces this lifecycle keeps all three.
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
    /// Called once, by <see cref="Global.StartKernel"/>, for the kernel to
    /// register its own PCI and USB class drivers through
    /// <see cref="Drivers.DriverManager"/>. It runs on the boot thread with
    /// interrupts on, after the built-in drivers HAL brings up bound their
    /// devices during HAL bring-up, after the built-in drivers written
    /// against the kit (xHCI, AHCI, NVMe and USB mass storage) were
    /// registered, and right before the driver pass offers all those drivers
    /// what HAL's built-ins left; the pass is also where the USB bus comes
    /// up, when xHCI publishes its controller. Then USB hot-plug starts and
    /// <see cref="OnBoot"/> runs. A kit built-in wins a
    /// tie against the drivers registered here, however early. The boot
    /// thread is the idle thread, so the override must not sleep or block.
    /// Registration closes as the pass starts. A kernel built without PCI
    /// (<c>CosmosEnablePCI=false</c>) never calls it, so ILC trims the
    /// override and the drivers only it registers.
    /// Registering from the kernel's constructor works too, but the
    /// constructor runs in every build, where only the kernel's own
    /// <see cref="KernelFeatures"/> guards trim the drivers, and with
    /// interrupts in an unspecified state: prefer this override. The default
    /// registers nothing. An exception it throws leaves StartKernel, as one
    /// from OnBoot would.
    /// </summary>
    /// <example>
    /// One switch per <c>if</c>: ILC folds a single switch only, so a
    /// kernel built without the subsystem trims the driver.
    /// <code>
    /// protected override void RegisterDrivers()
    /// {
    ///     if (KernelFeatures.Usb)
    ///     {
    ///         if (KernelFeatures.Mouse)
    ///         {
    ///             DriverManager.Register(UsbBootMouseDriver.CreateRegistration());
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    [Experimental(Cosmos.Kernel.HAL.Drivers.Experimentals.DriverKitDiagId)]
    protected virtual void RegisterDrivers()
    {
    }

    /// <summary>
    /// Lets <see cref="Global.StartKernel"/> call the protected
    /// <see cref="RegisterDrivers"/> hook.
    /// </summary>
    internal void InvokeRegisterDrivers() => RegisterDrivers();

    /// <summary>
    /// Called once during boot, before BeforeRun(). Interrupts are already
    /// enabled (unless the Interrupts switch is off), the driver pass has
    /// offered the kit's built-in drivers (AHCI and NVMe, whose disks are registered
    /// by now) and the drivers registered in <see cref="RegisterDrivers"/>
    /// every device HAL's built-ins left, and USB hot-plug is already
    /// started where it could start. Override to customize system
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
