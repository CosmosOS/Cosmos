using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core.SMBIOS;

namespace Cosmos.HAL.PCInfo
{
    /// <summary>
    /// This class contains the smbios structure parsed only once
    /// </summary>
    class SMBIOSContainer
    {
        private static SMBIOSStructure _smbiosStructure;
        public static SMBIOSStructure SmbiosStructure
        {
            get
            {
                //This way, we don't have to parse the structure every time.
                //Furthermore, the parsing remains transparent to the user.
                if (_smbiosStructure == null)
                {
                    _smbiosStructure = SMBIOS.BeginParseSMBIOS();
                }
                return _smbiosStructure;
            }
        }
    }
}
