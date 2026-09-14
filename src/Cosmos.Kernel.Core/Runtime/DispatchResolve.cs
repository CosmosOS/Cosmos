// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime;
using System.Runtime.InteropServices;
using Internal.Runtime;

#if DEBUG
using Debug = System.Diagnostics.Debug;
#endif

namespace Cosmos.Kernel.Core.Runtime
{
    internal static unsafe class DispatchResolve
    {
        public static IntPtr FindInterfaceMethodImplementationTarget(MethodTable* pTgtType,
                                                                 MethodTable* pItfType,
                                                                 ushort itfSlotNumber,
                                                                 ResolveFlags flags,
                                                                 /* out */ MethodTable** ppGenericContext)
        {
            // We set this bit below during second pass, callers should not set it.
#if DEBUG
            System.Diagnostics.Debug.Assert((flags & ResolveFlags.DefaultInterfaceImplementation) == 0);
#endif

            /*

            Cosmos.Kernel.Core.IO.Serial.WriteString("[FindImpl] TgtType=");
            Cosmos.Kernel.Core.IO.Serial.WriteHex((ulong)pTgtType);
            Cosmos.Kernel.Core.IO.Serial.WriteString(" ItfType=");
            Cosmos.Kernel.Core.IO.Serial.WriteHex((ulong)pItfType);
            Cosmos.Kernel.Core.IO.Serial.WriteString(" Slot=");
            Cosmos.Kernel.Core.IO.Serial.WriteHex(itfSlotNumber);
            Cosmos.Kernel.Core.IO.Serial.WriteString("\n");

            Cosmos.Kernel.Core.IO.Serial.WriteString("[FindImpl] HasDispatchMap=");
            Cosmos.Kernel.Core.IO.Serial.WriteHex((uint)(pTgtType->HasDispatchMap ? 1 : 0));
            Cosmos.Kernel.Core.IO.Serial.WriteString(" IsInterface=");
            Cosmos.Kernel.Core.IO.Serial.WriteHex((uint)(pItfType->IsInterface ? 1 : 0));
            Cosmos.Kernel.Core.IO.Serial.WriteString(" NumVtableSlots=");
            Cosmos.Kernel.Core.IO.Serial.WriteHex(pTgtType->NumVtableSlots);
            Cosmos.Kernel.Core.IO.Serial.WriteString(" NumInterfaces=");
            Cosmos.Kernel.Core.IO.Serial.WriteHex(pTgtType->NumInterfaces);
            Cosmos.Kernel.Core.IO.Serial.WriteString("\n");

            // Dump interface map
            for (ushort idx = 0; idx < pTgtType->NumInterfaces; idx++)
            {
                MethodTable* itf = pTgtType->InterfaceMap[idx];
                Cosmos.Kernel.Core.IO.Serial.WriteString("[FindImpl] Interface[");
                Cosmos.Kernel.Core.IO.Serial.WriteHex(idx);
                Cosmos.Kernel.Core.IO.Serial.WriteString("]=");
                Cosmos.Kernel.Core.IO.Serial.WriteHex((ulong)itf);
                if (itf == pItfType)
                {
                    Cosmos.Kernel.Core.IO.Serial.WriteString(" <<MATCH");
                }
                Cosmos.Kernel.Core.IO.Serial.WriteString("\n");
            }

            */

            // Start at the current type and work up the inheritance chain
            MethodTable* pCur = pTgtType;

        again:
            while (pCur != null)
            {
                /*
                Cosmos.Kernel.Core.IO.Serial.WriteString("[FindImpl] Checking type ");
                Cosmos.Kernel.Core.IO.Serial.WriteHex((ulong)pCur);
                Cosmos.Kernel.Core.IO.Serial.WriteString("\n");
                */

                ushort implSlotNumber;
                if (FindImplSlotForCurrentType(
                        pCur, pItfType, itfSlotNumber, flags, &implSlotNumber, ppGenericContext))
                {
                    /*
                    Cosmos.Kernel.Core.IO.Serial.WriteString("[FindImpl] Found! ImplSlot=");
                    Cosmos.Kernel.Core.IO.Serial.WriteHex(implSlotNumber);
                    Cosmos.Kernel.Core.IO.Serial.WriteString(" (Type has ");
                    Cosmos.Kernel.Core.IO.Serial.WriteHex(pCur->NumVtableSlots);
                    Cosmos.Kernel.Core.IO.Serial.WriteString(" vtable slots)\n");

                    // Dump entire vtable to see what's there
                    Cosmos.Kernel.Core.IO.Serial.WriteString("[FindImpl] VTable dump: ");
                    IntPtr* vtable = pTgtType->GetVTableStartAddress();
                    for (ushort i = 0; i < pTgtType->NumVtableSlots; i++)
                    {
                        Cosmos.Kernel.Core.IO.Serial.WriteString("[");
                        Cosmos.Kernel.Core.IO.Serial.WriteHex(i);
                        Cosmos.Kernel.Core.IO.Serial.WriteString("]=");
                        Cosmos.Kernel.Core.IO.Serial.WriteHex((ulong)vtable[i]);
                        if (i < pTgtType->NumVtableSlots - 1)
                            Cosmos.Kernel.Core.IO.Serial.WriteString(" ");
                    }
                    Cosmos.Kernel.Core.IO.Serial.WriteString("\n");
                    */

                    IntPtr targetMethod;
                    if (implSlotNumber < pCur->NumVtableSlots)
                    {
                        // true virtual - need to get the slot from the target type in case it got overridden
                        targetMethod = pTgtType->GetVTableStartAddress()[implSlotNumber];
                    }
                    else if (implSlotNumber == SpecialDispatchMapSlot.Reabstraction)
                    {
                        throw new EntryPointNotFoundException("Interface method is re-abstracted and cannot be called");
                    }
                    else if (implSlotNumber == SpecialDispatchMapSlot.Diamond)
                    {
                        throw new AmbiguousImplementationException("Ambiguous interface implementation detected");
                    }
                    else
                    {
                        // sealed virtual - need to get the slot form the implementing type, because
                        // it's not present on the target type
                        targetMethod = pCur->GetSealedVirtualSlot((ushort)(implSlotNumber - pCur->NumVtableSlots));
                    }
                    return targetMethod;
                }
                if (pCur->IsArray)
                {
                    pCur = pCur->GetArrayEEType();
                }
                else
                {
                    pCur = pCur->NonArrayBaseType;
                }
            }

            // If we haven't found an implementation, do a second pass looking for a default implementation.
            if ((flags & ResolveFlags.DefaultInterfaceImplementation) == 0)
            {
                flags |= ResolveFlags.DefaultInterfaceImplementation;
                pCur = pTgtType;
                goto again;
            }

            return IntPtr.Zero;
        }


