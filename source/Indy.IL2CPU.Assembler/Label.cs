using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.Assembler {
	public class Label: Instruction {
		private static MD5 mHash = MD5.Create();
		private string mName;
		public string Name {
			get {
				return mName;
			}
		}

		public Label(string aName) {
			SetName(aName);
		}

		public override string ToString() {
			return Name + ":";
		}

		public Label(string aType, params string[] aParamTypes) {
			SetName(Init(aType, typeof(void).FullName, ".ctor", aParamTypes));

		}

		private void SetName(string aName) {
			if (aName.StartsWith(".")) {
				aName = "." + DataMember.FilterStringForIncorrectChars(aName.Substring(1));
			} else {
				aName = DataMember.FilterStringForIncorrectChars(aName);
			}
			if (aName.Length > 245) {
				mName = mHash.ComputeHash(Encoding.Default.GetBytes(aName)).Aggregate("_", (r, x) => r + x.ToString("X2"));
			} else {
				mName = aName;
			}
		}

		public Label(MethodReference aMethod) {
			var xParams = new List<string>(aMethod.Parameters.Count);
			foreach (ParameterDefinition xParam in aMethod.Parameters) {
				//TODO: Is fullname just the name, or type too? IF just name, overloads could exist wtih same names but diff types...
				xParams.Add(xParam.ParameterType.FullName);
			}
			SetName(Init(aMethod.DeclaringType.FullName, aMethod.ReturnType.ReturnType.FullName, aMethod.Name, xParams.ToArray()));
		}

		public Label(string aType, string aMethodName, params string[] aParamTypes) {
			SetName(Init(aType, typeof(void).FullName, aMethodName, aParamTypes));
		}

		public Label(string aType, string aReturnType, string aMethodName, params string[] aParamTypes) {
			SetName(Init(aType, aReturnType, aMethodName, aParamTypes));
		}

		protected string Init(string aType, string aReturnType, string aMethodName, params string[] aParamTypes) {
			StringBuilder xSB = new StringBuilder();
			xSB.Append(aReturnType);
			xSB.Append("___");
			xSB.Append(aType);
			xSB.Append("_");
			xSB.Append(aMethodName);
			xSB.Append("__");
			foreach (string s in aParamTypes) {
				xSB.Append("_");
				xSB.Append(s);
				xSB.Append("_");
			}
			xSB.Append("__");
			return xSB.ToString().Replace('.', '_').Replace('+', '_').Replace('*', '_').Replace('[', '_').Replace(']', '_').Replace('&', '_');
		}
	}
}
