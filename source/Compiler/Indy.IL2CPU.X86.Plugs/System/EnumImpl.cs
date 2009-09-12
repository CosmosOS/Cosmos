using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.X86.Plugs.CustomImplementations.System {
	[Plug(Target=typeof(Enum))]
	public static class EnumImpl {
	}
}