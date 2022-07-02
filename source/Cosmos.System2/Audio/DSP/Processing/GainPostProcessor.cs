using System;
using Cosmos.HAL.Audio;

namespace Cosmos.System.Audio.DSP.Processing
{
    /// <summary>
    /// Changes the gain (volume) of audio samples.
    /// </summary>
    public class GainPostProcessor : AudioPostProcessor
    {
        float gain;

        /// <summary>
        /// Initializes a new instance of the <see cref="GainPostProcessor"/>,
        /// with its gain set to full volume.
        /// </summary>
        public GainPostProcessor()
        {
            gain = 1.0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GainPostProcessor"/>
        /// with the given gain value.
        /// </summary>
        /// <param name="gain">The gain value. This value must be in the range of [0; 1].</param>
        /// <exception cref="ArgumentOutOfRangeException">The provided gain value was outside the range of [0; 1].</exception>
        public GainPostProcessor(float gain)
        {
            Gain = gain;
        }

        /// <summary>
        /// The amount of the original signal to pass through. This value must
        /// be in the range of [0; 1].
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The provided value was outside the range of [0; 1].</exception>
        public float Gain
        {
            get => gain;
            set
            {
                if (gain < 0f || gain > 1f)
                    throw new ArgumentOutOfRangeException(nameof(value), "The gain property of the GainPostProcessor must be in the range of 0.0 to 1.0.");

                gain = value;
            }
        }

        // Note - there is a way to do this using fixed-point arithmetic in
        // order to avoid floating-point casting. This may prove beneficial
        // if IL2CPU won't optimize floating-point operations (i.e. won't generate
        // SIMD instructions), but in its current state, this method is fine.

        public unsafe override void Process(AudioBuffer buffer)
        {
            if(gain == 0f)
            {
                // If the gain is equal to 0, simply fill the array with 0s.
                // This reduces unnecessary reads and calculations.
                Array.Clear(buffer.RawData);
            } else
            {
                fixed (byte* ptr = buffer.RawData)
                {
                    switch (buffer.format.bitDepth)
                    {
                        case AudioBitDepth.Bits8:
                            if (buffer.format.signed)
                            {
                                sbyte* i8ptr = (sbyte*)ptr;
                                for (int i = 0; i < buffer.SampleAmount; ++i)
                                    i8ptr[i] = (sbyte)(i8ptr[i] * gain);
                            }
                            else
                            {
                                for (int i = 0; i < buffer.SampleAmount; ++i)
                                    ptr[i] = (byte)(ptr[i] * gain);
                            }
                            break;
                        case AudioBitDepth.Bits16:
                            if (buffer.format.signed)
                            {
                                short* i16ptr = (short*)ptr;
                                for (int i = 0; i < buffer.SampleAmount; ++i)
                                    i16ptr[i] = (short)(i16ptr[i] * gain);
                            }
                            else
                            {
                                ushort* u16ptr = (ushort*)ptr;
                                for (int i = 0; i < buffer.SampleAmount; ++i)
                                    u16ptr[i] = (ushort)(u16ptr[i] * gain);
                            }
                            break;
                        case AudioBitDepth.Bits24:
                            // TODO: 24-bit format support
                            break;
                        case AudioBitDepth.Bits32:
                            if (buffer.format.signed)
                            {
                                int* i32ptr = (int*)ptr;
                                for (int i = 0; i < buffer.SampleAmount; ++i)
                                    i32ptr[i] = (int)(i32ptr[i] * gain);
                            }
                            else
                            {
                                uint* u32ptr = (uint*)ptr;
                                for (int i = 0; i < buffer.SampleAmount; ++i)
                                    u32ptr[i] = (uint)(u32ptr[i] * gain);
                            }
                            break;
                    }
                }
            }
        }

        private unsafe void AdjustVolume(short* samples, int numSamples)
        {
            for (int i = 0; i < numSamples; ++i)
                samples[i] = (short)(samples[i] * gain);
        }
    }
}
