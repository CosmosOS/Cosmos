using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Cosmos.IL2CPU.X86.IL {
  [DebuggerDisplay("Field '{Id}'")]
  public class FieldInfo {
    public readonly string Id;
    /// <summary>
    /// Does NOT include any kind of method header!
    /// </summary>
    public uint Offset;
    public readonly Type DeclaringType;
    public Type FieldType;
    public uint Size;
    public bool IsExternalValue;

    public FieldInfo(string aId, uint aSize, Type aDeclaringType, Type aFieldType) {
      Id = aId;
      DeclaringType = aDeclaringType;
      FieldType = aFieldType;
      Size = aSize;
    }
  }
}