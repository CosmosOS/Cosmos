using System;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.Memory
{
    /// <summary>
    /// Heap class.
    /// </summary>
    public static unsafe class Heap
    {
        /// <summary>
        /// Init heap.
        /// </summary>
        /// <exception cref="Exception">Thrown on fatal error, contact support.</exception>
        public static void Init()
        {
            HeapSmall.Init();
            HeapMedium.Init();
            HeapLarge.Init();
        }

        /// <summary>
        /// Alloc memory block, of a given size.
        /// </summary>
        /// <param name="aSize">A size of block to alloc, in bytes.</param>
        /// <returns>Byte pointer to the start of the block.</returns>
        public static byte* Alloc(uint aSize)
        {
            //Debugger.DoSendNumber(0xA550C);
            //Debugger.DoSendNumber(aSize);

            if (aSize <= HeapSmall.mMaxItemSize)
            {
                return HeapSmall.Alloc((ushort)aSize);
            }
            else if (aSize <= HeapMedium.MaxItemSize)
            {
                return HeapMedium.Alloc(aSize);
            }
            else
            {
                return HeapLarge.Alloc(aSize);
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
        // who may have typed the pointer to their own needs.
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
        /// Increment reference count of a heap item
        /// Do not use this method on non-managed memory
        /// </summary>
        /// <param name="aPtr"></param>
        public static void IncRefCount(void* aPtr)
        {
            //Debugger.DoSendNumber(0x14C14C);
            var xType = RAT.GetPageType(aPtr);
            switch (xType)
            {
                case RAT.PageType.HeapSmall:
                    HeapSmall.IncRefCount(aPtr);
                    break;
                case RAT.PageType.HeapMedium:
                case RAT.PageType.HeapLarge:
                    HeapLarge.IncRefCount(aPtr);
                    break;

                    //default: we may be incorrectly trying to inc/dec string literals which is why we shouldnt throw an error here
                    //    throw new Exception("Heap item not found in RAT.");
            }
        }

        /// <summary>
        /// Decrement reference count of a heap item.
        /// Do not use this method on non-managed memory
        /// </summary>
        /// <param name="aPtr"></param>
        public static void DecRefCount(void* aPtr, uint id)
        {
            //Debugger.DoSendNumber(0xDECDEC);
            //Debugger.DoSendNumber(id);
            //Debugger.DoSendNumber((uint)aPtr);
            var xType = RAT.GetPageType(aPtr);
            switch (xType)
            {
                case RAT.PageType.HeapSmall:
                    HeapSmall.DecRefCount(aPtr);
                    break;
                case RAT.PageType.HeapMedium:
                case RAT.PageType.HeapLarge:
                    HeapLarge.DecRefCount(aPtr);
                    break;

                    //default: we may be incorrectly trying to inc/dec string literals which is why we shouldnt throw an error here
                    //    throw new Exception("Heap item not found in RAT.");
            }
        }

        /// <summary>
        /// Decrement reference count of a heap item. Does not free if count reaches 0
        /// Do not use this method on non-managed memory
        /// </summary>
        /// <param name="aPtr"></param>
        public static void WeakDecRefCount(void* aPtr, uint id)
        {
            //Debugger.DoSendNumber(id);
            var xType = RAT.GetPageType(aPtr);
            switch (xType)
            {
                case RAT.PageType.HeapSmall:
                    HeapSmall.WeakDecRefCount(aPtr);
                    break;
                case RAT.PageType.HeapMedium:
                case RAT.PageType.HeapLarge:
                    HeapLarge.WeakDecRefCount(aPtr);
                    break;

                    //default: we may be incorrectly trying to inc/dec string literals which is why we shouldnt throw an error here
                    //    throw new Exception("Heap item not found in RAT.");
            }
        }

        /// <summary>
        /// Get reference count of a heap item
        /// </summary>
        /// <param name="aPtr"></param>
        public static uint GetRefCount(void* aPtr)
        {
            var xType = RAT.GetPageType(aPtr);
            switch (xType)
            {
                case RAT.PageType.HeapSmall:
                    return HeapSmall.GetRefCount(aPtr);
                case RAT.PageType.HeapMedium:
                case RAT.PageType.HeapLarge:
                    return HeapLarge.GetRefCount(aPtr);
                default:
                    //Debugger.DoSendNumber((uint)aPtr);
                    //Debugger.DoSendNumber(xType);
                    throw new Exception($"Heap item not found in RAT. ");
            }
        }
    }
}
