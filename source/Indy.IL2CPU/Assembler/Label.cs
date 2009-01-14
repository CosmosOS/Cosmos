using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.Assembler {
	public class Label: Instruction {
		public static string GetFullName(MethodBase aMethod) {
			var xBuilder = new StringBuilder();
			string[] xParts = aMethod.ToString().Split(' ');
			string[] xParts2 = xParts.Skip(1).ToArray();
			var xMethodInfo = aMethod as MethodInfo;
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

        public static string FilterStringForIncorrectChars(string aName) {
            return DataMember.FilterStringForIncorrectChars(aName.Replace(".", "__DOT__"));
        }

		private string mName;
		public string Name {
			get { return mName; }
		}

		public static string GetLabel(object aObject) {
			Label xLabel = aObject as Label;
			if (xLabel == null)
				return "";
			return xLabel.Name;
		}

		public static string LastFullLabel {
			get;
			set;
		}							  

		public Label(string aName) {
			mName = aName;
            if (!aName.StartsWith(".")) {
                LastFullLabel = aName;
                QualifiedName = aName;
            } else {
                QualifiedName = LastFullLabel + aName;
            }
		}

        public string QualifiedName {
            get;
            private set;
        }

		public override string ToString() {
			return Name + ":";
		}

		public static string GenerateLabelName(MethodBase aMethod) {
			string xResult = DataMember.FilterStringForIncorrectChars(GetFullName(aMethod));
			if (xResult.Length > 245) {
                using (var xHash = MD5.Create()) {
                    xResult = xHash.ComputeHash(Encoding.Default.GetBytes(xResult)).Aggregate("_",
                                                                                              (r,
                                                                                               x) => r + x.ToString("X2"));
                }
			}
			return xResult;
		}

		public Label(MethodBase aMethod)
			: this(GenerateLabelName(aMethod)) {
		}

        public override bool IsComplete(Assembler aAssembler) {
            return true;
        }

        public override void UpdateAddress(Assembler aAssembler, ref ulong aAddress) {
            base.UpdateAddress(aAssembler, ref aAddress);
        }

        public override byte[] GetData(Assembler aAssembler) {
            return new byte[0];
        }
	}
}
