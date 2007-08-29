using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Indy.IL2CPU.IL;

namespace Indy.IL2CPU {
	public class Engine {
		private OpCodeMap mMap = new OpCodeMap();
		public void Execute(string assembly) {
			Assembly a = Assembly.ReflectionOnlyLoadFrom(assembly);
			if (a.EntryPoint == null)
				throw new NotSupportedException("Libraries are not yet supported!");
			ILReader reader = new ILReader(a.EntryPoint.GetMethodBody().GetILAsByteArray());
			byte curByte;
			while(reader.TryReadByte(out curByte))
			{
				mMap.GetOpForOpCode(curByte).Process(reader);
			}
			Console.WriteLine("Done");
		}
	}
}
