using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.Core;

/// <summary>
/// Centralized feature flags for Cosmos kernel components.
/// These flags can be set via RuntimeHostConfigurationOption in csproj
/// and are used by ILC for trimming.
/// </summary>
internal static class CosmosFeatures
{
    /// <summary>
    /// Controls interrupt setup (IDT/IRQ). Disabling this also disables Timer, Keyboard,
    /// Mouse, Network, Scheduler, and Graphics via the MSBuild cascade in Sdk.targets.
    /// Set via CosmosEnableInterrupts property in csproj.
    /// </summary>
    [FeatureSwitchDefinition("Cosmos.Kernel.HAL.Interrupts.Enabled")]
    public static bool InterruptsEnabled =>
        AppContext.TryGetSwitch("Cosmos.Kernel.HAL.Interrupts.Enabled", out bool enabled) ? enabled : true;

    /// <summary>
    /// Controls UART support initialization.
    /// Set via CosmosEnableUART property in csproj.
    /// </summary>
    [FeatureSwitchDefinition("Cosmos.Kernel.HAL.UART.Enabled")]
    public static bool UARTEnabled =>
        AppContext.TryGetSwitch("Cosmos.Kernel.HAL.UART.Enabled", out bool enabled) ? enabled : true;

    /// <summary>
    /// Controls PCI support initialization.
    /// Set via CosmosEnablePCI property in csproj.
    /// </summary>
    [FeatureSwitchDefinition("Cosmos.Kernel.HAL.PCI.Enabled")]
    public static bool PCIEnabled =>
        AppContext.TryGetSwitch("Cosmos.Kernel.HAL.PCI.Enabled", out bool enabled) ? enabled : true;

    /// <summary>
    /// Controls timer (PIT/HPET) initialization. Disabling this also disables Scheduler.
    /// Set via CosmosEnableTimer property in csproj.
    /// </summary>
    [FeatureSwitchDefinition("Cosmos.Kernel.System.Timer.Enabled")]
    public static bool TimerEnabled =>
        AppContext.TryGetSwitch("Cosmos.Kernel.System.Timer.Enabled", out bool enabled) ? enabled : true;

    /// <summary>
    /// Controls keyboard support initialization.
    /// Set via CosmosEnableKeyboard property in csproj.
    /// </summary>
    [FeatureSwitchDefinition("Cosmos.Kernel.System.Input.Keyboard.Enabled")]
    public static bool KeyboardEnabled =>
        AppContext.TryGetSwitch("Cosmos.Kernel.System.Input.Keyboard.Enabled", out bool enabled) ? enabled : true;

    /// <summary>
    /// Controls network support initialization.
    /// Set via CosmosEnableNetwork property in csproj.
    /// </summary>
    [FeatureSwitchDefinition("Cosmos.Kernel.System.Network.Enabled")]
    public static bool NetworkEnabled =>
        AppContext.TryGetSwitch("Cosmos.Kernel.System.Network.Enabled", out bool enabled) ? enabled : true;

    /// <summary>
    /// Controls scheduler/threading support initialization.
    /// Set via CosmosEnableScheduler property in csproj.
    /// </summary>
    [FeatureSwitchDefinition("Cosmos.Kernel.Core.Scheduler.Enabled")]
    public static bool SchedulerEnabled =>
        AppContext.TryGetSwitch("Cosmos.Kernel.Core.Scheduler.Enabled", out bool enabled) ? enabled : true;

    /// <summary>
    /// Controls graphics support initialization.
    /// Set via CosmosEnableGraphics property in csproj.
    /// </summary>
    [FeatureSwitchDefinition("Cosmos.Kernel.System.Graphics.Enabled")]
    public static bool GraphicsEnabled =>
        AppContext.TryGetSwitch("Cosmos.Kernel.System.Graphics.Enabled", out bool enabled) ? enabled : true;

    /// <summary>
    /// Controls mouse support initialization.
    /// Set via CosmosEnableMouse property in csproj.
    /// </summary>
    [FeatureSwitchDefinition("Cosmos.Kernel.System.Input.Mouse.Enabled")]
    public static bool MouseEnabled =>
        AppContext.TryGetSwitch("Cosmos.Kernel.System.Input.Mouse.Enabled", out bool enabled) ? enabled : true;

    /// <summary>
    /// Controls block storage support initialization (AHCI/SATA, etc.).
    /// Requires PCI and Interrupts; the MSBuild cascade in Sdk.targets disables this when either is off.
    /// Set via CosmosEnableStorage property in csproj.
    /// </summary>
    [FeatureSwitchDefinition("Cosmos.Kernel.System.Storage.Enabled")]
    public static bool StorageEnabled =>
        AppContext.TryGetSwitch("Cosmos.Kernel.System.Storage.Enabled", out bool enabled) ? enabled : true;

    /// <summary>
    /// Controls FAT filesystem driver registration. Requires Storage; the
    /// MSBuild cascade in Sdk.targets disables this when Storage is off.
    /// Set via CosmosEnableFat property in csproj.
    /// </summary>
    [FeatureSwitchDefinition("Cosmos.Kernel.System.Filesystems.Fat.Enabled")]
    public static bool FatEnabled =>
        AppContext.TryGetSwitch("Cosmos.Kernel.System.Filesystems.Fat.Enabled", out bool enabled) ? enabled : true;
}
