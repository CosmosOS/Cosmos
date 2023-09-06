using System;
using Cosmos.Debug.Kernel;
using IL2CPU.API;

namespace Cosmos.Core.Memory
{
    /// <summary>
    /// Enum to track an object's garbage collection status.
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

        /// <summary>
        /// Stores the ID used for strings for quick comparison in CleanUp.
        /// </summary>
        private static uint _StringType;

        /// <summary>
        /// Initializes the heap.
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
        /// Reallocates or "resizes" data assigned to a pointer.
        /// </summary>
        /// <param name="aPtr">Existing pointer.</param>
        /// <param name="newSize">Size to extend to.</param>
        /// <returns>New pointer with specified size while maintaining old data.</returns>
        public static byte* Realloc(byte* aPtr, uint newSize)
        {
            // Existing size
            uint Size = RAT.GetPageType(aPtr) == RAT.PageType.HeapSmall ? ((ushort*)aPtr)[-2] : ((uint*)aPtr)[-4];

            if (Size == newSize)
            {
                // Return existing pointer as nothing needs to be done.
                return aPtr;
            }

            if (Size > newSize)
            {
                Size -= newSize - Size;
            }

            // Allocate a new buffer to use
            byte* ToReturn = Alloc(newSize);
            MemoryOperations.Copy(ToReturn, aPtr, (int)Size);
            ((ushort*)ToReturn)[-1] = 0;
            Free(aPtr);
            return ToReturn;
        }

        /// <summary>
        /// Allocates memory block of a given size.
        /// </summary>
        /// <param name="aSize">Size of the block to allocate, in bytes.</param>
        /// <returns>Byte pointer to the start of the block.</returns>
        public static byte* Alloc(uint aSize)
        {
            CPU.DisableInterrupts();

            byte* ptr;

            if (aSize <= HeapSmall.mMaxItemSize)
            {
                ptr = HeapSmall.Alloc((ushort)aSize);
            }
            else if (aSize <= HeapMedium.MaxItemSize)
            {
                ptr = HeapMedium.Alloc(aSize);
            }
            else
            {
                ptr = HeapLarge.Alloc(aSize);
            }

            CPU.EnableInterrupts();
            return ptr;
        }

        /// <summary>
        /// Allocates memory and returns the pointer as uint.
        /// </summary>
        /// <param name="aSize">Size of memory to allocate.</param>
        /// <returns></returns>
        public static uint SafeAlloc(uint aSize)
        {
            return (uint)Alloc(aSize);
        }

        // Keep as void* and not byte* or other. Reduces typecasting from callers
        /// <summary>
        /// Frees a heap item.
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
        /// Collects all unreferenced objects after identifying them first.
        /// </summary>
        /// <returns>Number of objects freed.</returns>
        public static int Collect()
        {
            CPU.DisableInterrupts();

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

            int freed = 0;

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

            CPU.EnableInterrupts();
            return freed;
        }

        /// <summary>
        /// Marks a GC managed object as referenced and recursively marks child objects as well.
        /// </summary>
        /// <param name="aPtr"></param>
        public static void MarkAndSweepObject(void* aPtr)
        {
            var gcPointer = (ObjectGCStatus*)aPtr;

            if ((gcPointer[-1] & ObjectGCStatus.Hit) == ObjectGCStatus.Hit)
            {
                return;
            }

            gcPointer[-1] |= ObjectGCStatus.Hit;

            uint* obj = (uint*)aPtr;

            if (*(obj + 1) == (uint)ObjectUtils.InstanceTypeEnum.NormalObject)
            {
                var type = *obj;

                if (type == _StringType)
                {
                    return;
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
                            if (RAT.GetPageType(location) == RAT.PageType.HeapSmall)
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

        /// <summary>
        /// Marks all objects referenced.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
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
                    if (*location != 0)
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
            return UInt32.MaxValue;
        }
    }
}
