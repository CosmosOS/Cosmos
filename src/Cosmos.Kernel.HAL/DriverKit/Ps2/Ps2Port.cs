// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Ps2;

/// <summary>
/// The two ports of an 8042 keyboard controller. The identity of a Ps2 node
/// is its port and nothing else: the device behind it is identified by the
/// leaf driver, which resets it anyway.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum Ps2Port : byte
{
    /// <summary>The first port of the 8042: IRQ 1, 8042 commands 0xAB, 0xAD, 0xAE, configuration bit 0.</summary>
    Keyboard = 1,

    /// <summary>The second port: IRQ 12, commands 0xA9, 0xA7, 0xA8, the 0xD4 write prefix, configuration bit 1.</summary>
    Auxiliary = 2,
}
