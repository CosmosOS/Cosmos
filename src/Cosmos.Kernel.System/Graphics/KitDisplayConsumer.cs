// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// The display manager's consumer of the driver kit: every display the kit
/// publishes, the firmware framebuffer included, becomes a
/// <see cref="DisplayDevice"/> in the manager's list, and leaves it when
/// withdrawn. The list is kept in primary order, so the primary display is
/// its first entry: driver displays before the firmware one, driver
/// displays by their node path, firmware displays in publication order,
/// never arrival order, so the choice is stable whichever driver probes
/// first. Installed by <see cref="DisplayManager.Initialize"/>, before the
/// driver stage runs.
/// <para>
/// Contexts: <see cref="OnPublished"/> and <see cref="OnWithdrawn"/> run in
/// thread context, on the boot thread before the engine started for the
/// firmware display or on the kit worker for a driver's display, never
/// concurrently, which is what makes the copy-on-write array and the
/// primary recomputation safe without a lock; <see cref="Primary"/> and
/// <see cref="Count"/> are volatile reads of the current array.
/// <see cref="OnModeChanged"/> runs in the sink caller's context, an
/// interrupt included, and allocates nothing: the device is found by a
/// scan of the copy-on-write array and the line is written fragment by
/// fragment.
/// </para>
/// </summary>
internal sealed class KitDisplayConsumer : DisplayConsumer
{
    private const string Prefix = "[Display] ";

    /// <summary>The displays in primary order; replaced on every change, never changed in place.</summary>
    private DisplayDevice[] _displays = [];

    /// <summary>How many displays are published now. Any context.</summary>
    internal int Count => Volatile.Read(ref _displays).Length;

    /// <summary>The primary display, or null when none is published. Any context.</summary>
    internal DisplayDevice? Primary
    {
        get
        {
            DisplayDevice[] displays = Volatile.Read(ref _displays);
            return displays.Length == 0 ? null : displays[0];
        }
    }

    /// <summary>The display at <paramref name="index"/> in primary order. Any context.</summary>
    /// <param name="index">Position, from 0 to <see cref="Count"/> exclusive.</param>
    /// <param name="display">The display at that position.</param>
    /// <returns>False when the index is out of range.</returns>
    internal bool TryGet(int index, [NotNullWhen(true)] out DisplayDevice? display)
    {
        DisplayDevice[] displays = Volatile.Read(ref _displays);
        if (index < 0 || index >= displays.Length)
        {
            display = null;
            return false;
        }

        display = displays[index];
        return true;
    }

    /// <inheritdoc/>
    public override void OnPublished(PublishedDevice device)
    {
        DisplayDevice[] current = _displays;
        DisplayDevice[] displays = new DisplayDevice[current.Length + 1];
        Array.Copy(current, displays, current.Length);
        displays[current.Length] = new DisplayDevice(device);
        Replace(current, displays);
    }

    /// <inheritdoc/>
    public override void OnWithdrawn(PublishedDevice device)
    {
        DisplayDevice[] current = _displays;
        DisplayDevice? display = Find(current, device);
        if (display is null)
        {
            return;
        }

        DisplayDevice[] displays = new DisplayDevice[current.Length - 1];
        int kept = 0;
        for (int i = 0; i < current.Length; i++)
        {
            if (!ReferenceEquals(current[i], display))
            {
                displays[kept++] = current[i];
            }
        }

        Replace(current, displays);
    }

    /// <inheritdoc/>
    public override void OnModeChanged(PublishedDevice device)
    {
        DisplayDevice? display = Find(_displays, device);
        if (display is null)
        {
            return;
        }

        DisplayMode mode = display.Display.Mode;
        Serial.WriteString(Prefix);
        Serial.WriteString("mode changed: ");
        Serial.WriteString(display.DriverName);
        Serial.WriteString(" \"");
        Serial.WriteString(display.Name);
        Serial.WriteString("\" now ");
        Serial.WriteNumber(mode.Width);
        Serial.WriteString("x");
        Serial.WriteNumber(mode.Height);
        Serial.WriteString("x");
        Serial.WriteNumber(mode.BitsPerPixel);
        Serial.WriteString("\n");
    }

    /// <summary>
    /// Orders the new array by the primary rule, installs it, and logs the
    /// primary when the choice changed. Thread context.
    /// </summary>
    private void Replace(DisplayDevice[] current, DisplayDevice[] displays)
    {
        Sort(displays);
        Volatile.Write(ref _displays, displays);

        DisplayDevice? previous = current.Length == 0 ? null : current[0];
        DisplayDevice? primary = displays.Length == 0 ? null : displays[0];
        if (!ReferenceEquals(previous, primary))
        {
            LogPrimary(displays);
        }
    }

    /// <summary>
    /// A stable insertion sort by <see cref="Compare"/>: equal entries keep
    /// their publication order, which is the rule among firmware displays.
    /// </summary>
    private static void Sort(DisplayDevice[] displays)
    {
        for (int i = 1; i < displays.Length; i++)
        {
            DisplayDevice display = displays[i];
            int j = i - 1;
            while (j >= 0 && Compare(displays[j], display) > 0)
            {
                displays[j + 1] = displays[j];
                j--;
            }

            displays[j + 1] = display;
        }
    }

    /// <summary>
    /// The primary rule: a driver display before the firmware one; among
    /// driver displays the smallest node path by ordinal comparison; firmware
    /// displays compare equal, so their publication order holds.
    /// </summary>
    private static int Compare(DisplayDevice left, DisplayDevice right)
    {
        if (left.IsFirmware != right.IsFirmware)
        {
            return left.IsFirmware ? 1 : -1;
        }

        if (left.IsFirmware)
        {
            return 0;
        }

        return string.CompareOrdinal(left.NodePath, right.NodePath);
    }

    /// <summary>Writes the primary line for the given ordered list. Thread context, so an interpolated string is fine here.</summary>
    private static void LogPrimary(DisplayDevice[] displays)
    {
        if (displays.Length == 0)
        {
            Serial.WriteString($"{Prefix}primary: none\n");
            return;
        }

        DisplayDevice primary = displays[0];
        string reason;
        if (primary.IsFirmware)
        {
            reason = displays.Length == 1 ? "the only display" : "the first published";
        }
        else if (displays.Length == 1)
        {
            reason = "driver published, the only display";
        }
        else if (displays[displays.Length - 1].IsFirmware)
        {
            reason = "driver published, preferred over firmware";
        }
        else
        {
            reason = "driver published, first by node path";
        }

        Serial.WriteString($"{Prefix}primary: {primary.DriverName} \"{primary.Name}\" ({reason})\n");
    }

    /// <summary>The display of a published device, by reference: a scan of a copy-on-write array, allocation-free. Any context.</summary>
    private static DisplayDevice? Find(DisplayDevice[] displays, PublishedDevice device)
    {
        for (int i = 0; i < displays.Length; i++)
        {
            if (ReferenceEquals(displays[i].Published, device))
            {
                return displays[i];
            }
        }

        return null;
    }
}
