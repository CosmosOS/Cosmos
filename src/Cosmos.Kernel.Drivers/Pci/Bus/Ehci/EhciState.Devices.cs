// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// The devices: the address path (the default address claimed, an address
/// and a memory set taken, the control queue head built at address 0 and
/// linked, SET_ADDRESS, the queue head rewritten with the address while
/// unlinked, the descriptor head read and the packet size taken the same
/// way), the address table, the hub registration and the release that
/// unlinks the control queue head and pools the set.
/// </summary>
internal sealed partial class EhciState
{
    /// <inheritdoc/>
    protected override UsbDevice? AddressDeviceCore(UsbDevice? parentHub, byte port, UsbSpeed speed)
    {
        if (!ClaimAddressZero())
        {
            _binding.Log("the default address stayed busy");
            return null;
        }

        try
        {
            return AddressDevice(parentHub as EhciDevice, port, speed);
        }
        finally
        {
            // Every exit, the failed ones that released the device included:
            // the release never claims the default address itself.
            ReleaseAddressZero();
        }
    }

    /// <summary>
    /// The address path under the default address claim: the lowest free
    /// address and a memory set taken, the device created and put in the
    /// table so the drain sees it, its control queue head built for
    /// address 0 and linked into the async schedule, SET_ADDRESS through
    /// the normal control path, the recovery interval, the queue head
    /// rewritten with the address while unlinked after an acknowledged
    /// doorbell (dword 1 is static endpoint state the controller may
    /// cache), the 8-byte descriptor head read and bMaxPacketSize0 taken
    /// the same way when it differs. Any failure releases the device and
    /// returns null. Thread context.
    /// </summary>
    private EhciDevice? AddressDevice(EhciDevice? parent, byte port, UsbSpeed speed)
    {
        byte address = AllocateAddress();
        if (address == 0)
        {
            _binding.Log("no free USB address");
            return null;
        }

        EhciDeviceMemory? memory = TakeDeviceMemory();
        if (memory is null)
        {
            _binding.Log($"address {address}: no DMA memory for the endpoint");
            using (Lock.Acquire())
            {
                FreeAddress(address);
            }

            return null;
        }

        EhciDevice device = new(this, parent, port, speed, address, memory);
        using (Lock.Acquire())
        {
            _devices[address] = device;
        }

        // A full or low-speed device reaches the controller only through a
        // high-speed hub's transaction translator; root ports are high
        // speed alone, the rest went to a companion.
        if (speed is UsbSpeed.Low or UsbSpeed.Full && device.TtHubAddress == 0)
        {
            _binding.Log($"address {address}: no high-speed hub above a full-speed device");
            ReleaseDeviceCore(device, hostPresent: true);
            return null;
        }

        using (Lock.Acquire())
        {
            WriteQueueHead(ref memory.QueueHead.QueueHead, EhciProtocol.LinkTerminate,
                ControlCharacteristics(device, 0, device.MaxPacketSize0), ControlCapabilities(device),
                memory.Transfers[memory.DummyIndex].TransferPointer, 0);
            DmaBuffer.WriteBarrier();
            LinkAsync(memory.QueueHead);
        }

        UsbSetupPacket setAddress = new(UsbRequestType.HostToDevice | UsbRequestType.Standard | UsbRequestType.Device,
            EhciProtocol.SetAddressRequest, address, 0, 0);
        if (ControlTransfer(device, setAddress, Span<byte>.Empty) != UsbTransferStatus.Success)
        {
            _binding.Log($"address {address}: SET_ADDRESS failed");
            ReleaseDeviceCore(device, hostPresent: true);
            return null;
        }

        _binding.Delay(EhciProtocol.SetAddressRecoveryMs * EhciProtocol.MicrosecondsPerMillisecond);

        if (!RewriteControlQueueHead(device, address, device.MaxPacketSize0))
        {
            _binding.Log($"address {address}: the control queue head could not be rewritten");
            ReleaseDeviceCore(device, hostPresent: true);
            return null;
        }

        Span<byte> head = stackalloc byte[EhciProtocol.DeviceDescriptorHeadLength];
        UsbSetupPacket getDescriptor = new(UsbRequestType.DeviceToHost | UsbRequestType.Standard | UsbRequestType.Device,
            (byte)UsbStandardRequest.GetDescriptor, (ushort)((byte)UsbDescriptorType.Device << 8), 0, EhciProtocol.DeviceDescriptorHeadLength);
        if (ControlTransfer(device, getDescriptor, head) != UsbTransferStatus.Success)
        {
            _binding.Log($"address {address}: the addressed device did not return its descriptor");
            ReleaseDeviceCore(device, hostPresent: true);
            return null;
        }

        ushort maxPacketSize = head[EhciProtocol.MaxPacketSize0Offset];
        if (maxPacketSize != 0 && maxPacketSize != device.MaxPacketSize0)
        {
            if (!RewriteControlQueueHead(device, address, maxPacketSize))
            {
                _binding.Log($"address {address}: the control queue head could not be rewritten");
                ReleaseDeviceCore(device, hostPresent: true);
                return null;
            }

            device.SetMaxPacketSize0(maxPacketSize);
        }

        _devicesAddressed++;
        return device;
    }

