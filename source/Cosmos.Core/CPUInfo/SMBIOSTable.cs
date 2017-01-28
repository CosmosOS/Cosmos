using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Core.CPUInfo
{
    public unsafe abstract class SMBIOSTable
    {
        public byte* BeginningAddress { get; set; }
        public byte Type { get; set; }
        public byte Length { get; set; }
        public ushort Handle { get; set; }

        protected SMBIOSTable(byte* BeginningAddress)
        {
            this.BeginningAddress = BeginningAddress;
        }

        public abstract byte* Parse();
    }
}
