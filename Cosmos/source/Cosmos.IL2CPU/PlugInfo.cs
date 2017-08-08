using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU {
  public class PlugInfo {
    /// <summary>
    /// The index in mMethodsToProcess of the plug method.
    /// </summary>
    public readonly uint TargetUID;

    public readonly Type PlugMethodAssembler;

    public PlugInfo(uint aTargetUID, Type aPlugMethodAssembler) {
      TargetUID = aTargetUID;
      PlugMethodAssembler = aPlugMethodAssembler;
    }
  }
}
