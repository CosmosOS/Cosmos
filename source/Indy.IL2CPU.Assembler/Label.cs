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

		public static string GetLabel(object aObject) {
			Label xLabel = aObject as Label;
			if (xLabel == null)
				return "";
			return xLabel.Name;
		}

		public static string LastFullLabel {
			get;
			private set;
		}

		public Label(string aName) {
			mName = aName;
			if (!aName.StartsWith(".")) {
				LastFullLabel = aName;
			}
		}

		public override string ToString() {
			return Name + ":";
		}

		public Label(string aType, params string[] aParamTypes)
			: this(Init(aType, typeof(void).FullName, ".ctor", aParamTypes)) {
		}

		public static string GenerateLabelName(MethodReference aMethod) {
			var xParams = new List<string>(aMethod.Parameters.Count);
			foreach (ParameterDefinition xParam in aMethod.Parameters) {
				//TODO: Is fullname just the name, or type too? IF just name, overloads could exist wtih same names but diff types...
				xParams.Add(xParam.ParameterType.FullName);
			}
			return Init(aMethod.DeclaringType.FullName, aMethod.ReturnType.ReturnType.FullName, aMethod.Name, xParams.ToArray());
		}

		public Label(MethodReference aMethod)
			: this(GenerateLabelName(aMethod)) {
		}

		public Label(string aType, string aMethodName, params string[] aParamTypes)
			: this(Init(aType, typeof(void).FullName, aMethodName, aParamTypes)) {
		}

		public Label(string aType, string aReturnType, string aMethodName, params string[] aParamTypes)
			: this(Init(aType, aReturnType, aMethodName, aParamTypes)) {
		}

		protected static string Init(string aType, string aReturnType, string aMethodName, params string[] aParamTypes) {
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
			string xResult = DataMember.FilterStringForIncorrectChars(xSB.ToString());
			if (xResult.Length > 245) {
				xResult = mHash.ComputeHash(Encoding.Default.GetBytes(xResult)).Aggregate("_", (r, x) => r + x.ToString("X2"));
			}
			return xResult;
		}
	}
}
