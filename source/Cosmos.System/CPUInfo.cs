using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.HAL;

namespace Cosmos.System
{
    public class CPUInfo
    {
        public static string GetVendorName()
        {
            CPUHALInfo.ParseSMBIOS();
            return "shit";
        }
    }
}
