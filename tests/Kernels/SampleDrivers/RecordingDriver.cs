// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace SampleDrivers;

/// <summary>
/// A driver that only records that it was probed and returns a fixed
/// result, touching nothing on the function. The ranking cells register a
/// few of these to see in which order the pass offers a function.
/// </summary>
public sealed class RecordingDriver : PciDriver
{
    private readonly string _name;
    private readonly ProbeResult _result;

    /// <summary>Creates a driver that records <paramref name="name"/> in the probe log and answers <paramref name="result"/>.</summary>
    /// <param name="name">What its Probe records, normally its registration's name.</param>
    /// <param name="result">What its Probe returns.</param>
    public RecordingDriver(string name, ProbeResult result)
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
    public static PciDriverRegistration CreateRegistration(string name, ProbeResult result, PciMatch match) =>
        new(name, () => new RecordingDriver(name, result), match);

    /// <inheritdoc />
    protected override ProbeResult Probe(PciDeviceContext context)
    {
        ProbeLog.Record(_name);
        return _result;
    }
}
