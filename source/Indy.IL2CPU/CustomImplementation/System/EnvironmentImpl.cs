using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.CustomImplementation.System {
	[Plug(Target=typeof(Environment))]
	public static class EnvironmentImpl {
		public static string GetResourceFromDefault(string aResource) {
			return aResource;
		}

		public static string GetResourceString(string aResource) {
			return aResource;
		}
	}
}