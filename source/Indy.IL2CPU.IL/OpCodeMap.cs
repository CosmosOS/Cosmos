using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Indy.IL2CPU.IL;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	public abstract class OpCodeMap {
		private readonly SortedList<Code, Op> mMap = new SortedList<Code, Op>();

		protected OpCodeMap() {
			MethodHeaderOp = GetMethodHeaderOp();
			MethodFooterOp = GetMethodFooterOp();
		}

		protected abstract Assembly ImplementationAssembly {
			get;
		}

		protected abstract MethodHeaderOp GetMethodHeaderOp();
		protected abstract MethodFooterOp GetMethodFooterOp();
		

		public void Initialize(Assembler.Assembler aAssembler) {
			foreach (Type t in (from item in ImplementationAssembly.GetTypes()
								where item.IsSubclassOf(typeof(Op)) && item.GetCustomAttributes(typeof(OpCodeAttribute), true).Length > 0
								select item)) {
				Op xOp = Activator.CreateInstance(t) as Op;
				xOp.Assembler = aAssembler;
				object[] xAttribs = t.GetCustomAttributes(typeof(OpCodeAttribute), true);
				try {
					mMap.Add(((OpCodeAttribute)xAttribs[0]).OpCode, xOp);
				} catch {
					Console.WriteLine("Was adding op " + ((OpCodeAttribute)xAttribs[0]).OpCode.ToString());
					throw;
				}
			}
			MethodHeaderOp.Assembler = aAssembler;
			MethodFooterOp.Assembler = aAssembler;
		}

		public Op GetOpForOpCode(Code code) {
			if (!mMap.ContainsKey(code)) {
				throw new NotSupportedException("OpCode '" + code + "' not supported!");
			}
			return mMap[code];
		}

		public readonly MethodHeaderOp MethodHeaderOp;
		public readonly MethodFooterOp MethodFooterOp;
	}
}