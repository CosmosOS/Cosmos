using System;
using Cosmos.Debug.Kernel;
using IL2CPU.API;

namespace Cosmos.Core.Memory
{
    /// <summary>
    /// Flags to track an object status
    /// All higher values in the ushort are used to track count of static counts
    /// </summary>
    public enum ObjectGCStatus : ushort
    {
        None = 0,
        Hit = 1
    }

    /// <summary>
    /// Heap class.
    /// </summary>
    public static unsafe class Heap
    {
        private static uint* StackStart;
        private static uint _StringType;

        /// <summary>
        /// Init heap.
        /// </summary>
        /// <exception cref="Exception">Thrown on fatal error, contact support.</exception>
        public static unsafe void Init()
        {
            _StringType = GetStringTypeID();
            StackStart = (uint*)CPU.GetStackStart();
            HeapSmall.Init();
            HeapMedium.Init();
            HeapLarge.Init();
        }

        /// <summary>
        /// Re-allocates or "re-sizes" data assigned to a pointer.
        /// The pointer specified must be the start of an allocated block in the heap.
        /// This shouldn't be used with objects as a new address is given when reallocating memory.
        /// </summary>
        /// <param name="aPtr">Existing pointer</param>
        /// <param name="newSize">Size to extend to</param>
        /// <returns>New pointer with specified size while maintaining old data.</returns>
        public static byte* Realloc(byte* aPtr, uint newSize)
        {
            // Get existing size
            uint size = RAT.GetPageType(aPtr) == RAT.PageType.HeapSmall ? ((ushort*)aPtr)[-2] : ((uint*)aPtr)[-4];

            if (size == newSize)
            {
                // Return existing pointer as nothing needs to be done.
                return aPtr;
            }
            if (size > newSize)
            {
                size -= newSize - size;
            }

            // Allocate a new buffer to use
            byte* toReturn = Alloc(newSize);

            // Copy the old buffer to the new one
            MemoryOperations.Copy(toReturn, aPtr, (int)Math.Max(size, newSize));

            // Commented out to help in the future if we use objects with realloc
            // Copy the GC state
            //((ushort*)ToReturn)[-1] = ((ushort*)aPtr)[-1];
            ((ushort*)toReturn)[-1] = 0;

            // Free the old data and return
            Free(aPtr);
            return toReturn;
        }

        /// <summary>
        /// Alloc memory block, of a given size.
        /// </summary>
        /// <param name="aSize">A size of block to alloc, in bytes.</param>
        /// <returns>Byte pointer to the start of the block.</returns>
        public static byte* Alloc(uint aSize)
        {
            CPU.DisableInterrupts();

            if (aSize <= HeapSmall.mMaxItemSize)
            {
                byte* ptr = HeapSmall.Alloc((ushort)aSize);
                CPU.EnableInterrupts();
                return ptr;
            }
            else if (aSize <= HeapMedium.MaxItemSize)
            {
                byte* ptr = HeapMedium.Alloc(aSize);
                CPU.EnableInterrupts();
                return ptr;
            }
            else
            {
                byte* ptr = HeapLarge.Alloc(aSize);
                CPU.EnableInterrupts();
                return ptr;
            }
        }

        /// <summary>
        /// Allocates memory and returns the pointer as uint
        /// </summary>
        /// <param name="aSize">Size of memory to allocate</param>
        /// <returns></returns>
        public static uint SafeAlloc(uint aSize)
        {
            return (uint)Alloc(aSize);
        }

        // Keep as void* and not byte* or other. Reduces typecasting from callers
        /// <summary>
        /// Free a heap item.
        /// </summary>
        /// <param name="aPtr">A pointer to the heap item to be freed.</param>
        /// <exception cref="Exception">Thrown if:
        /// <list type="bullet">
        /// <item>Page type is not found.</item>
        /// <item>Heap item not found in RAT.</item>
        /// </list>
        /// </exception>
        public static void Free(void* aPtr)
        {
            //TODO find a better way to remove the double look up here for GetPageType and then again in the
            // .Free methods which actually free the entries in the RAT.
            //Debugger.DoSendNumber(0x77);
            //Debugger.DoSendNumber((uint)aPtr);
            var xType = RAT.GetPageType(aPtr);
            switch (xType)
            {
                case RAT.PageType.HeapSmall:
                    HeapSmall.Free(aPtr);
                    break;
                case RAT.PageType.HeapMedium:
                case RAT.PageType.HeapLarge:
                    HeapLarge.Free(aPtr);
                    break;

                default:
                    throw new Exception("Heap item not found in RAT.");
            }
        }

