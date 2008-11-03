using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.Assembler {
	public class DataMember: BaseAssemblerElement, IComparable<DataMember> {
		public const string IllegalIdentifierChars = "&.,+$<>{}-`\'/\\ ()[]*!=";
		public static string GetStaticFieldName(FieldInfo aField) {
			return FilterStringForIncorrectChars("static_field__" + aField.DeclaringType.FullName + "." + aField.Name);
		}

		public static string FilterStringForIncorrectChars(string aName) {
			string xTempResult = aName;
			foreach (char c in IllegalIdentifierChars) {
				xTempResult = xTempResult.Replace(c, '_');
			}
			return xTempResult;
		}

        [Obsolete]
		public DataMember(string aName, string aDataType, string aDefaultValue) {
			Name = aName;
			DataType = aDataType;
			DefaultValue = aDefaultValue;
		}

        public byte[] RawDefaultValue {
            get;
            set;
        }

        private object[] UntypedDefaultValue;

        public DataMember(string aName, params object[] aDefaultValue) {
            Name = aName;
            UntypedDefaultValue = aDefaultValue;
        }

        public DataMember(string aName, byte[] aDefaultValue) {
            Name = aName;
            RawDefaultValue = aDefaultValue;
        }

        public DataMember(string aName, short[] aDefaultValue) {
            Name = aName;
            RawDefaultValue = new byte[aDefaultValue.Length * 2];
            for (int i = 0; i < aDefaultValue.Length; i++) {
                Array.Copy(BitConverter.GetBytes(aDefaultValue[i]), 0,
                            RawDefaultValue, i * 2, 2);
            }
        }

        public DataMember(string aName, params ushort[] aDefaultValue) {
            Name = aName;
            RawDefaultValue = new byte[aDefaultValue.Length * 2];
            for (int i = 0; i < aDefaultValue.Length; i++) {
                Array.Copy(BitConverter.GetBytes(aDefaultValue[i]), 0,
                            RawDefaultValue, i * 2, 2);
            }
        }

        public DataMember(string aName, params uint[] aDefaultValue) {
            Name = aName;
            RawDefaultValue = new byte[aDefaultValue.Length * 4];
            for (int i = 0; i < aDefaultValue.Length; i++) {
                Array.Copy(BitConverter.GetBytes(aDefaultValue[i]), 0,
                            RawDefaultValue, i * 4, 4);
            }
        }

        public DataMember(string aName, params int[] aDefaultValue) {
            Name = aName;
            RawDefaultValue = new byte[aDefaultValue.Length * 4];
            for (int i = 0; i < aDefaultValue.Length; i++) {
                Array.Copy(BitConverter.GetBytes(aDefaultValue[i]), 0,
                            RawDefaultValue, i * 4, 4);
            }
        }

        public string Name {
			get;
			private set;
		}

		public readonly string DataType;

		public string DefaultValue {
			get;
			private set;
		}

		public override string ToString() {
            if(RawDefaultValue!=null) {
                if(RawDefaultValue.Length==0) {
                    return Name + ":";
                }
                if ((from item in RawDefaultValue
                     group item by item into i
                     select i).Count() > 1 || RawDefaultValue.Length<250) {
                    StringBuilder xSB = new StringBuilder();
                    xSB.AppendFormat("{0} db ", Name);
                    for (int i = 0; i < (RawDefaultValue.Length - 1); i++) {
                        xSB.AppendFormat("{0}, ",
                                         RawDefaultValue[i]);
                    }
                    xSB.Append(RawDefaultValue.Last());
                    return xSB.ToString();
                } else {
                    //aOutputWriter.WriteLine("TIMES 0x50000 db 0");
                    return Name + ": TIMES " + RawDefaultValue.Count() + " db " + RawDefaultValue[0];
                }
            }
            if (UntypedDefaultValue != null) {
                StringBuilder xSB = new StringBuilder();
                xSB.AppendFormat("{0} dd ", Name);
                Func<object, string> xGetTextForItem = delegate(object aItem) {
                    var xElementRef = aItem as ElementReference;
                    if (xElementRef == null) {
                        return aItem.ToString();
                    } else {
                        if (xElementRef.Offset == 0) {
                            return xElementRef.Name;
                        }
                        return xElementRef.Name + " + " + xElementRef.Offset;
                    }
                };
                for (int i = 0; i < (UntypedDefaultValue.Length - 1); i++) {
                    xSB.AppendFormat("{0}, ",
                                     xGetTextForItem(UntypedDefaultValue[i]));
                }
                xSB.Append(xGetTextForItem(UntypedDefaultValue.Last()));
                return xSB.ToString();
            }
			return Name + " " + DataType + " " + DefaultValue;
		}

		public int CompareTo(DataMember other) {
			return String.Compare(Name, other.Name);
		}


        public override ulong? ActualAddress {
            get { 
                // TODO: for now, we dont have any data alignment
                return StartAddress;
            }
        }

        public override bool DetermineSize(Assembler aAssembler, out ulong aSize) {
            if (RawDefaultValue != null) {
                aSize = (ulong)RawDefaultValue.LongLength;
                return true;
            }
            if (UntypedDefaultValue != null) {
                // TODO: what to do with 64bit target platforms? right now we only support 32bit
                aSize = (ulong)(UntypedDefaultValue.LongLength * 4);
                return true;
            }
            aSize = 0;
            return false;
        }

        public override bool IsComplete(Assembler aAssembler) {
            if (UntypedDefaultValue != null &&
                UntypedDefaultValue.LongLength>0) {
                foreach (var xReference in (from item in UntypedDefaultValue
                                            let xRef = item as ElementReference
                                            where xRef != null
                                            select xRef)) {
                    var xRef = aAssembler.TryResolveReference(xReference);
                    if (xRef == null) {
                        return false;
                    }
                    if (!xRef.IsComplete(aAssembler)) {
                        return false;
                    }
                }
            }
            return true;
        }

        public override byte[] GetData(Assembler aAssembler) {
            if (UntypedDefaultValue != null &&
                UntypedDefaultValue.LongLength > 0) {
                var xBuff = (byte[])Array.CreateInstance(typeof(byte), UntypedDefaultValue.LongLength * 4);
                for (int i = 0; i < UntypedDefaultValue.Length; i++) {
                    var xRef = UntypedDefaultValue[i] as ElementReference;
                    byte[] xTemp;
                    if (xRef != null) {
                        var xTheRef = aAssembler.TryResolveReference(xRef);
                        if (xTheRef == null) {
                            throw new Exception("Reference not found!");
                        }
                        xTemp = BitConverter.GetBytes(xTheRef.ActualAddress.Value);
                    } else {
                        if (UntypedDefaultValue[i] is int) {
                            xTemp = BitConverter.GetBytes((int)UntypedDefaultValue[i]);
                        } else {
                            if (UntypedDefaultValue[i] is uint) {
                                xTemp = BitConverter.GetBytes((uint)UntypedDefaultValue[i]);
                            } else {
                                throw new Exception("Invalid value inside UntypedDefaultValue");
                            }
                        }
                    }
                    Array.Copy(xTemp, 0, xBuff, i * 4, 4);
                }
                return xBuff;
            }
            return RawDefaultValue;
        }
    }
}
