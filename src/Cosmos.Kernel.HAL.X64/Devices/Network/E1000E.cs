// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
// Intel 82574 (E1000E) Gigabit Ethernet Controller Driver

using System.Runtime.InteropServices;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Pci;

namespace Cosmos.Kernel.HAL.X64.Devices.Network;

/// <summary>
/// Intel 82574 (E1000E) Gigabit Ethernet Controller Driver.
/// Supports MSI-X interrupts.
/// </summary>
internal class E1000E : PciDevice, INetworkDevice
{
    // E1000E Register Offsets
    private const uint REG_CTRL = 0x0000;        // Device Control
    private const uint REG_STATUS = 0x0008;      // Device Status
    private const uint REG_EECD = 0x0010;        // EEPROM/Flash Control
    private const uint REG_EERD = 0x0014;        // EEPROM Read
    private const uint REG_CTRL_EXT = 0x0018;    // Extended Device Control
    private const uint REG_ICR = 0x00C0;         // Interrupt Cause Read
    private const uint REG_ITR = 0x00C4;         // Interrupt Throttling Rate
    private const uint REG_ICS = 0x00C8;         // Interrupt Cause Set
    private const uint REG_IMS = 0x00D0;         // Interrupt Mask Set/Read
    private const uint REG_IMC = 0x00D8;         // Interrupt Mask Clear
    private const uint REG_RCTL = 0x0100;        // Receive Control
    private const uint REG_TCTL = 0x0400;        // Transmit Control
    private const uint REG_RDBAL = 0x2800;       // RX Descriptor Base Address Low
    private const uint REG_RDBAH = 0x2804;       // RX Descriptor Base Address High
    private const uint REG_RDLEN = 0x2808;       // RX Descriptor Length
    private const uint REG_RDH = 0x2810;         // RX Descriptor Head
    private const uint REG_RDT = 0x2818;         // RX Descriptor Tail
    private const uint REG_TDBAL = 0x3800;       // TX Descriptor Base Address Low
    private const uint REG_TDBAH = 0x3804;       // TX Descriptor Base Address High
    private const uint REG_TDLEN = 0x3808;       // TX Descriptor Length
    private const uint REG_TDH = 0x3810;         // TX Descriptor Head
    private const uint REG_TDT = 0x3818;         // TX Descriptor Tail
    private const uint REG_MTA = 0x5200;         // Multicast Table Array
    private const uint REG_TIPG = 0x0410;        // Transmit Inter Packet Gap
    private const uint REG_RAL0 = 0x5400;        // Receive Address Low
    private const uint REG_RAH0 = 0x5404;        // Receive Address High

    // Control Register Bits
    private const uint CTRL_FD = 1 << 0;         // Full Duplex
    private const uint CTRL_LRST = 1 << 3;       // Link Reset
    private const uint CTRL_ASDE = 1 << 5;       // Auto-Speed Detection Enable
    private const uint CTRL_SLU = 1 << 6;        // Set Link Up
    private const uint CTRL_ILOS = 1 << 7;       // Invert Loss-of-Signal
    private const uint CTRL_RST = 1 << 26;       // Device Reset
    private const uint CTRL_VME = 1 << 30;       // VLAN Mode Enable
    private const uint CTRL_PHY_RST = 1u << 31;  // PHY Reset

    // Status Register Bits
    private const uint STATUS_FD = 1 << 0;       // Full Duplex
    private const uint STATUS_LU = 1 << 1;       // Link Up

    // Receive Control Register Bits
    private const uint RCTL_EN = 1 << 1;         // Receiver Enable
    private const uint RCTL_SBP = 1 << 2;        // Store Bad Packets
    private const uint RCTL_UPE = 1 << 3;        // Unicast Promiscuous Enable
    private const uint RCTL_MPE = 1 << 4;        // Multicast Promiscuous Enable
    private const uint RCTL_LPE = 1 << 5;        // Long Packet Reception Enable
    private const uint RCTL_LBM = 1 << 6;        // Loopback Mode
    private const uint RCTL_BAM = 1 << 15;       // Broadcast Accept Mode
    private const uint RCTL_BSIZE_2048 = 0 << 16; // Buffer Size 2048
    private const uint RCTL_BSIZE_4096 = 3 << 16 | 1 << 25; // Buffer Size 4096
    private const uint RCTL_SECRC = 1 << 26;     // Strip Ethernet CRC