    /// <summary>Queue head dword 1 of a device's control endpoint: DTC set (the toggle comes from each stage's qTD), the C bit for a full or low-speed device, RL 0.</summary>
    /// <param name="device">The device.</param>
    /// <param name="address">The address the queue head targets: 0 until SET_ADDRESS completed.</param>
    /// <param name="maxPacketSize">bMaxPacketSize0.</param>
    private static uint ControlCharacteristics(EhciDevice device, byte address, ushort maxPacketSize)
    {
        return EndpointCharacteristics(address, EhciDevice.ControlEndpointId, device.Speed, toggleFromTransfer: true,
            maxPacketSize, controlEndpoint: device.Speed != UsbSpeed.High);
    }

    /// <summary>Queue head dword 2 of a device's control or bulk endpoint: no split masks, the transaction translator of a full or low-speed device, Mult 1.</summary>
    private static uint ControlCapabilities(EhciDevice device)
    {
        return EndpointCapabilities(0, 0, device.TtHubAddress, device.TtPort, 1);
    }

    /// <summary>
    /// Rewrites the control queue head's endpoint characteristics (the
    /// address after SET_ADDRESS, the packet size after the descriptor
    /// head) while it is unlinked: the async unlink with the doorbell,
    /// then dword 1 and the relink under the lock. Thread context, the
    /// address path.
    /// </summary>
    /// <returns>False when the doorbell was not acknowledged: the set is retired and the device cannot be used.</returns>
    private bool RewriteControlQueueHead(EhciDevice device, byte address, ushort maxPacketSize)
    {
        EhciQueueHeadSlot queueHead = device.Memory.QueueHead;
        if (!UnlinkAsyncQueueHead(queueHead))
        {
            using (Lock.Acquire())
            {
                RetireDeviceMemory(device.Memory);
            }

            return false;
        }

        using (Lock.Acquire())
        {
            queueHead.QueueHead.Characteristics = ControlCharacteristics(device, address, maxPacketSize);
            DmaBuffer.WriteBarrier();
            LinkAsync(queueHead);
        }

        return true;
    }

    // --- Addresses ---

    /// <summary>Takes the lowest free address from 1 under the lock. Thread context, the address path.</summary>
    /// <returns>The address, or 0 when all 127 are taken.</returns>
    private byte AllocateAddress()
    {
        using (Lock.Acquire())
        {
            for (int address = 1; address < _addressTaken.Length; address++)
            {
                if (!_addressTaken[address])
                {
                    _addressTaken[address] = true;
                    return (byte)address;
                }
            }
        }

        return 0;
    }

