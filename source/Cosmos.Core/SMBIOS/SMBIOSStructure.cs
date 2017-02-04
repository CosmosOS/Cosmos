using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Core.SMBIOS
{
    /// <summary>
    /// This class is a container for each table of the smbios structure
    /// This class should be forwarded to HAL for further parsing
    /// </summary>
    public class SMBIOSStructure
    {
        public EntryPointTable EntryPointTable { get; set; }
        public BIOSInfo BiosInfo { get; set; }
        public List<CPUInfo> CpuInfoList { get; set; }

        public SMBIOSStructure()
        {
            CpuInfoList = new List<CPUInfo>();
        }
    }
}
