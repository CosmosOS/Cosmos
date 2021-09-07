using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.Memory.Old
{
    // This class must be static, as for creating objects, we need the heap
    // this heap implementation it the very most basic one: no reentrancy, etc.
    // Interrupts are disabled when trying to allocate a new block of memory.
    public static unsafe class Heap
    {
        private static Debugger mDebugger = new Debugger("Core", "Memory");

        private static uint mEndOfRam;

        private static DataLookupTable* mLastTable;

        private static uint mLastEntryIndex = 0u;

        private static bool mInitialized = false;

        private static void DoInitialize(uint aEndOfRam)
        {
            mLastTable = null;
            mLastEntryIndex = 0u;
            mEndOfRam = aEndOfRam;
            //
        }

        internal static void EnsureIsInitialized()
        {
            if (!mInitialized)
            {
                mInitialized = true;
                DoInitialize((CPU.GetAmountOfRAM() - 1) * 1024 * 1024);
            }
        }

        private static void ClearMemory(void* aStartAddress, uint aLength)
        {
            //TODO: Move to memory. Internal access only...
            CPU.ZeroFill((uint)aStartAddress, aLength);
        }

        public static uint MemAlloc(uint aLength)
        {
            if (aLength == 0)
            {
                mDebugger.Send("    Request to retrieve block with size = 0 was halted!");
                while (true)
                {
                }
            }

            bool xInterruptsWereEnabled = CPU.DisableInterrupts();
            try
            {
                EnsureIsInitialized();

                DataLookupTable* xCurrentTable = GlobalSystemInfo.GlobalInformationTable->FirstDataLookupTable;
                DataLookupTable* xPreviousTable = null;
                uint xResult;

                if (mLastTable != null)
                {
                    xCurrentTable = mLastTable;
                }

                #region Loop through existing tables and see if we find a free spot
                while (xCurrentTable != null)
                {
                    if (ScanDataLookupTable(xCurrentTable, aLength, out xResult))
                    {
                        if (xResult < CPU.GetEndOfKernel())
                        {
                            Debugger.DoSend("Wrong handle returned!");
                            while (true)
                            {
                            }
                        }

                        return xResult;
                    }
                    mLastTable = xPreviousTable;
                    xPreviousTable = xCurrentTable;
                    xCurrentTable = xCurrentTable->Next;
                    mLastEntryIndex = 0;
                }
                #endregion Loop through existing tables and see if we find a free spot

                // no tables found, lets create a new one, and use that
                if (xPreviousTable == null)
                {
                    // this check should theoretically be unnecessary, but lets keep it, to do some double-checking.
                    Debugger.DoSend("No PreviousTable found!");
                    while (true)
                    {
                    }
                }

                var xLastItem = xPreviousTable->GetEntry(DataLookupTable.EntriesPerTable - 1);
                var xNextTablePointer = (DataLookupTable*)((uint)xLastItem->DataBlock + xLastItem->Size);
                // the memory hasn't been cleared yet, so lets do that now.
                ClearMemory(xNextTablePointer, GlobalSystemInfo.TotalDataLookupTableSize);
                xPreviousTable->Next = xNextTablePointer;
                xNextTablePointer->Previous = xPreviousTable;
                if (!ScanDataLookupTable(xNextTablePointer, aLength, out xResult))
                {
                    // Something seriously weird happened: we could create a new DataLookupTable (with new entries)
                    // but couldn't allocate a new handle from it.
                    Debugger.DoSend("    Something seriously weird happened: we could create a new DataLookupTable (with new entries), but couldn't allocate a new handle from it.");
                    while (true)
                    {
                    }
                }
                mLastTable = xNextTablePointer;
                mLastEntryIndex = 0;
                return xResult;
            }
            finally
            {
                if (xInterruptsWereEnabled)
                {
                    CPU.EnableInterrupts();
                }
                else
                {
                    //Debugger.DoSend("    Not enabling interrupts, because they weren't enabled yet!");
                }
            }
        }

        private static bool ScanDataLookupTable(DataLookupTable* aTable, uint aSize, out uint aHandle)
        {
            //Debugger.DoSend("At address:");
            //Debugger.DoSendNumber((uint)aTable);

            DataLookupEntry* xPreviousEntry = null;
            for (uint i = mLastEntryIndex; i < DataLookupTable.EntriesPerTable; i++)
            {
                var xCurrentEntry = aTable->GetEntry(i);
                //Debugger.DoSend($"Item.Size", xCurrentEntry->Size);
                //Debugger.DoSend($"Item.Refcount", xCurrentEntry->Refcount);
                if (xCurrentEntry->Size == 0)
                {
                    #region Found an uninitialized entry
                    // found an entry now. Let's set it
                    if (aTable->Next != null)
                    {
                        // once a handle is used, the size should be set. But at this point, it somehow got unset again.
                        // This should never occur.
                        Debugger.DoSend("Found an entry which has no size, but there is a followup DataLookupTable");
                        while (true)
                        {
                        }
                    }

                    void* xDataBlock;
                    //Debugger.DoSend("Now calculate datablock pointer");
                    // now we found ourself a free handle
                    if (i == 0)
                    {
                        //Debugger.DoSend("Using table end");
                        // we don't have a previous handle yet, so we take the FirstByteAfterTable field of the DataLookupTable
                        // note: we're explicitly initializing all blocks, as memory hasn't been cleared yet.
                        var xTableAddr = (uint)aTable;
                        //Debugger.DoSend("xTableAddr");
                        //Debugger.DoSendNumber(xTableAddr);
                        var xTotalTableSize = GlobalSystemInfo.TotalDataLookupTableSize;
                        //Debugger.DoSend("xTotalTableSize");
                        //Debugger.DoSendNumber(xTotalTableSize);
                        xDataBlock = (void*)(xTableAddr + xTotalTableSize);
                    }
                    else
                    {
                        //Debugger.DoSend("Using previous entry");
                        // We're not the very first handle being assigned, so calculate the start address using the previous block
                        xDataBlock = (void*)((uint)xPreviousEntry->DataBlock + xPreviousEntry->Size);
                    }

                    // make sure the memory is empty
                    ClearMemory(xDataBlock, aSize);
                    //Debugger.DoSend("Cleared memory");
                    xCurrentEntry->Size = aSize;
                    xCurrentEntry->DataBlock = xDataBlock;
                    xCurrentEntry->Refcount = 1;
                    aHandle = (uint)xCurrentEntry->DataBlock;
                    //Debugger.DoSend("Returning handle");
                    //Debugger.DoSendNumber(aHandle);
                    mLastEntryIndex = i;
                    #endregion Found an uninitialized entry
                    return true;
                }

                // Refcount == UInt32.MaxValue, it means that the block has been reclaimed, and can be reused now.
                if (xCurrentEntry->Refcount == UInt32.MaxValue)
                {
                    // we can reuse this entry if its Size >= aLength
                    if (xCurrentEntry->Size >= aSize)
                    {
                        // we can reuse this entry
                        xCurrentEntry->Refcount = 1;
                        aHandle = (uint)xCurrentEntry->DataBlock;
                        mLastEntryIndex = i;
                        return true;
                    }
                }
                xPreviousEntry = xCurrentEntry;
            }
            aHandle = 0;
            return false;
        }
    }
}
