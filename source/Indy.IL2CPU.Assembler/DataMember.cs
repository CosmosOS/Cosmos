using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
	public class DataMember {
		public DataMember(string aName, string aDataType, string aDefaultValue) {
			Assembler.Current.Add(this);
			Name = aName;
			DataType = aDataType;
			DefaultValue = aDefaultValue;
		}

		public readonly string Name;
		public readonly string DataType;
		public readonly string DefaultValue;
		public override string ToString() {
			return Name + " " + DataType + " " + DefaultValue + ",0";
		}
	}
}
