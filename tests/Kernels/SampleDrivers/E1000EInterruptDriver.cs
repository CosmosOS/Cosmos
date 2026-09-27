// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Pci;

namespace SampleDrivers;

/// <summary>
/// A driver naming the 82574L (8086:10d3), which only requests its
/// interrupts: on the e1000e-arm64 cells it shows which way the kit delivers
/// them, MSI-X through a GICv3 ITS or polled on GICv2, and that an MSI-X
/// entry is programmed masked and unmasked on Bound. It names the device
/// rather than the Ethernet class so that it beats the built-in E1000E
/// driver, whose class match would otherwise be offered the NIC first and
/// take it: a strictly more specific match is what takes a device from a
/// built-in, and this cell is where the kit proves it. No other NIC is
/// offered to it.
/// </summary>
public sealed class E1000EInterruptDriver : PciDriver
{
    /// <summary>The registration's name, and the owner the 82574L gets.</summary>
    public const string Name = "e1000e-irq";

    private const ushort IntelVendorId = 0x8086;
    private const ushort I82574LDeviceId = 0x10D3;

    /// <summary>Config offset of the Command register.</summary>
    private const ushort CommandOffset = 0x04;

    // Written from the interrupt handler, read by the cells.
    private static int s_handlerCalls;

    /// <summary>True once Probe ran on the 82574L.</summary>
    public static bool Probed { get; private set; }

    /// <summary>The Command register as Probe found it, the first driver the NIC was offered to.</summary>
    public static ushort CommandAtProbe { get; private set; }

    /// <summary>What TryRequestInterrupts answered.</summary>
    public static bool InterruptsGranted { get; private set; }

    /// <summary>MSI-X Enable right after the request.</summary>
    public static bool MsiXEnabledAfterRequest { get; private set; }

    /// <summary>The BAR holding the MSI-X table, mapped during Probe; null when it did not map.</summary>
    public static MmioRegion? MsiXTable { get; private set; }

    /// <summary>Where entry 0's Vector Control register sits in <see cref="MsiXTable"/>.</summary>
    public static ulong Entry0Control { get; private set; }

    /// <summary>Entry 0's mask bit during Probe, after the request.</summary>
    public static bool Entry0MaskedInProbe { get; private set; }

    /// <summary>The context Probe was handed, to read the function's state once Bound.</summary>
    public static PciDeviceContext? Context { get; private set; }

    /// <summary>
    /// Calls of the handler. The driver enables no interrupt cause on the
    /// NIC, so with MSI-X it is never called; polled, once per tick.
    /// </summary>
    public static int HandlerCalls => Volatile.Read(ref s_handlerCalls);

    /// <summary>
    /// The registration the kernel passes to DriverManager.Register: the
    /// 82574L by vendor and device ID, the one NIC this driver takes.
    /// </summary>
    /// <returns>A registration named <see cref="Name"/>.</returns>
    public static PciDriverRegistration CreateRegistration() =>
        new(Name, static () => new E1000EInterruptDriver(), PciMatch.Device(IntelVendorId, I82574LDeviceId));

    /// <inheritdoc />
    protected override ProbeResult Probe(PciDeviceContext context)
    {
        ProbeLog.Record(Name);
        PciFunction function = context.Function;
        Probed = true;
        Context = context;
        CommandAtProbe = function.ReadConfig16(CommandOffset);
        InterruptsGranted = context.TryRequestInterrupts(OnInterrupt);
        MsiXEnabledAfterRequest = MsiXState.IsEnabled(function);
        if (MsiXState.TryMapEntry0Control(context, out MmioRegion? table, out ulong entryControl))
        {
            MsiXTable = table;
            Entry0Control = entryControl;
            Entry0MaskedInProbe = (table.Read32(entryControl) & MsiXState.EntryMaskBit) != 0;
        }

        return ProbeResult.Bound;
    }

    private static void OnInterrupt(int vector) => s_handlerCalls++;
}
