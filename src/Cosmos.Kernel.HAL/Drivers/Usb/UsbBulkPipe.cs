// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Usb;
using SchedMutex = Cosmos.Kernel.Core.Scheduler.Mutex;

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// A bulk endpoint a driver opened through
/// <see cref="UsbDeviceContext.TryOpenBulk"/>. Its transfers are
/// synchronous and run in thread context: each call waits for the device.
/// One transfer runs at a time: a call made while another thread's
/// transfer or halt recovery runs on the same pipe waits for it to finish,
/// since both would share the endpoint's data toggle and the host
/// controller's buffer. Once the device left the bus, every call returns
/// <see cref="UsbTransferStatus.Disconnected"/>, before and after the kit
/// called the driver's Remove.
/// </summary>
internal sealed class UsbBulkPipe
{
    private readonly UsbDevice _device;
    private readonly UsbEndpoint _endpoint;

    /// <summary>Serializes the pipe's transfers and halt recoveries, whichever thread calls.</summary>
    private readonly SchedMutex _mutex = new();

    /// <summary>Set when the attempt that opened the pipe is torn down; every call throws from then on.</summary>
    private volatile bool _invalidated;

    /// <summary>bEndpointAddress of the endpoint, which the context looks an open pipe up by.</summary>
    internal byte EndpointAddress => _endpoint.Address;

    internal UsbBulkPipe(UsbDevice device, UsbEndpoint endpoint)
    {
        _device = device;
        _endpoint = endpoint;
    }

    /// <summary>
    /// Reads from a bulk IN endpoint until <paramref name="buffer"/> is full
    /// or the device sends a short packet, which is how it says it has
    /// nothing more. Thread context only.
    /// </summary>
    /// <param name="buffer">Receives the data; its length is the most the call reads.</param>
    /// <returns>The status, and how many bytes arrived, at the start of <paramref name="buffer"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// The endpoint is an OUT endpoint, or the binding attempt that opened
    /// the pipe was declined or failed.
    /// </exception>
    public UsbTransferResult Read(Span<byte> buffer)
    {
        ThrowIfInvalidated();
        if (!_endpoint.IsIn)
        {
            throw new InvalidOperationException("This bulk pipe is an OUT endpoint: write to it instead.");
        }

        _mutex.Acquire();
        try
        {
            ThrowIfInvalidated();
            UsbTransferStatus status = _device.BulkIn(_endpoint, buffer, out int transferred);
            return new UsbTransferResult(status, transferred);
        }
        finally
        {
            _mutex.Release();
        }
    }

    /// <summary>
    /// Writes <paramref name="data"/> to a bulk OUT endpoint. Thread
    /// context only. An empty span sends nothing.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <returns>The status, and how many bytes the device accepted.</returns>
    /// <exception cref="InvalidOperationException">
    /// The endpoint is an IN endpoint, or the binding attempt that opened
    /// the pipe was declined or failed.
    /// </exception>
    public UsbTransferResult Write(ReadOnlySpan<byte> data)
    {
        ThrowIfInvalidated();
        if (_endpoint.IsIn)
        {
            throw new InvalidOperationException("This bulk pipe is an IN endpoint: read from it instead.");
        }

        _mutex.Acquire();
        try
        {
            ThrowIfInvalidated();
            UsbTransferStatus status = _device.BulkOut(_endpoint, data, out int transferred);
            return new UsbTransferResult(status, transferred);
        }
        finally
        {
            _mutex.Release();
        }
    }

    /// <summary>
    /// Clears a halt on the endpoint, after a transfer ended with
    /// <see cref="UsbTransferStatus.Stall"/>: CLEAR_FEATURE(ENDPOINT_HALT)
    /// to the device (USB 2.0 §9.4.5), then the host side back to its
    /// initial state, so both restart their data toggle together. Thread
    /// context only.
    /// </summary>
    /// <returns>The status of the recovery.</returns>
    /// <exception cref="InvalidOperationException">The binding attempt that opened the pipe was declined or failed.</exception>
    public UsbTransferStatus ClearHalt()
    {
        ThrowIfInvalidated();
        _mutex.Acquire();
        try
        {
            ThrowIfInvalidated();
            return _device.ClearHalt(_endpoint);
        }
        finally
        {
            _mutex.Release();
        }
    }

    /// <summary>
    /// Makes every later call throw. Called when the attempt that opened the
    /// pipe is torn down: its interface may then be left without a driver,
    /// and the driver holding the pipe no longer owns it.
    /// </summary>
    internal void Invalidate() => _invalidated = true;

    private void ThrowIfInvalidated()
    {
        if (_invalidated)
        {
            throw new InvalidOperationException("The binding attempt that opened this bulk pipe was declined or failed; the interface is no longer the driver's.");
        }
    }
}
