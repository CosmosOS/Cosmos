// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Tools.Launcher;

/// <summary>
/// One extra <c>-device</c> model attached to the guest beside the NIC, input
/// and display selections of <see cref="QemuLaunchOptions"/>, such as QEMU's
/// <c>edu</c> test device or a NIC that no built-in driver claims.
/// </summary>
public sealed record DeviceAttachment
{
    /// <summary>QEMU <c>-device</c> model name, e.g. <c>edu</c> or <c>rtl8139</c>.</summary>
    public required string Model { get; init; }

    /// <summary>
    /// True for a NIC model. The launcher then gives it a user-mode
    /// <c>-netdev</c> of its own, so the card has a link and a DHCP server
    /// behind it instead of sitting unplugged.
    /// </summary>
    public bool IsNetworkCard { get; init; }
}
