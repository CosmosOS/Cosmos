using System;
using System.Collections.Generic;
using System.Linq;

namespace Indy.IL2CPU.IL {
	// TODO: abstract this one out to a X86 specific one
	public class TypeInformation {
		public struct Field {
			public readonly uint Offset;

			/// <summary>
			/// Contains the relative address. This should be used as follows:
			/// <example>new Push("[eax " + theField.RelativeAddress + "]");</example>
			/// </summary>
			public readonly string RelativeAddress;

			public readonly uint Size;

			public Field(uint aOffset, uint aSize) {
				Offset = aOffset;
				Size = aSize;
				RelativeAddress = "+ 0" + (/*4 + */Offset).ToString("X") + "h";
			}
		}

		public readonly SortedList<string, Field> Fields;
		public readonly uint StorageSize;
		public TypeInformation(uint aStorageSize, SortedList<string, Field> aFields) {
			Fields = aFields;
			StorageSize = aStorageSize;
		}
	}
}