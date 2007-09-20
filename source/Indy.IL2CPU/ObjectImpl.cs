using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU {
	public static class ObjectImpl {
		/// <summary>
		/// The object first stores any metadata involved. (Most likely containing a reference to the 
		/// object type). This is the number of bytes.
		/// </summary>
		public const uint FieldDataOffset = 4;
		[MethodAlias(Name = "System_Void___System_Object__ctor____")]
		public static void Ctor(IntPtr aThis) {
		}
	}
}
