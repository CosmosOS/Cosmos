using System;
using System.Collections.Generic;
using System.Linq;

namespace IL2CPU.API
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
	public sealed class PlugFieldAttribute: Attribute {
		public PlugFieldAttribute() {
		}

		public string FieldId {
			get;
			set;
		}

		public bool IsExternalValue {
			get;
			set;
		}

		public Type FieldType {
			get;
			set;
		}

        public bool IsMonoOnly = false;
        public bool IsMicrosoftdotNETOnly = false;
	}
}