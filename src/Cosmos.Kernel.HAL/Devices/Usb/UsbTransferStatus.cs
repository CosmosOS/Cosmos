// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// Outcome of a synchronous USB transfer, independent of the host
/// controller that carried it.
/// </summary>
internal enum UsbTransferStatus
{
    Success,

    /// <summary>The device answered STALL: the request is not supported or the endpoint is halted.</summary>
    Stall,

    /// <summary>The host controller reported no completion within the transfer budget.</summary>
    Timeout,

    /// <summary>Any other host controller or bus error (babble, transaction error, ...).</summary>
    Error,

    /// <summary>The device left the bus; nothing sent to it will run again.</summary>
    Disconnected
}
