using System;

namespace Indy.IL2CPU.Plugs {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class PlugAttribute: Attribute {
		public Type Target;
		public PlugScopeEnum Scope = PlugScopeEnum.All;

		public const string ScopePropertyName = "Scope";
		public const string TargetPropertyName = "Target";
	}
}