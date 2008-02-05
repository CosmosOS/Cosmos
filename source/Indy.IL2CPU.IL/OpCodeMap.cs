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

		public virtual void Initialize(Assembler.Assembler aAssembler, IEnumerable<Assembly> aApplicationAssemblies, IEnumerable<Assembly> aPlugs) {
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
			InitializePlugMethodsList(aAssembler, aPlugs);
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
		private SortedList<string, MethodBase> mPlugMethods;

		/// <summary>
		/// Gets the full name of a method, without the defining type included
		/// </summary>
		/// <param name="aSelf"></param>
		/// <returns></returns>
		private static string GetStrippedMethodBaseFullName(MethodBase aMethod) {
			StringBuilder xBuilder = new StringBuilder();
			string[] xParts = aMethod.ToString().Split(' ');
			string[] xParts2 = xParts.Skip(1).ToArray();
			MethodInfo xMethodInfo = aMethod as MethodInfo;
			if (xMethodInfo != null) {
				xBuilder.Append(xMethodInfo.ReturnType.FullName);
			} else {
				if (aMethod is ConstructorInfo) {
					xBuilder.Append(typeof(void).FullName);
				} else {
					xBuilder.Append(xParts[0]);
				}
			}
			xBuilder.Append("  ");
			xBuilder.Append(".");
			xBuilder.Append(aMethod.Name);
			xBuilder.Append("(");
			ParameterInfo[] xParams = aMethod.GetParameters();
			bool xParamAdded = false;
			for (int i = 0; i < xParams.Length; i++) {
				if (xParams[i].Name == "aThis" && i == 0) {
					continue;
				}
				if (xParams[i].IsDefined(typeof(FieldAccessAttribute), true)) {
					continue;
				}
				if (xParamAdded) {
					xBuilder.Append(", ");
				}
				xBuilder.Append(xParams[i].ParameterType.FullName);
				xParamAdded = true;
			}
			xBuilder.Append(")");
			return xBuilder.ToString();
		}

		private void InitializePlugMethodsList(Assembler.Assembler aAssembler, IEnumerable<Assembly> aPlugs) {
			if (mPlugMethods != null) {
				throw new Exception("PlugMethods list already initialized!");
			}
			mPlugMethods = new SortedList<string, MethodBase>();
			foreach (Assembly xAssemblyDef in GetPlugAssemblies().Union(aPlugs)) {
				foreach (var xType in (from item in xAssemblyDef.GetTypes()
									   let xCustomAttribs = item.GetCustomAttributes(typeof(PlugAttribute), false)
									   where xCustomAttribs != null && xCustomAttribs.Length > 0
									   select new KeyValuePair<Type, PlugAttribute>(item, (PlugAttribute)xCustomAttribs[0]))) {
					PlugAttribute xPlugAttrib = xType.Value;
					Type xTypeRef = xPlugAttrib.Target;
					if (xTypeRef == null) {
						xTypeRef = Type.GetType(xPlugAttrib.TargetName, true);
					}
					foreach (MethodBase xMethod in xType.Key.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
						PlugMethodAttribute xPlugMethodAttrib = xMethod.GetCustomAttributes(typeof(PlugMethodAttribute), true).Cast<PlugMethodAttribute>().FirstOrDefault();
						string xSignature = String.Empty;
						if (xPlugMethodAttrib != null) {
							xSignature = xPlugMethodAttrib.Signature;
							if (!xPlugMethodAttrib.Enabled) {
								continue;
							}
							if (aAssembler.InMetalMode && !xPlugMethodAttrib.InMetalMode) {
								continue;
							} else {
								if (!xPlugMethodAttrib.InNormalMode) {
									continue;
								}
							}
							if (!String.IsNullOrEmpty(xSignature)) {
								mPlugMethods.Add(xSignature, xMethod);
								continue;
							}
						}
						string xStrippedSignature = GetStrippedMethodBaseFullName(xMethod);
						foreach (MethodBase xOrigMethodDef in xTypeRef.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic)) {
							string xOrigStrippedSignature = GetStrippedMethodBaseFullName(xOrigMethodDef);
							if (xOrigStrippedSignature == xStrippedSignature) {
								mPlugMethods.Add(Label.GenerateLabelName(xOrigMethodDef), xMethod);
							}
						}
						foreach (MethodBase xOrigMethodDef in xTypeRef.GetConstructors(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic)) {
							string xOrigStrippedSignature = GetStrippedMethodBaseFullName(xOrigMethodDef);
							if (xOrigStrippedSignature == xStrippedSignature) {
								mPlugMethods.Add(Label.GenerateLabelName(xOrigMethodDef), xMethod);
							}
						}
					}
				}
			}
			//Console.Write(new String('-', Console.WindowWidth));
			Console.WriteLine("Recognized Plug methods:");
			foreach (string s in mPlugMethods.Keys) {
				Console.WriteLine(s);
			}
			//Console.Write(new String('-', Console.WindowWidth));
		}

		public virtual Type GetOpForCustomMethodImplementation(string aName) {
			return null;
		}

		protected virtual IList<Assembly> GetPlugAssemblies() {
			List<Assembly> xResult = new List<Assembly>();
			xResult.Add(typeof(OpCodeMap).Assembly);
			xResult.Add(Assembly.Load("Indy.IL2CPU"));
			return xResult;
		}

		public MethodBase GetCustomMethodImplementation(string aOrigMethodName, bool aInMetalMode) {
			if (mPlugMethods.ContainsKey(aOrigMethodName)) {
				return mPlugMethods[aOrigMethodName];
			}
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
	}
}