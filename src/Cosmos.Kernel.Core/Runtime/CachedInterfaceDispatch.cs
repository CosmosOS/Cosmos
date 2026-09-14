// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory.Heap;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Runtime
{
    /// <summary>
    /// Interface dispatch cell - used for cached interface method dispatch
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct InterfaceDispatchCell
    {
        public nuint m_pStub;  // Pointer to dispatch stub function
        public nuint m_pCache; // Cache pointer (interface type or cached MethodTable)

        // Flags stored in low bits of m_pCache (from rhbinder.h)
        public const int IDC_CachePointerMask = 0x3;
        public const int IDC_CachePointerPointsAtCache = 0x0;
        public const int IDC_CachePointerIsInterfacePointerOrMetadataToken = 0x1;
        public const int IDC_CachePointerIsIndirectedInterfaceRelativePointer = 0x2;
        public const int IDC_CachePointerIsInterfaceRelativePointer = 0x3;
        // Small values with flags==0 are VTable byte offsets, not cache pointers (from rhbinder.h)
        public const nuint IDC_MaxVTableOffsetPlusOne = 0x1000;
    }

    /// <summary>
    /// Information about a dispatch cell
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct DispatchCellInfo
    {
        public DispatchCellType CellType;
        public MethodTable* InterfaceType;
        public ushort InterfaceSlot;
        public byte HasCache;
        public uint MetadataToken;
        public uint VTableOffset;
    }

    internal enum DispatchCellType : byte
    {
        InterfaceAndSlot = 0x0,
        MetadataToken = 0x1,
        VTableOffset = 0x2,
    }

    internal static unsafe class CachedInterfaceDispatch
    {
        /// <summary>
        /// Main entry point for interface dispatch resolution.
        /// Called from RhpInitialDynamicInterfaceDispatch to resolve interface method calls.
        /// </summary>
        [RuntimeExport("RhpCidResolve")]
        internal static unsafe IntPtr RhpCidResolve(object pObject, IntPtr pCell)
        {
            //Serial.WriteString("[CID] Start\n");

            if (pObject is null)
            {
                Serial.WriteString("[CID] Null\n");
                throw new NullReferenceException("Attempted to invoke interface method on null object");
            }

            // Get cell info without calling any complex methods
            RhpGetDispatchCellInfo(pCell, out DispatchCellInfo cellInfo);

            /*
            Serial.WriteString("[CID] Slot=");
            Serial.WriteHex((ushort)cellInfo.InterfaceSlot);
            Serial.WriteString("\n");
            */

            // Resolve the dispatch
            IntPtr pTargetCode = RhResolveDispatchWorker(pObject, (void*)pCell, ref cellInfo);

            if (pTargetCode != IntPtr.Zero)
            {
                //Serial.WriteString("[CID] OK\n");
                return pTargetCode;
            }

            Serial.WriteString("[CID] FAIL Cell=");
            Serial.WriteHex((ulong)pCell);
            Serial.WriteString(" Slot=");
            Serial.WriteHex(cellInfo.InterfaceSlot);
            Serial.WriteString("\n");
            throw new EntryPointNotFoundException("Could not find implementation for interface method");
        }

        [RuntimeExport("RhpResolveInterfaceMethod")]
        internal static IntPtr RhpResolveInterfaceMethod(object pObject, IntPtr pCell)
        {
            if (pObject is null)
            {
                // Optimizer may perform code motion on dispatch such that it occurs independent of
                // null check on "this" pointer. Allow for this case by returning back an invalid pointer.
                return IntPtr.Zero;
            }

            MethodTable* pInstanceType = pObject.GetMethodTable();

            // This method is used for the implementation of LOAD_VIRT_FUNCTION and in that case the mapping we want
            // may already be in the cache.
            IntPtr pTargetCode = RhpSearchDispatchCellCache(pCell, pInstanceType);
            if (pTargetCode == IntPtr.Zero)
            {
                // Otherwise call the version of this method that knows how to resolve the method manually.
                pTargetCode = RhpCidResolve(pObject, pCell);
            }

            return pTargetCode;
        }

        [RuntimeExport("RhResolveDispatch")]
        internal static IntPtr RhResolveDispatch(object pObject, MethodTable* interfaceType, ushort slot)
        {
            DispatchCellInfo cellInfo = default;
            cellInfo.CellType = DispatchCellType.InterfaceAndSlot;
            cellInfo.InterfaceType = interfaceType;
            cellInfo.InterfaceSlot = slot;

            return RhResolveDispatchWorker(pObject, null, ref cellInfo);
        }

        [RuntimeExport("RhResolveDispatchOnType")]
        internal static IntPtr RhResolveDispatchOnType(MethodTable* pInstanceType, MethodTable* pInterfaceType, ushort slot)
        {
            return DispatchResolve.FindInterfaceMethodImplementationTarget(pInstanceType,
                                                                          pInterfaceType,
                                                                          slot,
                                                                          flags: default,
                                                                          ppGenericContext: null);
        }

        [RuntimeExport("RhResolveStaticDispatchOnType")]
        internal static IntPtr RhResolveStaticDispatchOnType(MethodTable* pInstanceType, MethodTable* pInterfaceType, ushort slot, MethodTable** ppGenericContext)
        {
            return DispatchResolve.FindInterfaceMethodImplementationTarget(pInstanceType,
                                                                          pInterfaceType,
                                                                          slot,
                                                                          DispatchResolve.ResolveFlags.Static,
                                                                          ppGenericContext);
        }

        private static unsafe IntPtr RhResolveDispatchWorker(object pObject, void* cell, ref DispatchCellInfo cellInfo)
        {
            // Type of object we're dispatching on.
            MethodTable* pInstanceType = pObject.GetMethodTable();

            if (cellInfo.CellType == DispatchCellType.InterfaceAndSlot)
            {
                IntPtr pTargetCode = DispatchResolve.FindInterfaceMethodImplementationTarget(pInstanceType,
                                                                              cellInfo.InterfaceType,
                                                                              cellInfo.InterfaceSlot,
                                                                              flags: default,
                                                                              ppGenericContext: null);
                return pTargetCode;
            }
            else if (cellInfo.CellType == DispatchCellType.VTableOffset)
            {
                // Dereference VTable
                return *(IntPtr*)(((byte*)pInstanceType) + cellInfo.VTableOffset);
            }
            else
            {
                // MetadataToken dispatch not supported in Cosmos
                Serial.WriteString("[CID] BadCellType\n");
                throw new NotSupportedException("Metadata token dispatch not supported");
            }
        }

        /// <summary>
        /// Get information about a dispatch cell
        /// </summary>
        private static unsafe void RhpGetDispatchCellInfo(IntPtr pCell, out DispatchCellInfo cellInfo)
        {
            cellInfo = default;

            InterfaceDispatchCell* pDispatchCell = (InterfaceDispatchCell*)pCell;

            // Extract the cache pointer and check flags
            nuint cachePointer = pDispatchCell[0].m_pCache;

            int flags = (int)(cachePointer & InterfaceDispatchCell.IDC_CachePointerMask);

            // VTable offset fast-path (from rhbinder.h IDC_MaxVTableOffsetPlusOne):
            // A small m_pCache value with flags==0 encodes a VTable byte offset directly,
            // not a pointer to a cache structure.
            if (cachePointer < InterfaceDispatchCell.IDC_MaxVTableOffsetPlusOne &&
                flags == InterfaceDispatchCell.IDC_CachePointerPointsAtCache)
            {
                cellInfo.CellType = DispatchCellType.VTableOffset;
                cellInfo.VTableOffset = (uint)cachePointer;
                return;
            }

            // Find the slot number - walk forward to find terminating cell (m_pStub == 0)
            InterfaceDispatchCell* currentCell = pDispatchCell;
            while (currentCell->m_pStub != 0)
            {
                currentCell++;
            }
            nuint cachePointerValueFlags = currentCell->m_pCache;

            // Extract cell type and slot from terminating cell
            DispatchCellType cellType = (DispatchCellType)(cachePointerValueFlags >> 16);
            ushort interfaceSlot = (ushort)(cachePointerValueFlags & 0xFFFF);

            cellInfo.CellType = cellType;
            cellInfo.InterfaceSlot = interfaceSlot;

            if (flags == InterfaceDispatchCell.IDC_CachePointerIsInterfacePointerOrMetadataToken)
            {
                // Cell contains interface type pointer (direct)
                cellInfo.InterfaceType = (MethodTable*)(cachePointer & ~(nuint)InterfaceDispatchCell.IDC_CachePointerMask);
                cellInfo.HasCache = 0;
            }
            else if (flags == InterfaceDispatchCell.IDC_CachePointerPointsAtCache)
            {
                // Cell has been cached - contains MethodTable pointer (direct)
                cellInfo.InterfaceType = (MethodTable*)(cachePointer & ~(nuint)InterfaceDispatchCell.IDC_CachePointerMask);
                cellInfo.HasCache = 1;
            }
            else if (flags == InterfaceDispatchCell.IDC_CachePointerIsInterfaceRelativePointer ||
                     flags == InterfaceDispatchCell.IDC_CachePointerIsIndirectedInterfaceRelativePointer)
            {
                // Calculate address of m_pCache field (8 bytes after cell start)
                nuint cacheFieldAddress = (nuint)pCell + 8;

                // Cast cachePointer to signed int32 BEFORE masking, then add to field address
                int signedOffset = (int)cachePointer;
                nuint interfacePointerValue = cacheFieldAddress + (nuint)(nint)signedOffset;

                // NOW mask off the low bits
                interfacePointerValue &= ~(nuint)InterfaceDispatchCell.IDC_CachePointerMask;

                // Check which subcase: direct (0x3) or indirected (0x2)
                MethodTable* actualInterfaceType;
                if (flags == InterfaceDispatchCell.IDC_CachePointerIsInterfaceRelativePointer)
                {
                    // 0x3: The calculated address IS the MethodTable (direct)
                    actualInterfaceType = (MethodTable*)interfacePointerValue;
                }
                else
                {
                    // 0x2: The calculated address points to the MethodTable (indirected)
                    actualInterfaceType = *(MethodTable**)interfacePointerValue;
                }

                cellInfo.InterfaceType = actualInterfaceType;
                cellInfo.HasCache = 0;
            }
            else
            {
                // Unknown format - should not happen
                cellInfo.InterfaceType = (MethodTable*)(cachePointer & ~(nuint)InterfaceDispatchCell.IDC_CachePointerMask);
                cellInfo.HasCache = 0;
            }
        }

        /// <summary>
        /// Search the dispatch cell cache for a matching entry
        /// </summary>
        private static IntPtr RhpSearchDispatchCellCache(IntPtr pCell, MethodTable* pInstanceType)
        {
            // In a simple implementation, we don't maintain a separate cache
            // The dispatch cell itself serves as the cache after first resolution
            // For now, always return Zero to trigger resolution
            return IntPtr.Zero;
        }

        /// <summary>
        /// Create a new interface dispatch cell
        /// </summary>
        [RuntimeExport("RhNewInterfaceDispatchCell")]
        internal static IntPtr RhNewInterfaceDispatchCell(MethodTable* pInterface, int slotNumber)
        {
            // Allocate two cells (8 bytes * 2 = 16 bytes on 32-bit, 16 bytes * 2 = 32 bytes on 64-bit)
            InterfaceDispatchCell* pCell = (InterfaceDispatchCell*)
                SmallHeap.Alloc((uint)(sizeof(InterfaceDispatchCell) * 2));

            if (pCell == null)
            {
                return IntPtr.Zero;
            }

            // Initialize the dispatch cell
            // Cell[0].m_pStub would point to RhpInitialDynamicInterfaceDispatch in a full implementation
            // Cell[0].m_pCache contains the interface type pointer with flag bit set
            pCell[0].m_pStub = 0; // Would be address of RhpInitialDynamicInterfaceDispatch
            pCell[0].m_pCache = ((nuint)pInterface) | InterfaceDispatchCell.IDC_CachePointerIsInterfacePointerOrMetadataToken;

            // Cell[1] contains slot number
            pCell[1].m_pStub = 0;
            pCell[1].m_pCache = (nuint)slotNumber;

            return (IntPtr)pCell;
        }
    }
}
