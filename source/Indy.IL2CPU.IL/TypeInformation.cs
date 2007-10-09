using System;
using System.Collections.Generic;
using System.Linq;

namespace Indy.IL2CPU.IL {
	// TODO: abstract this one out to a X86 specific one
	public class TypeInformation {
		public struct Field {
			public readonly int Offset;

			/// <summary>
			/// Contains the relative address. This should be used as follows:
			/// <example>new Push("[eax " + theField.RelativeAddress + "]");</example>
			/// </summary>
			public readonly string RelativeAddress;

			public readonly int Size;

			public Field(int aOffset, int aSize) {
				Offset = aOffset;
				Size = aSize;
				RelativeAddress = "+ 0" + (Offset).ToString("X") + "h";
			}
		}

		public readonly SortedList<string, Field> Fields;
		public readonly int StorageSize;
		public TypeInformation(int aStorageSize, SortedList<string, Field> aFields) {
			Fields = aFields;
			StorageSize = aStorageSize;
		}
	}
}