    // Transmit Control Register Bits
    private const uint TCTL_EN = 1 << 1;         // Transmitter Enable
    private const uint TCTL_PSP = 1 << 3;        // Pad Short Packets
    private const int TCTL_CT_SHIFT = 4;         // Collision Threshold
    private const int TCTL_COLD_SHIFT = 12;      // Collision Distance

    // Interrupt Bits
    private const uint ICR_TXDW = 1 << 0;        // TX Descriptor Written Back
    private const uint ICR_TXQE = 1 << 1;        // TX Queue Empty
    private const uint ICR_LSC = 1 << 2;         // Link Status Change
    private const uint ICR_RXDMT0 = 1 << 4;      // RX Descriptor Minimum Threshold
    private const uint ICR_RXT0 = 1 << 7;        // RX Timer Interrupt

    // TX descriptor CMD bits
    private const byte TX_CMD_EOP = 1 << 0;      // End of Packet
    private const byte TX_CMD_IFCS = 1 << 1;     // Insert FCS
    private const byte TX_CMD_RS = 1 << 3;       // Report Status

    // RX descriptor STATUS bits
    private const byte RX_STATUS_DD = 1 << 0;    // Descriptor Done
    private const byte RX_STATUS_EOP = 1 << 1;   // End of Packet

    // Ring geometry (descriptor counts must be a multiple of 8)
    private const int RxDescCount = 32;
    private const int TxDescCount = 32;
    private const int RxBufferSize = 2048;

    // PCI Capability IDs
    private const byte MsiCapabilityId = 0x05;

    /// <summary>
    /// Singleton instance of the E1000E driver.
    /// </summary>
    public static E1000E? Instance { get; private set; }

    private readonly ulong _mmioBase;
    private MACAddress? _macAddress;
    private bool _networkInitialized;
    private bool _linkUp;

    /// <summary>
    /// Event handler for packet received events.
    /// </summary>
    public PacketReceivedHandler? OnPacketReceived { get; set; }

    // Descriptor rings
    private unsafe RxDescriptor* _rxDescriptors;
    private unsafe TxDescriptor* _txDescriptors;
    private unsafe byte** _rxBuffers;
    private unsafe byte** _txBuffers;
    private uint _rxTail;
    private uint _txTail;

    // MSI-X support
    private bool _hasMsix;
    private ulong _msixTableBase;

    string INetworkDevice.Name => "Intel E1000E";
    public MACAddress MacAddress => _macAddress ?? throw new Exception($"{nameof(_macAddress)} is null)");
    public bool LinkUp => _linkUp;
    public bool Ready => _networkInitialized;

    /// <summary>
    /// RX Descriptor structure (16 bytes).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct RxDescriptor
    {
        public ulong BufferAddress;
        public ushort Length;
        public ushort Checksum;
        public byte Status;
        public byte Errors;
        public ushort Special;
    }

    /// <summary>
    /// TX Descriptor structure (16 bytes).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TxDescriptor
    {
        public ulong BufferAddress;
        public ushort Length;
        public byte CSO;          // Checksum Offset
        public byte CMD;          // Command
        public byte Status;
        public byte CSS;          // Checksum Start
        public ushort Special;
    }

    /// <summary>
    /// Creates a new E1000E driver instance.
    /// </summary>
    /// <param name="bus">PCI bus number.</param>
    /// <param name="slot">PCI slot number.</param>
    /// <param name="function">PCI function number.</param>
    public E1000E(uint bus, uint slot, uint function) : base(bus, slot, function)
    {
        // Get MMIO base address from BAR0
        if (BaseAddressBar is { Length: > 0 })
        {
            _mmioBase = BaseAddressBar[0].BaseAddress;
        }

        Instance = this;
    }

