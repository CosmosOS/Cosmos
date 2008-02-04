using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU {
	public enum InstanceTypeEnum: uint {
		NormalObject = 1,
		Array = 2,
		BoxedValueType = 3,
		StaticEmbeddedObject = 0x80000001,
		StaticEmbeddedArray = 0x80000002
	}

	[Plug(Target=typeof(Object))]
	public static class ObjectImpl {
		/// <summary>
		///		<para>
		///			The object first stores any metadata involved. (Most likely containing a reference to the 
		///			object type). This is the number of bytes.
		///		</para>
		///		<para>
		///			The first 4 bytes are the reference to the type information of the instance, the second 4 bytes 
		///			are the <see cref="InstanceTypeEnum"/> value. For arrays, there are 4 following bytes containing the element count, for objects, the amount of reference fields.
		/// For arrays, next 4 bytes containing the element size.
		///		</para>
		/// </summary>
		public const int FieldDataOffset = 12;
		//[MethodAlias(Name = "System_Void___System_Object__ctor____")]
		public static void Ctor(IntPtr aThis) {
		}

		public static Type GetType(object aThis) {
			return null;
		}
	}
}
