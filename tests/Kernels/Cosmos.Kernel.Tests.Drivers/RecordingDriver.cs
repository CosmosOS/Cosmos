// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Pci;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// A driver that only records that it was probed and returns a fixed
/// result, touching nothing on the function. The ranking cells register a
/// few of these to see in which order the pass offers a function.
/// </summary>
internal sealed class RecordingDriver : PciDriver
{
    private readonly string _name;
    private readonly ProbeResult _result;

    public RecordingDriver(string name, ProbeResult result)
    {
        _name = name;
        _result = result;
    }

    /// <inheritdoc />
    // protected internal, not protected: this assembly sees the HAL's
    // internals, so the override must keep the base's full accessibility.
    protected internal override ProbeResult Probe(PciDeviceContext context)
    {
        ProbeLog.Record(_name);
        return _result;
    }
}
