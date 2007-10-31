using System;


namespace Cosmos.Kernel.Boot.Glue {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
	public class GlueFieldAttribute: Attribute {
		public GlueFieldTypeEnum FieldType {
			get;
			set;
		}
	}
}
