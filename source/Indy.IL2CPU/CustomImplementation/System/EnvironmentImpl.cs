using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.CustomImplementation.System {
	public static class EnvironmentImpl {
		[MethodAlias(Name = "System.String System.Environment.GetResourceFromDefault(System.String)")]
		public static string GetResourceFromDefault(string aResource) {
			return aResource;
		}
	}
}