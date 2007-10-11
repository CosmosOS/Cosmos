using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Instruction=Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU.IL {
	public abstract class InitVmtImplementationOp: Op {
		public delegate int GetMethodIdentifierEventHandler(MethodDefinition aMethod);
		public InitVmtImplementationOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}

		public static string GetFullName(MethodReference aSelf) {
			StringBuilder sb = new StringBuilder(aSelf.ReturnType.ReturnType.FullName + " " + aSelf.DeclaringType.FullName + "." + aSelf.Name);
			sb.Append("(");
			if (aSelf.Parameters.Count > 0) {
				foreach (ParameterDefinition xParam in aSelf.Parameters) {
					sb.Append(xParam.ParameterType.FullName);
					sb.Append(",");
				}
			}
			return sb.ToString().TrimEnd(',') + ")";
		}

		private List<TypeDefinition> mTypes;
		public MethodDefinition LoadTypeTableRef;
		public MethodDefinition SetTypeInfoRef;
		public MethodDefinition SetMethodInfoRef;
		public IList<MethodDefinition> Methods;
		public event GetMethodIdentifierEventHandler GetMethodIdentifier;

		public List<TypeDefinition> Types {
			get {
				return mTypes;
			}
			set {
				mTypes = value;
			}
		}

		protected abstract void Pushd(string aValue);
		protected abstract void Call(MethodDefinition aMethod);

		public override void DoAssemble() {
			Pushd("0" + mTypes.Count.ToString("X") + "h");
			Call(LoadTypeTableRef);
			for (int i = 0; i < mTypes.Count; i++) {
				TypeDefinition xType = mTypes[i];
				List<MethodDefinition> xEmittedMethods = new List<MethodDefinition>();
				foreach (MethodDefinition xMethod in xType.Methods) {
					if (Methods.Contains(xMethod) && !xMethod.IsAbstract) {
						xEmittedMethods.Add(xMethod);
					}
				}
				foreach (MethodDefinition xCtor in xType.Constructors) {
					if (Methods.Contains(xCtor) && !xCtor.IsAbstract) {
						xEmittedMethods.Add(xCtor);
					}
				}
				Pushd("0" + i.ToString("X") + "h");
				int? xBaseIndex = null;
				if (xType.BaseType == null) {
					for (int t = 0; t < mTypes.Count; t++) {
						if (mTypes[t].BaseType == null && mTypes[t].FullName == xType.FullName) {
							xBaseIndex = t;
							break;
						}
					}
				} else {
					for (int t = 0; t < mTypes.Count; t++) {
						if (mTypes[t].BaseType == null) {
							continue;
						}
						if (mTypes[t].BaseType.FullName == xType.BaseType.FullName && mTypes[t].FullName == xType.FullName) {
							xBaseIndex = t;
							break;
						}
					}
				}
				if (xBaseIndex == null) {
					throw new Exception("Base type not found!");
				}
				Pushd("0" + xBaseIndex.Value.ToString("X") + "h");
				Pushd("0" + xEmittedMethods.Count.ToString("X") + "h");
				string xDataValue = Encoding.ASCII.GetBytes(mTypes[i].FullName + ", " + mTypes[i].Module.Assembly.Name.FullName).Aggregate("", (b, x) => b + x + ",") + "0";
				string xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(mTypes[i].FullName);
				mAssembler.DataMembers.Add(new DataMember(xDataName, "db", xDataValue));
				Pushd(xDataName);
				Call(SetTypeInfoRef);
				for (int j = 0; j < xEmittedMethods.Count; j++) {
					MethodDefinition xMethod = xEmittedMethods[j];
					Pushd("0" + i.ToString("X") + "h");
					Pushd("0" + j.ToString("X") + "h");
					TypeReference[] xMethodParams = new TypeReference[xMethod.Parameters.Count];
					for (int k = 0; k < xMethod.Parameters.Count; k++) {
						xMethodParams[k] = xMethod.Parameters[k].ParameterType;
					}
					Pushd("0" + GetMethodIdentifier(xMethod).ToString("X") + "h");
					Pushd(new Label(xMethod).Name);
					xDataValue = Encoding.ASCII.GetBytes(GetFullName(xMethod)).Aggregate("", (b, x) => b + x + ",") + "0";
					xDataName = "____SYSTEM____METHOD___" + DataMember.FilterStringForIncorrectChars(GetFullName(xMethod));
					mAssembler.DataMembers.Add(new DataMember(xDataName, "db", xDataValue));
					Pushd(xDataName);
					Call(SetMethodInfoRef);
				}
			}
		}
	}
}