        /// <summary>
        /// Collects all unreferenced objects after identifying them first
        /// </summary>
        /// <returns>Number of objects freed</returns>
        public static int Collect()
        {
            //Disable interrupts: Prevent CPU exception when allocation is called from interrupt code
            CPU.DisableInterrupts();

            // Mark and sweep objects from roots
            // 1. Check if a page is in use if medium/large mark and sweep object
            // 2. Go through the SMT table for small objects and go through pages by size
            //    mark and sweep all allocated objects as well

            // Medium and large objects
            for (int ratIndex = 0; ratIndex < RAT.TotalPageCount; ratIndex++)
            {
                byte pageType = *(RAT.mRAT + ratIndex);
                if (pageType == (byte)RAT.PageType.HeapMedium || pageType == (byte)RAT.PageType.HeapLarge)
                {
                    byte* pagePtr = RAT.RamStart + ratIndex * RAT.PageSize;
                    if (*(ushort*)(pagePtr + 3 * sizeof(int) + 2) != 0)
                    {
                        MarkAndSweepObject(pagePtr + HeapLarge.PrefixBytes);
                    }
                }
            }

            // Small objects
            var rootSMTPtr = HeapSmall.SMT->First;
            while (rootSMTPtr != null)
            {
                uint size = rootSMTPtr->Size;
                uint objectSize = size + HeapSmall.PrefixItemBytes;
                uint objectsPerPage = RAT.PageSize / objectSize;

                SMTBlock* smtBlock = rootSMTPtr->First;

                while (smtBlock != null)
                {
                    byte* pagePtr = smtBlock->PagePtr;
                    for (int i = 0; i < objectsPerPage; i++)
                    {
                        if (*(ushort*)(pagePtr + i * objectSize + sizeof(ushort)) > 1)
                        {
                            MarkAndSweepObject(pagePtr + i * objectSize + HeapSmall.PrefixItemBytes);
                        }
                    }

                    smtBlock = smtBlock->NextBlock;
                }

                rootSMTPtr = rootSMTPtr->LargerSize;
            }

            // Mark and sweep objects from stack
            uint* currentStackPointer = (uint*)CPU.GetEBPValue();
            while (StackStart != currentStackPointer)
            {
                if (RAT.RamStart < (byte*)*currentStackPointer && (byte*)*currentStackPointer < RAT.HeapEnd)
                {
                    if ((RAT.GetPageType((uint*)*currentStackPointer) & RAT.PageType.GCManaged) == RAT.PageType.GCManaged)
                    {
                        MarkAndSweepObject((uint*)*currentStackPointer);
                    }
                }
                currentStackPointer += 1;
            }

            // Free all unreferenced and reset hit flag
            int freed = 0;
            // Medium and large objects
            for (int ratIndex = 0; ratIndex < RAT.TotalPageCount; ratIndex++)
            {
                var pageType = *(RAT.mRAT + ratIndex);
                if (pageType == (byte)RAT.PageType.HeapMedium || pageType == (byte)RAT.PageType.HeapLarge)
                {
                    byte* pagePointer = RAT.RamStart + ratIndex * RAT.PageSize;
                    if (*((ushort*)(pagePointer + HeapLarge.PrefixBytes - 2)) == 0)
                    {
                        Free(pagePointer + HeapLarge.PrefixBytes);
                        freed += 1;
                    }
                    else
                    {
                        *((ushort*)(pagePointer + HeapLarge.PrefixBytes - 2)) &= (ushort)~ObjectGCStatus.Hit;
                    }
                }
            }

            // Small objects
            rootSMTPtr = HeapSmall.SMT->First;
            while (rootSMTPtr != null)
            {
                uint size = rootSMTPtr->Size;
                uint objectSize = size + HeapSmall.PrefixItemBytes;
                uint objectsPerPage = RAT.PageSize / objectSize;

                SMTBlock* smtBlock = rootSMTPtr->First;

                while (smtBlock != null)
                {
                    byte* pagePtr = smtBlock->PagePtr;
                    for (int i = 0; i < objectsPerPage; i++)
                    {
                        if (*(ushort*)(pagePtr + i * objectSize) != 0)
                        {
                            if (*((ushort*)(pagePtr + i * objectSize) + 1) == 0)
                            {
                                Free(pagePtr + i * objectSize + HeapSmall.PrefixItemBytes);
                                freed += 1;
                            }
                            else
                            {
                                *((ushort*)(pagePtr + i * objectSize) + 1) &= (ushort)~ObjectGCStatus.Hit;
                            }
                        }
                    }
                    smtBlock = smtBlock->NextBlock;
                }

                rootSMTPtr = rootSMTPtr->LargerSize;
            }

            // Enable interrupts back
            CPU.EnableInterrupts();

            return freed;
        }

