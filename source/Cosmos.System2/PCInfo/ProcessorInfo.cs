using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Xml.Schema;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.PCInfo
{
    public class ProcessorInfo
    {
        private static List<Processor> _listProcessors;
        public static List<Processor> ListProcessors
        {
            get
            {
                //This is to allow multiprocessor on a future
                //TODO: search a list of processors based on the topology
                if (_listProcessors == null)
                {
                    _listProcessors = new List<Processor>();
                    _listProcessors.Add(new Processor());
                }
                return _listProcessors;
            }
        }

        public delegate void WriteLine(string str);

        /// <summary>
        /// Debugging method that tries to emulate /proc/cpuinfo of linux
        /// Prints on the delegate passed as a parameter
        /// </summary>
        /// <remark>Use getters of processor instead and don't try to parse this.</remark>
        public static void ProcCpuinfo(WriteLine writeLineFunc)
        {
            foreach (var x in ListProcessors)
            {
                writeLineFunc("vendor_id: " + x.Manufacturer + "\n");
                writeLineFunc("cpu family: " + x.Family + "\n");
                writeLineFunc("model: " + x.ModelNumber + "\n");
                writeLineFunc("stepping: " + x.Stepping + "\n");
                /*
                //returnProc += "model family: " + x.ProcessorFamily + "\n";
                //in proc cpu info there is the raw type
                returnProc += "cpu MHz: " + x.Speed + "\n";
                */
                writeLineFunc("flags count: " + x.Flags.Count + "\n");
                writeLineFunc("flags: ");
                for(int i = 0; i < x.Flags.Count; i++)
                {
                    //Convert Here to string
                    writeLineFunc(Cosmos.HAL.PCInformation.ProcessorFlagsExtensions.ConvertEnumToString(
                        Cosmos.HAL.PCInformation.ProcessorFlagsExtensions.ConvertIntToEnum(x.Flags[i])
                    ) + " ");
                }
                writeLineFunc("\n");
                //Appending brand crashes deleting the entire string
                //returnProc += "Brand: " + new String(x.Brand.ToCharArray());
                writeLineFunc("Brand: " + x.Brand + "\n");
                writeLineFunc("Frequency: " + x.Frequency + "\n");
                writeLineFunc("\n\n");
            }
        }


    }
}