    /// <summary>
    /// Tries to find and create an E1000E device on the PCI bus.
    /// </summary>
    /// <returns>The E1000E device if found, null otherwise.</returns>
    public static E1000E? FindAndCreate()
    {
        // Try different E1000E device IDs - check each one directly to avoid array allocation
        PciDevice? device;

        device = PciManager.GetDevice(Pci.Enums.VendorId.Intel, Pci.Enums.DeviceId.E82574L);
        if (device != null && !device.Claimed)
        {
            return new E1000E(device.Bus, device.Slot, device.Function);
        }

        device = PciManager.GetDevice(Pci.Enums.VendorId.Intel, Pci.Enums.DeviceId.E82574IT);
        if (device != null && !device.Claimed)
        {
            return new E1000E(device.Bus, device.Slot, device.Function);
        }

        device = PciManager.GetDevice(Pci.Enums.VendorId.Intel, Pci.Enums.DeviceId.E82574);
        if (device != null && !device.Claimed)
        {
            return new E1000E(device.Bus, device.Slot, device.Function);
        }

        // Additional E1000E device IDs
        device = PciManager.GetDevice(Pci.Enums.VendorId.Intel, Pci.Enums.DeviceId.Pch82577Lm);
        if (device != null && !device.Claimed)
        {
            return new E1000E(device.Bus, device.Slot, device.Function);
        }

        device = PciManager.GetDevice(Pci.Enums.VendorId.Intel, Pci.Enums.DeviceId.Pch82577Lc);
        if (device != null && !device.Claimed)
        {
            return new E1000E(device.Bus, device.Slot, device.Function);
        }

        device = PciManager.GetDevice(Pci.Enums.VendorId.Intel, Pci.Enums.DeviceId.Pch82578Dm);
        if (device != null && !device.Claimed)
        {
            return new E1000E(device.Bus, device.Slot, device.Function);
        }

        // Also check by class (Network Controller = 0x02, Ethernet = 0x00)
        device = PciManager.GetDeviceClass(Pci.Enums.ClassId.NetworkController, (Pci.Enums.SubclassId)0x00, (Pci.Enums.ProgramIf)0x00);
        if (device != null && !device.Claimed && device.VendorId == (ushort)Pci.Enums.VendorId.Intel)
        {
            return new E1000E(device.Bus, device.Slot, device.Function);
        }

        return null;
    }

    public void Initialize()
    {
        InitializeNetwork();
    }

    /// <summary>
    /// Initialize the E1000E network device.
    /// </summary>
    private unsafe void InitializeNetwork()
    {
        if (_networkInitialized)
        {
            return;
        }

        Serial.Write("[E1000E] Initializing...\n");
        Serial.Write("[E1000E] MMIO Base: 0x");
        Serial.WriteHex(_mmioBase);
        Serial.Write("\n");

        // Enable PCI bus mastering and memory space
        EnableMemory(true);
        EnableBusMaster(true);
        Claimed = true;

        // Reset the device
        Reset();

        // Read MAC address
        ReadMacAddress();
        Serial.Write("[E1000E] MAC Address: ");
        Serial.WriteString(_macAddress?.ToString() ?? "No address");
        Serial.Write("\n");

        // Detect MSI-X capability
        DetectMsix();

        // Clear multicast table
        for (uint i = 0; i < 128; i++)
        {
            WriteMmio(REG_MTA + i * 4, 0);
        }

        // Initialize RX
        InitializeRx();

        // Initialize TX
        InitializeTx();

        // Enable interrupts
        EnableInterrupts();

        // Set link up
        uint ctrl = ReadMmio(REG_CTRL);
        ctrl |= CTRL_SLU;
        WriteMmio(REG_CTRL, ctrl);

        // Check link status
        uint status = ReadMmio(REG_STATUS);
        _linkUp = (status & STATUS_LU) != 0;

        Serial.Write("[E1000E] Link: ");
        Serial.Write(_linkUp ? "UP" : "DOWN");
        Serial.Write("\n");

        _networkInitialized = true;
        Serial.Write("[E1000E] Initialized\n");
    }

    /// <summary>
    /// Reset the device.
    /// </summary>
    private void Reset()
    {
        Serial.Write("[E1000E] Resetting device...\n");

        // Disable interrupts first
        WriteMmio(REG_IMC, 0xFFFFFFFF);

        // Reset device
        uint ctrl = ReadMmio(REG_CTRL);
        ctrl |= CTRL_RST;
        WriteMmio(REG_CTRL, ctrl);

        // Wait for reset to complete (typically < 1ms)
        for (int i = 0; i < 1000; i++)
        {
            if ((ReadMmio(REG_CTRL) & CTRL_RST) == 0)
            {
                break;
            }
        }

        // Clear interrupt mask
        WriteMmio(REG_IMC, 0xFFFFFFFF);

        Serial.Write("[E1000E] Reset complete\n");
    }

