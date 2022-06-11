namespace Cosmos.CPU.x86.Memory.Old;

public static unsafe class GlobalSystemInfo
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

    public static uint TotalDataLookupTableSize =>
        (uint)(sizeof(DataLookupTable) + DataLookupTable.EntriesPerTable * sizeof(DataLookupEntry));

    public static void EnsureInitialized()
    {
        if (mGlobalInformationTable == null)
        {
            // todo: should we align this structure somehow?

            var xEndOfKernel = Processor.GetEndOfKernel();
            xEndOfKernel = xEndOfKernel + 1024 * 1024; // for now, skip 1 MB
            Processor.ZeroFill(xEndOfKernel, (uint)(sizeof(GlobalInformationTable) + TotalDataLookupTableSize) * 4);
            mGlobalInformationTable = (GlobalInformationTable*)xEndOfKernel;
            var xFirstDataLookupLocation = (uint)(xEndOfKernel + sizeof(GlobalInformationTable));
            //Debugger.DoSend("Setting FirstDataLookupTable to ");
            //Debugger.DoSendNumber(xFirstDataLookupLocation);
            mGlobalInformationTable->FirstDataLookupTable = (DataLookupTable*)xFirstDataLookupLocation;
            //Debugger.DoSend("FirstDataLookupTable was set to ");
            //Debugger.DoSendNumber((uint)mGlobalInformationTable->FirstDataLookupTable);
        }
    }
}
