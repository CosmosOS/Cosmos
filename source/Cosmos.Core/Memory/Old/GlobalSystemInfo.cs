using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.Memory.Old
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

        public static uint TotalDataLookupTableSize
        {
            get
            {
                return (uint)(sizeof(DataLookupTable) + (DataLookupTable.EntriesPerTable * sizeof(DataLookupEntry)));
            }
        }

        internal static unsafe void EnsureInitialized()
        {
            if (mGlobalInformationTable == null)
            {
                // todo: should we align this structure somehow?

                var xEndOfKernel = CPU.GetEndOfKernel();
                xEndOfKernel = xEndOfKernel + (1024 * 1024); // for now, skip 1 MB
                CPU.ZeroFill(xEndOfKernel, (uint)(sizeof(GlobalInformationTable) + TotalDataLookupTableSize) * 4);
                mGlobalInformationTable = (GlobalInformationTable*)xEndOfKernel;
                uint xFirstDataLookupLocation = (uint)(xEndOfKernel + sizeof(GlobalInformationTable));
                Debugger.DoSend("Setting FirstDataLookupTable to ");
                Debugger.DoSendNumber(xFirstDataLookupLocation);
                mGlobalInformationTable->FirstDataLookupTable = (DataLookupTable*)xFirstDataLookupLocation;
                Debugger.DoSend("FirstDataLookupTable was set to ");
                Debugger.DoSendNumber((uint)mGlobalInformationTable->FirstDataLookupTable);
            }
        }
    }
}
