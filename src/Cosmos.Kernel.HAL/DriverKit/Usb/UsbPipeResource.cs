// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// A pipe on a binding's ledger: the device that opened it, the pipe and
/// the binding. Holds no <see cref="UsbAccess"/>: the ledger's teardown
/// reaches this type from every binding, and must not pull the access
/// object into a kernel without USB.
/// </summary>
internal sealed class UsbPipeResource : IKitResource
{
    private readonly UsbDevice _device;
    private int _released;

    internal UsbPipeResource(UsbDevice device, UsbPipe pipe, DeviceBinding binding)
    {
        _device = device;
        Pipe = pipe;
        Binding = binding;
    }

    /// <summary>The pipe, the lookup key of <see cref="DeviceBinding.FindPipe"/>.</summary>
    internal UsbPipe Pipe { get; }

    /// <summary>The binding that opened the pipe.</summary>
    internal DeviceBinding Binding { get; }

    /// <summary>
    /// Closes the pipe on the device unless the host already closed it.
    /// Thread context: the worker in the unwind, or the driver's thread
    /// through <see cref="UsbAccess.ClosePipe"/>. A second call is a no-op,
    /// and two callers racing (the worker's teardown step and a driver
    /// thread still closing the pipe) close it once: the flag is taken
    /// atomically.
    /// </summary>
    public void Release()
    {
        if (Interlocked.Exchange(ref _released, 1) != 0)
        {
            return;
        }

        if (!Pipe.IsClosed)
        {
            _device.ClosePipe(Pipe);
        }
    }
}
