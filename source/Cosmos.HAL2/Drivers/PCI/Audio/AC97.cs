using Cosmos.HAL;
using System;
using Cosmos.Core;
using Cosmos.HAL.Audio;

namespace Cosmos.HAL.Drivers.PCI.Audio
{
    /// <summary>
    /// Handles AC97-compatible sound cards at a low-level.
    /// </summary>
    public unsafe sealed class AC97 : AudioDriver
    {
        public override IAudioBufferProvider BufferProvider { get; set; }

        /// <summary>
        /// Describes a single entry in the Buffer Descriptor List of a
        /// AC97-compatible sound card.
        /// </summary>
        private unsafe struct BufferDescriptorListEntry
        {
            /// <summary>
            /// The pointer to the buffer's sample data in memory.
            /// </summary>
            public byte* pointer;

            /// <summary>
            /// The number of samples in this buffer.
            /// </summary>
            public ushort bufferSize;

            /// <summary>
            /// Describes the configuration of this buffer.
            /// <list type="bullet">
            ///     <item><b>Bits 0 - 13:</b> Reserved</item>
            ///     <item><b>Bit 14:</b> Last entry of the BDL; stop playing</item>
            ///     <item><b>Bit 15:</b> Fire an IRQ when data from this entry is transferred</item>
            /// </list>
            /// </summary>
            public ushort configuration;
        }

        /// <summary>
        /// PCI information for the sound card handled by this driver.
        /// </summary>
        public PCIDevice PCI { get; set; }

        AudioBuffer transferBuffer; // Acts as an intermediary buffer to convert between sample formats.
        byte[][] buffers;           // The buffers in memory. These buffers are described in the bufferDescriptorList.
        BufferDescriptorListEntry[] bufferDescriptorList; // Buffer information for the AC97.

        byte lastValidIdx;
        int bufferSizeSamples, bufferSizeBytes;

        // Private variables prefixed with a "p" indicate an I/O port.
        readonly ushort pTransferControl;    // Allows us to start/stop transfers and control what IRQs are fired
        readonly ushort pMasterVolume;       // Mixer volume setting
        readonly ushort pPCMOutVolume;       // Speaker output volume setting
        readonly ushort pBufferDescriptors;  // Buffer Descriptor List base address
        readonly ushort pLastValidEntry;     // Contains the number of the last Buffer Entry that will be processed
        readonly ushort pTransferStatus;     // Used to query the DMA transfer status and for IRQ acknowledgment
        readonly ushort pGlobalControl;      // Controls basic AC97 functions
        readonly ushort pResetRegister;      // Writing any value to port will cause a register reset

        const uint RESET_POLL_LIMIT = 500; // The maximum amount of polls for a reset the driver can perform.

        const int TC_RUN_OR_PAUSE = 1 << 0;
        const int TC_TRANSFER_RESET = 1 << 1;
        const int TC_ENABLE_LAST_VALID_BUF_INTERRUPT = 1 << 2;
        const int TC_ENABLE_FIFO_ERROR_INTERRUPT = 1 << 3;
        const int TC_ENABLE_COMPLETION_INTERRUPT = 1 << 4;

        const ushort IRQ_LVBCI = 1 << 2;
        const ushort IRQ_BCIS = 1 << 3;
        const ushort IRQ_FIFO_ERROR = 1 << 4;

        const ushort BD_FIRE_INTERRUPT_ON_CLEAR = 1 << 15;

        const int GC_GLOBAL_INTERRUPT_ENABLE = 0x1;

        const byte AC97_VOLUME_MAX = 0;
        const byte AC97_VOLUME_MIN = 63;

        /// <summary>
        /// Creates a mixer volume value to provide to an I/O port to set
        /// the output volume of the AC97.
        /// </summary>
        /// <param name="right">The right channel volume. Must be between 0 and 63.</param>
        /// <param name="left">The left chanel volume. Must be between 0 and 63.</param>
        /// <param name="mute">Whether to mute all output of the channel.</param>
        private static ushort CreateMixerVolumeValue(byte right, byte left, bool mute)
            => (ushort)((right & 0x3f) | ((left & 0x3f) << 8) | ((mute ? 1 : 0) & 1 << 15));

