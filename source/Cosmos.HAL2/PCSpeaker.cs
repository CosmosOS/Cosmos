using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL
{
    public static class SpeakerExtensions
    {
        public static uint MsToHz(this int ms)
        {
            return (uint)(1000 / ms);
        }
        public static uint MsToHz(this uint ms)
        {
            return (uint)(1000 / ms);
        }
    }
    public class PCSpeaker
    {
        // IO Port 61, channel 2 gate
        /// <summary>
        /// Gate IO port.
        /// </summary>
        public const int Gate = 0x61;
        // These two ports are shared with the PIT, so names are the same
        // IO Port 43
        /// <summary>
        /// Command register IO port.
        /// </summary>
        public const int CommandRegister = 0x43;
        // IO Port 42
        /// <summary>
        /// Channel to data IO port.
        /// </summary>
        public const int Channel2Data = 0x42;

        /// <summary>
        /// Enable sound.
        /// </summary>
        private static void EnableSound()
        {
            IOPort.Write8(Gate, (byte)(IOPort.Read8(Gate) | 0x03));
        }
        /// <summary>
        /// Disable sound.
        /// </summary>
        private static void DisableSound()
        {
            IOPort.Write8(Gate, (byte)(IOPort.Read8(Gate) & ~3));
            //IO.Port61.Byte = (byte)(IO.Port61.Byte | 0xFC);
        }

        /// <summary>
        /// Play beep sound, at a specified frequency for a specified duration.
        /// </summary>
        /// <param name="frequency">Audio frequency in Hz, must be between 37 and 32767Hz.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if frequency is invalid.</exception>
        public static void Beep(uint frequency)
        {
            if (frequency < 37 || frequency > 32767)
            {
                throw new ArgumentOutOfRangeException("Frequency must be between 37 and 32767Hz");
            }

            uint divisor = 1193180 / frequency;
            IOPort.Write8(CommandRegister, 0xB6);
            IOPort.Write8(Channel2Data, (byte)(divisor & 0xFF));
            IOPort.Write8(Channel2Data, (byte)((divisor >> 8) & 0xFF));
            var temp = IOPort.Read8(Gate);
            if (temp != (temp | 3))
            {
                IOPort.Write8(Gate, (byte)(temp | 3));
            }
            EnableSound();
        }

        // TODO: continue exception list, once HAL is documented.
        /// <summary>
        /// Play beep sound, at a specified frequency for a specified duration.
        /// </summary>
        /// <param name="frequency">Audio frequency in Hz, must be between 37 and 32767Hz.</param>
        /// <param name="duration">Beep duration, must be > 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if duration or frequency invalid.</exception>
        public static void Beep(uint frequency, uint duration)
        {
            if (duration <= 0)
            {
                throw new ArgumentOutOfRangeException("Duration must be more than 0");
            }

            Beep(frequency);
            Global.PIT.Wait(duration);
            DisableSound();
        }
    }

    /*
    public class PCSpeaker
    {
        public void playSound(UInt32 nFrequence)
        {
            UInt32 Div;
            UInt16 tmp;

            //Set the PIT to the desired frequency
            Div = 1193180 / nFrequence;
            BaseIOGroups.PCSpeaker.Port43.Byte = (byte)0xB6;
            BaseIOGroups.PCSpeaker.Port42.Byte = (byte)Div;
            BaseIOGroups.PCSpeaker.Port42.Byte = (byte)(Div >> 8);

            tmp = BaseIOGroups.PCSpeaker.Port61.Byte;
            if (tmp != (tmp | 3))
            {
                BaseIOGroups.PCSpeaker.Port61.Byte = (byte)(tmp | 3);
            }
        }

        public void nosound()
        {
            byte tmp = (byte)(BaseIOGroups.PCSpeaker.Port61.Byte & 0xFC);

            BaseIOGroups.PCSpeaker.Port61.Byte = tmp;
        }

        public void beep()
        {
            playSound(1000);
            Int32 div = 1193180;
            BaseIOGroups.PCSpeaker.Port42.Byte = (byte)div;
            //PIT.PITFrequency = 1193180;
        }
    }*/
}
