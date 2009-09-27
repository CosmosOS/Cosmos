using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cosmos.IL2CPU {
  public class MethodInfo {
    public enum TypeEnum { Normal, Plug, NeedsPlug };

    public readonly MethodBase MethodBase;
    public readonly TypeEnum Type;
    public readonly UInt32 UID;
    public readonly MethodInfo PlugMethod;
    public readonly Type MethodAssembler;
    public MethodInfo PluggedByMethod;

    public MethodInfo(MethodBase aMethodBase, UInt32 aUID, TypeEnum aType, MethodInfo aPlugMethod, Type aMethodAssembler):this(aMethodBase, aUID, aType, aPlugMethod) {
      MethodAssembler = aMethodAssembler;
    }


    public MethodInfo(MethodBase aMethodBase, UInt32 aUID, TypeEnum aType, MethodInfo aPlugMethod) {
      MethodBase = aMethodBase;
      UID = aUID;
      Type = aType;
      PlugMethod = aPlugMethod;
    }

  }
}
