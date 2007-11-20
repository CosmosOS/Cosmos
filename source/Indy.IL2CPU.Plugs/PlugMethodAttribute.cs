using System;

namespace Indy.IL2CPU.Plugs {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class PlugMethodAttribute: Attribute {
		public string Signature = null;
		public PlugScopeEnum Scope = PlugScopeEnum.All;
		public bool Enabled = true;

		public const string ScopePropertyName = "Scope";
		public const string SignaturePropertyName = "Signature";
		public const string EnabledPropertyName = "Enabled";
	}
}