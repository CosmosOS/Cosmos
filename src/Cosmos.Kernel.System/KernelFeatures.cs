using Cosmos.Kernel.Core;

namespace Cosmos.Kernel.System;

/// <summary>
/// Read-only view of the kernel feature switches, one per
/// <c>CosmosEnable*</c> MSBuild property. Each flag reports whether the
/// corresponding subsystem is compiled into this kernel; ILC folds the
/// flags to constants, so guarded code is trimmed when a feature is off.
/// </summary>
public static class KernelFeatures
{
    /// <summary>
    /// Whether interrupt support (IDT/IRQ) is enabled
    /// (<c>CosmosEnableInterrupts</c>). Disabling it also disables Timer,
    /// Keyboard, Mouse, Network, Scheduler, and Graphics.
    /// </summary>
    public static bool Interrupts => CosmosFeatures.InterruptsEnabled;

    /// <summary>
    /// Whether UART serial support is enabled (<c>CosmosEnableUART</c>).
    /// </summary>
    public static bool Uart => CosmosFeatures.UARTEnabled;

    /// <summary>
    /// Whether PCI support is enabled (<c>CosmosEnablePCI</c>).
    /// </summary>
    public static bool Pci => CosmosFeatures.PCIEnabled;

    /// <summary>
    /// Whether timer support is enabled (<c>CosmosEnableTimer</c>).
    /// Disabling it also disables the scheduler.
    /// </summary>
    public static bool Timer => CosmosFeatures.TimerEnabled;

    /// <summary>
    /// Whether keyboard support is enabled (<c>CosmosEnableKeyboard</c>).
    /// </summary>
    public static bool Keyboard => CosmosFeatures.KeyboardEnabled;

    /// <summary>
    /// Whether mouse support is enabled (<c>CosmosEnableMouse</c>).
    /// </summary>
    public static bool Mouse => CosmosFeatures.MouseEnabled;

    /// <summary>
    /// Whether network support is enabled (<c>CosmosEnableNetwork</c>).
    /// </summary>
    public static bool Network => CosmosFeatures.NetworkEnabled;

    /// <summary>
    /// Whether scheduler and threading support is enabled
    /// (<c>CosmosEnableScheduler</c>).
    /// </summary>
    public static bool Scheduler => CosmosFeatures.SchedulerEnabled;

    /// <summary>
    /// Whether graphics support is enabled (<c>CosmosEnableGraphics</c>).
    /// </summary>
    public static bool Graphics => CosmosFeatures.GraphicsEnabled;

    /// <summary>
    /// Whether block storage support is enabled
    /// (<c>CosmosEnableStorage</c>). Requires PCI and Interrupts.
    /// </summary>
    public static bool Storage => CosmosFeatures.StorageEnabled;

    /// <summary>
    /// Whether the FAT filesystem driver is enabled
    /// (<c>CosmosEnableFat</c>). Requires Storage.
    /// </summary>
    public static bool Fat => CosmosFeatures.FatEnabled;
}
