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
                    mGlobalInformationTable = (GlobalInformationTable*)xEndOfKernel;
                    mGlobalInformationTable->FirstDataLookupTable = (DataLookupTable*)(xEndOfKernel + sizeof(GlobalInformationTable));
                }
                return mGlobalInformationTable;
            }
        }
    }
}
