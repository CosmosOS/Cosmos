using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System {
	public static class Extensions {
		public static string GetFullName(this MethodBase aMethod) {
			StringBuilder xBuilder = new StringBuilder();
			string[] xParts = aMethod.ToString().Split(' ');
			string[] xParts2 = xParts.Skip(1).ToArray();
			MethodInfo xMethodInfo = aMethod as MethodInfo;
			if (xMethodInfo != null) {
				xBuilder.Append(xMethodInfo.ReturnType.FullName);
			} else {
				ConstructorInfo xCtor = aMethod as ConstructorInfo;
				if (xCtor != null) {
					xBuilder.Append(typeof(void).FullName);
				} else {
					xBuilder.Append(xParts[0]);
				}
			}
			xBuilder.Append("  ");
			xBuilder.Append(aMethod.DeclaringType.FullName);
			xBuilder.Append(".");
			xBuilder.Append(aMethod.Name);
			xBuilder.Append("(");
			ParameterInfo[] xParams = aMethod.GetParameters();
			for (int i = 0; i < xParams.Length; i++) {
				if (xParams[i].Name == "aThis" && i == 0) {
					continue;
				}
				xBuilder.Append(xParams[i].ParameterType.FullName);
				if (i < (xParams.Length - 1)) {
					xBuilder.Append(", ");
				}
			}
			xBuilder.Append(")");
			return xBuilder.ToString();
		}

		public static string GetFullName(this FieldInfo aField) {
			return aField.FieldType.FullName + " " + aField.DeclaringType.FullName + "." + aField.Name;
		}
	}
}