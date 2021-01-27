using System;
using Cosmos.Core;
using Cosmos.HAL.Drivers.PCI.Audio.Generic;

namespace Cosmos.HAL
{
    public class SoundBlaster16 : SoundCard
    {
        private bool sound_blaster = false;
        private byte sb16_version_major;
        private byte sb16_version_minor;
        /// <summary>
        /// Gets weather or not a soundblaster 16 is present on the system
        /// </summary>
        public bool IsPresent { get { return sound_blaster; } }
        /// <summary>
        /// Gets the DSP Version
        /// </summary>
        public Version DSPVersion { get { return new Version(sb16_version_major, sb16_version_minor); } }
        /// <summary>
        /// Gets weather or not audio is playing.
        /// </summary>
        public bool IsAudioPlaying { get { return PlayingSound; } }
        private byte vol = 0xff;
        public override byte Volume { get { return vol; } set { vol = value; SetVolume(value); } }

        #region DSP Ports
        const ushort DSP_RESET = 0x226;
        const ushort DSP_READ = 0x22A;
        const ushort DSP_WRITE = 0x22C;
        const ushort DSP_READSTATUS = 0x22E;
        #endregion

        #region Mixer Commands
        const byte MIXER_SETIRQ = 0x80;
        const byte MIXER_SETVOL = 0x22;
        #endregion

        #region DSP Commands
        const byte DSP_EnableSpeaker = 0xD1;
        const byte DSP_DisableSpeaker = 0xD3;
        const byte DSP_GETVERSION = 0xE1;
        const byte DSP_SETTIME = 0x40;
        const byte DSP_SETPLAYBACKRATE = 0x41;
        #endregion

