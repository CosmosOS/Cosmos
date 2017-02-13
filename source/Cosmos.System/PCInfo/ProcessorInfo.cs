using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Xml.Schema;
using Cosmos.HAL.PCInfo;

namespace Cosmos.System.PCInfo
{
    public class ProcessorInfo
    {
        private static List<Processor> _listProcessors;
        public static List<Processor> ListProcessors
        {
            get
            {
                if (_listProcessors == null)
                {
                    _listProcessors = new List<Processor>();
                    foreach (var x in Cosmos.HAL.PCInfo.ProcessorHALInfo.listProcessor)
                    {
                        _listProcessors.Add(new Processor(x)); 
                    }
                }
                return _listProcessors;
            }
        }

        public static string ProcCpuinfo()
        {
            string returnProc = "";
            foreach (var x in ListProcessors)
            {
                returnProc += "processor: " + x.SocketDesignation + "\n";
                returnProc += "vendor_id: " + x.Manufacturer + "\n";
                returnProc += "model name: " + x.ProcessorVersion + "\n";
                //returnProc += "model family: " + x.ProcessorFamily + "\n";
                //in proc cpu info there is the raw type
                returnProc += "cpu family: " + x.ProcessorType + "\n";
                returnProc += "cpu MHz: " + x.Speed + "\n";
                returnProc += "flags: ";
                for(int i = 0; i < x.Flags.Count; i++)
                {
                    //VERY INEFFICIENT
                    returnProc += Cosmos.HAL.PCInfo.ProcessorFlagsExtensions.ConvertEnumToString(
                        Cosmos.HAL.PCInfo.ProcessorFlagsExtensions.ConvertIntToEnum(x.Flags[i])
                    ) + " ";
                }
                returnProc += "\n";
                returnProc += "\n";
            }
            return returnProc;
        }


    }
}