using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class PlugMethodAttribute: Attribute {
		public string Signature = null;
		public PlugScopeEnum Scope = PlugScopeEnum.All;
	}
}