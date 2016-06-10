using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.Memory
{
    internal static unsafe class GlobalSystemInfo
    {
        private static volatile GlobalInformationTable* mGlobalInformationTable;
        public static GlobalInformationTable* GlobalInformationTable
        {
            get
            {
                EnsureInitialized();
                return mGlobalInformationTable;
            }
        }

        public static void EnsureInitialized()
        {
            if (mGlobalInformationTable == null)
            {
                // todo: should we align this structure somehow?

                var xEndOfKernel = CPU.GetEndOfKernel();
                xEndOfKernel = xEndOfKernel + (1024 * 1024); // for now, skip 1 MB
                CPU.ZeroFill(xEndOfKernel, CPU.GetAmountOfRAM() - xEndOfKernel);
                mGlobalInformationTable = (GlobalInformationTable*)xEndOfKernel;
                uint xFirstDataLookupLocation = (uint)(xEndOfKernel + sizeof(GlobalInformationTable));
                Debugger.DoSend("Setting FirstDataLookupTable to ");
                Debugger.DoSendNumber(xFirstDataLookupLocation);
                mGlobalInformationTable->FirstDataLookupTable = (DataLookupTable*)xFirstDataLookupLocation;
                Debugger.DoSend("FirstDataLookupTable was set to ");
                Debugger.DoSendNumber((uint)mGlobalInformationTable->FirstDataLookupTable);
                Heap.DoInitialize(CPU.GetAmountOfRAM() - (1024 * 1024));
            }
        }

        public static uint TotalDataLookupTableSize
        {
            get
            {
                return (uint)(sizeof(DataLookupTable) + (DataLookupTable.EntriesPerTable * sizeof(DataLookupEntry)));
            }
        }
    }
}
