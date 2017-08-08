using System;

namespace Cosmos.IL2CPU.API.Attribs
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class FieldType : Attribute {
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