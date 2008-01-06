using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target=typeof(bool))]
	public static class BooleanImpl {
		public static string ToString(bool aThis) {
			if (aThis) {
				return "true";
			} else {
				return "false";
			}
		}
	}
}