        private static bool FindImplSlotForCurrentType(MethodTable* pTgtType,
                                        MethodTable* pItfType,
                                        ushort itfSlotNumber,
                                        ResolveFlags flags,
                                        ushort* pImplSlotNumber,
                                        MethodTable** ppGenericContext)
        {
            // We set this below during second pass, callers should not set this.
#if DEBUG
            System.Diagnostics.Debug.Assert((flags & ResolveFlags.Variant) == 0);
#endif

            bool fRes = false;

            // If making a call and doing virtual resolution don't look into the dispatch map,
            // take the slot number directly.
            if (!pItfType->IsInterface)
            {
                *pImplSlotNumber = itfSlotNumber;

                // Only notice matches if the target type and search types are the same
                // This will make dispatch to sealed slots work correctly
                return pTgtType == pItfType;
            }

            if (pTgtType->HasDispatchMap)
            {
                //Cosmos.Kernel.Core.IO.Serial.WriteString("[FindImplSlot] Has dispatch map\n");

                // For variant interface dispatch, the algorithm is to walk the parent hierarchy, and at each level
                // attempt to dispatch exactly first, and then if that fails attempt to dispatch variantly. This can
                // result in interesting behavior such as a derived type only overriding one particular instantiation
                // and funneling all the dispatches to it, but its the algorithm.

                fRes = FindImplSlotInSimpleMap(
                    pTgtType, pItfType, itfSlotNumber, pImplSlotNumber, ppGenericContext, flags);

                /*
                Cosmos.Kernel.Core.IO.Serial.WriteString("[FindImplSlot] First pass result=");
                Cosmos.Kernel.Core.IO.Serial.WriteHex((uint)(fRes ? 1 : 0));
                Cosmos.Kernel.Core.IO.Serial.WriteString("\n");
                */

                if (!fRes)
                {
                    flags |= ResolveFlags.Variant; // check variance for second scan of dispatch map
                    fRes = FindImplSlotInSimpleMap(
                     pTgtType, pItfType, itfSlotNumber, pImplSlotNumber, ppGenericContext, flags);
                }
            }

            return fRes;
        }

