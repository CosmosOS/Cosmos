using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL
{
    public class PCSpeaker
    {
        public void playSound(UInt32 nFrequence)
        {
            UInt32 Div;
            UInt16 tmp;

            //Set the PIT to the desired frequency
            Div = 1193180 / nFrequence;
            BaseIOGroups.PCSpeaker.p43.Byte = (byte)0xB6;
            BaseIOGroups.PCSpeaker.p42.Byte = (byte)Div;
            BaseIOGroups.PCSpeaker.p42.Byte = (byte)(Div >> 8);

            tmp = BaseIOGroups.PCSpeaker.p61.Byte;
            if (tmp != (tmp | 3))
            {
                BaseIOGroups.PCSpeaker.p61.Byte = (byte)(tmp | 3);
            }
        }

        public void nosound()
        {
            byte tmp = (byte)(BaseIOGroups.PCSpeaker.p61.Byte & 0xFC);

            BaseIOGroups.PCSpeaker.p61.Byte = tmp;
        }

        public void beep()
        {
            playSound(1000);
            UInt32 div = 1193180;
            BaseIOGroups.PCSpeaker.p42.Byte = (byte)div;
            //PIT.PITFrequency = 1193180;
        }
    }
}
