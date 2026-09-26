// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace SampleDrivers;

/// <summary>
/// A USB driver that only records that it was offered an interface and
/// returns a fixed result, opening nothing. The USB ranking cells register a
/// few of these to see in which order the kit offers an interface, and
/// which candidates it never offers it to.
/// </summary>
public sealed class UsbRecordingDriver : UsbDriver
{
    private readonly string _name;
    private readonly ProbeResult _result;

    /// <summary>Creates a driver that records <paramref name="name"/> in the probe log and answers <paramref name="result"/>.</summary>
    /// <param name="name">What its Probe records, normally its registration's name.</param>
    /// <param name="result">What its Probe returns.</param>
    public UsbRecordingDriver(string name, ProbeResult result)
    {
        _name = name;
        _result = result;
    }

    /// <summary>
    /// A registration named <paramref name="name"/> whose factory creates a
    /// recording driver answering <paramref name="result"/>, matching
    /// <paramref name="match"/>.
    /// </summary>
    /// <param name="name">The registration's name, which the driver also records.</param>
    /// <param name="result">What every Probe returns.</param>
    /// <param name="match">What the registration matches.</param>
    /// <returns>The registration, for the kernel to pass to DriverManager.Register.</returns>
    public static UsbDriverRegistration CreateRegistration(string name, ProbeResult result, UsbMatch match) =>
        new(name, () => new UsbRecordingDriver(name, result), match);

    /// <inheritdoc />
    protected override ProbeResult Probe(UsbDeviceContext context)
    {
        ProbeLog.Record(_name, context.Path);
        return _result;
    }
}