        private static bool FindImplSlotInSimpleMap(MethodTable* pTgtType,
                                     MethodTable* pItfType,
                                     uint itfSlotNumber,
                                     ushort* pImplSlotNumber,
                                     MethodTable** ppGenericContext,
                                     ResolveFlags flags)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(pTgtType->HasDispatchMap, "Missing dispatch map");
#endif

            bool fCheckVariance = false;
            bool fArrayCovariance = false;

            if ((flags & ResolveFlags.Variant) != 0)
            {
                fCheckVariance = pItfType->HasGenericVariance;
                fArrayCovariance = pTgtType->IsArray;

                // Non-arrays can follow array variance rules iff
                // 1. They have one generic parameter
                // 2. That generic parameter is array covariant.
                //
                // This special case is to allow array enumerators to work
                if (!fArrayCovariance && pTgtType->HasGenericVariance)
                {
                    int tgtEntryArity = (int)pTgtType->GenericArity;
                    GenericVariance* pTgtVarianceInfo = pTgtType->GenericVariance;

                    if ((tgtEntryArity == 1) && pTgtVarianceInfo[0] == GenericVariance.ArrayCovariant)
                    {
                        fArrayCovariance = true;
                    }
                }

                // Arrays are covariant even though you can both get and set elements (type safety is maintained by
                // runtime type checks during set operations). This extends to generic interfaces implemented on those
                // arrays. We handle this by forcing all generic interfaces on arrays to behave as though they were
                // covariant (over their one type parameter corresponding to the array element type).
                if (fArrayCovariance && pItfType->IsGeneric)
                {
                    fCheckVariance = true;
                }

                // If there is no variance checking, there is no operation to perform. (The non-variance check loop
                // has already completed)
                if (!fCheckVariance)
                {
                    return false;
                }
            }

            bool fStaticDispatch = (flags & ResolveFlags.Static) != 0;
            bool checkDefaultImplementations = (flags & ResolveFlags.DefaultInterfaceImplementation) != 0;

            // We either scan the instance or static portion of the dispatch map. Depends on what the caller wants.
            DispatchMap* pMap = pTgtType->DispatchMap;

            /*Cosmos.Kernel.Core.IO.Serial.WriteString("[SimpleMap] Map=");
            Cosmos.Kernel.Core.IO.Serial.WriteHex((ulong)pMap);
            Cosmos.Kernel.Core.IO.Serial.WriteString(" StdEntries=");
            Cosmos.Kernel.Core.IO.Serial.WriteHex((uint)pMap->NumStandardEntries);
            Cosmos.Kernel.Core.IO.Serial.WriteString(" DefEntries=");
            Cosmos.Kernel.Core.IO.Serial.WriteHex((uint)pMap->NumDefaultEntries);
            Cosmos.Kernel.Core.IO.Serial.WriteString("\n");*/

