using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.X86.Plugs.NEW_PLUGS {
  [Plug(Target=typeof(Delegate), AlsoTargetSubtypes=true)]
  public static class DelegateImpl {
  }
}