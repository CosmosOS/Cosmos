using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.Audio.Devices.ES1370.Components
{
    class DACak4531 : DACEntity
    {
        public DACak4531(byte dacAddr,byte dacSizeAddr) : base(dacAddr, dacSizeAddr) { }

        public enum Bit : byte
        {
            LeftMasterVol = 0x00, //master volume left
            RightMasterVol = 0x01, //master volume right
            LeftVoiceVol= 0x02,  // channel volume left
            RightVoiceVol = 0x03, //channel volume right
            LeftFMVol = 0x04, //FM volume left
            RightFMVol = 0x05//FM volume right
        }
    }
}
