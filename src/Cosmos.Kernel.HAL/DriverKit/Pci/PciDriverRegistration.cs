// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Pci;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// A PCI driver as the kernel registers it: the name its functions are
/// owned under, how to create one driver instance per function, and which
/// functions to offer it. A kernel passes it to DriverManager.Register, in
/// Cosmos.Kernel.System.Drivers.
/// </summary>
[Experimental(Experimentals.DriverKitDiagId)]
public sealed class PciDriverRegistration
{
    /// <summary>A private copy of the caller's match table, so it cannot change once registered.</summary>
    private readonly PciMatch[] _matches;

    /// <summary>The driver's name, recorded as the owner of every function it binds. Any context.</summary>
    public string Name { get; }

    /// <summary>Creates the driver instance for one function.</summary>
    internal Func<PciDriver> Factory { get; }

    /// <summary>
    /// Creates a registration. Thread context: it copies
    /// <paramref name="matches"/> into a new array. The factory runs later,
    /// once per function the driver is offered, in the same context as
    /// <see cref="PciDriver.Probe"/>; an exception it throws, or a null it
    /// returns, counts as a failed probe.
    /// </summary>
    /// <param name="name">
    /// The driver's name: the owner recorded on every function it binds, and
    /// the prefix of its log lines. A short lowercase name, such as
    /// <c>rtl8139</c>, unique among the registered drivers, USB ones
    /// included.
    /// </param>
    /// <param name="factory">
    /// Creates one driver instance for each function the driver is offered.
    /// A factory that returns null fails that attempt, as
    /// <see cref="ProbeResult.Failed"/> does.
    /// </param>
    /// <param name="matches">The functions to offer the driver; copied.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty, <paramref name="matches"/> is
    /// empty, or one of its entries is a <c>default(PciMatch)</c>.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="factory"/> is null.</exception>
    public PciDriverRegistration(string name, Func<PciDriver> factory, params ReadOnlySpan<PciMatch> matches)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(factory);
        if (matches.IsEmpty)
        {
            throw new ArgumentException("A registration needs at least one match entry.", nameof(matches));
        }

        for (int i = 0; i < matches.Length; i++)
        {
            if (matches[i].Kind == PciMatchKind.None)
            {
                throw new ArgumentException("A default PciMatch matches nothing; build entries with PciMatch.Device or PciMatch.Class.", nameof(matches));
            }
        }

        Name = name;
        Factory = factory;
        _matches = matches.ToArray();
    }

    /// <summary>
    /// The most specific of this registration's entries that matches
    /// <paramref name="function"/>, which ranks it among the candidates for
    /// the function; <see cref="PciMatchKind.None"/> when none does.
    /// </summary>
    internal PciMatchKind BestMatch(PciDevice function)
    {
        PciMatchKind best = PciMatchKind.None;
        for (int i = 0; i < _matches.Length; i++)
        {
            PciMatch match = _matches[i];
            if (match.Kind > best && match.Matches(function))
            {
                best = match.Kind;
            }
        }

        return best;
    }
}
