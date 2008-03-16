#define VERBOSE_DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.IL;
using Indy.IL2CPU.Plugs;
using System.Runtime.InteropServices;

namespace Indy.IL2CPU {
	public class MethodBaseComparer: IComparer<MethodBase> {
		#region IComparer<MethodBase> Members
		public int Compare(MethodBase x, MethodBase y) {
			return x.GetFullName().CompareTo(y.GetFullName());
		}
		#endregion
	}

	public class FieldInfoComparer: IComparer<FieldInfo> {
		#region IComparer<FieldInfo> Members
		public int Compare(FieldInfo x, FieldInfo y) {
			return x.GetFullName().CompareTo(y.GetFullName());
		}
		#endregion
	}

	public class TypeComparer: IComparer<Type> {
		public int Compare(Type x, Type y) {
			return x.AssemblyQualifiedName.CompareTo(y.AssemblyQualifiedName);
		}
	}

	public class TypeEqualityComparer: IEqualityComparer<Type> {
		public bool Equals(Type x, Type y) {
			return x.FullName.Equals(y.FullName);
		}

		public int GetHashCode(Type obj) {
			return obj.FullName.GetHashCode();
		}
	}

	public class AssemblyEqualityComparer: IEqualityComparer<Assembly> {
		public bool Equals(Assembly x, Assembly y) {
			return x.GetName().FullName.Equals(y.GetName().FullName);
		}

		public int GetHashCode(Assembly obj) {
			return obj.GetName().FullName.GetHashCode();
		}
	}

	public enum LogSeverityEnum {
		Informational,
		Warning,
		Error
	}
	public delegate void DebugLogHandler(LogSeverityEnum aSeverity, string aMessage);

	public enum TargetPlatformEnum {
		Win32,
		NativeX86
	}

	public class QueuedMethodInformation {
		public bool Processed;
		public int Index;
	}

	public class Engine {
		protected static Engine mCurrent;
		protected Assembly mCrawledAssembly;
		protected DebugLogHandler mDebugLog;
		protected OpCodeMap mMap;
		protected Assembler.Assembler mAssembler;

		private SortedList<string, MethodBase> mPlugMethods;
		private SortedList<Type, Dictionary<string, PlugFieldAttribute>> mPlugFields;

		/// <summary>
		/// Contains a list of all methods. This includes methods to be processed and already processed.
		/// </summary>
		protected SortedList<MethodBase, QueuedMethodInformation> mMethods = new SortedList<MethodBase, QueuedMethodInformation>(new MethodBaseComparer());
		/// <summary>
		/// Contains a list of all static fields. This includes static fields to be processed and already processed.
		/// </summary>
		protected SortedList<FieldInfo, bool> mStaticFields = new SortedList<FieldInfo, bool>(new FieldInfoComparer());

		protected List<Type> mTypes = new List<Type>();
		protected TypeEqualityComparer mTypesEqualityComparer = new TypeEqualityComparer();

		/// <summary>
		/// Compiles an assembly to CPU-specific code. The entrypoint of the assembly will be 
		/// crawled to see what is neccessary, same goes for all dependencies.
		/// </summary>
		/// <remarks>For now, only entrypoints without params are supported!</remarks>
		/// <param name="aAssembly">The assembly of which to crawl the entry-point method.</param>
		/// <param name="aTargetPlatform">The platform to target when assembling the code.</param>
		/// <param name="aOutput"></param>
		/// <param name="aInMetalMode">Whether or not the output is metalmode only.</param>
		public void Execute(string aAssembly, TargetPlatformEnum aTargetPlatform, Func<string, string> aGetFileNameForGroup, bool aInMetalMode, bool aDebugMode, string aAssemblyDir, IEnumerable<string> aPlugs) {
			mCurrent = this;
			try {
				if (aGetFileNameForGroup == null) {
					throw new ArgumentNullException("aGetFileNameForGroup");
				}
				mCrawledAssembly = Assembly.LoadFile(aAssembly);

				MethodInfo xEntryPoint = (MethodInfo)mCrawledAssembly.EntryPoint;
				if (xEntryPoint == null)
					throw new NotSupportedException("No EntryPoint found!");

				Type xEntryPointType = xEntryPoint.DeclaringType;
				xEntryPoint = xEntryPointType.GetMethod("Init", new Type[0]);
				
				//List<string> xSearchDirs = new List<string>(new string[] { Path.GetDirectoryName(aAssembly), aAssemblyDir });
				//xSearchDirs.AddRange((from item in aPlugs
				//                      select Path.GetDirectoryName(item)).Distinct());
				switch (aTargetPlatform) {
					case TargetPlatformEnum.Win32: {
							mMap = (OpCodeMap)Activator.CreateInstance(Type.GetType("Indy.IL2CPU.IL.X86.Win32.Win32OpCodeMap, Indy.IL2CPU.IL.X86.Win32", true));
							mAssembler = new Assembler.X86.Win32.Assembler(aGetFileNameForGroup, aInMetalMode);
							break;
						}
					case TargetPlatformEnum.NativeX86: {
							mMap = (OpCodeMap)Activator.CreateInstance(Type.GetType("Indy.IL2CPU.IL.X86.Native.NativeOpCodeMap, Indy.IL2CPU.IL.X86.Native", true));
							mAssembler = new Assembler.X86.Native.Assembler(aGetFileNameForGroup, aInMetalMode);
							break;
						}
					default:
						throw new NotSupportedException("TargetPlatform '" + aTargetPlatform + "' not supported!");
				}
				InitializePlugs(aPlugs);
				using (mAssembler) {
					//mAssembler.OutputType = Assembler.Win32.Assembler.OutputTypeEnum.Console;
					//foreach (string xPlug in aPlugs) {
					//this.I
					List<Assembly> xAppDefs = new List<Assembly>();
					xAppDefs.Add(mCrawledAssembly);
					mAssembler.MainGroup = "main";
					mAssembler.CurrentGroup = "main";
					AssemblyEqualityComparer xComparer = new AssemblyEqualityComparer();
					foreach (Assembly xAsm in AppDomain.CurrentDomain.GetAssemblies()) {
						Assembly xAssemblyDef = Assembly.LoadFrom(xAsm.Location);
						if (!xAppDefs.Contains(xAssemblyDef)) {
							xAppDefs.Add(xAssemblyDef);
						}
					}
					//for (int i = 0; i < xAppDefs.Count; i++) {
					//    Assembly xCurDef = xAppDefs[i];
					//    foreach (ModuleDefinition xModDef in xCurDef.Modules) {
					//        foreach (AssemblyNameReference xAssemblyNameRef in xModDef.AssemblyReferences) {
					//            Assembly xReffedAssemblyDef = mCrawledAssembly.Resolver.Resolve(xAssemblyNameRef);
					//            if (xReffedAssemblyDef != null) {
					//                if (!xAppDefs.Contains(xReffedAssemblyDef, new AssemblyEqualityComparer())) {
					//                    xAppDefs.Add(xReffedAssemblyDef);
					//                }
					//            }
					//        }
					//    }
					//}
					mMap.Initialize(mAssembler, xAppDefs);
					mAssembler.DebugMode = aDebugMode;
					IL.Op.QueueMethod += QueueMethod;
					IL.Op.QueueStaticField += QueueStaticField;
					try {
						mTypes.Add(typeof(object));
						mMethods.Add(RuntimeEngineRefs.InitializeApplicationRef, new QueuedMethodInformation() {
							Processed = false,
							Index = mMethods.Count
						});
						mMethods.Add(RuntimeEngineRefs.FinalizeApplicationRef, new QueuedMethodInformation() {
							Processed = false,
							Index = mMethods.Count
						});
						if (!aInMetalMode) {
							mMethods.Add(VTablesImplRefs.LoadTypeTableRef, new QueuedMethodInformation() {
								Processed = false,
								Index = mMethods.Count
							});
							mMethods.Add(VTablesImplRefs.SetMethodInfoRef, new QueuedMethodInformation() {
								Processed = false,
								Index = mMethods.Count
							});
							mMethods.Add(VTablesImplRefs.IsInstanceRef, new QueuedMethodInformation() {
								Processed = false,
								Index = mMethods.Count
							});
							mMethods.Add(VTablesImplRefs.SetTypeInfoRef, new QueuedMethodInformation() {
								Processed = false,
								Index = mMethods.Count
							});
							mMethods.Add(VTablesImplRefs.GetMethodAddressForTypeRef, new QueuedMethodInformation() {
								Processed = false,
								Index = mMethods.Count
							});
						}
						mMethods.Add(xEntryPoint, new QueuedMethodInformation() {
							Processed = false,
							Index = mMethods.Count
						});
						ProcessAllMethods();
						if (!aInMetalMode) {
							do {
								int xOldCount = mMethods.Count;
								ScanForMethodsToIncludeForVMT();
								ProcessAllMethods();
								if (xOldCount == mMethods.Count) {
									break;
								}
							} while (true);
						}
						// initialize the runtime engine
						mAssembler.CurrentGroup = "main";
						MainEntryPointOp xEntryPointOp = (MainEntryPointOp)GetOpFromType(mMap.MainEntryPointOp, null, null);
						xEntryPointOp.Assembler = mAssembler;
						xEntryPointOp.Enter(Assembler.Assembler.EntryPointName);
						xEntryPointOp.Call(RuntimeEngineRefs.InitializeApplicationRef);
						if (!aInMetalMode) {
							xEntryPointOp.Call("____INIT__VMT____");
						}
						foreach (Type xType in mTypes) {
							foreach (MethodBase xMethod in xType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)) {
								if (xMethod.IsStatic) {
									xEntryPointOp.Call(xMethod);
								}
							}
						}
						xEntryPointOp.Call(xEntryPoint);
						if (xEntryPoint.ReturnType == typeof(void)) {
							xEntryPointOp.Pushd("0");
						}
						// todo: implement support for returncodes?
						xEntryPointOp.Call(RuntimeEngineRefs.FinalizeApplicationRef);
						xEntryPointOp.Exit();
						ProcessAllMethods();
						if (!aInMetalMode) {
							do {
								int xOldCount = mMethods.Count;
								ScanForMethodsToIncludeForVMT();
								ProcessAllMethods();
								if (xOldCount == mMethods.Count) {
									break;
								}
							} while (true);
							mAssembler.CurrentGroup = "main";
							GenerateVMT();
						}
						mMap.PostProcess(mAssembler);
						ProcessAllStaticFields();
					} finally {
						mAssembler.Flush();
						IL.Op.QueueMethod -= QueueMethod;
						IL.Op.QueueStaticField -= QueueStaticField;
					}
				}
			} finally {
				mCurrent = null;
			}
		}

