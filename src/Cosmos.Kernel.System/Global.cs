using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Usb;
using Cosmos.Kernel.HAL.Drivers.Engine;
using Cosmos.Kernel.System.Graphics;

namespace Cosmos.Kernel.System;

/// <summary>
/// Global system state and initialization for Cosmos.
/// </summary>
public static class Global
{
    /// <summary>
    /// The registered kernel instance that will be started.
    /// </summary>
    private static Kernel? s_kernel;

    /// <summary>
    /// Set by the first <see cref="StartKernel"/>, which never returns, so a
    /// second call can only come from the kernel it started.
    /// </summary>
    private static bool s_started;

    /// <summary>
    /// Gets the current kernel instance, or <see langword="null"/> until
    /// <see cref="RegisterKernel"/> has been called. The generated entry point
    /// registers it before <see cref="StartKernel"/> runs, so kernel code
    /// reached from <see cref="Kernel.Run"/> always sees one.
    /// </summary>
    public static Kernel? CurrentKernel => s_kernel;

    /// <summary>
    /// Registers a kernel instance to be started by the boot infrastructure.
    /// Called automatically by the generated entry point.
    /// </summary>
    /// <param name="kernel">The kernel instance to register.</param>
    public static void RegisterKernel(Kernel kernel)
    {
        Serial.WriteString("[Global] Registering kernel\n");
        s_kernel = kernel;
    }

    /// <summary>
    /// Brings up the graphical <see cref="KernelConsole"/>, which is what makes
    /// <c>Console.WriteLine</c> draw to the screen. Every other subsystem is
    /// already up by this point: the library initializer wires the managers to
    /// the HAL before any managed code runs. Called once by
    /// <see cref="Kernel.OnBoot"/>; a kernel customizes this step by overriding
    /// OnBoot instead.
    /// </summary>
    internal static void Initialize()
    {
        Serial.WriteString("[Global] Initialize() called\n");

        // Initialize graphics console (framebuffer + font)
        if (Core.CosmosFeatures.GraphicsEnabled)
        {
            Serial.WriteString("[Global] Initializing KernelConsole...\n");
            if (KernelConsole.Initialize())
            {
                Serial.WriteString("[Global] KernelConsole initialized: ");
                Serial.WriteNumber((ulong)KernelConsole.Default.Cols);
                Serial.WriteString("x");
                Serial.WriteNumber((ulong)KernelConsole.Default.Rows);
                Serial.WriteString(" chars\n");
            }
            else
            {
                Serial.WriteString("[Global] WARNING: KernelConsole initialization failed!\n");
            }
        }
        else
        {
            Serial.WriteString("[Global] Graphics disabled via feature switch.\n");
        }
    }

    /// <summary>
    /// Starts the registered kernel, once. Enables interrupts (unless the
    /// Interrupts switch is off), binds the drivers the kernel registered and
    /// starts USB hot-plug, then calls
    /// <see cref="Kernel.Start"/>, so <see cref="Kernel.OnBoot"/>,
    /// <see cref="Kernel.BeforeRun"/> and <see cref="Kernel.Run"/> all run
    /// with interrupts on, and a kernel that overrides Start keeps all three.
    /// Called by the generated entry point; it does not return.
    /// </summary>
    /// <exception cref="InvalidOperationException">StartKernel already ran.</exception>
    public static void StartKernel()
    {
        Serial.WriteString("[Global] StartKernel called\n");

        if (s_kernel is null)
        {
            Serial.WriteString("[Global] ERROR: No kernel registered!\n");
            Serial.WriteString("[Global] Check CosmosKernelClass property in your .csproj\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: No kernel registered!");
            Console.WriteLine("Set <CosmosKernelClass> in your .csproj to your kernel's full type name.");
            Console.ResetColor();

            // Halt
            while (true) { }
        }

        if (s_started)
        {
            throw new InvalidOperationException("Global.StartKernel already ran; the generated entry point calls it once.");
        }

        s_started = true;

        // On x64 the IDT load already unmasked interrupts during HAL
        // bring-up; on ARM64 they stay masked from boot until here, or until
        // storage bring-up needed them. Enabled before the kernel's Start
        // rather than inside it, so OnBoot runs with them on on both
        // architectures and an override of Start cannot lose them.
        if (InterruptManager.IsEnabled)
        {
            Serial.WriteString("[Global] Enabling interrupts...\n");
            InternalCpu.EnableInterrupts();
        }

        // The drivers the kernel registered from its constructor bind here,
        // to the PCI functions the built-in drivers left free during HAL
        // bring-up. After interrupts, so a probe runs with them on on both
        // architectures; before hot-plug starts, so the boot thread is the
        // only one binding devices while the pass runs; and before the
        // kernel's Start, so OnBoot finds them bound. PCI's switch alone, so
        // ILC folds it and a kernel without PCI trims the whole engine.
        if (Core.CosmosFeatures.PCIEnabled)
        {
            DriverCore.BindUserDrivers();
        }

        // After interrupts, since the thread only runs once the scheduler
        // switches to it, and last before the kernel runs: once started, the
        // hot-plug thread is the only writer of the USB device list, so
        // boot-time USB binding has to be over by then. Nested single-switch
        // guards, not one &&: ILC folds each on its own only, so a kernel
        // without the scheduler or without USB trims the hot-plug thread.
        if (Core.CosmosFeatures.SchedulerEnabled)
        {
            if (Core.CosmosFeatures.UsbEnabled)
            {
                UsbManager.StartHotPlug();
            }
        }

        Serial.WriteString("[Global] Starting kernel...\n");
        s_kernel.Start();

        // If kernel.Start() returns, halt the system
        Serial.WriteString("[Global] Kernel.Start() returned, halting...\n");
        while (true) { }
    }
}
