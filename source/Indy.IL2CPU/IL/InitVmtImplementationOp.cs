using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using System.Xml;
using System.Reflection;

namespace Indy.IL2CPU.IL {
    public class MethodBaseComparer : IComparer<MethodBase>
    {
        #region IComparer<MethodBase> Members
        public int Compare(MethodBase x, MethodBase y)
        {
            return x.GetFullName().CompareTo(y.GetFullName());
        }
        #endregion
    }
	public abstract class InitVmtImplementationOp: Op {
		public delegate int GetMethodIdentifierEventHandler(MethodBase aMethod);
		public InitVmtImplementationOp(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
        private static readonly bool mDebugMode = false;
        static InitVmtImplementationOp() {
            // flag for conditional debug code for Matthijs, please leave it.
            mDebugMode = Environment.MachineName.Equals("laptop-matthijs", StringComparison.InvariantCultureIgnoreCase);
        }
		private IList<Type> mTypes;
		public MethodBase LoadTypeTableRef;
		public MethodBase SetTypeInfoRef;				   
		public MethodBase SetMethodInfoRef;
		public FieldInfo TypesFieldRef;
		public int VTableEntrySize; 
		public uint ArrayTypeId;
		public IList<MethodBase> Methods;
		public event GetMethodIdentifierEventHandler GetMethodIdentifier;

		public IList<Type> Types {
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
            XmlWriter xDebug=null;
            if (mDebugMode)
            {
                xDebug = XmlWriter.Create(@"d:\vtables.xml");
                xDebug.WriteStartDocument();
                xDebug.WriteStartElement("VTables");
                xDebug.WriteStartElement("AllMethods");
                for (int i = 0; i < Methods.Count; i++)
                {
                    MethodBase xTheMethod = Methods[i];
                    xDebug.WriteStartElement("Method");
                    xDebug.WriteAttributeString("Id", GetMethodIdentifier(xTheMethod).ToString("X"));
                    xDebug.WriteAttributeString("Name", xTheMethod.GetFullName());
                    xDebug.WriteEndElement();
                }
                xDebug.WriteEndElement();
            }
			string xTheName = DataMember.GetStaticFieldName(TypesFieldRef);
			DataMember xDataMember = (from item in Assembler.DataMembers
									  where item.Name == xTheName
									  select item).FirstOrDefault();
			if (xDataMember != null) {
				Assembler.DataMembers.Remove((from item in Assembler.DataMembers
											  where item == xDataMember
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
			Assembler.DataMembers.Add(new DataMember(xTheName + "__Contents", "db", xDataByteArray.ToString().TrimEnd(',')));
			Assembler.DataMembers.Add(new DataMember(xTheName, "dd", xTheName + "__Contents"));
			Pushd("0" + mTypes.Count.ToString("X") + "h");
			Call(LoadTypeTableRef);
			for (int i = 0; i < mTypes.Count; i++) {
                if (mDebugMode)
                {
                    xDebug.WriteStartElement("Type");
                    xDebug.WriteAttributeString("Id", i.ToString("X"));
                    xDebug.WriteAttributeString("Name", mTypes[i].FullName);
                }
                    try
                    {
                        Type xType = mTypes[i];
                        if (xType.FullName == "MatthijsTest.TestImpl") { System.Diagnostics.Debugger.Break(); }
                        // value contains true if the method is an interface method definition
                        SortedList<MethodBase, bool> xEmittedMethods = new SortedList<MethodBase, bool>(new MethodBaseComparer());
                        foreach (MethodBase xMethod in xType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                        {
                            if (Methods.Contains(xMethod) && !xMethod.IsAbstract)
                            {
                                xEmittedMethods.Add(xMethod, false);
                            }
                        }
                        foreach (MethodBase xCtor in xType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                        {
                            if (Methods.Contains(xCtor) && !xCtor.IsAbstract)
                            {
                                xEmittedMethods.Add(xCtor, false);
                            }
                        }
                        foreach (var xIntf in xType.GetInterfaces())
                        {
                            foreach (var xMethodIntf in xIntf.GetMethods())
                            {
                                var xActualMethod = xType.GetMethod(xIntf.FullName + "." + xMethodIntf.Name,
                                                                    (from xParam in xMethodIntf.GetParameters()
                                                                     select xParam.ParameterType).ToArray());

                                if (xActualMethod == null)
                                {
                                    // get private implemenation
                                    xActualMethod = xType.GetMethod(xMethodIntf.Name,
                                                                    (from xParam in xMethodIntf.GetParameters()
                                                                     select xParam.ParameterType).ToArray());
                                } if (xActualMethod == null)
                                {
                                    try
                                    {
                                        var xMap = xType.GetInterfaceMap(xIntf);
                                        for (int k = 0; k < xMap.InterfaceMethods.Length; k++)
                                        {
                                            if (xMap.InterfaceMethods[k] == xMethodIntf)
                                            {
                                                xActualMethod = xMap.TargetMethods[k];
                                                break;
                                            }
                                        }
                                    }
                                    catch { }
                                }
                                if (Methods.Contains(xMethodIntf))
                                {
                                    if (!xEmittedMethods.ContainsKey(xMethodIntf))
                                    {
                                        xEmittedMethods.Add(xMethodIntf,
                                                            true);
                                    }
                                }

                            }
                        }
                        if (!xType.IsInterface)
                        {
                            Pushd("0" + i.ToString("X") + "h");
                        }
                        int? xBaseIndex = null;
                        if (xType.BaseType == null)
                        {
                            xBaseIndex = i;
                        }
                        else
                        {
                            for (int t = 0; t < mTypes.Count; t++)
                            {
                                if (mTypes[t].ToString() == xType.BaseType.ToString())
                                {
                                    xBaseIndex = t;
                                    break;
                                }
                            }
                        }
                        if (xBaseIndex == null)
                        {
                            throw new Exception("Base type not found!");
                        }
                        if (mDebugMode)
                        {
                            xDebug.WriteAttributeString("BaseId", xBaseIndex.Value.ToString("X"));
                        }
                        for (int x = xEmittedMethods.Count - 1; x >= 0; x--) {
                            if (!Methods.Contains(xEmittedMethods.Keys[x])) { xEmittedMethods.RemoveAt(x); }
                        }
                        if (!xType.IsInterface)
                        {

                            Pushd("0" + xBaseIndex.Value.ToString("X") + "h");
                            //Pushd("0" + xEmittedMethods.Count.ToString("X") + "h");
                            xDataByteArray.Remove(0, xDataByteArray.Length);
                            xDataByteArray.Append(BitConverter.GetBytes(ArrayTypeId).Aggregate("", (r, b) => r + b + ","));
                            xDataByteArray.Append(BitConverter.GetBytes(0x80000002 /* EmbeddedArray */).Aggregate("", (r, b) => r + b + ","));
                            xDataByteArray.Append(BitConverter.GetBytes(xEmittedMethods.Count).Aggregate("", (r, b) => r + b + ","));
                            xDataByteArray.Append("0,0,0,0,");
                            for (uint j = 0; j < xEmittedMethods.Count; j++)
                            {
                                xDataByteArray.Append("0,0,0,0,");
                            }
                            string xDataValue = xDataByteArray.ToString();
                            string xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(mTypes[i].FullName) + "__MethodIndexesArray";
                            Assembler.DataMembers.Add(new DataMember(xDataName, "db", xDataValue.TrimEnd(',')));
                            Pushd(xDataName);
                            xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(mTypes[i].FullName) + "__MethodAddressesArray";
                            Assembler.DataMembers.Add(new DataMember(xDataName, "db", xDataValue.TrimEnd(',')));
                            Pushd(xDataName);
                            xDataByteArray.Remove(0, xDataByteArray.Length);
                            xDataByteArray.Append(BitConverter.GetBytes(ArrayTypeId).Aggregate("", (r, b) => r + b + ","));
                            xDataByteArray.Append(BitConverter.GetBytes(0x80000002 /* EmbeddedArray */).Aggregate("", (r, b) => r + b + ","));
                            xDataByteArray.Append(BitConverter.GetBytes((mTypes[i].FullName + ", " + mTypes[i].Module.Assembly.GetName().FullName).Length).Aggregate("", (r, b) => r + b + ","));
                            xDataByteArray.Append(BitConverter.GetBytes((uint)2).Aggregate("", (r, b) => r + b + ","));
                            xDataByteArray.Append(Encoding.Unicode.GetBytes(mTypes[i].FullName + ", " + mTypes[i].Module.Assembly.GetName().FullName).Aggregate("", (b, x) => b + x + ",") + "0");
                            xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(mTypes[i].FullName);
                            mAssembler.DataMembers.Add(new DataMember(xDataName, "db", xDataByteArray.ToString()));
                            Pushd(xDataName);
                            //Pushd("0");
                            Call(SetTypeInfoRef);
                        }
                        for (int j = 0; j < xEmittedMethods.Count; j++)
                        {
                            MethodBase xMethod = xEmittedMethods.Keys[j];
                            var xMethodId = GetMethodIdentifier(xMethod);
                            if (mDebugMode)
                            {
                                xDebug.WriteStartElement("Method");
                                xDebug.WriteAttributeString("Id", xMethodId.ToString("X"));
                                xDebug.WriteAttributeString("Name", xMethod.GetFullName());
                                xDebug.WriteEndElement();
                            }
                            if (!xType.IsInterface)
                            {
                                if (xEmittedMethods.Values[j])
                                {
                                    var xNewMethod = xType.GetMethod(xMethod.DeclaringType.FullName + "." + xMethod.Name,
                                                                        (from xParam in xMethod.GetParameters()
                                                                         select xParam.ParameterType).ToArray());

                                    if (xNewMethod == null)
                                    {
                                        // get private implemenation
                                        xNewMethod = xType.GetMethod(xMethod.Name,
                                                                        (from xParam in xMethod.GetParameters()
                                                                         select xParam.ParameterType).ToArray());
                                    }
                                    if (xNewMethod == null)
                                    {
                                        try
                                        {
                                            var xMap = xType.GetInterfaceMap(xMethod.DeclaringType);
                                            for (int k = 0; k < xMap.InterfaceMethods.Length; k++)
                                            {
                                                if (xMap.InterfaceMethods[k] == xMethod)
                                                {
                                                    xNewMethod = xMap.TargetMethods[k];
                                                    break;
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                    if (xNewMethod == null) { System.Diagnostics.Debugger.Break(); }
                                    xMethod = xNewMethod;
                                }
                                Pushd("0" + i.ToString("X") + "h");
                                Pushd("0" + j.ToString("X") + "h");

                                Pushd("0" + xMethodId.ToString("X") + "h");
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
                    finally
                    {
                        if (mDebugMode)
                        {
                            xDebug.WriteEndElement();
                        }
                    }
			}
            if (mDebugMode)
            {
                xDebug.Close();
            }

		}
	}
}
