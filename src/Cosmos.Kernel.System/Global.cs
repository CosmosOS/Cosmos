// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.DriverKit.Usb;
using Cosmos.Kernel.HAL.Drivers.BuiltIn;
using Cosmos.Kernel.HAL.Usb;
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
    /// Interrupts switch is off), registers the built-in drivers written
    /// against the driver kit (xHCI with USB, AHCI and NVMe with storage,
    /// USB mass storage with storage and USB), calls
    /// <see cref="Kernel.RegisterDrivers"/> and binds those drivers and the
    /// kernel's (all only in a kernel built with PCI), which is when the USB
    /// devices are enumerated, starts USB hot-plug, then calls
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
        // bring-up; on ARM64 they stay masked from boot until here. Enabled
        // before the kernel's Start rather than inside it, so OnBoot runs
        // with them on on both architectures and an override of Start cannot
        // lose them.
        if (InterruptManager.IsEnabled)
        {
            Serial.WriteString("[Global] Enabling interrupts...\n");
            InternalCpu.EnableInterrupts();
        }

        // The built-in drivers written against the kit are registered here,
        // then the kernel registers its own, from RegisterDrivers or earlier
        // from its constructor, and they all bind here, to the PCI functions
        // the drivers HAL brings up itself left free, and to the USB
        // interfaces of the devices the xHCI driver's controllers carry,
        // which the USB core enumerates as the pass binds each controller.
        // After interrupts, so RegisterDrivers and every probe run with them
        // on on both architectures; before hot-plug starts, so the boot
        // thread is the only one binding devices while the pass runs; and
        // before the kernel's Start, so OnBoot finds them bound. PCI's switch
        // alone, so ILC folds it: a kernel without PCI never calls
        // RegisterDrivers, and trims its override and the whole engine.
        if (Core.CosmosFeatures.PCIEnabled)
        {
            RegisterBuiltInDrivers();
            s_kernel.InvokeRegisterDrivers();
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

    /// <summary>
    /// Registers the catalogue of Cosmos.Kernel.HAL.Drivers, the built-in
    /// drivers written against the driver kit, subsystem by subsystem behind
    /// the kernel's switches: that assembly sees no switch, so the guards
    /// live here. One switch per <c>if</c>, since ILC folds a single switch
    /// only: a kernel built without USB keeps no xHCI, USB mass storage or
    /// USB keyboard code, one built without storage keeps no AHCI, NVMe or
    /// USB mass storage code, one built without network keeps no virtio-net
    /// code, and one built without either input switch keeps no
    /// virtio-input code.
    /// They go through the kit's built-in path, which takes
    /// the names reserved for built-ins and ranks them ahead of every
    /// registration the kernel makes, so a built-in wins a tie and only a
    /// strictly more specific match takes a device from it.
    /// </summary>
    private static void RegisterBuiltInDrivers()
    {
        if (Core.CosmosFeatures.UsbEnabled)
        {
            IReadOnlyList<PciDriverRegistration> usbHosts = BuiltInDrivers.CreateUsbHostRegistrations();
            for (int i = 0; i < usbHosts.Count; i++)
            {
                DriverCore.RegisterBuiltIn(usbHosts[i]);
            }
        }

        if (Core.CosmosFeatures.NetworkEnabled)
        {
            IReadOnlyList<PciDriverRegistration> network = BuiltInDrivers.CreatePciNetworkRegistrations();
            for (int i = 0; i < network.Count; i++)
            {
                DriverCore.RegisterBuiltIn(network[i]);
            }
        }

        if (Core.CosmosFeatures.StorageEnabled)
        {
            IReadOnlyList<PciDriverRegistration> storage = BuiltInDrivers.CreatePciStorageRegistrations();
            for (int i = 0; i < storage.Count; i++)
            {
                DriverCore.RegisterBuiltIn(storage[i]);
            }

            if (Core.CosmosFeatures.UsbEnabled)
            {
                IReadOnlyList<UsbDriverRegistration> usbStorage = BuiltInDrivers.CreateUsbMassStorageRegistrations();
                for (int i = 0; i < usbStorage.Count; i++)
                {
                    DriverCore.RegisterBuiltIn(usbStorage[i]);
                }
            }
        }

        // The virtio-input driver publishes a keyboard or a mouse depending
        // on the device it binds, so either switch is reason to register it.
        // Two ifs rather than one ||, so each branch still tests a single
        // switch for ILC to fold; the driver is registered once either way.
        if (Core.CosmosFeatures.KeyboardEnabled)
        {
            RegisterPciInputDrivers();

            if (Core.CosmosFeatures.UsbEnabled)
            {
                IReadOnlyList<UsbDriverRegistration> usbInput = BuiltInDrivers.CreateUsbInputRegistrations();
                for (int i = 0; i < usbInput.Count; i++)
                {
                    DriverCore.RegisterBuiltIn(usbInput[i]);
                }
            }
        }
        else if (Core.CosmosFeatures.MouseEnabled)
        {
            RegisterPciInputDrivers();
        }
    }

    /// <summary>
    /// Registers the built-in input drivers that bind a PCI function, which
    /// <see cref="RegisterBuiltInDrivers"/> reaches from either input
    /// switch.
    /// </summary>
    private static void RegisterPciInputDrivers()
    {
        IReadOnlyList<PciDriverRegistration> input = BuiltInDrivers.CreatePciInputRegistrations();
        for (int i = 0; i < input.Count; i++)
        {
            DriverCore.RegisterBuiltIn(input[i]);
        }
    }
}
