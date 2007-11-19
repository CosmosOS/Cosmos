using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class PlugAttribute: Attribute {
		public Type Target;
		public PlugScopeEnum Scope = PlugScopeEnum.All;
	}
}