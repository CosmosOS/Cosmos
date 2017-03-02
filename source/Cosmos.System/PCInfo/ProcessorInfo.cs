using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Xml.Schema;

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
                    _listProcessors.Add(new Processor());
                }
                return _listProcessors;
            }
        }

        public static string ProcCpuinfo()
        {
            string returnProc = "";
            foreach (var x in ListProcessors)
            {
                returnProc += "vendor_id: " + x.Manufacturer + "\n";
                returnProc += "cpu family: " + x.Family + "\n";
                returnProc += "model: " + x.ModelNumber + "\n";
                returnProc += "stepping: " + x.Stepping + "\n";
                /*
                //returnProc += "model family: " + x.ProcessorFamily + "\n";
                //in proc cpu info there is the raw type
                returnProc += "cpu family: " + x.ProcessorType + "\n";
                returnProc += "cpu MHz: " + x.Speed + "\n";
                */
                returnProc += "flags count: " + x.Flags.Count + "\n";
                returnProc += "flags: ";
                for(int i = 0; i < x.Flags.Count; i++)
                {
                    //Convert Here to string
                    returnProc += Cosmos.HAL.PCInformation.ProcessorFlagsExtensions.ConvertEnumToString(
                        Cosmos.HAL.PCInformation.ProcessorFlagsExtensions.ConvertIntToEnum(x.Flags[i])
                    ) + " ";
                }
                returnProc += "\n";
                returnProc += "\n";
            }
            return returnProc;
        }


    }
}