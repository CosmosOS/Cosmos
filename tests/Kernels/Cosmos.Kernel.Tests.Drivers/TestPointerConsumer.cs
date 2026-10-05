// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// The suite's stand-in for the ring's mouse manager: installed with
/// <see cref="DeviceRegistry.SetConsumer"/> in <c>BeforeRun</c>, it counts
/// what the kit tells it, sums the movement it was handed and keeps the last
/// report. <see cref="OnRelative"/> and <see cref="OnAbsolute"/> run in the
/// sink caller's context, an interrupt included, so they only write fields.
/// </summary>
internal sealed class TestPointerConsumer : PointerConsumer
{
    private volatile int _publishedCount;
    private volatile int _withdrawnCount;
    private volatile int _relativeCount;
    private volatile int _absoluteCount;
    private volatile int _totalDeltaX;
    private volatile int _totalDeltaY;
    private volatile int _lastDeltaX;
    private volatile int _lastDeltaY;
    private volatile int _lastWheel;
    private volatile PointerButtons _lastButtons;

    /// <summary>How many pointers the kit published to this consumer.</summary>
    public int PublishedCount => _publishedCount;

    /// <summary>How many of them the kit withdrew.</summary>
    public int WithdrawnCount => _withdrawnCount;

    /// <summary>How many relative movement reports reached the consumer.</summary>
    public int RelativeCount => _relativeCount;

    /// <summary>How many absolute position reports reached the consumer.</summary>
    public int AbsoluteCount => _absoluteCount;

    /// <summary>The sum of every horizontal delta reported.</summary>
    public int TotalDeltaX => _totalDeltaX;

    /// <summary>The sum of every vertical delta reported.</summary>
    public int TotalDeltaY => _totalDeltaY;

    /// <summary>The horizontal delta of the last relative report.</summary>
    public int LastDeltaX => _lastDeltaX;

    /// <summary>The vertical delta of the last relative report.</summary>
    public int LastDeltaY => _lastDeltaY;

    /// <summary>The wheel movement of the last relative report.</summary>
    public int LastWheel => _lastWheel;

    /// <summary>The buttons held down in the last report, relative or absolute.</summary>
    public PointerButtons LastButtons => _lastButtons;

    /// <summary>The pointer published most recently, kept until withdrawn.</summary>
    public PublishedDevice? LastPublished { get; private set; }

    /// <summary>The pointer withdrawn most recently.</summary>
    public PublishedDevice? LastWithdrawn { get; private set; }

    /// <summary>The pointer that made the last report.</summary>
    public PublishedDevice? LastDevice { get; private set; }

    /// <inheritdoc/>
    public override void OnPublished(PublishedDevice device)
    {
        LastPublished = device;
        _publishedCount++;
    }

    /// <inheritdoc/>
    public override void OnWithdrawn(PublishedDevice device)
    {
        LastWithdrawn = device;
        if (ReferenceEquals(LastPublished, device))
        {
            LastPublished = null;
        }

        _withdrawnCount++;
    }

    /// <inheritdoc/>
    public override void OnRelative(PublishedDevice device, int deltaX, int deltaY, PointerButtons buttons, int wheel)
    {
        _totalDeltaX += deltaX;
        _totalDeltaY += deltaY;
        _lastDeltaX = deltaX;
        _lastDeltaY = deltaY;
        _lastWheel = wheel;
        _lastButtons = buttons;
        LastDevice = device;
        _relativeCount++;
    }

    /// <inheritdoc/>
    public override void OnAbsolute(PublishedDevice device, int x, int y, PointerButtons buttons)
    {
        _lastButtons = buttons;
        LastDevice = device;
        _absoluteCount++;
    }
}
