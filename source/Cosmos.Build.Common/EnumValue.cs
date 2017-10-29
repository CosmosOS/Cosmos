using System;
using System.Collections.Generic;
using System.Linq;

namespace Cosmos.Build.Common
{

    public class EnumValue
    {

        /// <summary>
        /// Parse string to enumeration.
        /// </summary>
        /// <typeparam name="T">Type of enumeration to use.</typeparam>
        /// <param name="value">Value which should be parsed as the enumeration.</param>
        /// <param name="default">Default value to use, if input string contains invalid value.</param>
        /// <returns>Parsed value, or default value if input string is invalid.</returns>
        public static T Parse<T>(string value, T @default)
            where T : struct
        {
            T result = @default;

            Type valueType = typeof(T);
            if (valueType.IsEnum == false)
            {
                throw new ArgumentException("Enum types only supported.", "T");
            }

            if (String.IsNullOrEmpty(value) == false)
            {
                Enum.TryParse(value, true, out result);
            }

            return result;
        }

        public static EnumValue Find(System.Collections.IEnumerable items, Enum value)
        {
            foreach (Object item in items)
            {
                if (item is EnumValue)
                {
                    if (Equals(((EnumValue)item).Value, value) == true)
                    {
                        return (EnumValue)item;
                    }
                }
            }

            return null;
        }

        public static EnumValue[] GetEnumValues(Type enumType, bool aSort)
        {
            if (!enumType.IsEnum)
            {
                throw new Exception("Invalid type, only enum types allowed.");
            }

            var xList = new List<EnumValue>();
            IEnumerable<object> xQry;
            if (aSort)
            {
                xQry = from x in Enum.GetValues(enumType).OfType<object>()
                       orderby x.ToString()
                       select x;
            }
            else
            {
                xQry = from x in Enum.GetValues(enumType).OfType<object>()
                       select x;
            }
            foreach (var x in xQry)
            {
                xList.Add(new EnumValue((Enum)x));
            }

            return xList.ToArray();
        }

        public EnumValue()
        {
        }

        public EnumValue(Enum value)
        {
            Value = value;
        }

        public Enum Value { get; set; }

        public override string ToString()
        {
            if (Value != null)
            {
                return Value.GetDescription();
            }
            return base.ToString();
        }
    }
}