    /// <summary>
    /// Read MAC address from EEPROM.
    /// </summary>
    private void ReadMacAddress()
    {
        byte[] mac = new byte[6];

        // Try reading from RAL/RAH first (might already be programmed)
        uint ral = ReadMmio(REG_RAL0);
        uint rah = ReadMmio(REG_RAH0);

        if (ral != 0 || (rah & 0xFFFF) != 0)
        {
            mac[0] = (byte)(ral & 0xFF);
            mac[1] = (byte)((ral >> 8) & 0xFF);
            mac[2] = (byte)((ral >> 16) & 0xFF);
            mac[3] = (byte)((ral >> 24) & 0xFF);
            mac[4] = (byte)(rah & 0xFF);
            mac[5] = (byte)((rah >> 8) & 0xFF);
            _macAddress = new MACAddress(mac);
            return;
        }

        // Read from EEPROM
        for (int i = 0; i < 3; i++)
        {
            ushort word = ReadEeprom((ushort)i);
            mac[i * 2] = (byte)(word & 0xFF);
            mac[i * 2 + 1] = (byte)((word >> 8) & 0xFF);
        }

        // Create MACAddress object
        _macAddress = new MACAddress(mac);

        // Program MAC address into RAL/RAH
        ral = (uint)mac[0] | ((uint)mac[1] << 8) |
              ((uint)mac[2] << 16) | ((uint)mac[3] << 24);
        rah = (uint)mac[4] | ((uint)mac[5] << 8) | (1u << 31); // AV bit

        WriteMmio(REG_RAL0, ral);
        WriteMmio(REG_RAH0, rah);
    }

    /// <summary>
    /// Read a word from the EEPROM.
    /// </summary>
    private ushort ReadEeprom(ushort address)
    {
        WriteMmio(REG_EERD, ((uint)address << 8) | 1);

        // Wait for read to complete
        uint value;
        for (int i = 0; i < 1000; i++)
        {
            value = ReadMmio(REG_EERD);
            if ((value & (1 << 4)) != 0)
            {
                return (ushort)((value >> 16) & 0xFFFF);
            }
        }

        return 0;
    }

    /// <summary>
    /// Detect MSI-X capability.
    /// </summary>
    private void DetectMsix()
    {
        // Check if device has capabilities
        ushort status = ReadRegister16((byte)Pci.Enums.Config.Status);
        if ((status & (1 << 4)) == 0)
        {
            Serial.Write("[E1000E] No PCI capabilities\n");
            return;
        }

        // Walk capability list
        byte capPtr = ReadRegister8((byte)Pci.Enums.Config.CapabilityPointer);
        while (capPtr != 0)
        {
            byte capId = ReadRegister8(capPtr);
            if (capId == MsiX.CapId)
            {
                _hasMsix = true;
                // MSI-X table info is at capPtr + 4
                uint tableInfo = ReadRegister32((byte)(capPtr + MsiX.TableOffsetBirOffset));
                uint bar = tableInfo & MsiX.TableBirMask;
                uint offset = tableInfo & MsiX.TableOffsetMask;

                if (BaseAddressBar != null && bar < BaseAddressBar.Length)
                {
                    _msixTableBase = BaseAddressBar[bar].BaseAddress + offset;
                }

                Serial.Write("[E1000E] MSI-X supported, table at BAR");
                Serial.WriteNumber(bar);
                Serial.Write(" offset 0x");
                Serial.WriteHex(offset);
                Serial.Write("\n");
                return;
            }
            else if (capId == MsiCapabilityId)
            {
                Serial.Write("[E1000E] MSI supported (not MSI-X)\n");
            }
            capPtr = ReadRegister8((byte)(capPtr + 1));
        }

        Serial.Write("[E1000E] No MSI-X capability found\n");
    }

