// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Contexts;

/// <summary>Endpoint Context EP State field (xHCI 1.2 table 6-8).</summary>
internal enum EndpointState : byte
{
    Disabled = 0,
    Running = 1,
    Halted = 2,
    Stopped = 3,
    Error = 4
}
