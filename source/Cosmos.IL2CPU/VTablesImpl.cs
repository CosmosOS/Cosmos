using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Cosmos.Common;
using Cosmos.Debug.Kernel;

namespace Cosmos.IL2CPU
{
  // todo: optimize this, probably using assembler
  public static partial class VTablesImpl
  {
    // this field seems to be always empty, but the VTablesImpl class is embedded in the final exe.
    public static VTable[] mTypes;

    static VTablesImpl()
    {

    }

    public static bool IsInstance(uint aObjectType, uint aDesiredObjectType)
    {
      var xCurrentType = aObjectType;
      if (aObjectType == 0)
      {
        return true;
      }
      do
      {
        if (xCurrentType == aDesiredObjectType)
        {
          return true;
        }
        if (xCurrentType == mTypes[xCurrentType].BaseTypeIdentifier)
        {
          return false;
        }
        xCurrentType = mTypes[xCurrentType].BaseTypeIdentifier;
      }
      while (xCurrentType != 0);
      return false;
    }

    public static void SetTypeInfo(int aType, uint aBaseType, uint[] aMethodIndexes, uint[] aMethodAddresses, uint aMethodCount)
      //public static void SetTypeInfo(int aType, uint aBaseType, uint aMethodIndexesA, uint aMethodIndexesB, uint aMethodAddressesA, uint aMethodAddressesB, uint aMethodCount)
    {
      if (aType == 2)
      {
        EnableDebug = true;
        Debug("DoTest");
        DebugHex("aMethodIndexes.Length", (uint)aMethodIndexes.Length);
        EnableDebug = false;
      }
      //var xType = mTypes[aType];
      //xType.BaseTypeIdentifier = aBaseType;
      //xType.MethodIndexes = aMethodIndexes;
      //xType.MethodAddresses = aMethodAddresses;
      //xType.MethodCount = (int)aMethodCount;
      //mTypes[aType] = xType;

      //Debug("SetTypeInfo");
      //DebugHex("Type", (uint) aType);
      //DebugHex("BaseType", (uint) aBaseType);
      //DebugHex("MethodCount", aMethodCount);
      ////DebugHex("aMethodAddressesA", aMethodAddressesA);
      ////DebugHex("aMethodAddressesB", aMethodAddressesB);
      ////DebugHex("aMethodIndexesA", aMethodIndexesA);
      ////DebugHex("aMethodIndexesB", aMethodIndexesB);
      //DebugHex("mTypes.Length", (uint)mTypes.Length);
      //DebugHex("aMethodAddresses.Length", (uint)aMethodAddresses.Length);
      //DebugHex("aMethodIndexes.Length", (uint)aMethodIndexes.Length);

      mTypes[aType].BaseTypeIdentifier = aBaseType;
      mTypes[aType].MethodIndexes = aMethodIndexes;
      mTypes[aType].MethodAddresses = aMethodAddresses;
      mTypes[aType].MethodCount = (int) aMethodCount;
      //Debugger.DoBochsBreak();
      //DebugHex("Read back BaseType", mTypes[aType].BaseTypeIdentifier);

      if (aType > 0x98)
      {
        //DebugHex("BaseType of 0x00000098", mTypes[0x00000098].BaseTypeIdentifier);
      }
      //if (aBaseType != mTypes[aType].BaseTypeIdentifier)
      //{
      //  DebugAndHalt("Fout!");
      //}
      //DebugHex("Read back aMethodAddresses.Length", (uint)mTypes[aType].MethodAddresses.Length);

    }

