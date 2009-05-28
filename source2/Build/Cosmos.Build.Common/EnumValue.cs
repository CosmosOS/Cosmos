using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common {

	public class EnumValue {

		public static EnumValue[] GetEnumValues(Type enumType)
		{
			if (enumType.IsEnum == false)
			{ throw new Exception("Invalid type, only enum types allowed."); }

			List<EnumValue> list = new List<EnumValue>();

			foreach (Object value in Enum.GetValues(enumType))
			{ list.Add(new EnumValue((Enum)value)); }

			return list.ToArray();
		}

		public EnumValue()
		{ }

		public EnumValue(Enum value)
		{ this.Value = value; }

		public Enum Value
		{ get; set; }

		public override string ToString()
		{
			if (this.Value != null)
			{ return DescriptionAttribute.GetDescription(this.Value); }
			return base.ToString();
		}
	}
}
