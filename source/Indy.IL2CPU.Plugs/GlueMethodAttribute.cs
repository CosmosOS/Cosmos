using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Plugs {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class GlueMethodAttribute: Attribute {
		public int Type;
		public const string TypePropertyName = "Type";
	}
}
