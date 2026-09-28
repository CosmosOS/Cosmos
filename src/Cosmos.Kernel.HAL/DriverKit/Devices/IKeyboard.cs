// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>What a keyboard driver implements and hands to <see cref="DeviceBinding.PublishKeyboard"/>. Called by the ring in thread context.</summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public interface IKeyboard
{
    /// <summary>The device's name.</summary>
    string Name { get; }

    /// <summary>Lights the given indicators.</summary>
    /// <param name="leds">The indicators to light.</param>
    void SetLeds(KeyboardLeds leds);
}