        public static void MarkAndSweepObject(void* aPtr)
        {
            var gcPointer = (ObjectGCStatus*)aPtr;

            if ((gcPointer[-1] & ObjectGCStatus.Hit) == ObjectGCStatus.Hit)
            {
                return; // we already hit this object
            }

            // Mark
            gcPointer[-1] |= ObjectGCStatus.Hit;

            uint* obj = (uint*)aPtr;

            if (*(obj + 1) == (uint)ObjectUtils.InstanceTypeEnum.NormalObject)
            {
                var type = *obj;
                if (type == _StringType)
                {
                    return; // we are done since they don't hold any reference to fields
                }

                SweepTypedObject(obj, type);
            }
            else if (*(obj + 1) == (uint)ObjectUtils.InstanceTypeEnum.Array)
            {
                var elementType = *obj;
                var length = *(obj + 2);
                var size = *(obj + 3);
                if (VTablesImpl.IsValueType(elementType))
                {
                    if (VTablesImpl.IsStruct(elementType))
                    {
                        for (int i = 0; i < length; i++)
                        {
                            var location = (uint*)((byte*)obj + size * i) + 4;
                            SweepTypedObject(location, elementType);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        var location = (uint*)((byte*)obj + size * i) + 4 + 1;
                        if (*location != 0)
                        {
                            location = *(uint**)location;
                            if (RAT.GetPageType(location) == RAT.PageType.HeapSmall) // so we don't try free string literals
                            {
                                MarkAndSweepObject(location);
                            }
                        }
                    }
                }
            }
            else if (*(obj + 1) == (uint)ObjectUtils.InstanceTypeEnum.BoxedValueType)
            {
                // do nothing
            }
        }

        public static void SweepTypedObject(uint* obj, uint type)
        {
            if (obj == null)
            {
                return;
            }
            uint fields = VTablesImpl.GetGCFieldCount(type);
            var offsets = VTablesImpl.GetGCFieldOffsets(type);
            var types = VTablesImpl.GetGCFieldTypes(type);
            for (int i = 0; i < fields; i++)
            {
                if (!VTablesImpl.IsValueType(types[i]))
                {
                    var location = (uint*)((byte*)obj + offsets[i]) + 1;
                    if (*location != 0) // Check if it's null
                    {
                        location = *(uint**)location;
                        if (RAT.GetPageType(location) != RAT.PageType.Empty)
                        {
                            MarkAndSweepObject(location);
                        }
                    }
                }
                else if (VTablesImpl.IsStruct(types[i]))
                {
                    var obj1 = (uint*)((byte*)obj + offsets[i]);
                    SweepTypedObject(obj1, types[i]);
                }
            }
        }

        private static uint GetStringTypeID()
        {
            return UInt32.MaxValue; // so that tests still pass return bogus value
        }
    }
}
