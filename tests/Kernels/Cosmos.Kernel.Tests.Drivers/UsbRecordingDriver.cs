// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// A USB driver that only records that it was offered an interface and
/// returns a fixed result, opening nothing. The USB ranking cells register a
/// few of these to see in which order the kit offers an interface, and
/// which candidates it never offers it to.
/// </summary>
internal sealed class UsbRecordingDriver : UsbDriver
{
    private readonly string _name;
    private readonly ProbeResult _result;

    /// <summary>Creates a driver that records <paramref name="name"/> in the probe log and answers <paramref name="result"/>.</summary>
    public UsbRecordingDriver(string name, ProbeResult result)
    {
        _name = name;
        _result = result;
    }

    /// <inheritdoc />
    protected internal override ProbeResult Probe(UsbDeviceContext context)
    {
        ProbeLog.Record(_name, context.Path);
        return _result;
    }
}
