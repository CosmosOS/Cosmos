using System;

namespace Cosmos.IL2CPU.API
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class FieldTypeAttribute: Attribute {
		private string mName;
		public string Name
		{
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
		}
	}
}