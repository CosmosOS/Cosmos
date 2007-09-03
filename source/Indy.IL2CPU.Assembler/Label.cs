using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.Assembler {
	public class Label: Instruction {
		public readonly string Name;

        public Label(string aName) {
			Name = aName;
		}

        public override string ToString() {
			return Name + ":";
		}

        public Label(string aType, params string[] aParamTypes) {
            Name = Init(aType, typeof(void).FullName, ".ctor", aParamTypes);
        }

        public Label(MethodReference aMethod) {
            var xParams = new List<string>(aMethod.Parameters.Count);
            foreach (ParameterDefinition xParam in aMethod.Parameters) {
                //TODO: Is fullname just the name, or type too? IF just name, overloads could exist wtih same names but diff types...
                xParams.Add(xParam.ParameterType.FullName);
            }
            Name = Init(aMethod.DeclaringType.FullName, aMethod.ReturnType.ReturnType.FullName, aMethod.Name, xParams.ToArray());
        }

        public Label(string aType, string aMethodName, params string[] aParamTypes) {
            Name = Init(aType, typeof(void).FullName, aMethodName, aParamTypes);
        }

        public Label(string aType, string aReturnType, string aMethodName, params string[] aParamTypes) {
            Name = Init(aType, aReturnType, aMethodName, aParamTypes);
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
            xSB.Replace('.', '_');
            xSB.Replace('+', '_');
        	xSB.Replace('*', '_');
        	xSB.Replace('[', '_');
			xSB.Replace(']', '_');
            return xSB.ToString();
        }

    }
}