            DispatchMap.DispatchMapEntry* i = fStaticDispatch ?
                pMap->GetStaticEntry(checkDefaultImplementations ? (int)pMap->NumStandardStaticEntries : 0) :
                pMap->GetEntry(checkDefaultImplementations ? (int)pMap->NumStandardEntries : 0);
            DispatchMap.DispatchMapEntry* iEnd = fStaticDispatch ?
                pMap->GetStaticEntry(checkDefaultImplementations ? (int)(pMap->NumStandardStaticEntries + pMap->NumDefaultStaticEntries) : (int)pMap->NumStandardStaticEntries) :
                pMap->GetEntry(checkDefaultImplementations ? (int)(pMap->NumStandardEntries + pMap->NumDefaultEntries) : (int)pMap->NumStandardEntries);

            int entryCount = 0;
            for (; i != iEnd; i = fStaticDispatch ? (DispatchMap.DispatchMapEntry*)(((DispatchMap.StaticDispatchMapEntry*)i) + 1) : i + 1)
            {
                entryCount++;
                /*
                Cosmos.Kernel.Core.IO.Serial.WriteString("[SimpleMap] Entry ");
                Cosmos.Kernel.Core.IO.Serial.WriteHex((uint)entryCount);
                Cosmos.Kernel.Core.IO.Serial.WriteString(": ItfIdx=");
                Cosmos.Kernel.Core.IO.Serial.WriteHex(i->_usInterfaceIndex);
                Cosmos.Kernel.Core.IO.Serial.WriteString(" ItfSlot=");
                Cosmos.Kernel.Core.IO.Serial.WriteHex(i->_usInterfaceMethodSlot);
                Cosmos.Kernel.Core.IO.Serial.WriteString(" ImplSlot=");
                Cosmos.Kernel.Core.IO.Serial.WriteHex(i->_usImplMethodSlot);
                Cosmos.Kernel.Core.IO.Serial.WriteString("\n");
                */

                if (i->_usInterfaceMethodSlot == itfSlotNumber)
                {
                    MethodTable* pCurEntryType =
                        pTgtType->InterfaceMap[i->_usInterfaceIndex];

                    if (pCurEntryType == pItfType)
                    {
                        *pImplSlotNumber = i->_usImplMethodSlot;

                        // If this is a static method, the entry point is not usable without generic context.
                        // (Instance methods acquire the generic context from their `this`.)
                        // Same for IDynamicInterfaceCastable (that has a `this` but it's not useful)
                        if (fStaticDispatch)
                        {
                            *ppGenericContext = GetGenericContextSource(pTgtType, i);
                        }
                        else if ((flags & ResolveFlags.IDynamicInterfaceCastable) != 0)
                        {
                            *ppGenericContext = pTgtType;
                        }

                        return true;
                    }
                    else if (fCheckVariance && ((fArrayCovariance && pCurEntryType->IsGeneric) || pCurEntryType->HasGenericVariance))
                    {
                        // For simplicity in Cosmos, we skip full variance checking
                        // This would require implementing TypeParametersAreCompatible which is complex
                        continue;
                    }
                }
            }

            return false;
        }

        private static unsafe MethodTable* GetGenericContextSource(MethodTable* pTgtType, DispatchMap.DispatchMapEntry* pEntry)
        {
            ushort usEncodedValue = ((DispatchMap.StaticDispatchMapEntry*)pEntry)->_usContextMapSource;
            return usEncodedValue switch
            {
                StaticVirtualMethodContextSource.None => null,
                StaticVirtualMethodContextSource.ContextFromThisClass => pTgtType,
                _ => pTgtType->InterfaceMap[usEncodedValue - StaticVirtualMethodContextSource.ContextFromFirstInterface]
            };
        }

        [Flags]
        public enum ResolveFlags
        {
            Variant = 0x1,
            DefaultInterfaceImplementation = 0x2,
            Static = 0x4,
            IDynamicInterfaceCastable = 0x8,
        }
    }

    internal static class SpecialDispatchMapSlot
    {
        internal const ushort Reabstraction = 0xFFFF;
        internal const ushort Diamond = 0xFFFE;
    }

    internal static class StaticVirtualMethodContextSource
    {
        internal const ushort None = 0xFFFF;
        internal const ushort ContextFromThisClass = 0xFFFE;
        internal const ushort ContextFromFirstInterface = 0;
    }
}
