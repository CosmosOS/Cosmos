using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Instruction = Mono.Cecil.Cil.Instruction;

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
		public FieldDefinition TypesFieldRef;
		public int VTableEntrySize;
		public uint ArrayTypeId;
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
			string xTheName = DataMember.GetStaticFieldName(TypesFieldRef);
			DataMember xDataMember = (from item in Assembler.DataMembers
									  where item.Value.Name == xTheName
									  select item.Value).FirstOrDefault();
			if (xDataMember != null) {
				Assembler.DataMembers.Remove((from item in Assembler.DataMembers
											  where item.Value == xDataMember
											  select item).First());
			}
			StringBuilder xDataByteArray = new StringBuilder();
			xDataByteArray.Append(BitConverter.GetBytes(ArrayTypeId).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append(BitConverter.GetBytes((uint)0x80000002).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append(BitConverter.GetBytes(mTypes.Count).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append(BitConverter.GetBytes(VTableEntrySize).Aggregate("", (r, b) => r + b + ","));
			for (uint i = 0; i < mTypes.Count; i++) {
				for (int j = 0; j < VTableEntrySize; j++) {
					xDataByteArray.Append("0,");
				}
			}
			Assembler.DataMembers.Add(new KeyValuePair<string, DataMember>(Assembler.CurrentGroup, new DataMember(xTheName + "__Contents", "db", xDataByteArray.ToString().TrimEnd(','))));
			Assembler.DataMembers.Add(new KeyValuePair<string, DataMember>(Assembler.CurrentGroup, new DataMember(xTheName, "dd", xTheName + "__Contents")));
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
				//Pushd("0" + xEmittedMethods.Count.ToString("X") + "h");
				xDataByteArray.Remove(0, xDataByteArray.Length);
				xDataByteArray.Append(BitConverter.GetBytes(ArrayTypeId).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(BitConverter.GetBytes(0x80000002 /* EmbeddedArray */).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(BitConverter.GetBytes(xEmittedMethods.Count).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append("0,0,0,0,");
				for (uint j = 0; j < xEmittedMethods.Count; j++) {
					xDataByteArray.Append("0,0,0,0,");
				}
				string xDataValue = xDataByteArray.ToString();
				string xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(mTypes[i].FullName) + "__MethodIndexesArray";
				Assembler.DataMembers.Add(new KeyValuePair<string, DataMember>(Assembler.CurrentGroup, new DataMember(xDataName, "db", xDataValue.TrimEnd(','))));
				Pushd(xDataName);
				xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(mTypes[i].FullName) + "__MethodAddressesArray";
				Assembler.DataMembers.Add(new KeyValuePair<string, DataMember>(Assembler.CurrentGroup, new DataMember(xDataName, "db", xDataValue.TrimEnd(','))));
				Pushd(xDataName);
				xDataByteArray.Remove(0, xDataByteArray.Length);
				xDataByteArray.Append(BitConverter.GetBytes(ArrayTypeId).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(BitConverter.GetBytes(0x80000002 /* EmbeddedArray */).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(BitConverter.GetBytes((mTypes[i].FullName + ", " + mTypes[i].Module.Assembly.Name.FullName).Length).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(BitConverter.GetBytes((uint)2).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(Encoding.Unicode.GetBytes(mTypes[i].FullName + ", " + mTypes[i].Module.Assembly.Name.FullName).Aggregate("", (b, x) => b + x + ",") + "0");
				xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(mTypes[i].FullName);
				mAssembler.DataMembers.Add(new KeyValuePair<string, DataMember>(Assembler.CurrentGroup, new DataMember(xDataName, "db", xDataByteArray.ToString())));
				Pushd(xDataName);
				//Pushd("0");
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
					Pushd(Label.GenerateLabelName(xMethod));
					//xDataValue = Encoding.ASCII.GetBytes(GetFullName(xMethod)).Aggregate("", (b, x) => b + x + ",") + "0";
					//xDataName = "____SYSTEM____METHOD___" + DataMember.FilterStringForIncorrectChars(GetFullName(xMethod));
					//mAssembler.DataMembers.Add(new DataMember(xDataName, "db", xDataValue));
					//Pushd(xDataName);
					Pushd("0");
					Call(SetMethodInfoRef);
				}
			}
		}
	}
}