		private void GenerateVMT() {
			Op xOp = GetOpFromType(mMap.MethodHeaderOp, null, new MethodInformation("____INIT__VMT____", new MethodInformation.Variable[0], new MethodInformation.Argument[0], 0, false, null, null, typeof(void)));
			xOp.Assembler = mAssembler;
			xOp.Assemble();
			InitVmtImplementationOp xInitVmtOp = (InitVmtImplementationOp)GetOpFromType(mMap.InitVmtImplementationOp, null, null);
			xInitVmtOp.Assembler = mAssembler;
			xInitVmtOp.Types = mTypes;
			xInitVmtOp.SetTypeInfoRef = VTablesImplRefs.SetTypeInfoRef;
			xInitVmtOp.SetMethodInfoRef = VTablesImplRefs.SetMethodInfoRef;
			xInitVmtOp.LoadTypeTableRef = VTablesImplRefs.LoadTypeTableRef;
			xInitVmtOp.TypesFieldRef = VTablesImplRefs.VTablesImplDef.GetField("mTypes", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			xInitVmtOp.Methods = mMethods.Keys;
			xInitVmtOp.VTableEntrySize = GetFieldStorageSize(GetType("", typeof(VTable).FullName.Replace('+', '.')));
			xInitVmtOp.GetMethodIdentifier += delegate(MethodBase aMethod) {
				ParameterInfo[] xParams = aMethod.GetParameters();
				Type[] xParamTypes = new Type[xParams.Length];
				for (int i = 0; i < xParams.Length; i++) {
					xParamTypes[i] = xParams[i].ParameterType;
				}
				MethodBase xMethod = GetUltimateBaseMethod(aMethod, xParamTypes, aMethod.DeclaringType);
				return GetMethodIdentifier(xMethod);
			};
			xInitVmtOp.Assemble();
			xOp = GetOpFromType(mMap.MethodFooterOp, null, new MethodInformation("____INIT__VMT____", new MethodInformation.Variable[0], new MethodInformation.Argument[0], 0, false, null, null, typeof(void)));
			xOp.Assembler = mAssembler;
			xOp.Assemble();
		}

		private void ScanForMethodsToIncludeForVMT() {
			List<Type> xCheckedTypes = new List<Type>();
			foreach (MethodBase xMethod in mMethods.Keys) {
				if (xMethod.IsStatic) {
					continue;
				}
				Type xCurrentType = xMethod.DeclaringType;
				if (!xCheckedTypes.Contains(xCurrentType, mTypesEqualityComparer)) {
					xCheckedTypes.Add(xCurrentType);
				}
			}
			foreach (Type xType in mTypes) {
				if (!xCheckedTypes.Contains(xType, mTypesEqualityComparer)) {
					xCheckedTypes.Add(xType);
				}
			}
			for (int i = 0; i < xCheckedTypes.Count; i++) {
				Type xCurrentType = xCheckedTypes[i];
				while (xCurrentType != null) {
					if (!xCheckedTypes.Contains(xCurrentType, mTypesEqualityComparer)) {
						xCheckedTypes.Add(xCurrentType);
					}
					if (xCurrentType.FullName == "System.Object") {
						break;
					}
					if (xCurrentType.BaseType == null) {
						break;
					}
					xCurrentType = xCurrentType.BaseType;
				}
			}
			foreach (Type xTD in xCheckedTypes) {
				foreach (MethodBase xMethod in xTD.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
					if (!xMethod.IsStatic) {
						if (xTD.BaseType == null) {
							continue;
						}
						if (xMethod.IsVirtual && !xMethod.IsConstructor&&!xMethod.IsFinal) {
							Type xCurrentInspectedType = xTD.BaseType;
							ParameterInfo[] xParams = xMethod.GetParameters();
							Type[] xMethodParams = new Type[xParams.Length];
							for (int i = 0; i < xParams.Length; i++) {
								xMethodParams[i] = xParams[i].ParameterType;
							}
							MethodBase xBaseMethod = GetUltimateBaseMethod(xMethod, xMethodParams, xTD);
							if (xBaseMethod != null && xBaseMethod != xMethod) {
								if (mMethods.ContainsKey(xBaseMethod)) {
									QueueMethod(xMethod);
								}
							}
						}
					}
				}
			}
		}

		private static MethodBase GetUltimateBaseMethod(MethodBase aMethod, Type[] aMethodParams, Type aCurrentInspectedType) {
			MethodBase xBaseMethod = null;
			try {
				while (true) {
					if (aCurrentInspectedType.BaseType == null) {
						break;
					}
					aCurrentInspectedType = aCurrentInspectedType.BaseType;
					MethodBase xFoundMethod = aCurrentInspectedType.GetMethod(aMethod.Name, aMethodParams);
					ParameterInfo[] xParams = xFoundMethod.GetParameters();
					bool xContinue = true;
					for (int i = 0; i < xParams.Length; i++) {
						if (xParams[i].ParameterType != aMethodParams[i]) {
							xContinue = false;
							continue;
						}
					}
					if (!xContinue) {
						continue;
					}
					if (xFoundMethod != null) {
						if (xFoundMethod.IsVirtual == aMethod.IsVirtual && xFoundMethod.IsPrivate == false && xFoundMethod.IsPublic == aMethod.IsPublic && xFoundMethod.IsFamily == aMethod.IsFamily && xFoundMethod.IsFamilyAndAssembly == aMethod.IsFamilyAndAssembly && xFoundMethod.IsFamilyOrAssembly == aMethod.IsFamilyOrAssembly && xFoundMethod.IsFinal == false) {
							xBaseMethod = xFoundMethod;
						}
					}
				}
			} catch (Exception E) {
				// todo: try to get rid of the try..catch
			}
			return xBaseMethod ?? aMethod;
		}

		//todo: remove?
		public static MethodBase GetDefinitionFromMethodBase2(MethodBase aRef) {
			Type xTypeDef;
			bool xIsArray = false;
			if (aRef.DeclaringType.FullName.Contains("[]") || aRef.DeclaringType.FullName.Contains("[,]") || aRef.DeclaringType.FullName.Contains("[,,]")) {
				xTypeDef = typeof(Array);
				xIsArray = true;
			} else {
				xTypeDef = aRef.DeclaringType;
			}
			MethodBase xMethod = null;
			if (xIsArray) {
				Type[] xParams = (from item in aRef.GetParameters()
								  select item.ParameterType).ToArray();
				if (aRef.Name == "Get") {
					xMethod = xTypeDef.GetMethod("GetValue", xParams);
				}
				if (aRef.Name == "Set") {
					xMethod = xTypeDef.GetMethod("SetValue", xParams);
				}
			}
			if (xMethod == null) {
				foreach (MethodBase xFoundMethod in xTypeDef.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
					if (xFoundMethod.Name != aRef.Name) {
						continue;
					}
					string[] xRefNameParts = aRef.ToString().Split(' ');
					string[] xFoundNameParts = xFoundMethod.ToString().Split(' ');
					if (xFoundNameParts[0] != xRefNameParts[0]) {
						//if (!(xFoundMethod.ReturnType.ReturnType is GenericParameter && aRef.ReturnType.ReturnType is GenericParameter)) {
						//    ArrayType xFoundArray = xFoundMethod.ReturnType.ReturnType as ArrayType;
						//    ArrayType xArray = aRef.ReturnType.ReturnType as ArrayType;
						//    if (xArray != null && xFoundArray != null) {
						//        if (xArray.Dimensions.Count != xFoundArray.Dimensions.Count) {
						//            continue;
						//        }
						//        GenericParameter xGenericParam = xArray.ElementType as GenericParameter;
						//        GenericParameter xFoundGenericParam = xFoundArray.ElementType as GenericParameter;
						//        if (xGenericParam != null && xFoundGenericParam != null) {
						//            if (xGenericParam.NextPosition != xFoundGenericParam.NextPosition) {
						//                continue;
						//            }
						//        }
						//    }
						//}
						continue;
					}
					ParameterInfo[] xFoundParams = xFoundMethod.GetParameters();
					ParameterInfo[] xRefParams = aRef.GetParameters();
					if (xFoundParams.Length != xRefParams.Length) {
						continue;
					}
					bool xMismatch = false;
					for (int i = 0; i < xFoundParams.Length; i++) {
						if (xFoundParams[i].ParameterType.FullName != xRefParams[i].ParameterType.FullName) {
							//if (xFoundMethod.Parameters[i].ParameterType is GenericParameter && aRef.Parameters[i].ParameterType is GenericParameter) {
							//	continue;
							//}
							xMismatch = true;
							break;
						}
					}
					if (!xMismatch) {
						xMethod = xFoundMethod;
					}
				}
			}
			if (xMethod != null) {
				return xMethod;
			}
			//xMethod = xTypeDef.GetConstructor(aRef.Name == MethodBase.Cctor, aRef.Parameters);
			//if (xMethod != null && (aRef.Name == MethodBase.Cctor || aRef.Name == MethodBase.Ctor)) {
			//    return xMethod;
			//}
			throw new Exception("Couldn't find Method! ('" + aRef.GetFullName() + "'");
		}

		[Obsolete]
		public static Type GetDefinitionFromType2(Type aRef) {
			if (aRef == null) {
				throw new ArgumentNullException("aRef");
			}
			if (mCurrent == null) {
				throw new Exception("No Current engine found!");
			}
			Type xTempResult = aRef as Type;
			if (xTempResult != null) {
				return xTempResult;
			}
			//GenericParameter xGenParam = aRef as GenericParameter;
			//if (xGenParam != null) {
			//    // todo: add support for generics
			//    return GetType("mscorlib", "System.Object");
			//}
			//TypeSpecification xTypeSpec = aRef as TypeSpecification;
			//if (xTypeSpec != null) {
			//    return GetDefinitionFromType(xTypeSpec.ElementType);
			//}
			//if (aRef.FullName.Contains("modreq")) {
			//    aRef = aRef.GetOriginalType();
			//}
			//AssemblyNameReference xAssemblyNameReference = aRef.Scope as AssemblyNameReference;
			//if (xAssemblyNameReference != null) {
			//    return GetType(mCurrent.mCrawledAssembly.Resolver.Resolve(xAssemblyNameReference), aRef.FullName);
			//} else {
			//    ModuleDefinition xReferencedModule = aRef.Scope as ModuleDefinition;
			//    if (xReferencedModule != null) {
			//        var xReferencedType = xReferencedModule.Types[aRef.FullName];
			//        if (xReferencedType != null) {
			//            return xReferencedType;
			//        }
			//        {
			//            string theName = aRef.FullName;
			//            while (theName.EndsWith("[]")) {
			//                theName = theName.Substring(0, theName.Length - 2);
			//            }
			//            xReferencedType = xReferencedModule.Types[theName];
			//            if (xReferencedType != null) {
			//                return xReferencedType;
			//            }
			//        }
			//        {
			//            string theName = aRef.FullName;
			//            while (theName.EndsWith("*")) {
			//                theName = theName.Substring(0, theName.Length - 1);
			//            }
			//            xReferencedType = xReferencedModule.Types[theName];
			//            if (xReferencedType != null) {
			//                return xReferencedType;
			//            }
			//        }
			//        {
			//            string theName = aRef.FullName;
			//            while (theName.EndsWith("&")) {
			//                theName = theName.Substring(0, theName.Length - 1);
			//            }
			//            xReferencedType = xReferencedModule.Types[theName];
			//            if (xReferencedType != null) {
			//                return xReferencedType;
			//            }
			//        }
			//        {
			//            string theName = aRef.FullName;
			//            if (theName.Contains("<") && theName.Contains(">")) {
			//                theName = theName.Substring(0, theName.IndexOf("<"));
			//            }
			//            xReferencedType = xReferencedModule.Types[theName];
			//            if (xReferencedType != null) {
			//                return xReferencedType;
			//            }
			//        }
			//    } else {
			//        try {
			//            string theScopeText = aRef.Scope == null ? "**NULL**" : aRef.Scope.GetType().FullName;
			//            mCurrent.OnDebugLog(LogSeverityEnum.Informational, "Error: Unhandled scope: " + theScopeText);
			//        } catch (NullReferenceException) {
			//            string theScopeText = aRef.Scope == null ? "**NULL**" : aRef.Scope.GetType().FullName;
			//            mCurrent.OnDebugLog(LogSeverityEnum.Informational, "Error: Unhandled scope: " + theScopeText);
			//        } catch {
			//            throw;
			//        }
			//    }
			//}
			//string xModuleName = "**NOT DEFINED**";
			//if (aRef.Module != null && aRef.Module.Assembly != null) {
			//    xModuleName = aRef.Module.Assembly.Name.Name;
			//}
			throw new Exception("Could not find Type! (" + aRef.ToString() + ")");
		}

		/// <summary>
		/// Gives the size to store an instance of the <paramref name="aType"/> for use in a field.
		/// </summary>
		/// <remarks>For classes, this is the pointer size.</remarks>
		/// <param name="aType"></param>
		/// <returns></returns>
		public static int GetFieldStorageSize(Type aType) {
			if (aType.FullName == "System.Void") {
				return 0;
			}
			if ((!aType.IsValueType && aType.IsClass) || aType.IsInterface) {
				return 4;
			}
			switch (aType.FullName) {
				case "System.Char":
					return 2;
				case "System.Byte":
				case "System.SByte":
					return 1;
				case "System.UInt16":
				case "System.Int16":
					return 2;
				case "System.UInt32":
				case "System.Int32":
					return 4;
				case "System.UInt64":
				case "System.Int64":
					return 8;
				// for now hardcode IntPtr and UIntPtr to be 32-bit
				case "System.UIntPtr":
				case "System.IntPtr":
					return 4;
				case "System.Boolean":
					return 1;
				case "System.Single":
					return 4;
				case "System.Double":
					return 8;
				case "System.Decimal":
					return 16;
				case "System.Guid":
					return 16;
				case "System.DateTime":
					return 8; // todo: check for correct size
			}
			if (aType.FullName.EndsWith("*")) {
				// pointer
				return 4;
			}
			// array
			//TypeSpecification xTypeSpec = aType as TypeSpecification;
			//if (xTypeSpec != null) {
			//    return 4;
			//}
			if (aType.IsEnum) {
				return GetFieldStorageSize(aType.GetField("value__").FieldType);
			}
			if (aType.IsValueType) {
				StructLayoutAttribute xSLA = aType.StructLayoutAttribute;
				if (xSLA != null) {
					if (xSLA.Size > 0) {
						return xSLA.Size;
					}
				}
			}
			int xResult;
			GetTypeFieldInfo(aType, out xResult);
			return xResult;
		}

		private static string GetGroupForType(Type aType) {
			return aType.Module.Assembly.GetName().Name;
		}

		private void ProcessAllStaticFields() {
			FieldInfo xCurrentField;
			while ((xCurrentField = (from item in mStaticFields.Keys
									 where !mStaticFields[item]
									 select item).FirstOrDefault()) != null) {
				mAssembler.CurrentGroup = GetGroupForType(xCurrentField.DeclaringType);
				string xFieldName = xCurrentField.GetFullName();
				OnDebugLog(LogSeverityEnum.Informational, "Processing Static Field '{0}'", xFieldName);
				xFieldName = DataMember.GetStaticFieldName(xCurrentField);
				if (mAssembler.DataMembers.Count(x => x.Value.Name == xFieldName) == 0) {
					RegisterType(xCurrentField.FieldType);
					//xCurrentField.
					//object xDefault = xCurrentField.GetRawConstantValue();
					//if (xDefault != null) {
					//    throw new Exception("Field has RawConstantValue, but this is not handled!");
					//}
					//if (xCurrentField.InitialValue != null && xCurrentField.InitialValue.Length > 0) {
					//    string xTheData = "";
					//    int xStorageSize = GetFieldStorageSize(xCurrentField.FieldType);
					//    if (xCurrentField.InitialValue.Length > 4) {
					//        xTheData = "0,0,0,0,";
					//        xTheData += BitConverter.GetBytes(0x80000002).Aggregate("", (r, b) => r + b + ",");
					//        xTheData += "1,0,0,0,";
					//    }
					//    xTheData += BitConverter.GetBytes(xCurrentField.InitialValue.Length).Aggregate("", (r, b) => r + b + ",");
					//    xTheData += xCurrentField.InitialValue.Aggregate("", (r, b) => r + b + ",");
					//    xTheData = xTheData.TrimEnd(',');
					//    if (xTheData.Length == 0) {
					//        throw new Exception("Field '" + xCurrentField.ToString() + "' doesn't have a valid size!");
					//    }
					//    mAssembler.DataMembers.Add(new KeyValuePair<string, DataMember>(mAssembler.CurrentGroup, new DataMember(xFieldName, "db", xTheData)));
					//} else 
					{
						int xTheSize;
						string theType = "db";
						Type xFieldTypeDef = xCurrentField.FieldType;
						//TypeSpecification xTypeSpec = xCurrentField.FieldType as TypeSpecification;
						//if (xTypeSpec == null) {
						if (!xFieldTypeDef.IsClass || xFieldTypeDef.IsValueType) {
							xTheSize = GetFieldStorageSize(xCurrentField.FieldType);
						} else {
							xTheSize = 4;
						}
						//} else {
						//xTheSize = 4;
						//}
						if (xTheSize == 4) {
							theType = "dd";
							xTheSize = 1;
						} else {
							if (xTheSize == 2) {
								theType = "dw";
								xTheSize = 1;
							}
						}
						string xTheData = "";
						if (xTheSize == 0) {
							throw new Exception("Field '" + xCurrentField.ToString() + "' doesn't have a valid size!");
						}
						for (uint i = 0; i < xTheSize; i++) {
							xTheData += "0,";
						}
						xTheData = xTheData.TrimEnd(',');
						mAssembler.DataMembers.Add(new KeyValuePair<string, DataMember>(mAssembler.CurrentGroup, new DataMember(xFieldName, theType, xTheData)));
					}
				}
				mStaticFields[xCurrentField] = true;
			}
		}

		private void ProcessAllMethods() {
			MethodBase xCurrentMethod;
			while ((xCurrentMethod = (from item in mMethods.Keys
									  where !mMethods[item].Processed
									  select item).FirstOrDefault()) != null) {
				try {
					mAssembler.CurrentGroup = GetGroupForType(xCurrentMethod.DeclaringType);
					OnDebugLog(LogSeverityEnum.Informational, "Processing method '{0}'", xCurrentMethod.GetFullName());
					RegisterType(xCurrentMethod.DeclaringType);
					if (xCurrentMethod.IsAbstract) {
						mMethods[xCurrentMethod].Processed = true;
						continue;
					}
					string xMethodName = Label.GenerateLabelName(xCurrentMethod);
					if (xMethodName=="System_Boolean__System_Object_Equals_System_Object_") {
						System.Diagnostics.Debugger.Break();
					}
					TypeInformation xTypeInfo = null;
					{
						if (!xCurrentMethod.IsStatic) {
							xTypeInfo = GetTypeInfo(xCurrentMethod.DeclaringType);
						}
					}
					MethodInformation xMethodInfo = GetMethodInfo(xCurrentMethod, xCurrentMethod, xMethodName, xTypeInfo);
					IL.Op xOp = GetOpFromType(mMap.MethodHeaderOp, null, xMethodInfo);
					xOp.Assembler = mAssembler;
#if VERBOSE_DEBUG
					string comment = "";
					if (xMethodInfo.TypeInfo == null) {
						comment = "(No Type Info available)";
					} else {
						comment = "Type Info:\r\n \r\n" + xMethodInfo.TypeInfo.ToString();
					}
					foreach (string s in comment.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)) {
						new Comment(s);
					}
					comment = xMethodInfo.ToString();
					foreach (string s in comment.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)) {
						new Comment(s);
					}
#endif
					xOp.Assemble();
					bool xIsCustomImplementation = false;
					MethodBase xCustomImplementation = GetCustomMethodImplementation(xMethodName);
					if (xCustomImplementation != null) {
						xIsCustomImplementation = true;
					}
					// what to do if a method doesn't have a body?
					bool xContentProduced = false;
					if (xIsCustomImplementation) {
						// this is for the support for having extra fields on types, and being able to use
						// them in custom implementation methods
						CustomMethodImplementationProxyOp xProxyOp = (CustomMethodImplementationProxyOp)GetOpFromType(mMap.CustomMethodImplementationProxyOp, null, xMethodInfo);
						xProxyOp.Assembler = mAssembler;
						xProxyOp.ProxiedMethod = xCustomImplementation;
						xProxyOp.Assemble();
						xContentProduced = true;
					}
					if (!xContentProduced) {
						Type xOpType = mMap.GetOpForCustomMethodImplementation(xMethodName);
						if (xOpType != null) {
							Op xMethodOp = GetOpFromType(xOpType, null, xMethodInfo);
							if (xMethodOp != null) {
								xMethodOp.Assembler = mAssembler;
								xMethodOp.Assemble();
								xContentProduced = true;
							}
						}
					}
					if (!xContentProduced) {
						if (mMap.HasCustomAssembleImplementation(xMethodInfo, mAssembler.InMetalMode)) {
							mMap.DoCustomAssembleImplementation(mAssembler.InMetalMode, mAssembler, xMethodInfo);
						} else {
							if (Enum.GetNames(typeof(CustomMethodEnum)).Contains(xMethodName)) {
								CustomMethodImplementationOp xCustomMethodImplOp = (CustomMethodImplementationOp)GetOpFromType(mMap.CustomMethodImplementationOp, null, xMethodInfo);
								xCustomMethodImplOp.Assembler = mAssembler;
								xCustomMethodImplOp.Method = (CustomMethodEnum)Enum.Parse(typeof(CustomMethodEnum), xMethodName);
								xCustomMethodImplOp.Assemble();
							} else {
								//xCurrentMethod.GetMethodImplementationFlags() == MethodImplAttributes.
								MethodBody xBody = xCurrentMethod.GetMethodBody();
								// todo: add better detection of implementation state
								if (xBody != null) {
									mInstructionsToSkip = 0;
									mAssembler.StackContents.Clear();
									ILReader xReader = new ILReader(xCurrentMethod);
									while (xReader.Read()) {
										if (mInstructionsToSkip > 0) {
											mInstructionsToSkip--;
											continue;
										}
										ExceptionHandlingClause xCurrentHandler = null;
										foreach (ExceptionHandlingClause xHandler in xBody.ExceptionHandlingClauses) {
											if (xHandler.TryOffset > 0) {
												if (xHandler.TryOffset <= xReader.NextPosition && (xHandler.TryLength + xHandler.TryOffset) > xReader.NextPosition) {
													if (xCurrentHandler == null) {
														xCurrentHandler = xHandler;
														continue;
													}
													if (xHandler.TryOffset > xCurrentHandler.TryOffset && (xHandler.TryLength + xHandler.TryOffset) < (xCurrentHandler.TryLength + xCurrentHandler.TryOffset)) {
														// only replace if the current found handler is narrower
														xCurrentHandler = xHandler;
														continue;
													}
												}
											}
											if (xHandler.HandlerOffset > 0) {
												if (xHandler.HandlerOffset <= xReader.NextPosition && (xHandler.HandlerOffset + xHandler.HandlerLength) > xReader.NextPosition) {
													if (xCurrentHandler == null) {
														xCurrentHandler = xHandler;
														continue;
													}
													if (xHandler.HandlerOffset > xCurrentHandler.HandlerOffset && (xHandler.HandlerOffset + xHandler.HandlerLength) < (xCurrentHandler.HandlerOffset + xCurrentHandler.HandlerLength)) {
														// only replace if the current found handler is narrower
														xCurrentHandler = xHandler;
														continue;
													}
												}
											}
											if ((xHandler.Flags & ExceptionHandlingClauseOptions.Filter) > 0) {
												if (xHandler.FilterOffset > 0) {
													if (xHandler.FilterOffset <= xReader.NextPosition) {
														if (xCurrentHandler == null) {
															xCurrentHandler = xHandler;
															continue;
														}
														if (xHandler.FilterOffset > xCurrentHandler.FilterOffset) {
															// only replace if the current found handler is narrower
															xCurrentHandler = xHandler;
															continue;
														}
													}
												}
											}
										}
										xMethodInfo.CurrentHandler = xCurrentHandler;
										xOp = GetOpFromType(mMap.GetOpForOpCode(xReader.OpCode), xReader, xMethodInfo);
										if ((!xOp.SupportsMetalMode) && mAssembler.InMetalMode) {
											throw new Exception("OpCode '" + xReader.OpCode + "' not supported in Metal mode!");
										}
										xOp.Assembler = mAssembler;
										new Comment("StackItems = " + mAssembler.StackContents.Count);
										foreach (var xStackContent in mAssembler.StackContents) {
											new Comment("    " + xStackContent.Size);
										}
										xOp.Assemble();
									}
								} else {
									if ((xCurrentMethod.Attributes & MethodAttributes.PinvokeImpl) != 0) {
										HandlePInvoke(xCurrentMethod, xMethodInfo);
									} else {
										OnDebugLog(LogSeverityEnum.Warning, "Method '{0}' not generated!", xCurrentMethod.GetFullName());
										new Comment("Method not being generated yet, as it's handled by an iCall");
									}
								}
							}
						}
					}
					xOp = GetOpFromType(mMap.MethodFooterOp, null, xMethodInfo);
					xOp.Assembler = mAssembler;
					xOp.Assemble();
					mAssembler.StackContents.Clear();
					mMethods[xCurrentMethod].Processed = true;
				} catch (Exception E) {
					OnDebugLog(LogSeverityEnum.Error, xCurrentMethod.GetFullName());
					throw;
				}
			}
		}

		private IList<Assembly> GetPlugAssemblies() {
			var xResult = this.mMap.GetPlugAssemblies();
			xResult.Add(typeof(Engine).Assembly);
			return xResult;
		}

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

		private void InitializePlugs(IEnumerable<string> aPlugs) {
			if (mPlugMethods != null) {
				throw new Exception("PlugMethods list already initialized!");
			}
			if (mPlugFields != null) {
				throw new Exception("PlugFields list already initialized!");
			}
			mPlugMethods = new SortedList<string, MethodBase>();
			mPlugFields = new SortedList<Type, Dictionary<string, PlugFieldAttribute>>(new TypeComparer());
			List<Assembly> xPlugs = new List<Assembly>();
			var xComparer = new AssemblyEqualityComparer();
			foreach (string s in aPlugs) {
				Assembly a = Assembly.LoadFrom(s);
				a.GetTypes();
				if (!xPlugs.Contains(a, xComparer)) {
					xPlugs.Add(a);
				}
			}
			foreach (var item in GetPlugAssemblies()) {
				if (!xPlugs.Contains(item, xComparer)) {
					xPlugs.Add(item);
				}
			}
			foreach (Assembly xAssemblyDef in xPlugs) {
				foreach (var xType in (from item in xAssemblyDef.GetTypes()
									   let xCustomAttribs = item.GetCustomAttributes(typeof(PlugAttribute), false)
									   where xCustomAttribs != null && xCustomAttribs.Length > 0
									   select new KeyValuePair<Type, PlugAttribute>(item, (PlugAttribute)xCustomAttribs[0]))) {
					PlugAttribute xPlugAttrib = xType.Value;
					Type xTypeRef = xPlugAttrib.Target;
					if (xTypeRef == null) {
						xTypeRef = Type.GetType(xPlugAttrib.TargetName, true);
					}
					PlugFieldAttribute[] xTypePlugFields = xType.Key.GetCustomAttributes(typeof(PlugFieldAttribute), false) as PlugFieldAttribute[];
					if (xTypePlugFields != null && xTypePlugFields.Length > 0) {
						Dictionary<string, PlugFieldAttribute> xPlugFields;
						if (mPlugFields.ContainsKey(xTypeRef)) {
							xPlugFields = mPlugFields[xTypeRef];
						} else {
							mPlugFields.Add(xTypeRef, xPlugFields = new Dictionary<string, PlugFieldAttribute>());
						}
						foreach (var xPlugField in xTypePlugFields) {
							if (xPlugFields.ContainsKey(xPlugField.FieldId)) {
								throw new Exception("PlugField already defined!");
							}
							xPlugFields.Add(xPlugField.FieldId, xPlugField);
						}
					}
					foreach (MethodBase xMethod in xType.Key.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
						PlugMethodAttribute xPlugMethodAttrib = xMethod.GetCustomAttributes(typeof(PlugMethodAttribute), true).Cast<PlugMethodAttribute>().FirstOrDefault();
						string xSignature = String.Empty;
						if (xPlugMethodAttrib != null) {
							xSignature = xPlugMethodAttrib.Signature;
							if (!xPlugMethodAttrib.Enabled) {
								continue;
							}
							if (mAssembler.InMetalMode && !xPlugMethodAttrib.InMetalMode) {
								continue;
							} else {
								if (!xPlugMethodAttrib.InNormalMode) {
									continue;
								}
							}
							if (!String.IsNullOrEmpty(xSignature)) {
								if (mPlugMethods.ContainsKey(xSignature)) {
									System.Diagnostics.Debugger.Break();
								}
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

		private MethodBase GetCustomMethodImplementation(string aMethodName) {
			if (mPlugMethods.ContainsKey(aMethodName)) {
				return mPlugMethods[aMethodName];
			}
			return null;
		}

		public static TypeInformation GetTypeInfo(Type aType) {
			TypeInformation xTypeInfo;
			int xObjectStorageSize;
			Dictionary<string, TypeInformation.Field> xTypeFields = GetTypeFieldInfo(aType, out xObjectStorageSize);
			xTypeInfo = new TypeInformation(xObjectStorageSize, xTypeFields, aType, (!aType.IsValueType) && aType.IsClass);
			return xTypeInfo;
		}

		public static MethodInformation GetMethodInfo(MethodBase aCurrentMethodForArguments, MethodBase aCurrentMethodForLocals, string aMethodName, TypeInformation aTypeInfo) {
			MethodInformation xMethodInfo;
			{
				MethodInformation.Variable[] xVars = new MethodInformation.Variable[0];
				int xCurOffset = 0;
				// todo:implement check for body
				//if (aCurrentMethodForLocals.HasBody) {
				MethodBody xBody = aCurrentMethodForLocals.GetMethodBody();
				if (xBody != null) {
					xVars = new MethodInformation.Variable[xBody.LocalVariables.Count];
					foreach (LocalVariableInfo xVarDef in xBody.LocalVariables) {
						int xVarSize = GetFieldStorageSize(xVarDef.LocalType);
						if ((xVarSize % 4) != 0) {
							xVarSize += 4 - (xVarSize % 4);
						}
						xVars[xVarDef.LocalIndex] = new MethodInformation.Variable(xCurOffset, xVarSize, !xVarDef.LocalType.IsValueType, xVarDef.LocalType);
						// todo: implement support for generic parameters?
						//if (!(xVarDef.VariableType is GenericParameter)) {
						RegisterType(xVarDef.LocalType);
						//}
						xCurOffset += xVarSize;
					}
				}
				MethodInformation.Argument[] xArgs;
				if (!aCurrentMethodForArguments.IsStatic) {
					ParameterInfo[] xParameters = aCurrentMethodForArguments.GetParameters();
					xArgs = new MethodInformation.Argument[xParameters.Length + 1];
					xCurOffset = 0;
					int xArgSize;
					for (int i = xArgs.Length - 1; i > 0; i--) {
						ParameterInfo xParamDef = xParameters[i - 1];
						xArgSize = GetFieldStorageSize(xParamDef.ParameterType);
						if ((xArgSize % 4) != 0) {
							xArgSize += 4 - (xArgSize % 4);
						}
						MethodInformation.Argument.KindEnum xKind = MethodInformation.Argument.KindEnum.In;
						if (xParamDef.IsOut) {
							if (xParamDef.IsIn) {
								xKind = MethodInformation.Argument.KindEnum.ByRef;
							} else {
								xKind = MethodInformation.Argument.KindEnum.Out;
							}
						}
						xArgs[i] = new MethodInformation.Argument(xArgSize, xCurOffset, xKind, !xParamDef.ParameterType.IsValueType, xParamDef.ParameterType);
						xCurOffset += xArgSize;
					}
					xArgSize = 4;
					// this
					xArgs[0] = new MethodInformation.Argument(xArgSize, xCurOffset, MethodInformation.Argument.KindEnum.In, !aCurrentMethodForArguments.DeclaringType.IsValueType, aCurrentMethodForArguments.DeclaringType);
				} else {
					ParameterInfo[] xParameters = aCurrentMethodForArguments.GetParameters();
					xArgs = new MethodInformation.Argument[xParameters.Length];
					xCurOffset = 0;
					for (int i = xArgs.Length - 1; i >= 0; i--) {
						ParameterInfo xParamDef = xParameters[i];//xArgs.Length - i - 1];
						int xArgSize = GetFieldStorageSize(xParamDef.ParameterType);
						if ((xArgSize % 4) != 0) {
							xArgSize += 4 - (xArgSize % 4);
						}
						MethodInformation.Argument.KindEnum xKind = MethodInformation.Argument.KindEnum.In;
						if (xParamDef.IsOut) {
							if (xParamDef.IsIn) {
								xKind = MethodInformation.Argument.KindEnum.ByRef;
							} else {
								xKind = MethodInformation.Argument.KindEnum.Out;
							}
						}
						xArgs[i] = new MethodInformation.Argument(xArgSize, xCurOffset, xKind, !xParamDef.ParameterType.IsValueType, xParamDef.ParameterType);
						xCurOffset += xArgSize;
					}
				}
				int xResultSize = 0;
				//= GetFieldStorageSize(aCurrentMethodForArguments.ReturnType.ReturnType);
				MethodInfo xMethInfo = aCurrentMethodForArguments as MethodInfo;
				Type xReturnType = typeof(void);
				if (xMethInfo != null) {
					xResultSize = GetFieldStorageSize(xMethInfo.ReturnType);
					xReturnType = xMethInfo.ReturnType;
				}
				xMethodInfo = new MethodInformation(aMethodName, xVars, xArgs, xResultSize, !aCurrentMethodForArguments.IsStatic, aTypeInfo, aCurrentMethodForArguments, xReturnType);
			}
			return xMethodInfo;
		}

		public static Dictionary<string, TypeInformation.Field> GetTypeFieldInfo(MethodBase aCurrentMethod, out int aObjectStorageSize) {
			Type xCurrentInspectedType = aCurrentMethod.DeclaringType;
			return GetTypeFieldInfo(xCurrentInspectedType, out aObjectStorageSize);
		}

		private static void GetTypeFieldInfoImpl(Dictionary<string, TypeInformation.Field> aTypeFields, Type aType, ref int aObjectStorageSize) {
			Type xActualType = aType;
			Dictionary<string, PlugFieldAttribute> xCurrentPlugFieldList = new Dictionary<string, PlugFieldAttribute>();
			do {
				if (mCurrent.mPlugFields.ContainsKey(aType)) {
					var xOrigList = mCurrent.mPlugFields[aType];
					foreach (var item in xOrigList) {
						xCurrentPlugFieldList.Add(item.Key, item.Value);
					}
				}
				foreach (FieldInfo xField in aType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
					if (xField.IsStatic) {
						continue;
					}
					//if (xField.HasConstant) {
					//    Console.WriteLine("Field is constant: " + xField.GetFullName());
					//}
					// todo: add support for constants?
					PlugFieldAttribute xPlugFieldAttr = null;
					if (xCurrentPlugFieldList.ContainsKey(xField.GetFullName())) {
						xPlugFieldAttr = xCurrentPlugFieldList[xField.GetFullName()];
						xCurrentPlugFieldList.Remove(xField.GetFullName());
					}
					Type xFieldType = null;
					int xFieldSize;
					string xFieldId;
					if (xPlugFieldAttr != null) {
						xFieldType = xPlugFieldAttr.FieldType;
						xFieldId = xPlugFieldAttr.FieldId;
					} else {
						xFieldId = xField.GetFullName();
					}
					if (xFieldType == null) {
						xFieldType = xField.FieldType;
					}
					//if ((!xFieldType.IsValueType && aGCObjects && xFieldType.IsClass) || (xPlugFieldAttr != null && xPlugFieldAttr.IsExternalValue && aGCObjects)) {
					//    continue;
					//}
					if ((xFieldType.IsClass && !xFieldType.IsValueType) || (xPlugFieldAttr != null && xPlugFieldAttr.IsExternalValue)) {
						xFieldSize = 4;
					} else {
						xFieldSize = GetFieldStorageSize(xFieldType);
					}
					//}
					if (aTypeFields.ContainsKey(xFieldId)) {
						continue;
					}											 
					int xOffset = aObjectStorageSize;
					FieldOffsetAttribute xOffsetAttrib = xField.GetCustomAttributes(typeof(FieldOffsetAttribute), true).FirstOrDefault() as FieldOffsetAttribute;
					if (xOffsetAttrib != null) {
						xOffset = xOffsetAttrib.Value;
					} else {
						aObjectStorageSize += xFieldSize;
					}
					aTypeFields.Add(xField.GetFullName(), new TypeInformation.Field(xFieldSize, xFieldType.IsClass && !xFieldType.IsValueType, xFieldType, (xPlugFieldAttr != null && xPlugFieldAttr.IsExternalValue)));
				}
				while (xCurrentPlugFieldList.Count > 0) {
					var xItem = xCurrentPlugFieldList.Values.First();
					xCurrentPlugFieldList.Remove(xItem.FieldId);
					Type xFieldType = xItem.FieldType;
					int xFieldSize;
					string xFieldId = xItem.FieldId;
					if (xFieldType == null) {
						xFieldType = xItem.FieldType;
					}
					if ((xFieldType.IsClass && !xFieldType.IsValueType) || xItem.IsExternalValue) {
						xFieldSize = 4;
					} else {
						xFieldSize = GetFieldStorageSize(xFieldType);
					}
					int xOffset = aObjectStorageSize;
					aObjectStorageSize += xFieldSize;
					aTypeFields.Add(xItem.FieldId, new TypeInformation.Field(xFieldSize, xFieldType.IsClass && !xFieldType.IsValueType, xFieldType, xItem.IsExternalValue));
				}
				if (aType.FullName != "System.Object" && aType.BaseType != null) {
					aType = aType.BaseType;
				} else {
					break;
				}
			} while (true);
		}

		public static Dictionary<string, TypeInformation.Field> GetTypeFieldInfo(Type aType, out int aObjectStorageSize) {
			Dictionary<string, TypeInformation.Field> xTypeFields = new Dictionary<string, TypeInformation.Field>();
			aObjectStorageSize = 0;
			if (aType == typeof(string)) {
				System.Console.Write("");				
			}
			GetTypeFieldInfoImpl(xTypeFields, aType, ref aObjectStorageSize);
			if (aType.IsExplicitLayout) {
				var xStructLayout = aType.StructLayoutAttribute;
				if (xStructLayout.Size == 0) {
					aObjectStorageSize = (from item in xTypeFields.Values
										  let xSize = item.Offset + item.Size
										  orderby xSize
										  select xSize).FirstOrDefault();
				} else {
					aObjectStorageSize = xStructLayout.Size;
				}
			}
			int xOffset = 0;
			Dictionary<string, TypeInformation.Field> xResult = new Dictionary<string,TypeInformation.Field>();
			foreach(var item in xTypeFields.Reverse()){
				var xItem = item.Value;
				xItem.Offset = xOffset;
				xOffset += xItem.Size;
				xResult.Add(item.Key, xItem);
			}
			return xResult;
		}

		private static Op GetOpFromType(Type aType, ILReader aReader, MethodInformation aMethodInfo) {
			return (IL.Op)Activator.CreateInstance(aType, aReader, aMethodInfo);
		}

		public static void QueueStaticField(FieldInfo aField) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			if (!mCurrent.mStaticFields.ContainsKey(aField)) {
				mCurrent.mStaticFields.Add(aField, false);
			}
		}

		public static void QueueStaticField(string aAssembly, string aType, string aField, out string aFieldName) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			Type xTypeDef = GetType(aAssembly, aType);
			var xFieldDef = xTypeDef.GetField(aField);
			if (xFieldDef != null) {
				QueueStaticField(xFieldDef);
				aFieldName = DataMember.GetStaticFieldName(xFieldDef);
				return;
			}
			throw new Exception("Field not found!(" + String.Format("{0}/{1}/{2}", aAssembly, aType, aField));
		}

		public static void QueueStaticField(FieldInfo aField, out string aDataName) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			if (!aField.IsStatic) {
				throw new Exception("Cannot add an instance field to the StaticField queue!");
			}
			aDataName = DataMember.GetStaticFieldName(aField);
			QueueStaticField(aField);
		}

		// MtW: 
		//		Right now, we only support one engine at a time per AppDomain. This might be changed
		//		later. See for example NHibernate does this with the ICurrentSessionContext interface
		public static void QueueMethod(MethodBase aMethod) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			if (!aMethod.IsStatic) {
				RegisterType(aMethod.DeclaringType);
			}
			if (aMethod.DeclaringType.FullName == "System.Object" && aMethod.Name == "Equals" && aMethod.GetParameters().Length == 1 && aMethod.GetParameters()[0].ParameterType.FullName == "System.Object") {
				System.Diagnostics.Debugger.Break();
			}
			if (!mCurrent.mMethods.ContainsKey(aMethod)) {
				mCurrent.mMethods.Add(aMethod, new QueuedMethodInformation() {
					Processed = false,
					Index = mCurrent.mMethods.Count
				});
			}
		}

		public static int GetMethodIdentifier(MethodBase aMethod) {
			QueueMethod(aMethod);
			return mCurrent.mMethods[aMethod].Index;
		}

		/// <summary>
		/// Registers a type and returns the Type identifier
		/// </summary>
		/// <param name="aType"></param>
		/// <returns></returns>
		public static int RegisterType(Type aType) {
			if (aType == null) {
				throw new ArgumentNullException("aType");
			}
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			if (aType.IsArray || aType.IsPointer) {
				if (aType.IsArray && aType.GetArrayRank() != 1) {
					throw new Exception("Multidimensional arrays are not yet supported!");
				}
				aType = aType.GetElementType();
			}
			Type xFoundItem = mCurrent.mTypes.FirstOrDefault(x => x.FullName.Equals(aType.FullName));
			if (xFoundItem == null) {
				mCurrent.mTypes.Add(aType);
				if (aType.FullName != "System.Object" && aType.BaseType != null) {
					Type xCurInspectedType = aType.BaseType;
					RegisterType(xCurInspectedType);
				}
				return RegisterType(aType);
			} else {
				return mCurrent.mTypes.IndexOf(xFoundItem);
			}
		}

		public static Assembly GetCrawledAssembly() {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			return mCurrent.mCrawledAssembly;
		}

		public static void QueueMethod2(string aAssembly, string aType, string aMethod) {
			MethodBase xMethodDef;
			QueueMethod2(aAssembly, aType, aMethod, out xMethodDef);
		}

		public static void QueueMethod2(string aAssembly, string aType, string aMethod, out MethodBase aMethodDef) {
			Type xTypeDef = GetType(aAssembly, aType);
			// todo: find a way to specify one overload of a method
			int xCount = 0;
			aMethodDef = null;
			foreach (MethodBase xMethodDef in xTypeDef.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
				if (xMethodDef.Name == aMethod) {
					QueueMethod(xMethodDef);
					if (aMethodDef == null) {
						aMethodDef = xMethodDef;
					}
					xCount++;
				}
			}
			foreach (MethodBase xMethodDef in xTypeDef.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
				if (xMethodDef.Name == aMethod) {
					QueueMethod(xMethodDef);
					xCount++;
				}
			}
			if (xCount == 0) {
				throw new Exception("Method '" + aType + "." + aMethod + "' not found in assembly '" + aAssembly + "'!");
			}
		}

		public event DebugLogHandler DebugLog {
			add {
				mDebugLog += value;
			}
			remove {
				mDebugLog -= value;
			}
		}

		private void OnDebugLog(LogSeverityEnum aSeverity, string aMessage, params object[] args) {
			if (mDebugLog != null) {
				mDebugLog(aSeverity, String.Format(aMessage, args));
			}
		}

		private SortedList<string, Assembly> mAssemblyDefCache = new SortedList<string, Assembly>();

		public static Type GetType(string aAssembly, string aType) {
			Assembly xAssemblyDef;
			if (mCurrent.mAssemblyDefCache.ContainsKey(aAssembly)) {
				xAssemblyDef = mCurrent.mAssemblyDefCache[aAssembly];
			} else {
				//
				//				Assembly xAssembly = (from item in AppDomain.CurrentDomain.GetAssemblies()
				//									  where item.FullName == aAssembly || item.GetName().Name == aAssembly
				//									  select item).FirstOrDefault();
				//				if (xAssembly == null) {
				//					if (String.IsNullOrEmpty(aAssembly) || aAssembly == typeof(Engine).Assembly.GetName().Name || aAssembly == typeof(Engine).Assembly.GetName().FullName) {
				//						xAssembly = typeof(Engine).Assembly;
				//					}
				//				}
				//				if (xAssembly != null) {
				//					if (aAssembly.StartsWith("mscorlib"))
				//						throw new Exception("Shouldn't be used!");
				//					Console.WriteLine("Using AssemblyFactory for '{0}'", aAssembly);
				//					xAssemblyDef = AssemblyFactory.GetAssembly(xAssembly.Location);
				//				} else {
				//					xAssemblyDef = mCurrent.mCrawledAssembly.Resolver.Resolve(aAssembly);
				//				}
				//				mCurrent.mAssemblyDefCache.Add(aAssembly, xAssemblyDef);
				if (String.IsNullOrEmpty(aAssembly) || aAssembly == typeof(Engine).Assembly.GetName().Name || aAssembly == typeof(Engine).Assembly.GetName().FullName) {
					aAssembly = typeof(Engine).Assembly.FullName;
				}
				xAssemblyDef = Assembly.Load(aAssembly);
			}
			return GetType(xAssemblyDef, aType);
		}

		public static Type GetType(Assembly aAssembly, string aType) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			string xActualTypeName = aType;
			if (xActualTypeName.Contains("<") && xActualTypeName.Contains(">")) {
				xActualTypeName = xActualTypeName.Substring(0, xActualTypeName.IndexOf("<"));
			}
			Type xResult = aAssembly.GetType(aType, false);
			if (xResult != null) {
				RegisterType(xResult);
				return xResult;
			}
			throw new Exception("Type '" + aType + "' not found in assembly '" + aAssembly + "'!");
		}

		public static MethodBase GetMethodBase(Type aType, string aMethod, params string[] aParamTypes) {
			foreach (MethodBase xMethod in aType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
				if (xMethod.Name != aMethod) {
					continue;
				}
				ParameterInfo[] xParams = xMethod.GetParameters();
				if (xParams.Length != aParamTypes.Length) {
					continue;
				}
				bool errorFound = false;
				for (int i = 0; i < xParams.Length; i++) {
					if (xParams[i].ParameterType.FullName != aParamTypes[i]) {
						errorFound = true;
						break;
					}
				}
				if (!errorFound) {
					return xMethod;
				}
			}
			foreach (MethodBase xMethod in aType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public)) {
				if (xMethod.Name != aMethod) {
					continue;
				}
				ParameterInfo[] xParams = xMethod.GetParameters();
				if (xParams.Length != aParamTypes.Length) {
					continue;
				}
				bool errorFound = false;
				for (int i = 0; i < xParams.Length; i++) {
					if (xParams[i].ParameterType.FullName != aParamTypes[i]) {
						errorFound = true;
						break;
					}
				}
				if (!errorFound) {
					return xMethod;
				}
			}
			throw new Exception("Method not found!");
		}

		private void HandlePInvoke(MethodBase aMethod, MethodInformation aMethodInfo) {
			IL.Op xPInvokeMethodBodyOp = (IL.Op)Activator.CreateInstance(mMap.PInvokeMethodBodyOp, aMethod, aMethodInfo);
			xPInvokeMethodBodyOp.Assembler = mAssembler;
			xPInvokeMethodBodyOp.Assemble();
		}

		public static IEnumerable<Assembly> GetAllAssemblies() {
			return (from item in mCurrent.mMethods.Keys
					select item.DeclaringType.Module.Assembly).Distinct(new AssemblyEqualityComparer());
		}

		private int mInstructionsToSkip = 0;
		public static void SetInstructionsToSkip(int aCount) {
			if (mCurrent == null) {
				throw new Exception("No Current Engine!");
			}
			mCurrent.mInstructionsToSkip = aCount;
		}
	}
}