    /// <summary>Frees an address. Under the lock.</summary>
    private void FreeAddress(byte address)
    {
        _addressTaken[address] = false;
    }

    // --- Hubs ---

    /// <summary>
    /// Records the hub facts on the device: the port count and the think
    /// time. The controller needs no registration of its own; a full or
    /// low-speed device below the hub finds its transaction translator
    /// through the device tree. Thread context; the body of
    /// <see cref="EhciDevice.ConfigureAsHubCore"/>.
    /// </summary>
    /// <param name="device">The hub's device.</param>
    /// <param name="portCount">bNbrPorts.</param>
    /// <param name="thinkTime">TT think time.</param>
    internal bool ConfigureAsHub(EhciDevice device, byte portCount, byte thinkTime)
    {
        device.IsHub = true;
        device.PortCount = portCount;
        device.ThinkTime = thinkTime;
        return true;
    }

    // --- Releasing devices ---

    /// <inheritdoc/>
    protected override void ReleaseDeviceCore(UsbDevice device, bool hostPresent)
    {
        if (device is not EhciDevice ehciDevice || !ReferenceEquals(ehciDevice.Controller, this))
        {
            return;
        }

        // Addresses are reused at once, so the table must still name this
        // object: a stale second release must free neither another device's
        // address nor its descriptors.
        using (Lock.Acquire())
        {
            if (!ReferenceEquals(_devices[ehciDevice.Address], ehciDevice))
            {
                return;
            }
        }

        // Idempotent; the attach failure paths release a device nobody
        // marked, and the mark wakes every waiter through OnDisconnectedCore.
        ehciDevice.MarkDisconnected();

        if (hostPresent)
        {
            // A thread's wait ends at once on a disconnected device, so the
            // bound only covers a thread between its claim and its wait.
            long deadline = DeadlineAfter(EhciProtocol.BulkTimeoutMs);
            while (true)
            {
                bool busy;
                using (Lock.Acquire())
                {
                    busy = ehciDevice.ControlBusy;
                    for (int id = 0; id < ehciDevice.BulkPipes.Length && !busy; id++)
                    {
                        EhciBulkPipe? pipe = ehciDevice.BulkPipes[id];
                        busy = pipe is not null && pipe.Busy;
                    }
                }

                if (!busy)
                {
                    break;
                }

                if (Stopwatch.GetTimestamp() >= deadline)
                {
                    _binding.Log($"address {ehciDevice.Address}: a transfer did not release the device");
                    break;
                }

                _binding.Delay(EhciProtocol.PollMicroseconds);
            }
        }

        // Every open pipe is closed: with the controller running its queue
        // head still leaves the schedule, so a reused slot is never fetched
        // as the old endpoint.
        for (int id = 0; id < ehciDevice.InterruptPipes.Length; id++)
        {
            EhciInterruptPipe? pipe = ehciDevice.InterruptPipes[id];
            if (pipe is not null)
            {
                ClosePipe(ehciDevice, pipe);
            }
        }

        for (int id = 0; id < ehciDevice.BulkPipes.Length; id++)
        {
            EhciBulkPipe? pipe = ehciDevice.BulkPipes[id];
            if (pipe is not null)
            {
                ClosePipe(ehciDevice, pipe);
            }
        }

        EhciDeviceMemory memory = ehciDevice.Memory;
        if (hostPresent)
        {
            // A no-op for a queue head that never reached the schedule (an
            // address path that failed before linking).
            if (!UnlinkAsyncQueueHead(memory.QueueHead))
            {
                using (Lock.Acquire())
                {
                    RetireDeviceMemory(memory);
                }
            }
        }
        else
        {
            using (Lock.Acquire())
            {
                RemoveFromList(memory.QueueHead);
            }
        }

        using (Lock.Acquire())
        {
            _devices[ehciDevice.Address] = null;
            FreeAddress(ehciDevice.Address);
            ReturnDeviceMemory(memory);
        }
    }
}
