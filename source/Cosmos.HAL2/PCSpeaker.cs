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
        protected static Core.IOGroup.PCSpeaker IO = BaseIOGroups.PCSpeaker;
        private static PIT SpeakerPIT = new PIT();

        private static void EnableSound()
        {
            IO.Gate.Byte = (byte)(IO.Gate.Byte | 0x03);
        }
        private static void DisableSound()
        {
            IO.Gate.Byte = (byte)(IO.Gate.Byte & ~3);
            //IO.Port61.Byte = (byte)(IO.Port61.Byte | 0xFC);
        }

        public static void Beep(uint frequency)
        {
            if (frequency < 37 || frequency > 32767)
            {
                throw new ArgumentOutOfRangeException("Frequency must be between 37 and 32767Hz");
            }

            uint divisor = 1193180 / frequency;
            byte temp;
            IO.CommandRegister.Byte = 0xB6;
            IO.Channel2Data.Byte = (byte)(divisor & 0xFF);
            IO.Channel2Data.Byte = (byte)((divisor >> 8) & 0xFF);
            temp = IO.Gate.Byte;
            if (temp != (temp | 3))
            {
                IO.Gate.Byte = (byte)(temp | 3);
            }
            EnableSound();
        }
        public static void Beep(uint frequency, uint duration)
        {
            if (duration <= 0)
            {
                throw new ArgumentOutOfRangeException("Duration must be more than 0");
            }

            Beep(frequency);
            SpeakerPIT.Wait(duration);
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