        #region Mixer Ports
        const ushort MixerPort = 0x224;
        const ushort MixerDataPort = 0x225;
        #endregion
        private byte[] Buffer = new byte[64000]; //64K Buffer
        private MemoryBlock bufferinmem;
        private bool PlayingSound = false;
        private bool IsAudioBiggerThenBuffer = false;
        private int AudioStop = 0;
        private byte[] audiodata;
        public override void Disable()
        {
            // Clean up
            StopSound();
            INTs.SetIrqHandler(5, null); //Remove handler

            //clean up buffer
            Buffer = null;
            bufferinmem = null;
        }
        public override void Enable()
        {
            //Reset DSP
            ResetDSP();

            if (sound_blaster == false)
            {
                //No sound blaster installed
                return;
            }

            //Get DSP version
            WriteDSP(DSP_GETVERSION);
            sb16_version_major = ReadDSP();
            sb16_version_minor = ReadDSP();

           

            Program_MixerPort();

            //hard coded buffer location
            bufferinmem = new MemoryBlock(0x4d7bbb, (uint)Buffer.Length);

            //Clear buffer to remove garbage
            for (uint i = 0; i < Buffer.Length; i++)
            {
                bufferinmem[i] = 0;
            }
        }
        /// <summary>
        /// Plays an 8 Bit Unsigned PCM audio (16KHz) on the sound blaster 16
        /// </summary>
        /// <param name="Sound"></param>
        public override void PlaySound(byte[] audio)
        {
            audiodata = audio;
            //Copy the sound into the buffer
            for (int i = 0; i < audio.Length; i++)
            {
                if (i >= Buffer.Length)
                {
                    IsAudioBiggerThenBuffer = true;
                    AudioStop = i;
                    break;
                }
                else
                {
                    Buffer[i] = audio[i];
                }
            }
            PlayBuffer();
        }
        /// <summary>
        /// Stops playing the currently playing sound in the sound blaster 16.
        /// </summary>
        public override void StopSound()
        {
            WriteDSP(0xD0);
            WriteDSP(0xDa); //exit auto
            PlayingSound = false;
            IsAudioBiggerThenBuffer = false;
            AudioStop = 0;
            audiodata = null;
        }
        /// <summary>
        /// Plays everything inside the buffer.
        /// </summary>
        private void PlayBuffer()
        {
            //Copy the Buffer into memory
            for (uint i = 0; i < Buffer.Length; i++)
            {
                bufferinmem[i] = Buffer[i];
            }
            PlayingSound = true;
            Program_dsp((ushort)Buffer.Length, (ushort)bufferinmem.Base);
        }
        /// <summary>
        /// Programs mixer port.
        /// </summary>
        private void Program_MixerPort()
        {
            SetVolume(0xFF); //set to max volume
            MixerPortWrite(MIXER_SETIRQ, 0x02); //IRQ 5

            INTs.SetIrqHandler(0x05, new INTs.IRQDelegate(IrqHandler));

            //Turn on speaker
            EnableSpeaker();
        }
        /// <summary>
        /// Sets the Sound blasters card volume. 
        /// Format: 0xRL - R = Right, L = Left. 
        /// Example: 0xFF for max volume and 0x00 for mute.
        /// </summary>
        /// <param name="data">The volume.</param>
        public void SetVolume(byte data)
        {
            vol = data;
            MixerPortWrite(MIXER_SETVOL, data);
        }
        /// <summary>
        /// Turns on speaker
        /// </summary>
        public void EnableSpeaker()
        {
            WriteDSP(DSP_EnableSpeaker);
        }
        /// <summary>
        /// Turns off speaker
        /// </summary>
        public void DisableSpeaker()
        {
            WriteDSP(DSP_DisableSpeaker);
        }
        /// <summary>
        /// For debuging
        /// </summary>
        public byte SampleRate = 192;
        private void Program_dsp(ushort AudioLength, ushort AudioPosition)
        {
            if (!IsPresent)
            {
                return;
            }
            byte highAudioLength = Convert.ToByte(AudioLength >> 8);
            byte lowerAudioLength = 4;//(byte)(AudioLength & 0xff);


            //Hard coded memory location
            byte firstAudioPosition = 0x4d;
            byte upperAudioPosition = 0x7b;
            byte lowerAudioPosition = 0xbb;

            //Calculate Time constant

            //Time constant = 65536 - (256000000 / (channels * sampling rate))

            int constt = 65536 - (256000000 / (1 * 16000));
            //The high byte is only used.
            SampleRate = (byte)(constt >> 8);


            //Program 8-bit transfers
            Outb(0x0A, 0x05); //Disable channel 1 (Channel # + 0x04)
            Outb(0x0C, 0); //Flip flop flip port
            Outb(0x0B, 0x59); //Auto Init mode

            //Send sound data location
            Outb(0x83, firstAudioPosition); //Page # example: 0x[01]0F04
            Outb(0x02, lowerAudioPosition); //Lower audio position bits  0x01[0F]04
            Outb(0x02, upperAudioPosition); //high audio position bits 0x010F[04]

            //Send length of data
            Outb(0x03, lowerAudioLength); //Send low bits of audio length
            Outb(0x03, highAudioLength); //Send high bits of audio length
            Outb(0x0A, 1); //Enable channel 1

            //Set time constant (speed)
            WriteDSP(DSP_SETTIME);
            WriteDSP(SampleRate);

            //set block size for 8-bit auto-init mode
            WriteDSP(0x48);

            //Send data length to DSP
            WriteDSP((byte)(lowerAudioLength - 1));
            WriteDSP((byte)(highAudioLength - 1));

            //Start auto init 8-bit transfer
            WriteDSP(0x1c);
        }
        private void IrqHandler(ref INTs.IRQContext c)
        {
            Global.mDebugger.Send("Got Sound blaster 16 IRQ");
            PlayingSound = false;

            //Play the next sound in the buffer
            if (IsAudioBiggerThenBuffer)
            {
                IsAudioBiggerThenBuffer = false;

                //Clear buffer
                for (uint i = 0; i < Buffer.Length; i++)
                {
                    Buffer[i] = 0;
                    bufferinmem[i] = 0;
                }

                //Copy the next audio data to the buffer
                for (int i = AudioStop; i < audiodata.Length; i++)
                {
                    if (i >= Buffer.Length)
                    {
                        Global.mDebugger.Send("Audio Buffer " + i);
                        IsAudioBiggerThenBuffer = true;
                        AudioStop = i;
                        break;
                    }
                    else
                    {
                        Buffer[i] = audiodata[i];
                    }
                }

                //Play the buffer
                PlayBuffer();
            }
            else
            {
                //Clean up
                PlayingSound = false;
                IsAudioBiggerThenBuffer = false;
                AudioStop = 0;
                audiodata = null;
            }
        }
        #region I/O
        private void MixerPortWrite(byte port, byte data)
        {
            Outb(MixerPort, port);
            Outb(MixerDataPort, data);
        }
        private byte MixerPortRead()
        {
            return Inb(MixerDataPort);
        }
        private static byte Inb(ushort port)
        {
            var io = new IOPort(port);
            return io.Byte;
        }
        private static void Outb(ushort port, byte data)
        {
            var io = new IOPort(port);
            io.Byte = data;
        }
        private void ResetDSP()
        {
            Outb(DSP_RESET, 1);
            Cosmos.HAL.Global.PIT.Wait(3);
            Outb(DSP_RESET, 0);
            if (Inb(DSP_READ) == 0xAA)
            {
                sound_blaster = true;
            }
        }
        private void WriteDSP(byte value)
        {
            Outb(DSP_WRITE, value);
        }
        private byte ReadDSP()
        {
            return Inb(DSP_READ);
        }
        #endregion
    }
}
