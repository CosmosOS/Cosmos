using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.IL;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL {
	public abstract class OpCodeMap {
		protected readonly SortedList<OpCodeEnum, Type> mMap = new SortedList<OpCodeEnum, Type>();

		protected OpCodeMap() {
			MethodHeaderOp = GetMethodHeaderOp();								   
			MethodFooterOp = GetMethodFooterOp();
			PInvokeMethodBodyOp = GetPInvokeMethodBodyOp();
			CustomMethodImplementationProxyOp = GetCustomMethodImplementationProxyOp();
			CustomMethodImplementationOp = GetCustomMethodImplementationOp();
			InitVmtImplementationOp = GetInitVmtImplementationOp();
			MainEntryPointOp = GetMainEntryPointOp();
		}

		protected abstract Assembly ImplementationAssembly {
			get;
		}

		protected abstract Type GetMethodHeaderOp();
		protected abstract Type GetMethodFooterOp();
		protected abstract Type GetPInvokeMethodBodyOp();
		protected abstract Type GetCustomMethodImplementationProxyOp();
		protected abstract Type GetCustomMethodImplementationOp();
		protected abstract Type GetInitVmtImplementationOp();
		protected abstract Type GetMainEntryPointOp();

		public virtual void Initialize(Assembler.Assembler aAssembler, IEnumerable<Assembly> aApplicationAssemblies) {
			foreach (var xItem in (from item in ImplementationAssembly.GetTypes()
								   let xAttrib = item.GetCustomAttributes(typeof(OpCodeAttribute), true).FirstOrDefault() as OpCodeAttribute
								   where item.IsSubclassOf(typeof(Op)) && xAttrib != null
								   select new {
									   OpCode = xAttrib.OpCode,
									   Type = item
								   })) {
				try {
					mMap.Add(xItem.OpCode, xItem.Type);
				} catch {
					Console.WriteLine("Was adding op " + xItem.OpCode);
					throw;
				}
			}
		}

		public Type GetOpForOpCode(OpCodeEnum code) {
			if (!mMap.ContainsKey(code)) {
				throw new NotSupportedException("OpCode '" + code + "' not supported!");
			}
			return mMap[code];
		}

		public readonly Type MethodHeaderOp;
		public readonly Type MethodFooterOp;
		public readonly Type PInvokeMethodBodyOp;
		public readonly Type CustomMethodImplementationProxyOp;
		public readonly Type CustomMethodImplementationOp;
		public readonly Type InitVmtImplementationOp;
		public readonly Type MainEntryPointOp;
		
		public virtual Type GetOpForCustomMethodImplementation(string aName) {
			return null;
		}

		public virtual IList<Assembly> GetPlugAssemblies() {
			List<Assembly> xResult = new List<Assembly>();
			xResult.Add(typeof(OpCodeMap).Assembly);
			xResult.Add(Assembly.Load("Indy.IL2CPU"));
			return xResult;
		}

		public MethodBase GetCustomMethodImplementation(string aOrigMethodName, bool aInMetalMode) {
			return null;
		}

		public virtual bool HasCustomAssembleImplementation(MethodInformation aMethod, bool aInMetalMode) {
			PlugMethodAttribute xResult = ((PlugMethodAttribute[])aMethod.Method.GetCustomAttributes(typeof(PlugMethodAttribute), true)).FirstOrDefault();
			if (xResult != null) {
				return xResult.MethodAssembler != null;
			}
			return false;
		}

		public virtual void DoCustomAssembleImplementation(bool aInMetalMode, Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			PlugMethodAttribute xAttrib = (PlugMethodAttribute)aMethodInfo.Method.GetCustomAttributes(typeof(PlugMethodAttribute), true).Cast<PlugMethodAttribute>().FirstOrDefault();
			if (xAttrib != null) {
				Type xAssemblerType = xAttrib.MethodAssembler;
				if (xAssemblerType != null) {
					AssemblerMethod xAssembler = (AssemblerMethod)Activator.CreateInstance(xAssemblerType);
					xAssembler.Assemble(aAssembler);
				}
			}
		}

		private static Type GetType(Assembly aAssembly, string aType) {
			string xActualTypeName = aType;
			if (xActualTypeName.Contains("<") && xActualTypeName.Contains(">")) {
				xActualTypeName = xActualTypeName.Substring(0, xActualTypeName.IndexOf("<"));
			}
			Type xResult = aAssembly.GetType(aType, false);
			if (xResult != null) {
				return xResult;
			}
			throw new Exception("Type '" + aType + "' not found in assembly '" + aAssembly + "'!");
		}

		public virtual void PostProcess(Assembler.Assembler aAssembler) {
		}

		public virtual void EmitOpHeader(Assembler.Assembler aAssembler, uint aOpId, string aOpLabel, ushort aDebugComport) {
		}
	}
}