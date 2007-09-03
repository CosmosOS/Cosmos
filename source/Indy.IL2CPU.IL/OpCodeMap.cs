using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Indy.IL2CPU.IL;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	public abstract class OpCodeMap {
		private readonly SortedList<Code, Type> mMap = new SortedList<Code, Type>();

		protected OpCodeMap() {
			MethodHeaderOp = GetMethodHeaderOp();
			MethodFooterOp = GetMethodFooterOp();
		}

		protected abstract Assembly ImplementationAssembly {
			get;
		}

		protected abstract Type GetMethodHeaderOp();
		protected abstract Type GetMethodFooterOp();
		

		public void Initialize(Assembler.Assembler aAssembler) {
			foreach (Type t in (from item in ImplementationAssembly.GetTypes()
								where item.IsSubclassOf(typeof(Op)) && item.GetCustomAttributes(typeof(OpCodeAttribute), true).Length > 0
								select item)) {
				object[] xAttribs = t.GetCustomAttributes(typeof(OpCodeAttribute), true);
				try {
					mMap.Add(((OpCodeAttribute)xAttribs[0]).OpCode, t);
				} catch {
					Console.WriteLine("Was adding op " + ((OpCodeAttribute)xAttribs[0]).OpCode);
					throw;
				}
			}
		}

		public Type GetOpForOpCode(Code code) {
			if (!mMap.ContainsKey(code)) {
				throw new NotSupportedException("OpCode '" + code + "' not supported!");
			}
			return mMap[code];
		}

		public readonly Type MethodHeaderOp;
		public readonly Type MethodFooterOp;
	}
}