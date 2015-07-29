using Cosmos.Debug.Kernel;

namespace Cosmos.Core
{
    internal static unsafe class GlobalSystemInfo
    {
        private static GlobalInformationTable* mGlobalInformationTable;
        public static GlobalInformationTable* GlobalInformationTable
        {
            get
            {
                if (mGlobalInformationTable == null)
                {
                    // todo: should we align this structure somehow?

                    var xEndOfKernel = CPU.GetEndOfKernel();
                    CPU.ZeroFill(xEndOfKernel, (uint)(sizeof(GlobalInformationTable) + TotalDataLookupTableSize) * 4);
                    mGlobalInformationTable = (GlobalInformationTable*)xEndOfKernel;
                    uint xFirstDataLookupLocation = (uint)(xEndOfKernel + sizeof(GlobalInformationTable));
                    Debugger.DoSend("Setting FirstDataLookupTable to ");
                    Debugger.DoSendNumber(xFirstDataLookupLocation);
                    mGlobalInformationTable->FirstDataLookupTable = (DataLookupTable*)xFirstDataLookupLocation;
                    Debugger.DoSend("FirstDataLookupTable was set to ");
                    Debugger.DoSendNumber((uint)mGlobalInformationTable->FirstDataLookupTable);
                }
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
    }
}
