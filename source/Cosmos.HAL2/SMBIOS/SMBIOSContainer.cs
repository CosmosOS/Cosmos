using Cosmos.Core.SMBIOS;

namespace Cosmos.HAL.SMBIOS
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
                    _smbiosStructure = Core.SMBIOS.SMBIOS.BeginParseSMBIOS();
                }
                return _smbiosStructure;
            }
        }
    }
}
