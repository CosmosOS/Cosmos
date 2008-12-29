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
		public uint VTableEntrySize; 
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

		protected abstract void Push(uint aValue);
        protected abstract void Push(string aLabelName);
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
            var xData = new byte[16 + (mTypes.Count * VTableEntrySize)];
		    var xTemp = BitConverter.GetBytes(ArrayTypeId);
            Array.Copy(xTemp, 0, xData, 0, 4);
            xTemp = BitConverter.GetBytes(0x80000002);
            Array.Copy(xTemp, 0, xData, 4, 4);
            xTemp = BitConverter.GetBytes(mTypes.Count);
            Array.Copy(xTemp, 0, xData, 8, 4);
            xTemp = BitConverter.GetBytes(VTableEntrySize);
            Array.Copy(xTemp, 0, xData, 12, 4);
			Assembler.DataMembers.Add(new DataMember(xTheName + "__Contents", xData));
			Assembler.DataMembers.Add(new DataMember(xTheName, new ElementReference(xTheName + "__Contents")));
			Push((uint)mTypes.Count);
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
                            Push((uint)i);
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

                            Push((uint)xBaseIndex.Value);
                            //Push("0" + xEmittedMethods.Count.ToString("X") + "h");
                            xData = new byte[16 + (xEmittedMethods.Count * 4)];
                            xTemp = BitConverter.GetBytes(ArrayTypeId);
                            Array.Copy(xTemp, 0, xData, 0, 4);
                            xTemp = BitConverter.GetBytes(0x80000002); // embedded array
                            Array.Copy(xTemp, 0, xData, 4, 4);
                            xTemp = BitConverter.GetBytes(xEmittedMethods.Count); // embedded array
                            Array.Copy(xTemp, 0, xData, 8, 4);
                            xTemp = BitConverter.GetBytes(4); // embedded array
                            Array.Copy(xTemp, 0, xData, 12, 4);
                            string xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(mTypes[i].FullName) + "__MethodIndexesArray";
                            Assembler.DataMembers.Add(new DataMember(xDataName, xData));
                            Push(xDataName);
                            xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(mTypes[i].FullName) + "__MethodAddressesArray";
                            Assembler.DataMembers.Add(new DataMember(xDataName, xData));
                            Push(xDataName);
                            xData = new byte[16 + Encoding.Unicode.GetByteCount(mTypes[i].FullName + ", " + mTypes[i].Module.Assembly.GetName().FullName)];
                            xTemp = BitConverter.GetBytes(ArrayTypeId);
                            Array.Copy(xTemp, 0, xData, 0, 4);
                            xTemp = BitConverter.GetBytes(0x80000002); // embedded array
                            Array.Copy(xTemp, 0, xData, 4, 4);
                            xTemp = BitConverter.GetBytes((mTypes[i].FullName + ", " + mTypes[i].Module.Assembly.GetName().FullName).Length);
                            Array.Copy(xTemp, 0, xData, 8, 4);
                            xTemp = BitConverter.GetBytes(2); // embedded array
                            Array.Copy(xTemp, 0, xData, 12, 4);
                            xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(mTypes[i].FullName);
                            mAssembler.DataMembers.Add(new DataMember(xDataName, xData));
                            Push((uint)xEmittedMethods.Count);
                            //Push("0");
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
                                Push((uint)i);
                                Push((uint)j);

                                Push((uint)xMethodId);
                                Push(Label.GenerateLabelName(xMethod));
                                //xDataValue = Encoding.ASCII.GetBytes(GetFullName(xMethod)).Aggregate("", (b, x) => b + x + ",") + "0";
                                //xDataName = "____SYSTEM____METHOD___" + DataMember.FilterStringForIncorrectChars(GetFullName(xMethod));
                                //mAssembler.DataMembers.Add(new DataMember(xDataName, "db", xDataValue));
                                //Push(xDataName);
                                Push(0);
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