        /// <summary>
        /// Creates a new instance of the <see cref="AC97"/> class, with the
        /// given buffer size.
        /// </summary>
        /// <param name="bufferSize">The buffer size in samples to use. This value cannot be an odd number, as per the AC97 specification.</param>
        /// <exception cref="ArgumentException">Thrown when the given buffer size is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no AC97-compatible sound card is present.</exception>
        private AC97(ushort bufferSize)
        {
            if (bufferSize % 2 != 0)
                // As per the AC97 specification, the buffer size cannot be odd.
                // (1.2.4.2 PCM Buffer Restrictions, Intel document 302349-003)
                throw new ArgumentException("The buffer size must be an even number.", nameof(bufferSize));

            PCIDevice pci = Cosmos.HAL.PCI.GetDeviceClass(
                ClassID.MultimediaDevice, // 0x04
                (SubclassID)0x01          // 0x01
            );

            if (pci == null || !pci.DeviceExists || pci.InterruptLine > 0xF)
                throw new InvalidOperationException("No AC97-compatible device could be found.");

            PCI = pci; // Expose PCI device to the public API

            pci.EnableBusMaster(true);
            pci.EnableMemory(true);
            pci.EnableDevice(); // enable I/O space

            INTs.SetIrqHandler(pci.InterruptLine, HandleInterrupt);

            ushort NAMbar = (ushort)pci.BaseAddressBar[0].BaseAddress;  // Native Audio Mixer
            ushort NABMbar = (ushort)pci.BaseAddressBar[1].BaseAddress; // Native Audio Bus Master

            pTransferControl = (ushort)(NABMbar + 0x1B);
            pMasterVolume = (ushort)(NAMbar + 0x02);
            pPCMOutVolume = (ushort)(NAMbar + 0x18);
            pBufferDescriptors = (ushort)(NABMbar + 0x10);
            pTransferStatus = (ushort)(NABMbar + 0x16);
            pLastValidEntry = (ushort)(NABMbar + 0x15);
            pGlobalControl = (ushort)(NABMbar + 0x2C);
            pResetRegister = (ushort)(NAMbar + 0x00);

            // Reset device
            IOPort.Write8(pGlobalControl, 0x2);
            IOPort.Write32(pResetRegister, 0xDEADBEEF); // any value will do here

            // Reset PCM out
            uint polls = 0; // The amount we polled the device for a reset

            IOPort.Write8(pTransferControl, (byte)(IOPort.Read8(pTransferControl) | TC_TRANSFER_RESET));
            while ((IOPort.Read8(pTransferControl) & TC_TRANSFER_RESET) != 0 && polls < RESET_POLL_LIMIT)
            {
                // Wait until the byte is cleared
                polls++;
            }

            // The device hasn't responded to our reset request. Probably not a fully-compatible AC97 card.
            if (polls >= RESET_POLL_LIMIT)
                throw new InvalidOperationException("No AC97-compatible device could be found - the reset timeout has expired.");

            // Volume
            IOPort.Write16(pMasterVolume, CreateMixerVolumeValue(AC97_VOLUME_MAX, AC97_VOLUME_MAX, false));
            IOPort.Write16(pPCMOutVolume, CreateMixerVolumeValue(AC97_VOLUME_MAX, AC97_VOLUME_MAX, false));

            // Create all needed buffers
            CreateBuffers(bufferSize);

            // Initialization done - driver can now be activated by using Enable()
        }

        /// <summary>
        /// The global instance of the AC97. This property will return
        /// <see langword="null"/> if the driver has not been initialized.
        /// </summary>
        public static AC97 Instance { get; private set; } = null;

        /// <summary>
        /// Initializes the AC97 driver. This method will return
        /// an existing instance if the driver is already initialized
        /// and has a running instance.
        /// </summary>
        /// <param name="bufferSize">The buffer size in samples to use. This value cannot be an odd number, as per the AC97 specification.</param>
        /// <exception cref="ArgumentException">Thrown when the given buffer size is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no AC97-compatible sound card is present.</exception>
        public static AC97 Initialize(ushort bufferSize)
        {
            if (Instance != null)
            {
                if (Instance.bufferSizeSamples != bufferSize)
                    Instance.ChangeBufferSize(bufferSize);

                return Instance;
            }

            Instance = new AC97(bufferSize);
            return Instance;
        }

        const int BUFFER_COUNT = 32;
        private unsafe void CreateBuffers(ushort bufferSize)
        {
            transferBuffer = new AudioBuffer(bufferSize, new SampleFormat(AudioBitDepth.Bits16, 2, true));
            bufferSizeSamples = bufferSize;
            bufferSizeBytes = bufferSize * transferBuffer.Format.Size;

            bufferDescriptorList = new BufferDescriptorListEntry[BUFFER_COUNT];
            buffers = new byte[BUFFER_COUNT][];

            for (int i = 0; i < BUFFER_COUNT; i++)
            {
                buffers[i] = new byte[bufferSizeBytes];
                fixed (byte* ptr = buffers[i])
                {
                    bufferDescriptorList[i].pointer = ptr;
                }

                bufferDescriptorList[i].bufferSize = bufferSize;
                bufferDescriptorList[i].configuration |= BD_FIRE_INTERRUPT_ON_CLEAR;
            }
        }