    public static void SetMethodInfo(int aType, int aMethodIndex, uint aMethodIdentifier, uint aMethodAddress)
    {
      //var xType = mTypes[aType];
      //DebugHex("BaseTypeID from type", xType.BaseTypeIdentifier);
      //var xArray = mTypes[aType].MethodAddresses;
      //Debug("Na array");
      //DebugHex("Array length", (uint)xArray.Length);
      //DebugHex("Array[0]", xArray[0]);
      //DebugHex("Array[1]", xArray[1]);
      //DebugHex("Array[2]", xArray[2]);
      //DebugHex("Array[3]", xArray[3]);

      //Debugger.DoBochsBreak();
      mTypes[aType].MethodIndexes[aMethodIndex] = aMethodIdentifier;
      mTypes[aType].MethodAddresses[aMethodIndex] = aMethodAddress;
      //if (aType == 0x9D && aMethodIdentifier == 0x9C)
      //{
      //  Debug("SetMethodInfo");
      //  DebugHex("Type", (uint)aType);
      //  DebugHex("MethodId", (uint)aMethodIdentifier);
      //  DebugHex("Read back MethodId: ", (uint)mTypes[aType].MethodIndexes[aMethodIndex]);
      //  DebugHex("MethodIdx", (uint)aMethodIndex);
      //  DebugHex("aMethodAddress", (uint)aMethodAddress);
      //  DebugHex("Read back address: ", (uint)mTypes[aType].MethodAddresses[aMethodIndex]);
      //}

      if (mTypes[aType].MethodIndexes[aMethodIndex] != aMethodIdentifier)
      {
        DebugAndHalt("Setting went wrong! (1)");
      }
      //DebugAndHalt("Klaar");
      //if (mTypes[aType].MethodAddresses[aMethodIndex] != aMethodAddress)
      //{
      //  DebugHex("aMethodAddress", aMethodAddress);
      //  DebugHex("MethodAddress from array", mTypes[aType].MethodAddresses[aMethodIndex]);
      //  DebugAndHalt("Setting went wrong! (2)");
      //}
    }

    public static uint GetMethodAddressForType(uint aType, uint aMethodId)
    {
      if (aType > 0xFFFF)
      {
        EnableDebug = true;
        DebugHex("Type", aType);
        DebugHex("MethodId", aMethodId);
        Debugger.SendKernelPanic(KernelPanicTypes.VMT_TypeIdInvalid);
        while (true)
        ;
      }
      var xCurrentType = aType;
      DebugHex("Type", xCurrentType);
      DebugHex("MethodId", aMethodId);
      do
      {
        DebugHex("Now checking type", xCurrentType);
        var xCurrentTypeInfo = mTypes[xCurrentType];
        DebugHex("It's basetype is", xCurrentTypeInfo.BaseTypeIdentifier);

        if (xCurrentTypeInfo.MethodIndexes == null)
        {
          EnableDebug = true;
          DebugHex("MethodIndexes is null for type", aType);
          Debugger.SendKernelPanic(KernelPanicTypes.VMT_MethodIndexesNull);
          while (true)
            ;
        }
        if (xCurrentTypeInfo.MethodAddresses == null)
        {
          EnableDebug = true;
          DebugHex("MethodAddresses is null for type", aType);
          Debugger.SendKernelPanic(KernelPanicTypes.VMT_MethodAddressesNull);
          while (true)
            ;
        }

        for (int i = 0; i < xCurrentTypeInfo.MethodIndexes.Length; i++)
        {
          if (xCurrentTypeInfo.MethodIndexes[i] == aMethodId)
          {
            var xResult = xCurrentTypeInfo.MethodAddresses[i];
            if (xResult < 1048576) // if pointer is under 1MB, some issue exists!
            {
              EnableDebug = true;
              DebugHex("Result", (uint)xResult);
              DebugHex("i", (uint)i);
              DebugHex("MethodCount", (uint)xCurrentTypeInfo.MethodCount);
              DebugHex("MethodAddresses.Length", (uint)xCurrentTypeInfo.MethodAddresses.Length);
              Debug("Method found, but address is invalid!");
              Debugger.SendKernelPanic(KernelPanicTypes.VMT_MethodFoundButAddressInvalid);
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
      //}
      EnableDebug = true;
      Debugger.DoSend("Type");
      Debugger.DoSendNumber(aType);
      Debugger.DoSend("MethodId");
      Debugger.DoSendNumber(aMethodId);
      Debugger.DoSend("Not FOUND!");
      Debugger.SendKernelPanic(KernelPanicTypes.VMT_MethodNotFound);
      while (true)
        ;
      throw new Exception("Cannot find virtual method!");
    }
  }

  [StructLayout(LayoutKind.Explicit, Size = 24)]
  public struct VTable
  {
    [FieldOffset(0)]
    public uint BaseTypeIdentifier;

    [FieldOffset(4)]
    public int MethodCount;

    [FieldOffset(8)]
    public uint[] MethodIndexes;

    [FieldOffset(16)]
    public uint[] MethodAddresses;
  }
}