    /// <summary>
    /// Initialize receive descriptors.
    /// </summary>
    private unsafe void InitializeRx()
    {
        Serial.Write("[E1000E] Initializing RX...\n");

        // Allocate descriptor ring (16-byte aligned required by E1000)
        // Allocate extra space to ensure we can align to 16 bytes
        int descSize = RxDescCount * sizeof(RxDescriptor);
        byte* rawRxDesc = (byte*)MemoryOp.Alloc((uint)(descSize + 16));
        // Align to 16 bytes
        ulong aligned = ((ulong)rawRxDesc + 15) & ~15UL;
        _rxDescriptors = (RxDescriptor*)aligned;
        MemoryOp.MemSet((byte*)_rxDescriptors, 0, descSize);

        // Allocate buffer pointers array
        _rxBuffers = (byte**)MemoryOp.Alloc((uint)(RxDescCount * sizeof(byte*)));

        // Allocate buffers and set up descriptors
        for (int i = 0; i < RxDescCount; i++)
        {
            _rxBuffers[i] = (byte*)MemoryOp.Alloc(RxBufferSize);
            // E1000 needs physical addresses for DMA
            _rxDescriptors[i].BufferAddress = VirtToPhys((ulong)_rxBuffers[i]);
            _rxDescriptors[i].Status = 0;
        }

        // Set descriptor base address (physical)
        ulong descPhysAddr = VirtToPhys((ulong)_rxDescriptors);
        WriteMmio(REG_RDBAL, (uint)(descPhysAddr & 0xFFFFFFFF));
        WriteMmio(REG_RDBAH, (uint)(descPhysAddr >> 32));

        // Set descriptor length
        WriteMmio(REG_RDLEN, (uint)descSize);

        // Set head and tail
        WriteMmio(REG_RDH, 0);
        WriteMmio(REG_RDT, RxDescCount - 1);
        _rxTail = RxDescCount - 1;

        // Enable receiver. MPE: IPv6 Neighbor Discovery arrives on solicited-node
        // multicast groups, which the empty multicast table would filter out.
        uint rctl = RCTL_EN | RCTL_BAM | RCTL_MPE | RCTL_BSIZE_2048 | RCTL_SECRC;
        WriteMmio(REG_RCTL, rctl);

        Serial.Write("[E1000E] RX initialized\n");
    }

    /// <summary>
    /// Initialize transmit descriptors.
    /// </summary>
    private unsafe void InitializeTx()
    {
        Serial.Write("[E1000E] Initializing TX...\n");

        // Allocate descriptor ring (16-byte aligned required by E1000)
        // Allocate extra space to ensure we can align to 16 bytes
        int descSize = TxDescCount * sizeof(TxDescriptor);
        byte* rawTxDesc = (byte*)MemoryOp.Alloc((uint)(descSize + 16));
        // Align to 16 bytes
        ulong aligned = ((ulong)rawTxDesc + 15) & ~15UL;
        _txDescriptors = (TxDescriptor*)aligned;
        MemoryOp.MemSet((byte*)_txDescriptors, 0, descSize);

        // Allocate buffer pointers array
        _txBuffers = (byte**)MemoryOp.Alloc((uint)(TxDescCount * sizeof(byte*)));

        // Allocate buffers
        for (int i = 0; i < TxDescCount; i++)
        {
            _txBuffers[i] = (byte*)MemoryOp.Alloc(RxBufferSize);
        }

        // Set descriptor base address (physical)
        ulong descPhysAddr = VirtToPhys((ulong)_txDescriptors);
        WriteMmio(REG_TDBAL, (uint)(descPhysAddr & 0xFFFFFFFF));
        WriteMmio(REG_TDBAH, (uint)(descPhysAddr >> 32));

        // Set descriptor length
        WriteMmio(REG_TDLEN, (uint)descSize);

        // Set head and tail
        WriteMmio(REG_TDH, 0);
        WriteMmio(REG_TDT, 0);
        _txTail = 0;

        // Set Transmit Inter Packet Gap (required for E1000)
        // IPGT = 10, IPGR1 = 8, IPGR2 = 6 (default values for IEEE 802.3)
        uint tipg = 10 | (8 << 10) | (6 << 20);
        WriteMmio(REG_TIPG, tipg);

        // Enable transmitter
        // CT = 15, COLD = 64 bytes for full duplex
        uint tctl = TCTL_EN | TCTL_PSP | (15u << TCTL_CT_SHIFT) | (64u << TCTL_COLD_SHIFT);
        WriteMmio(REG_TCTL, tctl);

        Serial.Write("[E1000E] TX initialized\n");
    }

    /// <summary>
    /// Enable interrupts.
    /// </summary>
    private void EnableInterrupts()
    {
        // Enable RX and link change interrupts
        uint ims = ICR_RXT0 | ICR_LSC | ICR_RXDMT0;
        WriteMmio(REG_IMS, ims);
    }

    /// <summary>
    /// Register the IRQ handler for the E1000E.
    /// </summary>
    public void RegisterIRQHandler()
    {
        byte irq = InterruptLine;
        Serial.Write("[E1000E] Registering IRQ ");
        Serial.WriteNumber(irq);
        Serial.Write(" handler...\n");

        InterruptManager.SetIrqHandler(irq, HandleIRQ);

        Serial.Write("[E1000E] IRQ handler registered\n");
    }