        /// <summary>
        /// Changes the size of the internal buffers. This will result
        /// in a slight interruption in audio.
        /// </summary>
        /// <param name="newSize">The new buffer size, in samples. This value cannot be an odd number, as per the AC97 specification.</param>
        /// <exception cref="ArgumentException">Thrown when the given buffer size is invalid.</exception>
        public void ChangeBufferSize(ushort newSize)
        {
            if (newSize % 2 != 0)
                throw new ArgumentException("The new buffer size must be an even number.", nameof(newSize));

            if (newSize == bufferSizeSamples)
                return; // No action needed

            CreateBuffers(newSize);
            ProvideBuffers();
        }

        /// <summary>
        /// Provides the buffers to the sound card.
        /// </summary>
        private void ProvideBuffers()
        {
            // Tell BDL location
            fixed (void* ptr = bufferDescriptorList)
            {
                IOPort.Write32(pBufferDescriptors, (uint)ptr);
            }

            // Set last valid index
            lastValidIdx = 2; // Start at the 3rd buffer. This will give us some headroom and will decrease clicks.
            IOPort.Write8(pLastValidEntry, lastValidIdx);
        }

        private void HandleInterrupt(ref INTs.IRQContext aContext)
        {
            ushort sr = IOPort.Read16(pTransferStatus);

            if ((sr & IRQ_LVBCI) > 0)
            {
                // Last Valid Buffer interrupt
                IOPort.Write16(pTransferStatus, IRQ_LVBCI);
            }
            else if ((sr & IRQ_BCIS) > 0)
            {
                // Load a buffer ahead
                int next = lastValidIdx + 1;
                if (next >= BUFFER_COUNT)
                    next -= BUFFER_COUNT;

                BufferProvider.RequestBuffer(transferBuffer);

                fixed (byte* mainBufPtr = transferBuffer.RawData)
                {
                    MemoryOperations.Copy(
                        dest: bufferDescriptorList[next].pointer,
                        src: mainBufPtr,
                        size: bufferSizeBytes
                    );
                }

                // Set the index to the current one
                lastValidIdx++;
                if (lastValidIdx == BUFFER_COUNT)
                    lastValidIdx = 0;

                IOPort.Write8(pLastValidEntry, lastValidIdx);
                IOPort.Write16(pTransferStatus, IRQ_BCIS);
            }
            else if ((sr & IRQ_FIFO_ERROR) > 0)
            {
                IOPort.Write16(pTransferStatus, IRQ_FIFO_ERROR);
            }
        }

        public override SampleFormat[] GetSupportedSampleFormats()
            => new SampleFormat[]
            {
                new SampleFormat(AudioBitDepth.Bits16, 2, true)
                // TODO: Implement more channels, as the AC97 spec defines 2/4/6 channel audio
            };

        public override void SetSampleFormat(SampleFormat sampleFormat)
        {
            if (sampleFormat.BitDepth != AudioBitDepth.Bits16)
                throw new NotSupportedException("The AC97 driver only supports 16-bit audio.");

            if (sampleFormat.Channels != 2)
                throw new NotSupportedException("The AC97 driver only supports stereo audio.");

            if (!sampleFormat.Signed)
                throw new NotSupportedException("The AC97 driver does not support unsigned audio.");

            // TODO: The AC97 specification defines support 2/4/6 channel audio. Currently, only stereo audio output is supported.
        }

        public override bool Enabled =>
            (IOPort.Read8(pTransferControl) & TC_RUN_OR_PAUSE) != 0;

        public override void Enable()
        {
            if (Enabled)
                return; // Ignore calls to Enable() if the driver is already enabled

            ProvideBuffers();

            uint globalControl = IOPort.Read32(pGlobalControl);
            globalControl &= ~(0x3U << 22); // 16-bit output
            globalControl &= ~(0x3U << 20); // 2 channels
            globalControl |= GC_GLOBAL_INTERRUPT_ENABLE;
            IOPort.Write32(pGlobalControl, globalControl);

            IOPort.Write8(pTransferControl, TC_ENABLE_COMPLETION_INTERRUPT |
                                            TC_ENABLE_FIFO_ERROR_INTERRUPT |
                                            TC_ENABLE_LAST_VALID_BUF_INTERRUPT);

            IOPort.Write8(pTransferControl, (byte)(IOPort.Read8(pTransferControl) | TC_RUN_OR_PAUSE));
        }

        public override void Disable()
        {
            // Set audio to paused
            IOPort.Write8(pTransferControl, (byte)(IOPort.Read8(pTransferControl) | 0b1111_1110));
        }
    }
}
