// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// The suite's stand-in for the ring's keyboard manager: installed with
/// <see cref="DeviceRegistry.SetConsumer"/> before any keyboard is
/// published, it counts what the kit tells it and keeps the last key it was
/// handed. <see cref="OnKey"/> runs in the sink caller's context, an
/// interrupt included, so it only writes fields.
/// </summary>
internal sealed class TestKeyboardConsumer : KeyboardConsumer
{
    private volatile int _publishedCount;
    private volatile int _withdrawnCount;
    private volatile int _keyCount;
    private volatile byte _lastScanCode;
    private volatile bool _lastReleased;

    /// <summary>How many keyboards the kit published to this consumer.</summary>
    public int PublishedCount => _publishedCount;

    /// <summary>How many of them the kit withdrew.</summary>
    public int WithdrawnCount => _withdrawnCount;

    /// <summary>How many key reports reached the consumer.</summary>
    public int KeyCount => _keyCount;

    /// <summary>The scan code of the last key report.</summary>
    public byte LastScanCode => _lastScanCode;

    /// <summary>Whether the last key report was a release.</summary>
    public bool LastReleased => _lastReleased;

    /// <summary>The keyboard published most recently, kept until withdrawn.</summary>
    public PublishedDevice? LastPublished { get; private set; }

    /// <summary>The keyboard withdrawn most recently.</summary>
    public PublishedDevice? LastWithdrawn { get; private set; }

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
    public override void OnKey(PublishedDevice device, byte scanCode, bool released)
    {
        _lastScanCode = scanCode;
        _lastReleased = released;
        _keyCount++;
    }
}
