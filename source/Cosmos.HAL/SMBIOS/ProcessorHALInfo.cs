using System.Collections.Generic;
using Cosmos.Core.PCInformation;
using Cosmos.HAL.PCInformation;

namespace Cosmos.HAL.SMBIOS
{
    /// <summary>
    /// This class converts the data from smbios cpu to a human readable form.
    /// </summary>
    public class ProcessorHALInfo
    {
        private static List<Processor> _listProcessor;
        /// <summary>
        /// List of processors avaliable on the machine (since there are more than one
        /// </summary>
        public static List<Processor> listProcessor
        {
            get
            {
                //We do the parsing once ONCE
                if (_listProcessor == null)
                {
                    _listProcessor = new List<Processor>();
                    ParseSMBIOS();
                }
                return _listProcessor;
            }
        }

        public ProcessorHALInfo()
        {
        }

        private static void ParseSMBIOS()
        {
            var structure = SMBIOSContainer.SmbiosStructure;
            //Parse every single processor
            foreach (var x in structure.CpuInfoList)
            {
                //var info = new Processor(x);
                //_listProcessor.Add(info); 
            }
        }

        public static string GetVendorName()
        {
            //return ProcessorInformation.GetVendorName();
            return null;
        }
    }
}
