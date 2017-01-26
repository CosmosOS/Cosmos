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
        public static string ParseSMBIOS()
        {
            CPUHALInfo.ParseSMBIOS();
            return "shit";
        }

        public static string GetVendorName()
        {
            CPUHALInfo.ParseSMBIOS(); //FIXME: For debugging purposes
            return CPUHALInfo.GetVendorName();
        }
    }
}