    /// <summary>
    /// IRQ handler.
    /// </summary>
    private static void HandleIRQ(ref IRQContext context)
    {
        if (Instance == null)
        {
            return;
        }

        // Read and clear interrupt cause
        uint icr = Instance.ReadMmio(REG_ICR);

        if ((icr & ICR_LSC) != 0)
        {
            // Link status changed
            uint status = Instance.ReadMmio(REG_STATUS);
            Instance._linkUp = (status & STATUS_LU) != 0;
            Serial.Write("[E1000E] Link status changed: ");
            Serial.Write(Instance._linkUp ? "UP" : "DOWN");
            Serial.Write("\n");
        }

        if ((icr & (ICR_RXT0 | ICR_RXDMT0)) != 0)
        {
            // Packet received
            Instance.ProcessReceivedPackets();
        }
    }

    /// <summary>
    /// Process received packets.
    /// </summary>
    private unsafe void ProcessReceivedPackets()
    {
        while (true)
        {
            uint head = ReadMmio(REG_RDH);
            uint next = (_rxTail + 1) % RxDescCount;

            if (next == head)
            {
                break;
            }

            RxDescriptor* desc = &_rxDescriptors[next];

            if ((desc->Status & RX_STATUS_DD) == 0)
            {
                break;
            }

            if ((desc->Status & RX_STATUS_EOP) != 0 && desc->Errors == 0)
            {
                // Valid packet received
                int length = desc->Length;
                byte[] data = new byte[length];

                byte* src = _rxBuffers[next];
                for (int i = 0; i < length; i++)
                {
                    data[i] = src[i];
                }

                // Invoke callback
                OnPacketReceived?.Invoke(data, length);
            }

            // Clear status and give buffer back to hardware
            desc->Status = 0;
            _rxTail = next;
            WriteMmio(REG_RDT, _rxTail);
        }
    }

    /// <summary>
    /// Send a packet.
    /// </summary>
    public unsafe bool Send(byte[] data, int length)
    {
        if (!_networkInitialized || data == null || length <= 0 || length > RxBufferSize)
        {
            return false;
        }

        uint next = (_txTail + 1) % TxDescCount;
        uint head = ReadMmio(REG_TDH);

        // Check if ring is full
        if (next == head)
        {
            return false;
        }

        // Copy data to buffer
        byte* dst = _txBuffers[_txTail];
        for (int i = 0; i < length; i++)
        {
            dst[i] = data[i];
        }

        // Set up descriptor (use physical address for DMA)
        TxDescriptor* desc = &_txDescriptors[_txTail];
        desc->BufferAddress = VirtToPhys((ulong)dst);
        desc->Length = (ushort)length;
        desc->CMD = TX_CMD_EOP | TX_CMD_IFCS | TX_CMD_RS;
        desc->Status = 0;

        // Advance tail
        _txTail = next;
        WriteMmio(REG_TDT, _txTail);

        return true;
    }

    /// <summary>
    /// Enable the network device.
    /// </summary>
    public void Enable()
    {
        // Enable RX
        uint rctl = ReadMmio(REG_RCTL);
        rctl |= RCTL_EN;
        WriteMmio(REG_RCTL, rctl);

        // Enable TX
        uint tctl = ReadMmio(REG_TCTL);
        tctl |= TCTL_EN;
        WriteMmio(REG_TCTL, tctl);
    }

    /// <summary>
    /// Disable the network device.
    /// </summary>
    public void Disable()
    {
        // Disable RX
        uint rctl = ReadMmio(REG_RCTL);
        rctl &= ~RCTL_EN;
        WriteMmio(REG_RCTL, rctl);

        // Disable TX
        uint tctl = ReadMmio(REG_TCTL);
        tctl &= ~TCTL_EN;
        WriteMmio(REG_TCTL, tctl);
    }

    /// <summary>
    /// Read a device register from MMIO.
    /// </summary>
    private uint ReadMmio(uint offset)
    {
        return Native.MMIO.Read32(_mmioBase + offset);
    }

    /// <summary>
    /// Write a device register to MMIO.
    /// </summary>
    private void WriteMmio(uint offset, uint value)
    {
        Native.MMIO.Write32(_mmioBase + offset, value);
    }

    /// <summary>
    /// Gets whether MSI-X is supported.
    /// </summary>
    public bool HasMsix => _hasMsix;

    /// <summary>
    /// Convert virtual address to physical address for DMA.
    /// </summary>
    private static ulong VirtToPhys(ulong virtualAddress)
    {
        if (virtualAddress >= PageAllocator.DefaultHhdmOffset)
        {
            return virtualAddress - PageAllocator.DefaultHhdmOffset;
        }

        return virtualAddress;
    }

}
