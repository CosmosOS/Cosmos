// this file supports the VERBOSE_DEBUG define. this makes it emit a bunch of comments in the assembler output.
// note that the tests are supposed to NOT include these comments
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
using Mono.Cecil;
using Mono.Cecil.Cil;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU {
	public class MethodDefinitionComparer: IComparer<MethodDefinition> {
		#region IComparer<MethodDefinition> Members
		public int Compare(MethodDefinition x, MethodDefinition y) {
			return x.GetFullName().CompareTo(y.GetFullName());
		}
		#endregion
	}

	public class FieldDefinitionComparer: IComparer<FieldDefinition> {
		#region IComparer<FieldDefinition> Members
		public int Compare(FieldDefinition x, FieldDefinition y) {
			return x.GetFullName().CompareTo(y.GetFullName());
		}
		#endregion
	}

	public class TypeDefinitionComparer: IComparer<TypeDefinition> {
		public int Compare(TypeDefinition x, TypeDefinition y) {
			return x.FullName.CompareTo(y.FullName);
		}
	}

	public class TypeDefinitionEqualityComparer: IEqualityComparer<TypeDefinition> {
		public bool Equals(TypeDefinition x, TypeDefinition y) {
			return x.FullName.Equals(y.FullName);
		}

		public int GetHashCode(TypeDefinition obj) {
			return obj.FullName.GetHashCode();
		}
	}

	public enum LogSeverityEnum {
		Informational,
		Warning
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
		protected AssemblyDefinition mCrawledAssembly;
		protected DebugLogHandler mDebugLog;
		protected OpCodeMap mMap;
		protected Assembler.NativeX86.Assembler mAssembler;

		/// <summary>
		/// Contains a list of all methods. This includes methods to be processed and already processed.
		/// </summary>
		protected SortedList<MethodDefinition, QueuedMethodInformation> mMethods = new SortedList<MethodDefinition, QueuedMethodInformation>(new MethodDefinitionComparer());
		/// <summary>
		/// Contains a list of all static fields. This includes static fields to be processed and already processed.
		/// </summary>
		protected SortedList<FieldDefinition, bool> mStaticFields = new SortedList<FieldDefinition, bool>(new FieldDefinitionComparer());

		/// <summary>
		/// Contains a list of custom method implementations. This is mainly used for iCalls
		/// </summary>
		protected SortedList<string, MethodInfo> mCustomMethodImplementation = new SortedList<string, MethodInfo>();

		protected List<TypeDefinition> mTypes = new List<TypeDefinition>();
		protected TypeDefinitionEqualityComparer mTypesEqualityComparer = new TypeDefinitionEqualityComparer();

		/// <summary>
		/// Compiles an assembly to CPU-specific code. The entrypoint of the assembly will be 
		/// crawled to see what is neccessary, same goes for all dependencies.
		/// </summary>
		/// <remarks>For now, only entrypoints without params and return code are supported!</remarks>
		/// <param name="aAssembly">The assembly of which to crawl the entry-point method.</param>
		/// <param name="aTargetPlatform">The platform to target when assembling the code.</param>
		/// <param name="aOutput"></param>
		public void Execute(string aAssembly, TargetPlatformEnum aTargetPlatform, StreamWriter aOutput) {
			mCurrent = this;
			try {
				if (aOutput == null) {
					throw new ArgumentNullException("aOutput");
				}
				mCrawledAssembly = AssemblyFactory.GetAssembly(aAssembly);
				if (mCrawledAssembly.EntryPoint == null) {
					throw new NotSupportedException("Libraries are not supported!");
				}
				using (mAssembler = new Assembler.NativeX86.Assembler(aOutput, true)) {
					switch (aTargetPlatform) {
						case TargetPlatformEnum.Win32: {
								mMap = (OpCodeMap)Activator.CreateInstance(Type.GetType("Indy.IL2CPU.IL.Win32.Win32OpCodeMap, Indy.IL2CPU.IL.Win32", true));
								break;
							}
						case TargetPlatformEnum.NativeX86: {
								mMap = (OpCodeMap)Activator.CreateInstance(Type.GetType("Indy.IL2CPU.IL.NativeX86.NativeX86OpCodeMap, Indy.IL2CPU.IL.NativeX86", true));
								break;
							}
						default:
							throw new NotSupportedException("TargetPlatform '" + aTargetPlatform + "' not supported!");
					}
					//mAssembler.OutputType = Assembler.Win32.Assembler.OutputTypeEnum.Console;
					mMap.Initialize(mAssembler);
					foreach (Type t in typeof(Engine).Assembly.GetTypes()) {
						foreach (MethodInfo mi in t.GetMethods()) {
							object[] xAttribs = mi.GetCustomAttributes(typeof(MethodAliasAttribute), true);
							if (xAttribs != null && xAttribs.Length > 0) {
								MethodAliasAttribute xMethodAlias = (MethodAliasAttribute)xAttribs[0];
								mCustomMethodImplementation.Add(xMethodAlias.Name, mi);
							}
						}
					}
					IL.Op.QueueMethod += QueueMethod;
					IL.Op.QueueStaticField += QueueStaticField;
					try {
						mTypes.Add(GetTypeDefinition("mscorlib", "System.Object"));
						mMethods.Add(RuntimeEngineRefs.InitializeApplicationRef, new QueuedMethodInformation() {
							Processed = false,
							Index = mMethods.Count
						});
						mMethods.Add(RuntimeEngineRefs.FinalizeApplicationRef, new QueuedMethodInformation() {
							Processed = false,
							Index = mMethods.Count
						});
						//						mMethods.Add(VTablesImplRefs.LoadTypeTableRef, new QueuedMethodInformation() {
						//							Processed = false,
						//							Index = mMethods.Count
						//						});
						//						mMethods.Add(VTablesImplRefs.SetMethodInfoRef, new QueuedMethodInformation() {
						//							Processed = false,
						//							Index = mMethods.Count
						//						});
						//						mMethods.Add(VTablesImplRefs.SetTypeInfoRef, new QueuedMethodInformation() {
						//							Processed = false,
						//							Index = mMethods.Count
						//						});
						//						mMethods.Add(VTablesImplRefs.GetMethodAddressForTypeRef, new QueuedMethodInformation() {
						//							Processed = false,
						//							Index = mMethods.Count
						//						});
						mMethods.Add(mCrawledAssembly.EntryPoint, new QueuedMethodInformation() {
							Processed = false,
							Index = mMethods.Count
						});
						// initialize the runtime engine
						MainEntryPointOp xEntryPointOp = (MainEntryPointOp)GetOpFromType(mMap.MainEntryPointOp, null, null);
						xEntryPointOp.Assembler = mAssembler;
						xEntryPointOp.Call(RuntimeEngineRefs.InitializeApplicationRef);
						//new Assembler.X86.Call("____INIT__VMT____");
						xEntryPointOp.Call(mCrawledAssembly.EntryPoint);
						if (mCrawledAssembly.EntryPoint.ReturnType.ReturnType.FullName.StartsWith("System.Void", StringComparison.InvariantCultureIgnoreCase)) {
							xEntryPointOp.Pushd("0");
						}
						xEntryPointOp.Call(RuntimeEngineRefs.FinalizeApplicationRef);
						ProcessAllMethods();
						//						do {
						//							int xOldCount = mMethods.Count;
						//							ScanForMethodToIncludeForVMT();
						//							ProcessAllMethods();
						//							if (xOldCount == mMethods.Count) {
						//								break;
						//							}
						//						} while (true);
						//						GenerateVMT();
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
			// todo: abstract this code generation out to IL.* implementation
			Op xOp = GetOpFromType(mMap.MethodHeaderOp, null, new MethodInformation("____INIT__VMT____", new MethodInformation.Variable[0], new MethodInformation.Argument[0], false, false, null));
			xOp.Assembler = mAssembler;
			xOp.Assemble();
			InitVmtImplementationOp xInitVmtOp = (InitVmtImplementationOp)GetOpFromType(mMap.InitVmtImplementationOp, null, null);
			xInitVmtOp.Assembler = mAssembler;
			xInitVmtOp.Types = mTypes;
			xInitVmtOp.SetTypeInfoRef = VTablesImplRefs.SetTypeInfoRef;
			xInitVmtOp.SetMethodInfoRef = VTablesImplRefs.SetMethodInfoRef;
			xInitVmtOp.LoadTypeTableRef = VTablesImplRefs.LoadTypeTableRef;
			xInitVmtOp.Methods = mMethods.Keys;
			xInitVmtOp.GetMethodIdentifier += delegate(MethodDefinition aMethod) {
				TypeReference[] xParams = new TypeReference[aMethod.Parameters.Count];
				for (int i = 0; i < aMethod.Parameters.Count; i++) {
					xParams[i] = aMethod.Parameters[i].ParameterType;
				}
				MethodDefinition xMethod = GetUltimateBaseMethod(aMethod, null, GetDefinitionFromTypeReference(aMethod.DeclaringType));
				if (xMethod == null) {
					xMethod = aMethod;
				}
				return GetMethodIdentifier(xMethod);
			};
			xInitVmtOp.Assemble();
			xOp = GetOpFromType(mMap.MethodFooterOp, null, new MethodInformation("____INIT__VMT____", new MethodInformation.Variable[0], new MethodInformation.Argument[0], false, false, null));
			xOp.Assembler = mAssembler;
			xOp.Assemble();
		}



		private void ScanForMethodToIncludeForVMT() {
			List<TypeDefinition> xCheckedTypes = new List<TypeDefinition>();
			foreach (MethodDefinition xMethod in mMethods.Keys) {
				if (xMethod.IsStatic) {
					continue;
				}
				TypeDefinition xCurrentType = GetDefinitionFromTypeReference(xMethod.DeclaringType);
				if (!xCheckedTypes.Contains(xCurrentType, mTypesEqualityComparer)) {
					xCheckedTypes.Add(xCurrentType);
				}
			}
			foreach (TypeDefinition xType in mTypes) {
				if (!xCheckedTypes.Contains(xType, mTypesEqualityComparer)) {
					xCheckedTypes.Add(xType);
				}
			}
			for (int i = 0; i < xCheckedTypes.Count; i++) {
				TypeDefinition xCurrentType = xCheckedTypes[i];
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
					xCurrentType = GetDefinitionFromTypeReference(xCurrentType.BaseType);
				}
			}
			foreach (TypeDefinition xTD in xCheckedTypes) {
				foreach (MethodDefinition xMethod in xTD.Methods) {
					if (!xMethod.IsStatic) {
						if (xTD.BaseType == null) {
							continue;
						}
						if (xMethod.IsVirtual) {
							TypeDefinition xCurrentInspectedType = GetDefinitionFromTypeReference(xTD.BaseType);
							TypeReference[] xMethodParams = new TypeReference[xMethod.Parameters.Count];
							for (int i = 0; i < xMethod.Parameters.Count; i++) {
								xMethodParams[i] = xMethod.Parameters[i].ParameterType;
							}
							MethodDefinition xBaseMethod = GetUltimateBaseMethod(xMethod, xMethodParams, xCurrentInspectedType);
							if (xBaseMethod != null) {
								if (mMethods.ContainsKey(xBaseMethod)) {
									QueueMethod(xMethod);
								}
							}
						}
					}
				}
			}
		}

		private static MethodDefinition GetUltimateBaseMethod(MethodDefinition aMethod, TypeReference[] aMethodParams, TypeDefinition aCurrentInspectedType) {
			MethodDefinition xBaseMethod = null;
			do {
				MethodDefinition xFoundMethod = aCurrentInspectedType.Methods.GetMethod(aMethod.Name, aMethodParams);
				if (xFoundMethod != null) {
					if (xFoundMethod.IsVirtual == aMethod.IsVirtual && xFoundMethod.IsPrivate == false && xFoundMethod.IsPublic == aMethod.IsPublic
						&& xFoundMethod.IsFamily == aMethod.IsFamily && xFoundMethod.IsFamilyAndAssembly == aMethod.IsFamilyAndAssembly
						&& xFoundMethod.IsFamilyOrAssembly == aMethod.IsFamilyOrAssembly && xFoundMethod.IsFinal == false) {
						xBaseMethod = xFoundMethod;
					}
				}
				if (aCurrentInspectedType.BaseType == null) {
					break;
				}
				aCurrentInspectedType = GetDefinitionFromTypeReference(aCurrentInspectedType.BaseType);
			} while (true);
			return xBaseMethod;
		}

		public static MethodDefinition GetDefinitionFromMethodReference(MethodReference aRef) {
			TypeDefinition xTypeDef;
			bool xIsArray = false;
			if (aRef.DeclaringType.FullName.Contains("[]") || aRef.DeclaringType.FullName.Contains("[,]") || aRef.DeclaringType.FullName.Contains("[,,]")) {
				xTypeDef = GetTypeDefinition("mscorlib", "System.Array");
				xIsArray = true;
			} else {
				xTypeDef = GetDefinitionFromTypeReference(aRef.DeclaringType);
			}
			MethodDefinition xMethod = null;
			if (xIsArray) {
				if (aRef.Name == "Get") {
					xMethod = xTypeDef.Methods.GetMethod("GetValue", aRef.Parameters);
				}
				if (aRef.Name == "Set") {
					xMethod = xTypeDef.Methods.GetMethod("SetValue", aRef.Parameters);
				}
			}
			if (xMethod == null) {
				foreach (MethodDefinition xFoundMethod in xTypeDef.Methods) {
					if (xFoundMethod.Name != aRef.Name) {
						continue;
					}
					if (xFoundMethod.ReturnType.ReturnType.FullName != aRef.ReturnType.ReturnType.FullName) {
						continue;
					}
					if (xFoundMethod.Parameters.Count != aRef.Parameters.Count) {
						continue;
					}
					bool xMismatch = false;
					for (int i = 0; i < xFoundMethod.Parameters.Count; i++) {
						if (xFoundMethod.Parameters[i].ParameterType.FullName != aRef.Parameters[i].ParameterType.FullName) {
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
			xMethod = xTypeDef.Constructors.GetConstructor(aRef.Name == MethodDefinition.Cctor, aRef.Parameters);
			if (xMethod != null) {
				return xMethod;
			}
			throw new Exception("Couldn't find Method! ('" + aRef.GetFullName() + "'");
		}

		public static FieldDefinition GetDefinitionFromFieldReference(FieldReference aRef) {
			if (aRef == null) {
				throw new ArgumentNullException("aRef");
			}
			if (mCurrent == null) {
				throw new Exception("No Current engine found!");
			}
			FieldDefinition xTempResult = aRef as FieldDefinition;
			if (xTempResult != null) {
				return xTempResult;
			}
			TypeDefinition xTypeDef = GetDefinitionFromTypeReference(aRef.DeclaringType);
			FieldDefinition xResult = xTypeDef.Fields.GetField(aRef.Name);
			if (xResult == null) {
				throw new Exception("Field not Found! (" + aRef.Name + ")");
			}
			return xResult;
		}

		public static TypeDefinition GetDefinitionFromTypeReference(TypeReference aRef) {
			if (aRef == null) {
				throw new ArgumentNullException("aRef");
			}
			if (mCurrent == null) {
				throw new Exception("No Current engine found!");
			}
			TypeDefinition xTempResult = aRef as TypeDefinition;
			if (xTempResult != null) {
				return xTempResult;
			}
			GenericParameter xGenParam = aRef as GenericParameter;
			if (xGenParam != null) {
				// todo: add support for generics
				return GetTypeDefinition("mscorlib", "System.Object");
			}
			TypeSpecification xTypeSpec = aRef as TypeSpecification;
			if (xTypeSpec != null) {
				return GetDefinitionFromTypeReference(xTypeSpec.ElementType);
			}
			if (aRef.FullName.Contains("modreq")) {
				aRef = aRef.GetOriginalType();
			}
			AssemblyNameReference xAssemblyNameReference = aRef.Scope as AssemblyNameReference;
			if (xAssemblyNameReference != null) {
				AssemblyDefinition xReferencedFieldAssembly;
				if (xAssemblyNameReference.FullName == typeof(RuntimeEngine).Assembly.GetName().FullName) {
					xReferencedFieldAssembly = RuntimeEngineRefs.RuntimeAssemblyDef;
				} else {
					xReferencedFieldAssembly = mCurrent.mCrawledAssembly.Resolver.Resolve(xAssemblyNameReference);
				}
				if (xReferencedFieldAssembly != null) {
					foreach (ModuleDefinition xModule in xReferencedFieldAssembly.Modules) {
						var xReferencedType = xModule.Types[aRef.FullName];
						if (xReferencedType != null) {
							return xReferencedType;
						}
						{
							string theName = aRef.FullName;
							while (theName.EndsWith("[]")) {
								theName = theName.Substring(0, theName.Length - 2);
							}
							xReferencedType = xModule.Types[theName];
							if (xReferencedType != null) {
								return xReferencedType;
							}
						}
						{
							string theName = aRef.FullName;
							while (theName.EndsWith("*")) {
								theName = theName.Substring(0, theName.Length - 1);
							}
							xReferencedType = xModule.Types[theName];
							if (xReferencedType != null) {
								return xReferencedType;
							}
						}
						{
							string theName = aRef.FullName;
							while (theName.EndsWith("&")) {
								theName = theName.Substring(0, theName.Length - 1);
							}
							xReferencedType = xModule.Types[theName];
							if (xReferencedType != null) {
								return xReferencedType;
							}
						}
						{
							string theName = aRef.FullName;
							if (theName.Contains("<") && theName.Contains(">")) {
								theName = theName.Substring(0, theName.IndexOf("<"));
							}
							xReferencedType = xModule.Types[theName];
							if (xReferencedType != null) {
								return xReferencedType;
							}
						}
					}
				}
			} else {
				ModuleDefinition xReferencedModule = aRef.Scope as ModuleDefinition;
				if (xReferencedModule != null) {
					var xReferencedType = xReferencedModule.Types[aRef.FullName];
					if (xReferencedType != null) {
						return xReferencedType;
					}
					{
						string theName = aRef.FullName;
						while (theName.EndsWith("[]")) {
							theName = theName.Substring(0, theName.Length - 2);
						}
						xReferencedType = xReferencedModule.Types[theName];
						if (xReferencedType != null) {
							return xReferencedType;
						}
					}
					{
						string theName = aRef.FullName;
						while (theName.EndsWith("*")) {
							theName = theName.Substring(0, theName.Length - 1);
						}
						xReferencedType = xReferencedModule.Types[theName];
						if (xReferencedType != null) {
							return xReferencedType;
						}
					}
					{
						string theName = aRef.FullName;
						while (theName.EndsWith("&")) {
							theName = theName.Substring(0, theName.Length - 1);
						}
						xReferencedType = xReferencedModule.Types[theName];
						if (xReferencedType != null) {
							return xReferencedType;
						}
					}
					{
						string theName = aRef.FullName;
						if (theName.Contains("<") && theName.Contains(">")) {
							theName = theName.Substring(0, theName.IndexOf("<"));
						}
						xReferencedType = xReferencedModule.Types[theName];
						if (xReferencedType != null) {
							return xReferencedType;
						}
					}
				} else {
					try {
						string theScopeText = aRef.Scope == null ? "**NULL**" : aRef.Scope.GetType().FullName;
						mCurrent.OnDebugLog(LogSeverityEnum.Informational, "Error: Unhandled scope: " + theScopeText);
					} catch (NullReferenceException) {
						string theScopeText = aRef.Scope == null ? "**NULL**" : aRef.Scope.GetType().FullName;
						mCurrent.OnDebugLog(LogSeverityEnum.Informational, "Error: Unhandled scope: " + theScopeText);
					} catch {
						throw;
					}
				}
			}
			string xModuleName = "**NOT DEFINED**";
			if (aRef.Module != null && aRef.Module.Assembly != null) {
				xModuleName = aRef.Module.Assembly.Name.Name;
			}
			throw new Exception("Could not find TypeDefinition! (" + aRef.FullName + " in assembly " + xModuleName + ". TypeReference type = '" + aRef.GetType().FullName + "')");
		}

		/// <summary>
		/// Gives the size to store an instance of the <paramref name="aType"/> for use in a field.
		/// </summary>
		/// <remarks>For classes, this is the pointer size.</remarks>
		/// <param name="aType"></param>
		/// <returns></returns>
		public static uint GetFieldStorageSize(TypeReference aType) {
			if (!aType.IsValueType) {
				return 4;
			}
			switch (aType.FullName) {
				case "System.Char":
					return 4;
				case "System.Byte":
				case "System.SByte":
					return 4;
				case "System.UInt16":
				case "System.Int16":
					return 4;
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
					return 4;
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
			TypeDefinition xTypeDef = GetDefinitionFromTypeReference(aType);
			if (xTypeDef.IsEnum) {
				return GetFieldStorageSize(xTypeDef.Fields.GetField("value__").FieldType);
			}
			uint xResult;
			GetTypeFieldInfo(xTypeDef, out xResult);
			return xResult;
		}

		private void ProcessAllStaticFields() {
			FieldDefinition xCurrentField;
			while ((xCurrentField = (from item in mStaticFields.Keys
									 where !mStaticFields[item]
									 select item).FirstOrDefault()) != null) {
				string xFieldName = xCurrentField.GetFullName();
				OnDebugLog(LogSeverityEnum.Informational, "Processing Static Field '{0}', Constant = '{1}'({2})", xFieldName, xCurrentField.Constant, xCurrentField.Constant == null ? "**NULL**" : xCurrentField.Constant.GetType().FullName);
				xFieldName = DataMember.GetStaticFieldName(xCurrentField);
				RegisterType(GetDefinitionFromTypeReference(xCurrentField.FieldType));
				if (xCurrentField.HasConstant) {
					// emit the constant, but first find out how we get it.
				} else {
					if (xCurrentField.InitialValue != null && xCurrentField.InitialValue.Length > 0) {
						string xTheData = "";
						uint xStorageSize = GetFieldStorageSize(xCurrentField.FieldType);

						if (xCurrentField.InitialValue.Length > 4) {
							xTheData = "0,0,0,0,2,0,0,0,";
						}
						foreach (byte x in BitConverter.GetBytes(xCurrentField.InitialValue.Length)) {
							xTheData += x + ",";
						}
						foreach (byte x in xCurrentField.InitialValue) {
							xTheData += x + ",";
						}
						xTheData = xTheData.TrimEnd(',');
						if (xTheData.Length == 0) {
							throw new Exception("Field '" + xCurrentField.ToString() + "' doesn't have a valid size!");
						}
						mAssembler.DataMembers.Add(new DataMember(xFieldName, "db", xTheData));
					} else {
						uint xTheSize;
						string theType = "db";
						if (!GetDefinitionFromTypeReference(xCurrentField.FieldType).IsClass) {
							xTheSize = GetFieldStorageSize(xCurrentField.FieldType);
						} else {
							xTheSize = 4;
						}
						if (xTheSize == 4) {
							theType = "dd";
							xTheSize = 1;
						}
						string xTheData = "";
						for (uint i = 0; i < xTheSize; i++) {
							xTheData += "0,";
						}
						if (xTheSize == 0) {
							throw new Exception("Field '" + xCurrentField.ToString() + "' doesn't have a valid size!");
						}
						xTheData = xTheData.TrimEnd(',');
						mAssembler.DataMembers.Add(new DataMember(xFieldName, theType, xTheData));
					}
				}
				mStaticFields[xCurrentField] = true;
			}
		}

		private void ProcessAllMethods() {
			MethodDefinition xCurrentMethod;
			while ((xCurrentMethod = (from item in mMethods.Keys
									  where !mMethods[item].Processed
									  select item).FirstOrDefault()) != null) {
				OnDebugLog(LogSeverityEnum.Informational, "Processing method '{0}'", xCurrentMethod.GetFullName());
				if (xCurrentMethod.IsAbstract) {
					mMethods[xCurrentMethod].Processed = true;
					continue;
				}
				string xMethodName = new Label(xCurrentMethod).Name;
				foreach (CustomAttribute xAttrib in xCurrentMethod.CustomAttributes) {
					if (xAttrib.Constructor.DeclaringType.FullName == typeof(MethodAliasAttribute).FullName) {
						//xMethodName = (string)xAttrib.Fields["Name"];
						break;
					}
				}
				TypeInformation xTypeInfo = null;
				{
					if (!xCurrentMethod.IsStatic) {
						uint xObjectStorageSize;
						SortedList<string, TypeInformation.Field> xTypeFields = GetTypeFieldInfo(xCurrentMethod, out xObjectStorageSize);
						xTypeInfo = new TypeInformation(xObjectStorageSize, xTypeFields);
					}
				}
				MethodInformation xMethodInfo = GetMethodInfo(xCurrentMethod, xCurrentMethod, xMethodName, xTypeInfo);
				IL.Op xOp = GetOpFromType(mMap.MethodHeaderOp, null, xMethodInfo);
				xOp.Assembler = mAssembler;
#if VERBOSE_DEBUG
				string comment = xMethodInfo.ToString();
				foreach (string s in comment.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)) {
					mAssembler.Add(new Literal(";" + s));
				}
#endif
				xOp.Assemble();
				bool xIsCustomImplementation = false;
				MethodDefinition xCustomImplementation = null;
				if (mCustomMethodImplementation.ContainsKey(xCurrentMethod.GetFullName())) {
					xIsCustomImplementation = true;
					MethodInfo xReplacementMethod = mCustomMethodImplementation[xCurrentMethod.GetFullName()];
					string[] xParamTypes = new string[xReplacementMethod.GetParameters().Length];
					for (int i = 0; i < xReplacementMethod.GetParameters().Length; i++) {
						xParamTypes[i] = xReplacementMethod.GetParameters()[i].ParameterType.FullName;
					}
					xCustomImplementation = GetMethodDefinition(GetTypeDefinition(xReplacementMethod.DeclaringType.Assembly.FullName, xReplacementMethod.DeclaringType.FullName), xReplacementMethod.Name, xParamTypes);
					if (xCustomImplementation == null) {
						throw new Exception("CustomMethodImplementation not found!");
					}
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
					if (Enum.GetNames(typeof(CustomMethodEnum)).Contains(xMethodName)) {
						CustomMethodImplementationOp xCustomMethodImplOp = (CustomMethodImplementationOp)GetOpFromType(mMap.CustomMethodImplementationOp, null, xMethodInfo);
						xCustomMethodImplOp.Assembler = mAssembler;
						xCustomMethodImplOp.Method = (CustomMethodEnum)Enum.Parse(typeof(CustomMethodEnum), xMethodName);
						xCustomMethodImplOp.Assemble();
					} else {
						if (xCurrentMethod.HasBody) {
							// todo: add support for types which need different stack size
							foreach (Instruction xInstruction in xCurrentMethod.Body.Instructions) {
								MethodReference xMethodReference = xInstruction.Operand as MethodReference;
								if (xMethodReference != null) {
									QueueMethodRef(xMethodReference);
								}
								xOp = GetOpFromType(mMap.GetOpForOpCode(xInstruction.OpCode.Code), xInstruction, xMethodInfo);
								if ((!xOp.SupportsMetalMode) && mAssembler.InMetalMode) {
									throw new Exception("OpCode '" + xInstruction.OpCode.Code + "' not supported in Metal mode!");
								}
								xOp.Assembler = mAssembler;
								xOp.Assemble();
							}
						} else {
							if (xCurrentMethod.IsPInvokeImpl) {
								HandlePInvoke(xCurrentMethod, xMethodInfo);
							} else {
								OnDebugLog(LogSeverityEnum.Warning, "Method '{0}' not generated!", xCurrentMethod.GetFullName());
								mAssembler.Add(new Literal("; Method not being generated yet, as it's handled by an iCall"));
							}
						}
					}
				}
				xOp = GetOpFromType(mMap.MethodFooterOp, null, xMethodInfo);
				xOp.Assembler = mAssembler;
				xOp.Assemble();
				mMethods[xCurrentMethod].Processed = true;
			}
		}

		public static MethodInformation GetMethodInfo(MethodDefinition aCurrentMethodForArguments, MethodDefinition aCurrentMethodForLocals, string aMethodName, TypeInformation aTypeInfo) {
			MethodInformation xMethodInfo;
			{
				MethodInformation.Variable[] xVars = new MethodInformation.Variable[0];
				int xCurOffset = 0;
				if (aCurrentMethodForLocals.HasBody) {
					xVars = new MethodInformation.Variable[aCurrentMethodForLocals.Body.Variables.Count];
					foreach (VariableDefinition xVarDef in aCurrentMethodForLocals.Body.Variables) {
						int xVarSize = 4;
						xVars[xVarDef.Index] = new MethodInformation.Variable(xCurOffset, xVarSize, GetDefinitionFromTypeReference(xVarDef.VariableType).IsClass);
						if (!(xVarDef.VariableType is GenericParameter)) {
							RegisterType(GetDefinitionFromTypeReference(xVarDef.VariableType));
						}
						xCurOffset += xVarSize;
					}
				}
				MethodInformation.Argument[] xArgs;
				if (!aCurrentMethodForArguments.IsStatic) {
					xArgs = new MethodInformation.Argument[aCurrentMethodForArguments.Parameters.Count + 1];
					xCurOffset = 0;
					int xArgSize;
					for (int i = xArgs.Length - 1; i > 0; i--) {
						xArgSize = 4;
						ParameterDefinition xParamDef = aCurrentMethodForArguments.Parameters[xArgs.Length - i - 1];
						MethodInformation.Argument.KindEnum xKind = MethodInformation.Argument.KindEnum.In;
						if (xParamDef.IsOut) {
							if (xParamDef.IsIn) {
								xKind = MethodInformation.Argument.KindEnum.ByRef;
							} else {
								xKind = MethodInformation.Argument.KindEnum.Out;
							}
						}
						xArgs[i] = new MethodInformation.Argument(xArgSize, xCurOffset, xKind);
						xCurOffset += xArgSize;
					}
					xArgSize = 4;
					// this
					xArgs[0] = new MethodInformation.Argument(xArgSize, xCurOffset, MethodInformation.Argument.KindEnum.In);
				} else {
					xArgs = new MethodInformation.Argument[aCurrentMethodForArguments.Parameters.Count];
					xCurOffset = 0;
					for (int i = xArgs.Length - 1; i >= 0; i--) {
						int xArgSize = 4;
						ParameterDefinition xParamDef = aCurrentMethodForArguments.Parameters[xArgs.Length - i - 1];
						MethodInformation.Argument.KindEnum xKind = MethodInformation.Argument.KindEnum.In;
						if (xParamDef.IsOut) {
							if (xParamDef.IsIn) {
								xKind = MethodInformation.Argument.KindEnum.ByRef;
							} else {
								xKind = MethodInformation.Argument.KindEnum.Out;
							}
						}
						xArgs[i] = new MethodInformation.Argument(xArgSize, xCurOffset, xKind);
						xCurOffset += xArgSize;
					}
				}
				xMethodInfo = new MethodInformation(aMethodName, xVars, xArgs, !aCurrentMethodForArguments.ReturnType.ReturnType.FullName.Contains("System.Void"), aCurrentMethodForArguments.HasThis, aTypeInfo);
			}
			return xMethodInfo;
		}

		public static SortedList<string, TypeInformation.Field> GetTypeFieldInfo(MethodDefinition aCurrentMethod, out uint aObjectStorageSize) {
			TypeDefinition xCurrentInspectedType = GetDefinitionFromTypeReference(aCurrentMethod.DeclaringType);
			return GetTypeFieldInfo(xCurrentInspectedType, out aObjectStorageSize);
		}

		public static SortedList<string, TypeInformation.Field> GetTypeFieldInfo(TypeDefinition aType, out uint aObjectStorageSize) {
			SortedList<string, TypeInformation.Field> xTypeFields = new SortedList<string, TypeInformation.Field>();
			TypeDefinition xActualType = aType;
			aObjectStorageSize = 0;
			do {
				foreach (FieldDefinition xField in aType.Fields) {
					if (xField.IsStatic) {
						continue;
					}
					uint xFieldSize;
					TypeSpecification xTypeSpec = xField.FieldType as TypeSpecification;
					if (xTypeSpec != null) {
						xFieldSize = 4;
						RegisterTypeRef(xTypeSpec.ElementType);
					} else {
						TypeDefinition xFieldType = GetDefinitionFromTypeReference(xField.FieldType);
						if (xFieldType.IsClass) {
							xFieldSize = 4;
						} else {
							xFieldSize = GetFieldStorageSize(xField.FieldType);
						}
					}
					xTypeFields.Add(xField.ToString(), new TypeInformation.Field(aObjectStorageSize, xFieldSize));
					aObjectStorageSize += xFieldSize;
				}
				if (aType.FullName != "System.Object" && aType.BaseType != null) {
					aType = GetDefinitionFromTypeReference(aType.BaseType);
				} else {
					break;
				}
			} while (true);
			if (xActualType.FullName == "System.String") {
				xTypeFields.Add("$$Storage$$", new TypeInformation.Field(aObjectStorageSize, 4));
				aObjectStorageSize += 4;
			}
			return xTypeFields;
		}

		private static Op GetOpFromType(Type aType, Instruction aInstruction, MethodInformation aMethodInfo) {
			return (IL.Op)Activator.CreateInstance(aType, aInstruction, aMethodInfo);
		}

		public static void QueueStaticField(FieldDefinition aField) {
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
			TypeDefinition xTypeDef = GetTypeDefinition(aAssembly, aType);
			var xFieldDef = xTypeDef.Fields.GetField(aField);
			if (xFieldDef != null) {
				QueueStaticField(xFieldDef);
				aFieldName = DataMember.GetStaticFieldName(xFieldDef);
				return;
			}
			throw new Exception("Field not found!(" + String.Format("{0}/{1}/{2}", aAssembly, aType, aField));
		}

		public static void QueueStaticField(FieldReference aFieldRef, out string aDataName) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			aDataName = "";
			FieldDefinition xFieldDef = GetDefinitionFromFieldReference(aFieldRef);
			aDataName = DataMember.GetStaticFieldName(xFieldDef);
			QueueStaticField(xFieldDef);
		}

		// MtW: 
		//		Right now, we only support one engine at a time per AppDomain. This might be changed
		//		later. See for example NHibernate does this with the ICurrentSessionContext interface
		public static void QueueMethod(MethodDefinition aMethod) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			if (!aMethod.IsStatic) {
				RegisterType(GetDefinitionFromTypeReference(aMethod.DeclaringType));
			}
			if (!mCurrent.mMethods.ContainsKey(aMethod)) {
				mCurrent.mMethods.Add(aMethod, new QueuedMethodInformation() {
					Processed = false,
					Index = mCurrent.mMethods.Count
				});
			}
		}

		public static int GetMethodIdentifier(MethodDefinition aMethod) {
			QueueMethod(aMethod);
			return mCurrent.mMethods[aMethod].Index;
		}

		public static int RegisterTypeRef(TypeReference aTypeRef) {
			if (aTypeRef == null) {
				throw new ArgumentNullException("aTypeRef");
			}
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			TypeSpecification xTypeSpec = aTypeRef as TypeSpecification;
			if (xTypeSpec != null) {
				return RegisterTypeRef(xTypeSpec.ElementType);
			}
			if (aTypeRef is GenericParameter) {
				return -1;
			}
			return RegisterType(GetDefinitionFromTypeReference(aTypeRef));
		}

		/// <summary>
		/// Registers a type and returns the Type identifier
		/// </summary>
		/// <param name="aType"></param>
		/// <returns></returns>
		public static int RegisterType(TypeDefinition aType) {
			if (aType == null) {
				throw new ArgumentNullException("aType");
			}
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			TypeDefinition xFoundItem = mCurrent.mTypes.FirstOrDefault(x => x.FullName.Equals(aType.FullName));
			if (xFoundItem == null) {
				mCurrent.mTypes.Add(aType);
				if (aType.FullName != "System.Object" && aType.BaseType != null) {
					TypeDefinition xCurInspectedType = GetDefinitionFromTypeReference(aType.BaseType);
					RegisterType(xCurInspectedType);
				}
				return RegisterType(aType);
			} else {
				return mCurrent.mTypes.IndexOf(xFoundItem);
			}
		}

		public static void QueueMethodRef(MethodReference aMethod) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			QueueMethod(GetDefinitionFromMethodReference(aMethod));
		}

		public static void QueueMethod2(string aAssembly, string aType, string aMethod) {
			MethodDefinition xMethodDef;
			QueueMethod2(aAssembly, aType, aMethod, out xMethodDef);
		}

		public static void QueueMethod2(string aAssembly, string aType, string aMethod, out MethodDefinition aMethodDef) {
			TypeDefinition xTypeDef = GetTypeDefinition(aAssembly, aType);
			// todo: find a way to specify one overload of a method
			int xCount = 0;
			aMethodDef = null;
			foreach (MethodDefinition xMethodDef in xTypeDef.Methods) {
				if (xMethodDef.Name == aMethod) {
					QueueMethod(xMethodDef);
					if (aMethodDef == null) {
						aMethodDef = xMethodDef;
					}
					xCount++;
				}
			}
			foreach (MethodDefinition xMethodDef in xTypeDef.Constructors) {
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

		public static TypeDefinition GetTypeDefinition(string aAssembly, string aType) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			AssemblyDefinition xAssemblyDef;
			Assembly xAssembly = (from item in AppDomain.CurrentDomain.GetAssemblies()
								  where item.FullName == aAssembly || item.GetName().Name == aAssembly
								  select item).FirstOrDefault();
			if (xAssembly == null) {
				if (String.IsNullOrEmpty(aAssembly) || aAssembly == typeof(Engine).Assembly.GetName().Name || aAssembly == typeof(Engine).Assembly.GetName().FullName) {
					xAssembly = typeof(Engine).Assembly;
				}
			}
			if (xAssembly != null) {
				xAssemblyDef = AssemblyFactory.GetAssembly(xAssembly.Location);
			} else {
				xAssemblyDef = mCurrent.mCrawledAssembly.Resolver.Resolve(aAssembly);
			}
			TypeDefinition xTypeDef = null;
			string xActualTypeName = aType;
			if (xActualTypeName.Contains("<") && xActualTypeName.Contains(">")) {
				xActualTypeName = xActualTypeName.Substring(0, xActualTypeName.IndexOf("<"));
			}
			foreach (ModuleDefinition xModDef in xAssemblyDef.Modules) {
				if (xModDef.Types.Contains(xActualTypeName)) {
					xTypeDef = xModDef.Types[xActualTypeName];
					break;
				}
			}
			if (xTypeDef == null) {
				throw new Exception("Type '" + aType + "' not found in assembly '" + aAssembly + "'!");
			}
			return xTypeDef;
		}

		public static MethodDefinition GetMethodDefinition(TypeDefinition aType, string aMethod, params string[] aParamTypes) {
			foreach (MethodDefinition xMethod in aType.Methods) {
				if (xMethod.Name != aMethod) {
					continue;
				}
				if (xMethod.Parameters.Count != aParamTypes.Length) {
					continue;
				}
				bool errorFound = false;
				for (int i = 0; i < xMethod.Parameters.Count; i++) {
					if (xMethod.Parameters[i].ParameterType.FullName != aParamTypes[i]) {
						errorFound = true;
						break;
					}
				}
				if (!errorFound) {
					return xMethod;
				}
			}
			foreach (MethodDefinition xMethod in aType.Constructors) {
				if (xMethod.Name != aMethod) {
					continue;
				}
				if (xMethod.Parameters.Count != aParamTypes.Length) {
					continue;
				}
				bool errorFound = false;
				for (int i = 0; i < xMethod.Parameters.Count; i++) {
					if (xMethod.Parameters[i].ParameterType.FullName != aParamTypes[i]) {
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

		private void HandlePInvoke(MethodDefinition aMethod, MethodInformation aMethodInfo) {
			IL.Op xPInvokeMethodBodyOp = (IL.Op)Activator.CreateInstance(mMap.PInvokeMethodBodyOp, aMethod, aMethodInfo);
			xPInvokeMethodBodyOp.Assembler = mAssembler;
			xPInvokeMethodBodyOp.Assemble();
		}
	}
}