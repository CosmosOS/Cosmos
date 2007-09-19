using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.Assembler {
	public class DataMember {
		public static string GetStaticFieldName(FieldDefinition aField) {
			return ("static_field__" + aField.DeclaringType.FullName + "_" + aField.Name).Replace('.', '_').Replace('+', '_');
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
