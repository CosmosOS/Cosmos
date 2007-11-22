using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.IL;
using Indy.IL2CPU.Plugs;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	public abstract class OpCodeMap {
		protected readonly SortedList<Code, Type> mMap = new SortedList<Code, Type>();

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

		public virtual void Initialize(Assembler.Assembler aAssembler, IEnumerable<AssemblyDefinition> aPlugs, Func<TypeReference, TypeDefinition> aTypeResolver, Func<string, AssemblyDefinition> aAssemblyResolver) {
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
			InitializePlugMethodsList(aAssembler, aPlugs, aTypeResolver, aAssemblyResolver);
		}

		public Type GetOpForOpCode(Code code) {
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
		private SortedList<string, MethodDefinition> mPlugMethods;

		private static string GetMethodDefinitionFullName(MethodReference aSelf) {
			StringBuilder sb = new StringBuilder(aSelf.ReturnType.ReturnType.FullName + " " + aSelf.DeclaringType.FullName + "." + aSelf.Name);
			sb.Append("(");
			if (aSelf.Parameters.Count > 0) {

				foreach (ParameterDefinition xParam in aSelf.Parameters) {
					sb.Append(xParam.ParameterType.FullName);
					sb.Append(",");
				}
			}
			return sb.ToString().TrimEnd(',') + ")";
		}

		private void InitializePlugMethodsList(Assembler.Assembler aAssembler, IEnumerable<AssemblyDefinition> aPlugs, Func<TypeReference, TypeDefinition> aTypeResolver, Func<string, AssemblyDefinition> aAssemblyResolver) {
			if (mPlugMethods != null) {
				throw new Exception("PlugMethods list already initialized!");
			}
			mPlugMethods = new SortedList<string, MethodDefinition>();
			foreach (AssemblyDefinition xAssemblyDef in GetPlugAssemblies().Union(aPlugs)) {
				foreach (ModuleDefinition xModuleDef in xAssemblyDef.Modules) {
					foreach (TypeDefinition xType in (from item in xModuleDef.Types.Cast<TypeDefinition>()
													  where item.CustomAttributes.Cast<CustomAttribute>().Count(x => x.Constructor.DeclaringType.FullName == typeof(PlugAttribute).FullName) != 0
													  select item)) {
						CustomAttribute xPlugAttrib = (from item in xType.CustomAttributes.Cast<CustomAttribute>()
													   where item.Constructor.DeclaringType.FullName == typeof(PlugAttribute).FullName
													   select item).First();
						TypeReference xTypeRef = xModuleDef.TypeReferences.Cast<TypeReference>().FirstOrDefault(x => (x.FullName + ", " + x.Scope.ToString()) == (string)xPlugAttrib.Fields[PlugAttribute.TargetPropertyName]);
						if (xTypeRef == null) {
							string xTypeFullyQualedName = (string)xPlugAttrib.Fields[PlugAttribute.TargetPropertyName];
							string xAsmName = xTypeFullyQualedName.Substring(xTypeFullyQualedName.IndexOf(",") + 1).TrimStart();
							string xTypeName = xTypeFullyQualedName.Substring(0, xTypeFullyQualedName.IndexOf(","));
							AssemblyDefinition xAsmDef = aAssemblyResolver(xAsmName);
							foreach (ModuleDefinition xModDef in xAsmDef.Modules) {
								if (xModDef.Types.Contains(xTypeName)) {
									xTypeRef = xModDef.Types[xTypeName];
									break;
								}
							}
							if (xTypeRef == null) {
								throw new Exception("TypeRef for '" + (string)xPlugAttrib.Fields[PlugAttribute.TargetPropertyName] + "' not found! (" + xType.FullName + ")");
							}
						}
						TypeDefinition xReplaceTypeDef = aTypeResolver(xTypeRef);
						foreach (MethodDefinition xMethod in (from item in xType.Methods.Cast<MethodDefinition>()
															  select item)) {
							CustomAttribute xPlugMethodAttrib = (from item in xMethod.CustomAttributes.Cast<CustomAttribute>()
																 where item.Constructor.DeclaringType.FullName == typeof(PlugMethodAttribute).FullName
																 select item).FirstOrDefault();
							//System.Diagnostics.Debugger.Break();
							string xSignature = String.Empty;
							if (xPlugMethodAttrib != null) {
								if (!xPlugMethodAttrib.Resolved) {
									xPlugMethodAttrib.Resolve();
								}
								xSignature = xPlugMethodAttrib.Fields[PlugMethodAttribute.SignaturePropertyName] as string;
								if (!String.IsNullOrEmpty(xPlugMethodAttrib.Fields[PlugMethodAttribute.EnabledPropertyName] as String)) {
									if (!Boolean.Parse((string)xPlugMethodAttrib.Fields[PlugMethodAttribute.EnabledPropertyName])) {
										continue;
									}
								}
								//System.Diagnostics.Debugger.Break();
								if (aAssembler.InMetalMode) {
									if (xPlugMethodAttrib.Fields[PlugMethodAttribute.InMetalModePropertyName] != null && !((bool)xPlugMethodAttrib.Fields[PlugMethodAttribute.InMetalModePropertyName])) {
										continue;
									}
								} else {
									if (xPlugMethodAttrib.Fields[PlugMethodAttribute.InNormalModePropertyName] != null && !((bool)xPlugMethodAttrib.Fields[PlugMethodAttribute.InNormalModePropertyName])) {
										continue;
									}
								}
								if (!String.IsNullOrEmpty(xSignature)) {
									mPlugMethods.Add(xSignature, xMethod);
									continue;
								}
							}
							string xStrippedSignature = GetMethodDefinitionFullName(xMethod).Replace(xType.FullName, "");
							foreach (MethodDefinition xOrigMethodDef in xReplaceTypeDef.Methods) {
								string xOrigStrippedSignature = GetMethodDefinitionFullName(xOrigMethodDef).Replace(xReplaceTypeDef.FullName, "");
								if (xOrigStrippedSignature == xStrippedSignature) {
									mPlugMethods.Add(Label.GenerateLabelName(xOrigMethodDef), xMethod);
								}
							}
							foreach (MethodDefinition xOrigMethodDef in xReplaceTypeDef.Constructors) {
								string xOrigStrippedSignature = GetMethodDefinitionFullName(xOrigMethodDef).Replace(xReplaceTypeDef.FullName, "");
								if (xOrigStrippedSignature == xStrippedSignature) {
									mPlugMethods.Add(Label.GenerateLabelName(xOrigMethodDef), xMethod);
								}
							}
						}
					}
				}
			}
			Console.Write(new String('-', Console.WindowWidth));
			Console.WriteLine("Recognized Plug methods:");
			foreach (string s in mPlugMethods.Keys) {
				Console.WriteLine(s);
			}
			Console.Write(new String('-', Console.WindowWidth));
		}

		public virtual Type GetOpForCustomMethodImplementation(string aName) {
			return null;
		}

		protected virtual IList<AssemblyDefinition> GetPlugAssemblies() {
			List<AssemblyDefinition> xResult = new List<AssemblyDefinition>();
			xResult.Add(AssemblyFactory.GetAssembly(typeof(OpCodeMap).Assembly.Location));
			xResult.Add(AssemblyFactory.GetAssembly(Assembly.Load("Indy.IL2CPU").Location));
			return xResult;
		}

		public MethodReference GetCustomMethodImplementation(string aOrigMethodName, bool aInMetalMode) {
			if (mPlugMethods.ContainsKey(aOrigMethodName)) {
				return mPlugMethods[aOrigMethodName];
			}
			return GetCustomMethodImplementation_Old(aOrigMethodName, aInMetalMode);
		}

		[Obsolete("Try to use the GetPlugAssemblies infrastructure!")]
		public virtual MethodReference GetCustomMethodImplementation_Old(string aOrigMethodName, bool aInMetalMode) {
			return null;
		}

		public virtual bool HasCustomAssembleImplementation(MethodInformation aMethod, bool aInMetalMode) {
			CustomAttribute xAttrib = (from item in aMethod.MethodDefinition.CustomAttributes.Cast<CustomAttribute>()
									   where item.Constructor.DeclaringType.FullName == typeof(PlugMethodAttribute).FullName
									   && item.Fields.Contains(PlugMethodAttribute.MethodAssemblerPropertyName)
									   select item).FirstOrDefault();
			return xAttrib != null;
		}

		public virtual void DoCustomAssembleImplementation(bool aInMetalMode, Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			CustomAttribute xAttrib = (from item in aMethodInfo.MethodDefinition.CustomAttributes.Cast<CustomAttribute>()
									   where item.Constructor.DeclaringType.FullName == typeof(PlugMethodAttribute).FullName
									   && item.Fields.Contains(PlugMethodAttribute.MethodAssemblerPropertyName)
									   select item).FirstOrDefault();
			System.Diagnostics.Debugger.Break();
			if (xAttrib != null) {
				string xAssemblerTypeName = (string)xAttrib.Fields[PlugMethodAttribute.MethodAssemblerPropertyName];
				TypeDefinition xTypeDef = GetTypeDefinition(aMethodInfo.MethodDefinition.DeclaringType.Module.Assembly, xAssemblerTypeName);
				if (xTypeDef != null) {
					Type xAssemblerType = Type.GetType(xAssemblerTypeName + ", " + xTypeDef.Module.Assembly.Name.FullName, true);
					BaseMethodAssembler xAssembler = (BaseMethodAssembler)Activator.CreateInstance(xAssemblerType);
					xAssembler.Assemble(aAssembler);
				}
			}
		}

		private static TypeDefinition GetTypeDefinition(AssemblyDefinition aAssembly, string aType) {
			TypeDefinition xTypeDef = null;
			string xActualTypeName = aType;
			if (xActualTypeName.Contains("<") && xActualTypeName.Contains(">")) {
				xActualTypeName = xActualTypeName.Substring(0, xActualTypeName.IndexOf("<"));
			}
			foreach (ModuleDefinition xModDef in aAssembly.Modules) {
				if (xModDef.Types.Contains(xActualTypeName)) {
					return xModDef.Types[xActualTypeName];
				}
			}
			throw new Exception("Type '" + aType + "' not found in assembly '" + aAssembly + "'!");
		}

		public virtual void PostProcess(Assembler.Assembler aAssembler) {
		}
	}
}