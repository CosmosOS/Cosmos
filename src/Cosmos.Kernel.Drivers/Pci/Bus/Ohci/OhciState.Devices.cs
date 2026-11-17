// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// The address path and its undo: the control ED built for address 0 and
/// linked into the control list, SET_ADDRESS through the control path,
/// the ED's function address and packet size rewritten while skipped, the
/// descriptor head read; the hub registration; the release of a device
/// with every pipe closed and the control ED taken out of the list; and
/// the address table.
/// </summary>
internal sealed partial class OhciState
{
    /// <inheritdoc/>
    protected override UsbDevice? AddressDeviceCore(UsbDevice? parentHub, byte port, UsbSpeed speed)
    {
        if (!ClaimAddressZero())
        {
            _binding.Log($"port {port}: address 0 stayed claimed past its budget");
            return null;
        }

        try
        {
            byte address = AllocateAddress();
            if (address == 0)
            {
                _binding.Log($"port {port}: no free USB address");
                return null;
            }

            OhciDeviceMemory? memory = TakeDeviceMemory();
            if (memory is null)
            {
                _binding.Log($"address {address}: no DMA memory for the endpoint");
                FreeAddress(address);
                return null;
            }

            OhciDevice device = new(this, parentHub as OhciDevice, port, speed, address, memory);
            BuildEndpoint(memory, 0, 0, OhciProtocol.EndpointDirectionFromTd, speed, OhciProtocol.DefaultMaxPacketSize0);
            using (Lock.Acquire())
            {
                _devices[address] = device;
                SetOwner(memory, device);
                LinkEndpoint(ControlHead, memory.Endpoint);
            }

            UsbSetupPacket setAddress = new(UsbRequestType.HostToDevice | UsbRequestType.Standard | UsbRequestType.Device,
                OhciProtocol.SetAddressRequest, address, 0, 0);
            if (ControlTransfer(device, setAddress, Span<byte>.Empty) != UsbTransferStatus.Success)
            {
                _binding.Log($"address {address}: SET_ADDRESS failed");
                ReleaseDeviceCore(device, hostPresent: true);
                return null;
            }

            _binding.Delay(OhciProtocol.SetAddressRecoveryMs * OhciProtocol.MicrosecondsPerMillisecond);
            RewriteControlEndpoint(device, address, device.MaxPacketSize0);

            Span<byte> head = stackalloc byte[OhciProtocol.DeviceDescriptorHeadLength];
            UsbSetupPacket getDescriptor = new(UsbRequestType.DeviceToHost | UsbRequestType.Standard | UsbRequestType.Device,
                (byte)UsbStandardRequest.GetDescriptor, (ushort)((byte)UsbDescriptorType.Device << 8), 0, OhciProtocol.DeviceDescriptorHeadLength);
            if (ControlTransfer(device, getDescriptor, head) != UsbTransferStatus.Success)
            {
                _binding.Log($"address {address}: the addressed device did not return its descriptor");
                ReleaseDeviceCore(device, hostPresent: true);
                return null;
            }

            ushort maxPacketSize = head[OhciProtocol.MaxPacketSize0Offset];
            if (maxPacketSize != 0 && maxPacketSize != device.MaxPacketSize0)
            {
                RewriteControlEndpoint(device, address, maxPacketSize);
                device.SetMaxPacketSize0(maxPacketSize);
            }

            _devicesAddressed++;
            return device;
        }
        finally
        {
            ReleaseAddressZero();
        }
    }

    /// <summary>
    /// Rewrites the static fields of a device's control ED while it stays
    /// in the control list: skipped under the lock, one frame for the
    /// controller to pass it, then dword 0 written whole with the new
    /// function address and packet size and K clear (OHCI 1.0a section
    /// 5.2.7.1.3). Thread context; the caller owns the ED.
    /// </summary>
    private void RewriteControlEndpoint(OhciDevice device, byte address, ushort maxPacketSize)
    {
        using (Lock.Acquire())
        {
            SetSkip(device.Memory, true);
        }

        SettleFrames(OhciProtocol.EndpointModifyFrames);

        using (Lock.Acquire())
        {
            ref OhciEndpointDescriptor ed = ref Ed(device.Memory.Endpoint.Descriptor);
            Volatile.Write(ref ed.Control, EndpointControl(address, 0, OhciProtocol.EndpointDirectionFromTd, device.Speed, maxPacketSize));
        }
    }

