//#define MTW_DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using System.Xml;
using System.Reflection;

namespace Indy.IL2CPU.IL {
	public abstract class InitVmtImplementationOp: Op {
		public delegate int GetMethodIdentifierEventHandler(MethodBase aMethod);
		public InitVmtImplementationOp(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}

		private List<Type> mTypes;
		public MethodBase LoadTypeTableRef;
		public MethodBase SetTypeInfoRef;				   
		public MethodBase SetMethodInfoRef;
		public FieldInfo TypesFieldRef;
		public int VTableEntrySize;
		public uint ArrayTypeId;
		public IList<MethodBase> Methods;
		public event GetMethodIdentifierEventHandler GetMethodIdentifier;

		public List<Type> Types {
			get {
				return mTypes;
			}
			set {
				mTypes = value;
			}
		}

		protected abstract void Pushd(string aValue);
		protected abstract void Call(MethodBase aMethod);

		public override void DoAssemble() {
#if MTW_DEBUG
			using (XmlWriter xDebug = XmlWriter.Create(@"d:\vtables.xml")) {
				xDebug.WriteStartDocument();
				xDebug.WriteStartElement("VTables");
				xDebug.WriteStartElement("AllMethods");
				for(int i = 0; i < Methods.Count; i++){
					MethodBase xTheMethod = Methods[i];
					xDebug.WriteStartElement("Method");
					xDebug.WriteAttributeString("Id", GetMethodIdentifier(xTheMethod).ToString("X"));
					xDebug.WriteAttributeString("Name", xTheMethod.GetFullName());
					xDebug.WriteEndElement();
				}
				xDebug.WriteEndElement();
#endif
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
#if MTW_DEBUG
					xDebug.WriteStartElement("Type");
					xDebug.WriteAttributeString("Id", i.ToString("X"));
#endif
				Type xType = mTypes[i];
				List<MethodBase> xEmittedMethods = new List<MethodBase>();
				foreach (MethodBase xMethod in xType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
					if (Methods.Contains(xMethod) && !xMethod.IsAbstract) {
						xEmittedMethods.Add(xMethod);
					}
				}
				foreach (MethodBase xCtor in xType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
					if (Methods.Contains(xCtor) && !xCtor.IsAbstract) {
						xEmittedMethods.Add(xCtor);
					}
				}
				Pushd("0" + i.ToString("X") + "h");
				int? xBaseIndex = null;
				if (xType.BaseType == null) {
					xBaseIndex = i;
				} else {
					for (int t = 0; t < mTypes.Count; t++) {
						if (mTypes[t].ToString() == xType.BaseType.ToString()) {
							xBaseIndex = t;
							break;
						}
					}
				}
				if (xBaseIndex == null) {
					throw new Exception("Base type not found!");
				}
#if MTW_DEBUG
					xDebug.WriteAttributeString("BaseId", xBaseIndex.Value.ToString("X"));
					xDebug.WriteAttributeString("Name", mTypes[i].FullName);
#endif
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
				xDataByteArray.Append(BitConverter.GetBytes((mTypes[i].FullName + ", " + mTypes[i].Module.Assembly.GetName().FullName).Length).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(BitConverter.GetBytes((uint)2).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(Encoding.Unicode.GetBytes(mTypes[i].FullName + ", " + mTypes[i].Module.Assembly.GetName().FullName).Aggregate("", (b, x) => b + x + ",") + "0");
				xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(mTypes[i].FullName);
				mAssembler.DataMembers.Add(new KeyValuePair<string, DataMember>(Assembler.CurrentGroup, new DataMember(xDataName, "db", xDataByteArray.ToString())));
				Pushd(xDataName);
				//Pushd("0");
				Call(SetTypeInfoRef);
				for (int j = 0; j < xEmittedMethods.Count; j++) {
					MethodBase xMethod = xEmittedMethods[j];
#if MTW_DEBUG
						xDebug.WriteStartElement("Method");
						xDebug.WriteAttributeString("Id", GetMethodIdentifier(xMethod).ToString("X"));
						xDebug.WriteAttributeString("Name", xMethod.GetFullName());
						xDebug.WriteEndElement();
#endif
					Pushd("0" + i.ToString("X") + "h");
					Pushd("0" + j.ToString("X") + "h");
					ParameterInfo[] xParams = xMethod.GetParameters();
					Type[] xMethodParams = new Type[xParams.Length];
					for (int k = 0; k < xParams.Length; k++) {
						xMethodParams[k] = xParams[k].ParameterType;
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
#if MTW_DEBUG
					xDebug.WriteEndElement();
#endif
			}
#if MTW_DEBUG
			}
#endif
		}
	}
}
