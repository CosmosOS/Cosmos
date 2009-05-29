using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common
{
	public abstract class PropertiesBase
	{

		private Dictionary<String, String> propertiesTable;

		public PropertiesBase()
		{
			propertiesTable = new Dictionary<String, String>();
		}

		public void Reset()
		{
			propertiesTable.Clear();
		}

		public Dictionary<String, String> GetProperties()
		{
			Dictionary<string, string> clonedTable = new Dictionary<String, String>();

			foreach (KeyValuePair<string, string> pair in this.propertiesTable)
			{ clonedTable.Add(pair.Key, pair.Value); }

			return clonedTable;
		}

		public void SetProperty(String name, String value)
		{
			if (this.propertiesTable.ContainsKey(name) == false)
			{ this.propertiesTable.Add(name, value); }
			else
			{ this.propertiesTable[name] = value; }
		}

		public void SetProperty(String name, Object value)
		{ this.SetProperty(name, value.ToString()); }

		protected String GetProperty(String name)
		{ return this.GetProperty(name, String.Empty); }

		protected T GetProperty<T>(String name, T @default)
		{
			T value = @default;
			if (this.propertiesTable.ContainsKey(name) == true)
			{
				String stringValue = this.propertiesTable[name];
				Type valueType = typeof(T);
				String valueTypeName = valueType.Name;

				if (valueType.IsEnum == true)
				{
					value = EnumValue.Parse(stringValue, @default);
				}else{
					if (valueTypeName == "String"){
						value = (T)((Object)stringValue);
					}else if ((valueTypeName == "Int16") || (valueTypeName == "Short")){
						Int16 newValue;
						if (Int16.TryParse(stringValue, out newValue) == true)
						{ value = (T)((Object)newValue); }

					}else if ((valueTypeName == "Int32") || (valueTypeName == "Integer")){
						Int32 newValue;
						if (Int32.TryParse(stringValue, out newValue) == true)
						{ value = (T)((Object)newValue); }

					}else if ((valueTypeName == "Int64") || (valueTypeName == "Long")){
						Int32 newValue;
						if (Int32.TryParse(stringValue, out newValue) == true)
						{ value = (T)((Object)newValue); }

					}else if (valueTypeName == "Boolean"){
						Boolean newValue;
						if (Boolean.TryParse(stringValue, out newValue) == true)
						{ value = (T)((Object)newValue); }

					}if ((valueTypeName == "UInt16") || (valueTypeName == "UShort")){
						UInt16 newValue;
						if (UInt16.TryParse(stringValue, out newValue) == true)
						{ value = (T)((Object)newValue); }

					}else if ((valueTypeName == "UInt32") || (valueTypeName == "UInteger")){
						UInt32 newValue;
						if (UInt32.TryParse(stringValue, out newValue) == true)
						{ value = (T)((Object)newValue); }

					}else if ((valueTypeName == "UInt64") || (valueTypeName == "ULong")){
						UInt64 newValue;
						if (UInt64.TryParse(stringValue, out newValue) == true)
						{ value = (T)((Object)newValue); }

					}else{
							//why does it keep flowing to this line??!?!?!?
							//throw new ArgumentException("Unsupported value type.", "T");
					}
				}
			}

			return value;
		}

	}
}
