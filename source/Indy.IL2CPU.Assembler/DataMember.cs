using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.Assembler {
	public class DataMember: IComparable<DataMember> {
		public const string IllegalIdentifierChars = "&.,+$<>{}-`\'/\\ ()[]*!=";
		public static string GetStaticFieldName(FieldInfo aField) {
			return FilterStringForIncorrectChars("static_field__" + aField.DeclaringType.FullName + "." + aField.Name);
		}

		public static string FilterStringForIncorrectChars(string aName) {
			string xTempResult = aName;
			foreach (char c in IllegalIdentifierChars) {
				xTempResult = xTempResult.Replace(c, '_');
			}
			return xTempResult;
		}

		public DataMember(string aName, string aDataType, string aDefaultValue) {
			Name = aName;
			DataType = aDataType;
			DefaultValue = aDefaultValue;
		}

		public string Name {
			get;
			private set;
		}

		public readonly string DataType;

		public string DefaultValue {
			get;
			private set;
		}

		public override string ToString() {
			return Name + " " + DataType + " " + DefaultValue;
		}

		public int CompareTo(DataMember other) {
			return String.Compare(Name, other.Name);
		}
	}
}
