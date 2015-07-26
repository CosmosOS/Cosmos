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
                    CPU.ZeroFill(xEndOfKernel, (uint)(sizeof(GlobalInformationTable) + GetTotalDataLookupSize));
                    mGlobalInformationTable = (GlobalInformationTable*)xEndOfKernel;
                    mGlobalInformationTable->FirstDataLookupTable = (DataLookupTable*)(xEndOfKernel + sizeof(GlobalInformationTable));
                }
                return mGlobalInformationTable;
            }
        }

        public static uint GetTotalDataLookupSize
        {
            get
            {
                return (uint)(sizeof(DataLookupTable) + (DataLookupTable.EntriesPerTable * sizeof(DataLookupEntry)));
            }
        }
    }
}