    // --- Hubs ---

    /// <summary>
    /// Records the hub's port count and think time on the device. A
    /// full-speed host routes nothing through a transaction translator, so
    /// the controller needs no word of it. Thread context; the body of
    /// <see cref="OhciDevice.ConfigureAsHubCore"/>.
    /// </summary>
    internal bool ConfigureAsHub(OhciDevice device, byte portCount, byte thinkTime)
    {
        device.SetHub(portCount, thinkTime);
        return true;
    }

    // --- Releasing devices ---

    /// <inheritdoc/>
    protected override void ReleaseDeviceCore(UsbDevice device, bool hostPresent)
    {
        if (device is not OhciDevice ohciDevice || !ReferenceEquals(ohciDevice.Controller, this))
        {
            return;
        }

        // Addresses are reused at once, so the table entry has to be this
        // very object: a stale second release must not free another
        // device's address or descriptors.
        using (Lock.Acquire())
        {
            if (!ReferenceEquals(_devices[ohciDevice.Address], ohciDevice))
            {
                return;
            }
        }

        // Idempotent; the attach failure paths release a device nobody
        // marked, and the mark wakes every waiter through OnDisconnectedCore.
        ohciDevice.MarkDisconnected();

        if (hostPresent)
        {
            // A thread's wait ends at once on a disconnected device, so the
            // bound only covers a thread between its claim and its wait.
            long deadline = DeadlineAfter(OhciProtocol.BulkTimeoutMs);
            while (true)
            {
                bool busy;
                using (Lock.Acquire())
                {
                    busy = ohciDevice.ControlBusy;
                    for (int i = 0; i < ohciDevice.BulkPipes.Length && !busy; i++)
                    {
                        OhciBulkPipe? pipe = ohciDevice.BulkPipes[i];
                        busy = pipe is not null && pipe.Busy;
                    }
                }

                if (!busy)
                {
                    break;
                }

                if (Stopwatch.GetTimestamp() >= deadline)
                {
                    _binding.Log($"address {ohciDevice.Address}: a transfer did not release the device");
                    break;
                }

                _binding.Delay(OhciProtocol.PollMicroseconds);
            }
        }

        // Every open pipe is closed: with the hardware present its ED still
        // leaves the list, so the controller never fetches a reused slot.
        for (int i = 0; i < ohciDevice.InterruptPipes.Length; i++)
        {
            OhciInterruptPipe? pipe = ohciDevice.InterruptPipes[i];
            if (pipe is not null)
            {
                ClosePipe(ohciDevice, pipe);
            }
        }

        for (int i = 0; i < ohciDevice.BulkPipes.Length; i++)
        {
            OhciBulkPipe? pipe = ohciDevice.BulkPipes[i];
            if (pipe is not null)
            {
                ClosePipe(ohciDevice, pipe);
            }
        }

        OhciDeviceMemory memory = ohciDevice.Memory;
        if (hostPresent && ControllerRuns)
        {
            UnlinkEndpoint(memory.Endpoint);
            QuiesceEndpoint(memory, keepToggle: false, ohciDevice.Address, 0);
        }
        else
        {
            using (Lock.Acquire())
            {
                UnlinkEndpointCore(memory.Endpoint);
            }
        }

        using (Lock.Acquire())
        {
            _devices[ohciDevice.Address] = null;
            _addressInUse[ohciDevice.Address] = false;
            SetOwner(memory, null);
            _freeDeviceSets.Add(memory);
        }
    }

    // --- Addresses ---

    /// <summary>Takes the lowest free USB address from 1; 0 when all 127 are in use. Thread context.</summary>
    private byte AllocateAddress()
    {
        using (Lock.Acquire())
        {
            for (int address = 1; address <= OhciProtocol.MaxAddress; address++)
            {
                if (!_addressInUse[address])
                {
                    _addressInUse[address] = true;
                    return (byte)address;
                }
            }
        }

        return 0;
    }

    /// <summary>Gives an address back before a device object existed for it. Thread context.</summary>
    private void FreeAddress(byte address)
    {
        using (Lock.Acquire())
        {
            _addressInUse[address] = false;
        }
    }
}
