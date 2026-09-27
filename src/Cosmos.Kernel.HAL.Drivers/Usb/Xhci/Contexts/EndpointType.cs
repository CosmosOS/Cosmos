// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Contexts;

/// <summary>Endpoint Context EP Type field (xHCI 1.2 table 6-9).</summary>
internal enum EndpointType : byte
{
    IsochOut = 1,
    BulkOut = 2,
    InterruptOut = 3,
    Control = 4,
    IsochIn = 5,
    BulkIn = 6,
    InterruptIn = 7
}
