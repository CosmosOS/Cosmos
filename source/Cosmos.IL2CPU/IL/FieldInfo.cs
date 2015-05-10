using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Cosmos.IL2CPU.X86.IL {
  [DebuggerDisplay("Field = '{Id}', Offset = {Offset}, Size = {Size}")]
  public class FieldInfo {
    public readonly string Id;
    /// <summary>
    /// Does NOT include any kind of method header!
    /// </summary>
    private uint mOffset;
    public bool IsOffsetSet = false;
    public uint Offset
    {
        get
        {
            if (!IsOffsetSet)
            {
                throw new Exception("Offset is being used, but hasnt been set yet!");
            }
            return mOffset;
        }
        set
        {
            IsOffsetSet = true;
            mOffset = value;
        }
    }

      public global::System.Reflection.FieldInfo Field
      {
          get;
          set;
      }

      public readonly Type DeclaringType;
    public Type FieldType;
    public uint Size;
    public bool IsExternalValue;
    public bool IsStatic;

    public FieldInfo(string aId, uint aSize, Type aDeclaringType, Type aFieldType) {
      Id = aId;
      DeclaringType = aDeclaringType;
      FieldType = aFieldType;
      Size = aSize;
    }
  }
}
