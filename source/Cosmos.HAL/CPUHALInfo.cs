using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core;
using Cosmos.Core.CPUInfo;

namespace Cosmos.HAL
{
    public class CPUHALInfo
    {
        public static string ParseSMBIOS()
        {
            EntryPointTable entry = ProcessorInformation.BeginParseSMBIOS();
            return "shit";
        }
    }
}
