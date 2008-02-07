using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.CustomImplementation.System {
	[Plug(Target=typeof(Environment))]
	public static class EnvironmentImpl {
		public static string GetResourceFromDefault(string aResource) {
			Console.Write("Getting Resource(3) '");
			Console.Write(aResource);
			Console.WriteLine("'");
			return aResource;
		}

		public static string GetResourceString(string key, params object[] values) {
			Console.Write("Getting Resource(2) '");
			Console.Write(key);
			Console.WriteLine("'");
			return key;
		}

		public static string GetResourceString(string aResource) {
			Console.Write("Getting Resource(1) '");
			Console.Write(aResource);
			Console.WriteLine("'");
			return aResource;
		}
	}
}