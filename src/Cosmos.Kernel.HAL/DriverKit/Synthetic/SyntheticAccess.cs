// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Synthetic;

/// <summary>
/// The synthetic bus's access object: the bytes the test attached to the
/// device, and, when a window was asked for, a view of the RAM page behind
/// resource 0, so a test can read what a driver wrote through its
/// <see cref="RegisterWindow"/> and write what the driver will read.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed unsafe class SyntheticAccess
{
    private readonly SyntheticPage? _page;
    private readonly int _windowBytes;

    internal SyntheticAccess(byte[] data, SyntheticPage? page, int windowBytes)
    {
        Data = data;
        _page = page;
        _windowBytes = windowBytes;
    }

    /// <summary>The bytes the test attached.</summary>
    public byte[] Data { get; }

    /// <summary>True when the device has a register window (resource 0).</summary>
    public bool HasWindow => _page is not null;

    /// <summary>
    /// The register window's memory, as the test sees it. Empty without a
    /// window, or once the node was retracted and the page released.
    /// </summary>
    public Span<byte> Window => _page is null || _page.IsReleased
        ? Span<byte>.Empty
        : new Span<byte>((void*)_page.Address, _windowBytes);
}
