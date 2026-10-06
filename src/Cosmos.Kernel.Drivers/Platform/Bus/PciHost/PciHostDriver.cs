// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.DriverKit.Platform;

namespace Cosmos.Kernel.Drivers.Platform.Bus.PciHost;

/// <summary>
/// The PCI host driver: binds a platform node whose access object is a
/// <see cref="PciHostAccess"/> (the legacy port mechanism on x64, an ECAM
/// window from MCFG on ARM64) and publishes one child node per function it
/// finds. The walk is the legacy scan's: the host's first bus, then every
/// bus a PCI-to-PCI bridge leads to and, on the legacy host, every bus a
/// host bridge function of device 00:00 roots, each bus once. A PCI
/// Express root port or downstream port with a hot-plug slot is published
/// and its bus left to <c>PcieRootPortDriver</c>. The kit
/// offers the children to the device drivers and retracts them with this
/// node, so the driver keeps no per-device state and has no detach work.
/// <see cref="Probe"/> runs in thread context on the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Pci)]
public sealed class PciHostDriver : Driver
{
    /// <summary>Highest device number on a bus: the device field is five bits.</summary>
    private const int MaxDevice = 31;

    /// <summary>Highest function number of a device: the function field is three bits.</summary>
    private const int MaxFunction = 7;

    /// <summary>Number of buses a host can decode: the bus number is eight bits.</summary>
    private const int BusCount = 256;

    /// <summary>Bits per word of the visited set.</summary>
    private const int BitsPerWord = 64;

    /// <summary>Header type register offset.</summary>
    private const ushort HeaderTypeOffset = 0x0E;

    /// <summary>Header type bit 7: the device implements functions beyond 0.</summary>
    private const byte MultiFunctionBit = 0x80;

    /// <summary>Secondary bus number offset of a type 1 header.</summary>
    private const ushort SecondaryBusOffset = 0x19;

    /// <summary>Header type of a PCI-to-PCI bridge, the one layout with a secondary bus number at <see cref="SecondaryBusOffset"/>.</summary>
    private const byte PciToPciBridgeHeaderType = 1;

    /// <summary>Base class of the bridge devices.</summary>
    private const byte BridgeClassCode = 0x06;

    /// <summary>Bridge subclass of a host bridge.</summary>
    private const byte HostBridgeSubclass = 0x00;

    /// <summary>Bridge subclass of a PCI-to-PCI bridge.</summary>
    private const byte PciToPciBridgeSubclass = 0x04;

    /// <summary>The compatible string of the host over the x86 port mechanism, whose bus 0 host bridge functions root further buses.</summary>
    private const string LegacyHostCompatible = "pci-host-legacy";

    /// <summary>The compatible string of a host over an ECAM window.</summary>
    private const string EcamHostCompatible = "pci-host-ecam-generic";

    private readonly PlatformMatch _legacyHost = PlatformMatch.Compatible(LegacyHostCompatible);
    private readonly DeviceMatch[] _matches =
    [
        PlatformMatch.Compatible(EcamHostCompatible),
        PlatformMatch.Compatible(LegacyHostCompatible),
    ];

    /// <inheritdoc/>
    public override string Name => nameof(PciHostDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <summary>
    /// Enumerates the host's buses and publishes a child node for every
    /// function found. Thread context on the kit worker; the children are
    /// offered once this probe returns.
    /// </summary>
    /// <param name="binding">The host node and the kit facilities for it.</param>
    /// <returns>Bound once the scan is published; declined when the node carries no <see cref="PciHostAccess"/>.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        if (!binding.Node.TryGetAccess(out PciHostAccess? host))
        {
            return ProbeResult.Declined("the node carries no PCI host access");
        }

        bool legacyHost = _legacyHost.Matches(binding.Node.Identity);
        Span<ulong> visited = stackalloc ulong[BusCount / BitsPerWord];
        visited.Clear();
        Span<byte> pending = stackalloc byte[BusCount];
        int pendingCount = 0;
        int nextPending = 0;
        int functions = 0;
        int buses = 0;

        QueueBus(host, visited, pending, ref pendingCount, host.StartBus);
        while (nextPending < pendingCount)
        {
            byte bus = pending[nextPending++];
            buses++;
            for (int device = 0; device <= MaxDevice; device++)
            {
                if (!host.TryDescribeFunction(bus, (byte)device, 0, out PciFunctionDescription first))
                {
                    continue;
                }

                functions++;
                Publish(binding, host, visited, pending, ref pendingCount, legacyHost, first);
                bool multiFunction = (host.ReadConfig8(bus, (byte)device, 0, HeaderTypeOffset) & MultiFunctionBit) != 0;
                if (!multiFunction)
                {
                    continue;
                }

                for (int function = 1; function <= MaxFunction; function++)
                {
                    if (!host.TryDescribeFunction(bus, (byte)device, (byte)function, out PciFunctionDescription description))
                    {
                        continue;
                    }

                    functions++;
                    Publish(binding, host, visited, pending, ref pendingCount, legacyHost, description);
                }
            }
        }

        binding.Log($"{functions} functions on {buses} buses");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Publishes one function as a child node and queues the bus it bridges
    /// to, if any: the secondary bus of a PCI-to-PCI bridge (class 06.04
    /// with a type 1 header), or, on the legacy host, bus N for a host
    /// bridge at function N of device 00:00. Thread context on the kit
    /// worker, from the probe.
    /// </summary>
    private static void Publish(DeviceBinding binding, PciHostAccess host, Span<ulong> visited, Span<byte> pending, ref int pendingCount, bool legacyHost, PciFunctionDescription description)
    {
        binding.PublishChild(description.Identity, description.Resources, description.Interrupts, description.Access);

        PciIdentity identity = description.Identity;
        if (identity.ClassCode != BridgeClassCode)
        {
            return;
        }

        if (identity.Subclass == PciToPciBridgeSubclass && identity.HeaderType == PciToPciBridgeHeaderType)
        {
            if (description.Access.IsHotPlugSlot)
            {
                // The bus behind a hot-plug slot belongs to the slot's driver, which
                // describes and publishes what sits there at boot and on every arrival
                // and places the registers of a function firmware never saw.
                return;
            }

            byte secondary = host.ReadConfig8(identity.Bus, identity.Device, identity.Function, SecondaryBusOffset);
            QueueBus(host, visited, pending, ref pendingCount, secondary);
        }
        else if (legacyHost && identity.Subclass == HostBridgeSubclass && identity.Bus == 0 && identity.Device == 0)
        {
            // The legacy scan's rule: function N of device 00:00 that is a
            // host bridge roots bus N.
            QueueBus(host, visited, pending, ref pendingCount, identity.Function);
        }
    }

    /// <summary>Queues a bus for the walk when the host decodes it and it was not queued before. Thread context on the kit worker, from the probe; allocation-free.</summary>
    private static void QueueBus(PciHostAccess host, Span<ulong> visited, Span<byte> pending, ref int pendingCount, int bus)
    {
        if (bus < host.StartBus || bus > host.EndBus)
        {
            return;
        }

        ref ulong word = ref visited[bus / BitsPerWord];
        ulong bit = 1UL << (bus % BitsPerWord);
        if ((word & bit) != 0)
        {
            return;
        }

        word |= bit;
        pending[pendingCount++] = (byte)bus;
    }
}
