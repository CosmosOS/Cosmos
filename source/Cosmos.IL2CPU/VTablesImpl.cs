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

    public static void SetTypeInfo(int aType, uint aBaseType, uint[] aMethodIndexes, uint[] aMethodAddresses, uint aMethodCount)
    {
      mTypes[aType].BaseTypeIdentifier = aBaseType;
      mTypes[aType].MethodIndexes = aMethodIndexes;
      mTypes[aType].MethodAddresses = aMethodAddresses;
      mTypes[aType].MethodCount = (int) aMethodCount;
    }

    public static void SetMethodInfo(int aType, int aMethodIndex, uint aMethodIdentifier, uint aMethodAddress)
    {
      mTypes[aType].MethodIndexes[aMethodIndex] = aMethodIdentifier;
      mTypes[aType].MethodAddresses[aMethodIndex] = aMethodAddress;

      if (mTypes[aType].MethodIndexes[aMethodIndex] != aMethodIdentifier)
      {
        DebugAndHalt("Setting went wrong! (1)");
      }
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
