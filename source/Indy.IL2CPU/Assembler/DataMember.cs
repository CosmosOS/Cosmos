using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.Assembler {
	public class DataMember: IComparable<DataMember> {
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

		public DataMember(string aName, string aDataType, string aDefaultValue) {
			Name = aName;
			DataType = aDataType;
			DefaultValue = aDefaultValue;
		}

        public byte[] RawDefaultValue {
            get;
            set;
        }

        public DataMember(string aName, byte[] aDefaultValue) {
            Name = aName;
            RawDefaultValue = aDefaultValue;
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
			return Name + " " + DataType + " " + DefaultValue;
		}

		public int CompareTo(DataMember other) {
			return String.Compare(Name, other.Name);
		}
	}
}
