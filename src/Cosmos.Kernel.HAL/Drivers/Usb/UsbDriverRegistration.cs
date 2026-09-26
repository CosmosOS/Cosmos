// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Usb;

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// A USB class driver as the kernel registers it: the name its interfaces
/// are owned under, how to create one driver instance per interface, and
/// which interfaces to offer it.
/// </summary>
internal sealed class UsbDriverRegistration
{
    /// <summary>A private copy of the caller's match table, so it cannot change once registered.</summary>
    private readonly UsbMatch[] _matches;

    /// <summary>
    /// The driver's name, recorded as the owner of every interface it binds.
    /// PCI and USB registrations share one set of names, which also holds
    /// every built-in driver's.
    /// </summary>
    public string Name { get; }

    /// <summary>Creates the driver instance for one interface.</summary>
    internal Func<UsbDriver> Factory { get; }

    /// <summary>
    /// Creates a registration.
    /// </summary>
    /// <param name="name">
    /// The driver's name: the owner recorded on every interface it binds,
    /// and the prefix of its log lines. A short lowercase name, such as
    /// <c>usb-boot-mouse</c>, unique among the registered drivers, PCI ones
    /// included.
    /// </param>
    /// <param name="factory">Creates one driver instance for each interface the driver is offered.</param>
    /// <param name="matches">The interfaces to offer the driver; copied.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty, <paramref name="matches"/> is
    /// empty, or one of its entries is a <c>default(UsbMatch)</c>.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="factory"/> is null.</exception>
    public UsbDriverRegistration(string name, Func<UsbDriver> factory, params ReadOnlySpan<UsbMatch> matches)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(factory);
        if (matches.IsEmpty)
        {
            throw new ArgumentException("A registration needs at least one match entry.", nameof(matches));
        }

        for (int i = 0; i < matches.Length; i++)
        {
            if (matches[i].Kind == UsbMatchKind.None)
            {
                throw new ArgumentException("A default UsbMatch matches nothing; build entries with UsbMatch.Device or UsbMatch.Interface.", nameof(matches));
            }
        }

        Name = name;
        Factory = factory;
        _matches = matches.ToArray();
    }

    /// <summary>
    /// The most specific of this registration's entries that matches
    /// <paramref name="usbInterface"/> of <paramref name="device"/>, which
    /// ranks it among the candidates for the interface;
    /// <see cref="UsbMatchKind.None"/> when none does.
    /// </summary>
    internal UsbMatchKind BestMatch(UsbDevice device, UsbInterface usbInterface)
    {
        UsbMatchKind best = UsbMatchKind.None;
        for (int i = 0; i < _matches.Length; i++)
        {
            UsbMatch match = _matches[i];
            if (match.Kind > best && match.Matches(device, usbInterface))
            {
                best = match.Kind;
            }
        }

        return best;
    }
}
