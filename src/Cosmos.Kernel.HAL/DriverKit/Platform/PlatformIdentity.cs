// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Platform;

/// <summary>
/// The identity of a platform device: a root node the machine description
/// publishes because no bus enumerates it, named the way a device tree
/// would name it. <see cref="Address"/> is the node's "name@hex" form
/// (<c>pci@cf8</c>, <c>pci@3f000000</c>), and <see cref="Compatible"/>
/// the list of strings drivers match on, most specific first. Path is
/// <c>platform:name@hex</c>.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class PlatformIdentity : DeviceIdentity
{
    private readonly string[] _compatible;

    /// <summary>Creates the identity; the strings are copied.</summary>
    /// <param name="address">The node's "name@hex" address, such as <c>pci@cf8</c>.</param>
    /// <param name="compatible">The compatible strings, at least one, most specific first.</param>
    /// <exception cref="ArgumentException"><paramref name="address"/> is empty, <paramref name="compatible"/> is empty, or one of its strings is null or empty.</exception>
    public PlatformIdentity(string address, IReadOnlyList<string> compatible)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);
        ArgumentNullException.ThrowIfNull(compatible);
        if (compatible.Count == 0)
        {
            throw new ArgumentException("A platform identity carries at least one compatible string.", nameof(compatible));
        }

        string[] copy = new string[compatible.Count];
        for (int i = 0; i < copy.Length; i++)
        {
            string value = compatible[i];
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Every compatible string is non-empty.", nameof(compatible));
            }

            copy[i] = value;
        }

        Address = address;
        _compatible = copy;
    }

    /// <summary>The strings drivers match on, most specific first; compared ordinally.</summary>
    public IReadOnlyList<string> Compatible => _compatible;

    /// <inheritdoc/>
    public override string BusName => "platform";

    /// <inheritdoc/>
    public override string Address { get; }

    /// <summary>True when <paramref name="value"/> is one of the compatible strings, compared ordinally; for <see cref="PlatformMatch"/>.</summary>
    /// <param name="value">The string to look for.</param>
    internal bool IsCompatible(string value)
    {
        for (int i = 0; i < _compatible.Length; i++)
        {
            if (string.Equals(_compatible[i], value, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public override string Describe() => $"compatible {string.Join(", ", _compatible)}";
}
