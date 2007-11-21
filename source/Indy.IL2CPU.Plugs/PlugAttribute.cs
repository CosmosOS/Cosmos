using System;

namespace Indy.IL2CPU.Plugs {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class PlugAttribute: Attribute {
		public Type Target;

		public const string TargetPropertyName = "Target";
	}
}