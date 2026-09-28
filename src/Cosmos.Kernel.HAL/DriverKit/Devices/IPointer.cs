// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>What a pointer driver implements and hands to <see cref="DeviceBinding.PublishPointer"/>.</summary>
internal interface IPointer
{
    /// <summary>The device's name.</summary>
    string Name { get; }
}
