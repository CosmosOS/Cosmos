using System;

namespace Cosmos.Kernel.Boot.Glue {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
	public class GlueMethodAttribute: Attribute {
		public GlueMethodTypeEnum MethodType {
			get;
			set;
		}
	}
}
