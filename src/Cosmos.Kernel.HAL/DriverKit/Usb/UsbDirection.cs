// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// Which way an endpoint moves data, seen from the host: bit 7 of its
/// bEndpointAddress (USB 2.0 §9.6.6).
/// </summary>
[Experimental(Experimentals.DriverKitDiagId)]
public enum UsbDirection
{
    /// <summary>Host to device.</summary>
    Out,

    /// <summary>Device to host.</summary>
    In
}
