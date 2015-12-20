using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Common;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core
{
    // This class must be static, as for creating objects, we need the heap
    // this heap implementation it the very most basic one: no reentrancy, etc.
    // Interrupts are disabled when trying to allocate a new block of memory.
    public static unsafe partial class Heap
    {
        private static uint mEndOfRam;

        private static uint mLastTableIndex = 0u;

        private static uint mLastEntryIndex = 0u;

        private static void DoInitialize(uint aEndOfRam)
        {
            mLastTableIndex = 0u;
            mLastEntryIndex = 0u;
            mEndOfRam = aEndOfRam;
            //
        }

        private static bool mInitialized = false;
        internal static void EnsureIsInitialized()
        {
            if (!mInitialized)
            {
                mInitialized = true;
                DoInitialize((CPU.GetAmountOfRAM() - 1) * 1024 * 1024);
                //DoInitialize(4 * 1024 * 1024, 16 * 1024 * 1024);
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
                DebugAndHalt("    Request to retrieve block with size = 0 was halted!");
            }
            var xInterruptsWereEnabled = CPU.DisableInterrupts();
            try
            {
                EnsureIsInitialized();

                var xCurrentTableIdx = mLastTableIndex;
                DataLookupTable* xCurrentTable = GlobalSystemInfo.GlobalInformationTable->FirstDataLookupTable;
                DataLookupTable* xPreviousTable = null;
                uint xResult;
                while (xCurrentTable != null)
                {
                    DebugHex("Scanning DataLookupTable ", xCurrentTableIdx);
                    //DebugHex("At address", (uint)xCurrentTable);
                    if (ScanDataLookupTable(xCurrentTableIdx, xCurrentTable, aLength, out xResult))
                    {
                        DebugHex("Returning handle", xResult);
                        DebugHex("For dataobject size", aLength);
                        if (xResult < CPU.GetEndOfKernel())
                        {
                            DebugAndHalt("Wrong handle returned!");
                        }
                        return xResult;
                    }
                    xCurrentTableIdx ++;
                    xPreviousTable = xCurrentTable;
                    xCurrentTable = xCurrentTable->Next;
                    mLastTableIndex = xCurrentTableIdx;
                    mLastEntryIndex = 0;
                }

                // no tables found, lets create a new one, and use that
                if (xPreviousTable == null)
                {
                    // this check should theoretically be unnecessary, but lets keep it, to do some double-checking.
                    DebugAndHalt("No PreviousTable found!");
                }
                Debug("Creating new DataLookupTable");
                var xLastItem = xPreviousTable->GetEntry(DataLookupTable.EntriesPerTable - 1);
                var xNextTablePointer = (DataLookupTable*)((uint)xLastItem->DataBlock + xLastItem->Size);
                // the memory hasn't been cleared yet, so lets do that now.
                ClearMemory(xNextTablePointer, GlobalSystemInfo.TotalDataLookupTableSize);
                xPreviousTable->Next = xNextTablePointer;
                xNextTablePointer->Previous = xPreviousTable;

                if (!ScanDataLookupTable(xCurrentTableIdx, xNextTablePointer, aLength, out xResult))
                {
                    // Something seriously weird happened: we could create a new DataLookupTable (with new entries)
                    // but couldn't allocate a new handle from it.
                    DebugAndHalt("    Something seriously weird happened: we could create a new DataLookupTable (with new entries), but couldn't allocate a new handle from it.");
                }
                DebugHex("Returning handle", xResult);
                DebugHex("For dataobject size", aLength);
                mLastTableIndex = xCurrentTableIdx;
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
                    //Debug("    Not enabling interrupts, because they weren't enabled yet!");
                }
            }
        }

        private static bool ScanDataLookupTable(uint aTableIdx, DataLookupTable* aTable, uint aSize, out uint aHandle)
        {
            DataLookupEntry* xPreviousEntry = null;
            for (uint i = mLastEntryIndex; i < DataLookupTable.EntriesPerTable; i++)
            {
                var xCurrentEntry = aTable->GetEntry(i);

                //DebugHex("Item.Size", xCurrentEntry->Size);
                //DebugHex("Item.Refcount", xCurrentEntry->Refcount);
                if (xCurrentEntry->Size == 0)
                {
                    DebugHex("Found an entry at position ", (uint)i);
                    // found an entry now. Let's set it
                    if (aTable->Next != null)
                    {
                        // once a handle is used, the size should be set. But at this point, it somehow got unset again.
                        // This should never occur.
                        DebugHex("TableIdx", aTableIdx);
                        DebugHex("Index", (uint)i);
                        DebugAndHalt("Found an entry which has no size, but there is a followup DataLookupTable");
                    }

                    void* xDataBlock;
                    //Debug("Now calculate datablock pointer");
                    // now we found ourself a free handle
                    if (i == 0)
                    {
                        //Debug("Using table end");
                        // we don't have a previous handle yet, so we take the FirstByteAfterTable field of the DataLookupTable
                        // note: we're explicitly initializing all blocks, as memory hasn't been cleared yet.
                        var xTableAddr = (uint)aTable;
                        //DebugHex("aTableAddr", xTableAddr);
                        var xTotalTableSize = GlobalSystemInfo.TotalDataLookupTableSize;
                        //DebugHex("TotalTableSize", xTotalTableSize);
                        xDataBlock = (void*)(((uint)aTable) + GlobalSystemInfo.TotalDataLookupTableSize);
                    }
                    else
                    {
                        //Debug("Using previous entry");
                        // We're not the very first handle being assigned, so calculate the start address using the previous block
                        xDataBlock = (void*)((uint)xPreviousEntry->DataBlock + xPreviousEntry->Size);
                    }
                    DebugHex("Datablock", (uint)xDataBlock);

                    // make sure the memory is empty
                    ClearMemory(xDataBlock, aSize);
                    //Debug("Cleared memory");
                    xCurrentEntry->Size = aSize;
                    xCurrentEntry->DataBlock = xDataBlock;
                    xCurrentEntry->Refcount = 1;

                    aHandle = (uint)xCurrentEntry;
                    //DebugHex("Returning handle ", aHandle);
                    if (aHandle == 0x0213D185)
                    {
                        Debug("Last known one");
                    }
                    mLastEntryIndex = i;
                    return true;
                }

                // Refcount == UInt32.MaxValue, it means that the block has been reclaimed, and can be reused now.
                if (xCurrentEntry->Refcount == UInt32.MaxValue)
                {
                    Debug("Found a reclaimed entry");
                    // we can reuse this entry if its Size >= aLength
                    if (xCurrentEntry->Size >= aSize)
                    {
                        Debug("Can be reused");
                        // we can reuse this entry
                        xCurrentEntry->Refcount = 1;
                        aHandle = (uint)xCurrentEntry;
                        DebugHex("Returning reused handle ", aHandle);
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
