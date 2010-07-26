using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common {

	public class EnumValue {

		public static T Parse<T>(String value, T @default)
		{
			T result = @default;

			Type valueType = typeof(T);
			if (valueType.IsEnum == false)
			{ throw new ArgumentException("Enum types only supported.", "T"); }

			if (String.IsNullOrEmpty(value) == false)
			{
				if (Enum.IsDefined(valueType, value) == true)
				{
					result = (T)Enum.Parse(valueType, value);
				}
			}
			
			return result;
		}

		public static EnumValue Find(System.Collections.IEnumerable items, Enum value)
		{
			foreach (Object item in items)
			{
				if (item is EnumValue)
				{
					if (Object.Equals(((EnumValue)item).Value, value) == true)
					{ return (EnumValue)item; }
				}
			}

			return null;
		}

		public static EnumValue[] GetEnumValues(Type enumType, bool aSort) {
			if (!enumType.IsEnum) { 
                throw new Exception("Invalid type, only enum types allowed."); 
            }

			var xList = new List<EnumValue>();
            IEnumerable<object> xQry;
            if (aSort) {
                xQry = from x in Enum.GetValues(enumType).OfType<object>()
                       orderby x.ToString()
                       select x;
            } else {
                xQry = from x in Enum.GetValues(enumType).OfType<object>()
                       select x;
            }
            foreach (var x in xQry) {
                xList.Add(new EnumValue((Enum)x)); 
            }

            return xList.ToArray();
		}

		public EnumValue() {
        }

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
