using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.Audio.Devices.ES1370.Register
{
    class MainRegister
    {
        public enum Bit : byte
        {
            Control = 0x00,
            Status = 0x04,
            UartData=0x08,
            UartInfo=0x09,
            MemAddrPage=0x0c,
            ADCPage=0x0d,
            UartPage=0x0e,
            Uart1Page=0x0f,
            Codec=0x10,
            SerialIntContr=0x20,
            Dac1SampleCount=0x24,
            Dac2Samplecount=0x28,
            Dac1FrameAddr=0x30,
            Dac1FrameSize=0x34,
            Dac2FrameAddr = 0x38,
            Dac2FrameSize=0x3c
        }
    }
}
