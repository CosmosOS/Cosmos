using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.Assembler {
	public class DataMember {
		public static string GetStaticFieldName(FieldDefinition aField) {
			return FilterStringForIncorrectChars("static_field__" + aField.DeclaringType.FullName + "_" + aField.Name);
		}

		public static string FilterStringForIncorrectChars(string aName) {
			string xTempResult = aName;
			foreach (char c in new char[] { '.', ',', '+', '$', '<', '>', '{', '}', '-', '`', '\'', '/', '\\', ' ', '(', ')' }) {
				xTempResult = xTempResult.Replace(c, '_');
			}
			return xTempResult;
		}

		public DataMember(string aName, string aDataType, string aDefaultValue) {
			Name = aName;
			DataType = aDataType;
			DefaultValue = aDefaultValue;
		}

		public readonly string Name;
		public readonly string DataType;
		public readonly string DefaultValue;
		public override string ToString() {
			return Name + " " + DataType + " " + DefaultValue;
		}
	}
}
