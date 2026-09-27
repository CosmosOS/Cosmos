// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// A display a registered driver published, as the display manager sees it:
/// an <see cref="IGraphicDevice"/> in front of the driver's own, which
/// forwards every call to it while the display is live.
/// </summary>
/// <remarks>
/// <para>
/// Where <see cref="PublishedBlockDevice"/> throws once its disk stops being
/// live, this one draws nothing and reports a zero-sized screen. A canvas
/// paints many times a second from whichever thread holds the screen, and a
/// frame that lands after the device is gone is simply not shown; there is
/// nothing to lose the way a dropped write loses data. A caller that needs to
/// know asks <see cref="IsWithdrawn"/>.
/// </para>
/// <para>
/// Withdrawal cannot happen in this version: a display arrives on the PCI
/// bus, and a PCI function never leaves it. The state is still tracked, so a
/// display published by an attempt that is then declined or fails is dropped
/// before it ever reaches the manager, and so a bus that can lose a device
/// needs no change here.
/// </para>
/// </remarks>
internal sealed class PublishedDisplay : IGraphicDevice
{
    // As in PublishedBlockDevice: starts Queued and moves forward only, with
    // Gone and Withdrawn final, and an int so Interlocked can carry it.
    private const int Queued = 0;
    private const int Live = 1;
    private const int Gone = 2;
    private const int Withdrawn = 3;

    private readonly DeviceContext _context;
    private readonly IGraphicDevice _device;
    private int _state = Queued;

    /// <summary>The driver's display, which does the drawing.</summary>
    internal IGraphicDevice Device => _device;

    /// <summary>The binding that published the display, whose name its log lines carry.</summary>
    internal DeviceContext Context => _context;

    /// <summary>True once the kit withdrew the display; it draws nothing from then on.</summary>
    internal bool IsWithdrawn => Volatile.Read(ref _state) == Withdrawn;

    /// <summary>True while the display is delivered and drawing reaches the driver.</summary>
    internal bool IsLive => Volatile.Read(ref _state) == Live;

    /// <summary>Creates the display <paramref name="context"/>'s driver publishes. Thread context.</summary>
    /// <param name="context">The binding that published it, which names its log lines.</param>
    /// <param name="device">The driver's display.</param>
    internal PublishedDisplay(DeviceContext context, IGraphicDevice device)
    {
        _context = context;
        _device = device;
    }

    /// <inheritdoc />
    public uint Width => IsLive ? _device.Width : 0;

    /// <inheritdoc />
    public uint Height => IsLive ? _device.Height : 0;

    /// <inheritdoc />
    public uint Pitch => IsLive ? _device.Pitch : 0;

    /// <inheritdoc />
    public void Initialize()
    {
        if (IsLive)
        {
            _device.Initialize();
        }
    }

    /// <inheritdoc />
    public void ClearScreen(uint color)
    {
        if (IsLive)
        {
            _device.ClearScreen(color);
        }
    }

    /// <inheritdoc />
    public void DrawPixel(uint color, int x, int y)
    {
        if (IsLive)
        {
            _device.DrawPixel(color, x, y);
        }
    }

    /// <inheritdoc />
    public uint GetPixel(int x, int y) => IsLive ? _device.GetPixel(x, y) : 0;

    /// <inheritdoc />
    public void GetVRAM(int sourceByteOffset, int[] dest, int destIndex, int count)
    {
        if (IsLive)
        {
            _device.GetVRAM(sourceByteOffset, dest, destIndex, count);
        }
    }

    /// <inheritdoc />
    public void CopyBuffer(ReadOnlyMemory<uint> pixels, int x, int y, int width, int height)
    {
        if (IsLive)
        {
            _device.CopyBuffer(pixels, x, y, width, height);
        }
    }

    /// <inheritdoc />
    public void CopyBuffer(ReadOnlyMemory<int> pixels, int x, int y, int width, int height)
    {
        if (IsLive)
        {
            _device.CopyBuffer(pixels, x, y, width, height);
        }
    }

    /// <inheritdoc />
    public void ClearVRAM(int startByteOffset, int count, int value)
    {
        if (IsLive)
        {
            _device.ClearVRAM(startByteOffset, count, value);
        }
    }

    /// <inheritdoc />
    public void Swap()
    {
        if (IsLive)
        {
            _device.Swap();
        }
    }

    /// <inheritdoc />
    public void Disable()
    {
        if (IsLive)
        {
            _device.Disable();
        }
    }

    /// <summary>
    /// Makes a queued display live, as the kit delivers it to the display
    /// manager: from then on drawing reaches the driver.
    /// </summary>
    /// <returns>False when the display was dropped or withdrawn first, and must not be delivered.</returns>
    internal bool TryGoLive() => Interlocked.CompareExchange(ref _state, Live, Queued) == Queued;

    /// <summary>
    /// Drops a queued display with the attempt that published it, which was
    /// declined or failed: it never reaches the display manager.
    /// </summary>
    internal void Drop() => Interlocked.CompareExchange(ref _state, Gone, Queued);

    /// <summary>
    /// Withdraws the display whose device left the bus, before the kit asks
    /// the display manager to let go of it: drawing from here on reaches
    /// nothing, even meanwhile.
    /// </summary>
    internal void Withdraw() => Volatile.Write(ref _state, Withdrawn);
}
