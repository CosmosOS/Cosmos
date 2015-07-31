using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Cosmos.Debug.Kernel;

namespace Cosmos.IL2CPU
{
  // todo: optimize this, probably using assembler
  public static partial class VTablesImpl
  {
    // this field seems to be always empty, but the VTablesImpl class is embedded in the final exe.
    public static VTable[] mTypes;

    public static bool IsInstance(int aObjectType, int aDesiredObjectType)
    {
      int xCurrentType = aObjectType;
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

    public static void SetTypeInfo(int aType, int aBaseType, int[] aMethodIndexes, int[] aMethodAddresses, int aMethodCount)
    {
      //mTypes[aType] = new VTable();
      mTypes[aType].BaseTypeIdentifier = aBaseType;
      mTypes[aType].MethodIndexes = aMethodIndexes;
      mTypes[aType].MethodAddresses = aMethodAddresses;
      mTypes[aType].MethodCount = aMethodCount;
    }

    public static void SetMethodInfo(int aType, int aMethodIndex, int aMethodIdentifier, int aMethodAddress, char[] aName)
    {
      mTypes[aType].MethodIndexes[aMethodIndex] = aMethodIdentifier;
      mTypes[aType].MethodAddresses[aMethodIndex] = aMethodAddress;
      if (aType == 0x9D && aMethodIdentifier == 0x9C)
      {
        Debug("SetMethodInfo");
        DebugHex("Type", (uint)aType);
        DebugHex("MethodId", (uint)aMethodIdentifier);
        DebugHex("MethodIdx", (uint)aMethodIndex);
        DebugHex("aMethodAddress", (uint)aMethodAddress);
        DebugHex("Read back address: ", (uint)mTypes[aType].MethodAddresses[aMethodIndex]);
      }

      if (mTypes[aType].MethodIndexes[aMethodIndex] != aMethodIdentifier)
      {
        DebugAndHalt("Setting went wrong! (1)");
      }
      if (mTypes[aType].MethodAddresses[aMethodIndex] != aMethodAddress)
      {
        DebugAndHalt("Setting went wrong! (2)");
      }
    }

    public static int GetMethodAddressForType(int aType, int aMethodId)
    {
      if (aType == 0x9D && aMethodId == 0x9C)
      {
        ;
      }
      do
      {
        var xCurrentTypeInfo = mTypes[aType];

        if (xCurrentTypeInfo.MethodIndexes == null)
        {
          DebugHex("MethodIndexes is null for type", (uint)aType);
          while (true)
            ;
        }
        var xMethodCount = xCurrentTypeInfo.MethodIndexes.Length;
        // for now:
        //if (xMethodCount > 4096)
        //{
        //  xMethodCount = 4096;
        //}
        for (int i = 0; i < xCurrentTypeInfo.MethodIndexes.Length; i++)
        {
          if (xCurrentTypeInfo.MethodAddresses == null)
          {
            DebugHex("MethodAddresses is null for type", (uint)aType);
            while (true)
              ;
          }
          if (xCurrentTypeInfo.MethodIndexes[i] == aMethodId)
          {
            var xResult = xCurrentTypeInfo.MethodAddresses[i];
            if (xResult < 1048576) // if pointer is under 1MB, some issue exists!
            {
              DebugHex("Type", (uint)aType);
              DebugHex("MethodId", (uint)aMethodId);
              DebugHex("Result", (uint)xResult);
              DebugHex("i", (uint)i);
              DebugHex("MethodCount", (uint)xCurrentTypeInfo.MethodCount);
              DebugHex("MethodAddresses.Length", (uint)xCurrentTypeInfo.MethodAddresses.Length);
              Debug("Method found, but address is invalid!");
              while (true)
                ;
            }
            DebugHex("Type", (uint)aType);
            DebugHex("MethodIndex", (uint)aMethodId);
            Debug("Found.");
            return xResult;
          }
        }
        if (aType == xCurrentTypeInfo.BaseTypeIdentifier)
        {
          break;
        }
        aType = xCurrentTypeInfo.BaseTypeIdentifier;
      }
      while (true);
      //}
      Debugger.DoSend("Type");
      Debugger.DoSendNumber((uint)aType);
      Debugger.DoSend("MethodIndex");
      Debugger.DoSendNumber((uint)aMethodId);
      Debugger.DoSend("Not FOUND!");
      while (true)
        ;
      throw new Exception("Cannot find virtual method!");
    }
  }

  [StructLayout(LayoutKind.Explicit, Size = 16)]
  public struct VTable
  {
    [FieldOffset(0)]
    public int BaseTypeIdentifier;

    [FieldOffset(4)]
    public int MethodCount;

    [FieldOffset(8)]
    public int[] MethodIndexes;

    [FieldOffset(12)]
    public int[] MethodAddresses;
  }
}
