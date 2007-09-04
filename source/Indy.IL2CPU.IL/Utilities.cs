using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.IL {
	public class Utilities {
		public static string GetUniqueConstName(FieldDefinition aField) {
			StringBuilder xSB = new StringBuilder();
			xSB.Append("const ");
			xSB.Append(aField.FieldType.FullName);
			xSB.Append(" ");
			xSB.Append(aField.Name);
			return FixupIdentifierForAsm(xSB.ToString());
		}

		public static string FixupIdentifierForAsm(string aIdentifier) {
			return aIdentifier.Replace('.', '_').Replace('+', '_').Replace('*', '_').Replace('[', '_').Replace(']', '_').Replace('&', '_');
		}
	}
}