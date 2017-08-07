using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmos.CPU.x86 {
    internal static unsafe class GlobalSystemInfo {
        //private static volatile GlobalInformationTable* mGlobalInformationTable;
        //public static GlobalInformationTable* GlobalInformationTable {
        //    get {
        //        EnsureInitialized();
        //        return mGlobalInformationTable;
        //    }
        //}

        internal static unsafe void EnsureInitialized() {
            //if (mGlobalInformationTable == null) {
                //var xEndOfKernel = Processor.GetEndOfKernel();
                //xEndOfKernel = xEndOfKernel + (1024 * 1024); // for now, skip 1 MB
                //Processor.ZeroFill(xEndOfKernel, (uint)(sizeof(GlobalInformationTable) + TotalDataLookupTableSize) * 4);
                //mGlobalInformationTable = (GlobalInformationTable*)xEndOfKernel;
                //uint xFirstDataLookupLocation = (uint)(xEndOfKernel + sizeof(GlobalInformationTable));
                //mGlobalInformationTable->FirstDataLookupTable = (DataLookupTable*)xFirstDataLookupLocation;
            //}
        }

        //public static uint TotalDataLookupTableSize {
          //  get {
                //return (uint)(sizeof(DataLookupTable) + (DataLookupTable.EntriesPerTable * sizeof(DataLookupEntry)));
            //}
        //}
    }
}
