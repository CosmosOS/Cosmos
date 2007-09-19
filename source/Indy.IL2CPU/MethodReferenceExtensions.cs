using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace System {
	public static class MethodReferenceExtensions {
		public static string GetFullName(this MethodReference aSelf) {
			StringBuilder sb = new StringBuilder(aSelf.DeclaringType.FullName + "." + aSelf.Name);
				sb.Append("(");
			if(aSelf.Parameters.Count>0) {

				foreach(ParameterDefinition xParam in aSelf.Parameters) {
					sb.Append(xParam.ParameterType.FullName);
					sb.Append(",");
				}
			}
			return sb.ToString().TrimEnd(',') + ")";
		}
	}
}
