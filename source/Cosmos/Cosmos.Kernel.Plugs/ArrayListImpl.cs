using System;
using System.Collections.Generic;
using Indy.IL2CPU.Plugs;
using System.Collections;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target=typeof(ArrayList))]
	public static class ArrayListImpl {
		public static ArrayList Synchronized(ArrayList aList) {
			throw new NotSupportedException("Threading support not implemented yet!");
		}
	}
}