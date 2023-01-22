using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core
{
    // todo: optimize this, probably using assembler
    [SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
    [SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
    [SuppressMessage("Style", "IDE0011:Add braces")]
    public static partial class VTablesImpl
    {
        // this field seems to be always empty, but the VTablesImpl class is embedded in the final bin.
        public static VTable[] mTypes;
        public static GCTable[] gcTypes;

        static VTablesImpl()
        {

        }

        public static uint GetBaseType(uint aObjectType)
        {
            if (aObjectType >= mTypes.Length)
            {
                EnableDebug = true;
                DebugAndHalt("Requested GetBaseType for invalid aObjectType: " + aObjectType);
                throw new IndexOutOfRangeException();
            }
            return mTypes[aObjectType].BaseTypeIdentifier;
        }

        public static uint GetSize(uint aObjectType)
        {
            if (aObjectType >= mTypes.Length)
            {
                EnableDebug = true;
                DebugAndHalt("Requested GetSize for invalid aObjectType: " + aObjectType);
                throw new IndexOutOfRangeException();
            }
            return mTypes[aObjectType].Size;
        }

        public static bool IsInstance(uint aObjectType, uint aDesiredObjectType, bool aIsInterface)
        {
            if (aObjectType == 0)
            {
                return true;
            }

            if (aIsInterface)
            {
                var xType = mTypes[aObjectType];

                for (int i = 0; i < xType.InterfaceCount; i++)
                {
                    if (xType.InterfaceIndexes[i] == aDesiredObjectType)
                    {
                        return true;
                    }
                }

                return false;
            }

            var xCurrentType = aObjectType;

            do
            {
                if (xCurrentType == aDesiredObjectType)
                {
                    return true;
                }

                if (xCurrentType == mTypes[xCurrentType].BaseTypeIdentifier)
                {
                    Debug("IsInstance failed (1):");
                    DebugHex("aObjectType: ", aObjectType);
                    DebugHex("aDesiredObjectType: ", aDesiredObjectType);

                    return false;
                }

                xCurrentType = mTypes[xCurrentType].BaseTypeIdentifier;
            }
            while (xCurrentType != 0);

            Debug("IsInstance failed (2):");
            DebugHex("aObjectType: ", aObjectType);
            DebugHex("aDesiredObjectType: ", aDesiredObjectType);

            return false;
        }

        public static void SetTypeInfo(int aType, uint aBaseType, uint aSize, uint aInterfaceCount, uint[] aInterfaceIndexes,
          uint aMethodCount, uint[] aMethodIndexes, uint[] aMethodAddresses,
          uint aInterfaceMethodCount, uint[] aInterfaceMethodIndexes, uint[] aTargetMethodIndexes, uint aGCFieldCount,
          uint[] aGCFieldOffsets, uint[] aGCFieldTypes, bool aIsValueType, bool aIsStruct, string aName, string aAssemblyQualifiedName)
        {
            var vTable = new VTable();
            vTable.BaseTypeIdentifier = aBaseType;
            vTable.Size = aSize;
            vTable.InterfaceCount = aInterfaceCount;
            vTable.InterfaceIndexes = aInterfaceIndexes;
            vTable.MethodCount = aMethodCount;
            vTable.MethodIndexes = aMethodIndexes;
            vTable.MethodAddresses = aMethodAddresses;
            vTable.InterfaceMethodCount = aInterfaceMethodCount;
            vTable.InterfaceMethodIndexes = aInterfaceMethodIndexes;
            vTable.TargetMethodIndexes = aTargetMethodIndexes;
            vTable.IsValueType = aIsValueType;
            vTable.IsStruct = aIsStruct;
            vTable.Name = aName;
            vTable.AssemblyQualifiedName = aAssemblyQualifiedName;
            mTypes[aType] = vTable;
            var gcTable = new GCTable();
            gcTable.GCFieldCount = aGCFieldCount;
            gcTable.GCFieldOffsets = aGCFieldOffsets;
            gcTable.GCFieldTypes = aGCFieldTypes;
            gcTypes[aType] = gcTable;
        }

        public static void SetInterfaceInfo(int aType, int aInterfaceIndex, uint aInterfaceIdentifier)
        {
            mTypes[aType].InterfaceIndexes[aInterfaceIndex] = aInterfaceIdentifier;

            if (mTypes[aType].InterfaceIndexes[aInterfaceIndex] != aInterfaceIdentifier)
            {
                DebugAndHalt("Setting interface info failed!");
            }
        }

        public static void SetMethodInfo(int aType, int aMethodIndex, uint aMethodIdentifier, uint aMethodAddress)
        {
            mTypes[aType].MethodIndexes[aMethodIndex] = aMethodIdentifier;
            mTypes[aType].MethodAddresses[aMethodIndex] = aMethodAddress;

            if (mTypes[aType].MethodIndexes[aMethodIndex] != aMethodIdentifier)
            {
                DebugAndHalt("Setting method info failed! (1)");
            }
        }

        public static void SetInterfaceMethodInfo(int aType, int aMethodIndex, uint aInterfaceMethodId, uint aTargetMethodId)
        {
            mTypes[aType].InterfaceMethodIndexes[aMethodIndex] = aInterfaceMethodId;
            mTypes[aType].TargetMethodIndexes[aMethodIndex] = aTargetMethodId;
        }

        public static uint GetMethodAddressForType(uint aType, uint aMethodId)
        {
            if (aType > 0xFFFF)
            {
                EnableDebug = true;
                DebugHex("Type", aType);
                DebugHex("MethodId", aMethodId);
                Debugger.SendKernelPanic(KernelPanics.VMT_TypeIdInvalid);
                while (true) ;
            }
            var xCurrentType = aType;
            do
            {
                DebugHex("Now checking type", xCurrentType);
                var xCurrentTypeInfo = mTypes[xCurrentType];
                DebugHex("It's basetype is", xCurrentTypeInfo.BaseTypeIdentifier);

                if (xCurrentTypeInfo.MethodIndexes == null)
                {
                    EnableDebug = true;
                    DebugHex("MethodIndexes is null for type", aType);
                    Debugger.SendKernelPanic(KernelPanics.VMT_MethodIndexesNull);
                    while (true) ;
                }
                if (xCurrentTypeInfo.MethodAddresses == null)
                {
                    EnableDebug = true;
                    DebugHex("MethodAddresses is null for type", aType);
                    Debugger.SendKernelPanic(KernelPanics.VMT_MethodAddressesNull);
                    while (true) ;
                }

                for (int i = 0; i < xCurrentTypeInfo.MethodIndexes.Length; i++)
                {
                    if (xCurrentTypeInfo.MethodIndexes[i] == aMethodId)
                    {
                        var xResult = xCurrentTypeInfo.MethodAddresses[i];
                        if (xResult < 1048576) // if pointer is under 1MB, some issue exists!
                        {
                            EnableDebug = true;
                            DebugHex("Type", xCurrentType);
                            DebugHex("MethodId", aMethodId);
                            DebugHex("Result", xResult);
                            DebugHex("i", (uint)i);
                            DebugHex("MethodCount", xCurrentTypeInfo.MethodCount);
                            DebugHex("MethodAddresses.Length", (uint)xCurrentTypeInfo.MethodAddresses.Length);
                            Debug("Method found, but address is invalid!");
                            Debugger.SendKernelPanic(KernelPanics.VMT_MethodFoundButAddressInvalid);
                            while (true)
                                ;
                        }
                        Debug("Found.");
                        return xResult;
                    }
                }
                if (xCurrentType == xCurrentTypeInfo.BaseTypeIdentifier)
                {
                    Debug("Ultimate base type already found!");
                    break;
                }
                xCurrentType = xCurrentTypeInfo.BaseTypeIdentifier;
            }
            while (true);

            EnableDebug = true;
            DebugHex("Type", aType);
            DebugHex("MethodId", aMethodId);
            Debug("Not FOUND!");

            Debugger.SendKernelPanic(KernelPanics.VMT_MethodNotFound);
            while (true) ;
            throw new Exception("Cannot find virtual method!");
        }

        // For a certain type and virtual method, find which type defines the virtual method actually used
        public static uint GetDeclaringTypeOfMethodForType(uint aType, uint aMethodId)
        {
            var xCurrentType = aType;
            do
            {
                var xCurrentTypeInfo = mTypes[xCurrentType];

                for (int i = 0; i < xCurrentTypeInfo.MethodIndexes.Length; i++)
                {
                    if (xCurrentTypeInfo.MethodIndexes[i] == aMethodId)
                    {
                        return xCurrentType;
                    }
                }
                if (xCurrentType == xCurrentTypeInfo.BaseTypeIdentifier)
                {
                    Debug("Ultimate base type already found!");
                    break;
                }
                xCurrentType = xCurrentTypeInfo.BaseTypeIdentifier;
            }
            while (true);

            EnableDebug = true;
            DebugHex("Type", aType);
            DebugHex("MethodId", aMethodId);
            Debug("Not FOUND Declaring TYPE!");
            Debugger.DoBochsBreak();
            Debugger.SendKernelPanic(KernelPanics.VMT_MethodNotFound);
            while (true) ;
        }

        public static uint GetMethodAddressForInterfaceType(uint aType, uint aInterfaceMethodId)
        {
            if (aType > 0xFFFF)
            {
                EnableDebug = true;
                DebugHex("Type", aType);
                DebugHex("InterfaceMethodId", aInterfaceMethodId);
                Debugger.SendKernelPanic(KernelPanics.VMT_TypeIdInvalid);
                while (true) ;
            }

            var xTypeInfo = mTypes[aType];

            if (xTypeInfo.InterfaceMethodIndexes == null)
            {
                EnableDebug = true;
                DebugHex("InterfaceMethodIndexes is null for type", aType);
                Debugger.SendKernelPanic(KernelPanics.VMT_MethodIndexesNull);
                while (true) ;
            }

            if (xTypeInfo.TargetMethodIndexes == null)
            {
                EnableDebug = true;
                DebugHex("TargetMethodIndexes is null for type", aType);
                Debugger.SendKernelPanic(KernelPanics.VMT_MethodAddressesNull);
                while (true) ;
            }

            for (int i = 0; i < xTypeInfo.InterfaceMethodIndexes.Length; i++)
            {
                if (xTypeInfo.InterfaceMethodIndexes[i] == aInterfaceMethodId)
                {
                    var xTargetMethodId = xTypeInfo.TargetMethodIndexes[i];
                    return GetMethodAddressForType(aType, xTargetMethodId);
                }
            }

            EnableDebug = true;
            DebugHex("Type", aType);
            DebugHex("InterfaceMethodId", aInterfaceMethodId);
            Debug("Not FOUND!");

            Debugger.SendKernelPanic(KernelPanics.VMT_MethodNotFound);
            while (true) ;
        }

        /// <summary>
        /// Get nnumber of GC tracked Fields in Type
        /// This includes all objects and struct fields
        /// </summary>
        /// <param name="aType"></param>
        /// <returns></returns>
        public static uint GetGCFieldCount(uint aType)
        {
            return gcTypes[aType].GCFieldCount;
        }

        /// <summary>
        /// Get Field Offsets of all Fields tracked by GC in bytes
        /// </summary>
        /// <param name="aType"></param>
        /// <returns></returns>
        public static uint[] GetGCFieldOffsets(uint aType)
        {
            return gcTypes[aType].GCFieldOffsets;
        }

        /// <summary>
        /// Get Types of Types Fields
        /// </summary>
        /// <param name="aType"></param>
        /// <returns></returns>
        public static uint[] GetGCFieldTypes(uint aType)
        {
            return gcTypes[aType].GCFieldTypes;
        }

        /// <summary>
        /// Determine if Type is a ValueType
        /// </summary>
        /// <param name="aType"></param>
        /// <returns></returns>
        public static bool IsValueType(uint aType)
        {
            return mTypes[aType].IsValueType;
        }

        /// <summary>
        /// Determine if a Type is a Struct
        /// </summary>
        /// <param name="aType"></param>
        /// <returns></returns>
        public static bool IsStruct(uint aType)
        {
            return mTypes[aType].IsStruct;
        }

        /// <summary>
        /// Gets the Name of the type
        /// </summary>
        /// <param name="aType"></param>
        /// <returns></returns>
        public static string GetName(uint aType)
        {
            return mTypes[aType].Name;
        }

        /// <summary>
        /// Gets the Assembly Qualified Name for the type
        /// </summary>
        /// <param name="aType"></param>
        /// <returns></returns>
        public static string GetAssemblyQualifiedName(uint aType)
        {
            return mTypes[aType].AssemblyQualifiedName;
        }

        /// <summary>
        /// Get type id of type matching the name
        /// The name can either be name of the class or the assembly qualified name
        /// Only inlcuding the first or first two parts of the assembly qualified name also works
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns -1 if no type can be found</returns>
        public static int GetType(string name)
        {
            for (int i = 0; i < mTypes.Length; i++)
            {
                var currType = mTypes[i];
                if (currType.Name == name || currType.AssemblyQualifiedName == name)
                {
                    return i;
                }
                else
                {
                    bool difference = false;
                    for (int k = 0; k < name.Length; k++)
                    {
                        if (name[k] != currType.AssemblyQualifiedName[k])
                        {
                            difference = true;
                            break;
                        }
                    }
                    if (!difference)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }

    public struct VTable
    {
        public string Name;
        public string AssemblyQualifiedName;
        public uint BaseTypeIdentifier;
        public uint Size;

        public uint InterfaceCount;
        public uint[] InterfaceIndexes;

        public uint MethodCount;
        public uint[] MethodIndexes;
        public uint[] MethodAddresses;

        public uint InterfaceMethodCount;
        public uint[] InterfaceMethodIndexes;
        public uint[] TargetMethodIndexes;

        public bool IsValueType;
        public bool IsStruct;
    }

    public struct GCTable
    {
        public uint GCFieldCount; // Number of fields where objects are stored on the heap
        public uint[] GCFieldOffsets;
        public uint[] GCFieldTypes;
    }
}
