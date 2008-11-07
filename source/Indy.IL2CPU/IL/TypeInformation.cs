using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL {
	// TODO: abstract this one out to a X86 specific one
	public class TypeInformation {
		public struct Field {
			public int Offset;

			public readonly int Size;

			public readonly bool NeedsGC;
			public readonly Type FieldType;
			public readonly bool IsExternalField;

			public Field(int aSize, bool aNeedsGC, Type aFieldType, bool aIsExternalField) {
				NeedsGC = aNeedsGC;
				FieldType = aFieldType;
				IsExternalField = aIsExternalField;
				Size = aSize;
				Offset = -1;
			}

			public override string ToString() {
				return String.Format("{0}\t{1}", Offset, Size);
			}
		}

		public readonly Dictionary<string, Field> Fields;
		public readonly uint StorageSize;
		public readonly Type TypeDef;
		public readonly bool NeedsGC;
		public TypeInformation(uint aStorageSize, Dictionary<string, Field> aFields, Type aTypeDef, bool aNeedsGC) {
			Fields = aFields;
			StorageSize = aStorageSize;
			TypeDef = aTypeDef;
			NeedsGC = aNeedsGC;
		}

		public override string ToString() {
			StringBuilder xResult = new StringBuilder();
			xResult.AppendLine(String.Format("Type '{0}'", TypeDef.AssemblyQualifiedName));
			xResult.AppendLine(String.Format("StorageSize = {0}, NeedsGC = {1}", StorageSize, NeedsGC));
			xResult.AppendLine("Fields:");
			if (Fields.Count == 0) {
				xResult.AppendLine("\t(None)");
			} else {
				foreach (KeyValuePair<string, Field> xField in (from item in Fields
																orderby item.Value.Offset
																select item)) {
					xResult.AppendLine(String.Format("\t'{0}': '{1}'", xField.Key, xField.Value.FieldType.AssemblyQualifiedName));
					xResult.AppendLine(String.Format("\t\tSize: {0}, Offset: {1}, NeedsGC: {2}", xField.Value.Size, xField.Value.Offset, xField.Value.NeedsGC));
				}
			}
			return xResult.ToString();
		}
	}
}