// this file supports the VERBOSE_DEBUG define. this makes it emit a bunch of comments in the assembler output.
// note that the tests are supposed to NOT include these comments
#define VERBOSE_DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

	public delegate void DebugLogHandler(string aMessage);

	public enum TargetPlatformEnum {
		x86
	}

	public class Engine {
		protected static Engine mCurrent;
		protected AssemblyDefinition mCrawledAssembly;
		protected DebugLogHandler mDebugLog;
		protected OpCodeMap mMap;
		protected Assembler.Assembler mAssembler;

		/// <summary>
		/// Contains a list of all methods. This includes methods to be processed and already processed.
		/// </summary>
		protected SortedList<MethodDefinition, bool> mMethods = new SortedList<MethodDefinition, bool>(new MethodDefinitionComparer());
		/// <summary>
		/// Contains a list of all static fields. This includes static fields to be processed and already processed.
		/// </summary>
		protected SortedList<FieldDefinition, bool> mStaticFields = new SortedList<FieldDefinition, bool>(new FieldDefinitionComparer());

		/// <summary>
		/// Contains a list of custom method implementations. This is mainly used for iCalls
		/// </summary>
		protected SortedList<string, MethodInfo> mCustomMethodImplementation = new SortedList<string, MethodInfo>();


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
				using (mAssembler = new Assembler.X86.Assembler(aOutput)) {
					switch (aTargetPlatform) {
						case TargetPlatformEnum.x86: {
								mMap = (OpCodeMap)Activator.CreateInstance(Type.GetType("Indy.IL2CPU.IL.X86.X86OpCodeMap, Indy.IL2CPU.IL.X86", true));
								break;
							}
						default:
							throw new NotSupportedException("TargetPlatform '" + aTargetPlatform + "' not supported!");
					}
					mAssembler.OutputType = Indy.IL2CPU.Assembler.Assembler.OutputTypeEnum.Console;
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
						mMethods.Add(RuntimeEngineRefs.InitializeApplicationRef, false);
						mMethods.Add(RuntimeEngineRefs.FinalizeApplicationRef, false);
						mMethods.Add(mCrawledAssembly.EntryPoint, false);
						// initialize the runtime engine
						mAssembler.Add(
							new Assembler.X86.Call(new Label(RuntimeEngineRefs.InitializeApplicationRef).Name),
							new Assembler.X86.Call(new Label(mCrawledAssembly.EntryPoint).Name));
						if (mCrawledAssembly.EntryPoint.ReturnType.ReturnType.FullName.StartsWith("System.Void", StringComparison.InvariantCultureIgnoreCase)) {
							mAssembler.Add(new Pushd("0"));
						} else {
							mAssembler.Add(new Pushd("eax"));
						}
						mAssembler.Add(new Assembler.X86.Call(new Label(RuntimeEngineRefs.FinalizeApplicationRef).Name));
						ProcessAllMethods();
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

		public static MethodDefinition GetDefinitionFromMethodReference(MethodReference aRef) {
			TypeDefinition xTypeDef = GetDefinitionFromTypeReference(aRef.DeclaringType);
			MethodDefinition xMethod = xTypeDef.Methods.GetMethod(aRef.Name, aRef.Parameters);
			if (xMethod != null) {
				return xMethod;
			}
			xMethod = xTypeDef.Constructors.GetConstructor(aRef.Name == MethodDefinition.Cctor, aRef.Parameters);
			if (xMethod != null) {
				return xMethod;
			}
			throw new Exception("Couldn't find Method! ('" + aRef.GetFullName() + "'");
		}

		public static TypeDefinition GetDefinitionFromTypeReference(TypeReference aRef) {
			if (mCurrent == null) {
				throw new Exception("No Current engine found!");
			}
			if(aRef.FullName.Contains("modreq")) {
				aRef = aRef.GetOriginalType();
			}
			if (aRef.FullName.EndsWith("[]")) {
				return GetTypeDefinition(aRef.Module.Assembly.Name.Name, aRef.FullName.Substring(0, aRef.FullName.Length - 2));
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
					}
				}
			} else {
				ModuleDefinition xReferencedModule = aRef.Scope as ModuleDefinition;
				if (xReferencedModule != null) {
					var xReferencedType = xReferencedModule.Types[aRef.FullName];
					if (xReferencedType != null) {
						return xReferencedType;
					}
				} else {
					mCurrent.OnDebugLog("Error: Unhandled scope: " + aRef.Scope == null ? "**NULL**" : aRef.Scope.GetType().FullName);
				}
			}
			throw new Exception("Could not find TypeDefinition! (" + aRef.FullName + " in assembly " + aRef.Module.Assembly.Name.Name + ")");
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
			}
			throw new Exception("Unable to determine ValueType size!");
		}

		private void ProcessAllStaticFields() {
			FieldDefinition xCurrentField;
			while ((xCurrentField = (from item in mStaticFields.Keys
									 where !mStaticFields[item]
									 select item).FirstOrDefault()) != null) {
				string xFieldName = xCurrentField.GetFullName();
				OnDebugLog("Processing Static Field '{0}', Constant = '{1}'({2})", xFieldName, xCurrentField.Constant, xCurrentField.Constant == null ? "**NULL**" : xCurrentField.Constant.GetType().FullName);
				xFieldName = DataMember.GetStaticFieldName(xCurrentField);
				if (xCurrentField.HasConstant) {
					// emit the constant, but first find out how we get it.
					System.Diagnostics.Debugger.Break();
				} else {
					if (xCurrentField.InitialValue != null && xCurrentField.InitialValue.Length > 0) {
						string xTheData = "";
						if(xCurrentField.InitialValue.Length>4) {
							xTheData = "0,0,0,0,2,0,0,0,";
						}
						foreach (byte x in BitConverter.GetBytes(xCurrentField.InitialValue.Length)) {
							xTheData += x + ",";
						}
						foreach (byte x in xCurrentField.InitialValue) {
							xTheData += x + ",";
						}
						xTheData = xTheData.TrimEnd(',');
						mAssembler.DataMembers.Add(new DataMember(xFieldName, "db", xTheData));
					} else {
						uint xTheSize;
						string theType = "db";
						if (xCurrentField.FieldType.IsValueType) {
							xTheSize = GetFieldStorageSize(xCurrentField.FieldType);
						} else {
							xTheSize = 4;
						}
						if (xTheSize == 4) {
							theType = "dd";
						}
						string xTheData = "";
						for (uint i = 0; i < xTheSize; i++) {
							xTheData += "0,";
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
									  where !mMethods[item]
									  select item).FirstOrDefault()) != null) {
				OnDebugLog("Processing method '{0}'", xCurrentMethod.GetFullName());
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
						if (xCurrentMethod.GetFullName() == "System.String..ctor(System.Char[],System.Int32,System.Int32)") {
							Console.Beep();
						}
						uint xObjectStorageSize;
						SortedList<string, TypeInformation.Field> xTypeFields = GetTypeFieldInfo(xCurrentMethod, out xObjectStorageSize);
						
						xTypeInfo = new TypeInformation(xObjectStorageSize, xTypeFields);
					}
				}
				MethodInformation xMethodInfo;
				{
					MethodInformation.Variable[] xVars = new MethodInformation.Variable[0];
					int xCurOffset = 0;
					if (xCurrentMethod.HasBody) {
						xVars = new MethodInformation.Variable[xCurrentMethod.Body.Variables.Count];
						foreach (VariableDefinition xVarDef in xCurrentMethod.Body.Variables) {
							int xVarSize = 4;
							xVars[xVarDef.Index] = new MethodInformation.Variable(xCurOffset, xVarSize);
							xCurOffset += xVarSize;
						}
					}
					MethodInformation.Argument[] xArgs;
					if (!xCurrentMethod.IsStatic) {
						xArgs = new MethodInformation.Argument[xCurrentMethod.Parameters.Count + 1];
						xCurOffset = 0;
						int xArgSize;
						for (int i = xArgs.Length - 1; i > 0; i--) {
							xArgSize = 4;
							ParameterDefinition xParamDef = xCurrentMethod.Parameters[xArgs.Length - i - 1];
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
						xArgs = new MethodInformation.Argument[xCurrentMethod.Parameters.Count];
						xCurOffset = 0;
						for (int i = xArgs.Length - 1; i >= 0; i--) {
							int xArgSize = 4;
							ParameterDefinition xParamDef = xCurrentMethod.Parameters[xArgs.Length - i - 1];
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
					xMethodInfo = new MethodInformation(xMethodName, xVars, xArgs, !xCurrentMethod.ReturnType.ReturnType.FullName.Contains("System.Void"), !xCurrentMethod.IsStatic, xTypeInfo);
				}
				IL.Op xOp = GetOpFromType(mMap.MethodHeaderOp, null, xMethodInfo);
				xOp.Assembler = mAssembler;
#if VERBOSE_DEBUG
				string comment = "Method: " + xCurrentMethod + "\r\n";
				if (xCurrentMethod.Body == null) {
					comment += "  (No locals)\r\n";
				} else {
					comment += "  Locals:\r\n";
					foreach (VariableDefinition xVarDef in xCurrentMethod.Body.Variables) {
						comment += String.Format("    [{0}] {1}\r\n", xVarDef.Index, xVarDef.Name);
					}
				}
				comment += "  Args:\r\n";
				foreach (ParameterDefinition xParamDef in xCurrentMethod.Parameters) {
					comment += String.Format("    [{0}] {1}\r\n", xParamDef.Sequence, xParamDef.Name);
				}
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
								xOp.Assembler = mAssembler;
								xOp.Assemble();
							}
						} else {
							if (xCurrentMethod.IsPInvokeImpl) {
								HandlePInvoke(xCurrentMethod, xMethodInfo);
							} else {
								Console.WriteLine("\t-- Method not handled!");
								mAssembler.Add(new Literal("; Method not being generated yet, as it's handled by an iCall"));
							}
						}
					}
				}
				xOp = GetOpFromType(mMap.MethodFooterOp, null, xMethodInfo);
				xOp.Assembler = mAssembler;
				xOp.Assemble();
				mMethods[xCurrentMethod] = true;
			}
		}

		public static SortedList<string, TypeInformation.Field> GetTypeFieldInfo(MethodDefinition aCurrentMethod, out uint aObjectStorageSize) {
			SortedList<string, TypeInformation.Field> xTypeFields = new SortedList<string, TypeInformation.Field>();
			aObjectStorageSize = ObjectImpl.FieldDataOffset;
			TypeDefinition xCurrentInspectedType = GetDefinitionFromTypeReference(aCurrentMethod.DeclaringType);
			do {
				foreach (FieldDefinition xField in xCurrentInspectedType.Fields) {
					if (xField.IsStatic) {
						continue;
					}
					TypeDefinition xFieldType = GetDefinitionFromTypeReference(xField.FieldType);
					uint xFieldSize;
					if (xFieldType.IsClass) {
						xFieldSize = 4;
					} else {
						xFieldSize = GetFieldStorageSize(xFieldType);
					}
					xTypeFields.Add(xField.ToString(), new TypeInformation.Field(aObjectStorageSize, xFieldSize));
					aObjectStorageSize += xFieldSize;
				}
				if (xCurrentInspectedType.FullName != "System.Object") {
					xCurrentInspectedType = GetDefinitionFromTypeReference(xCurrentInspectedType.BaseType);
				} else {
					break;
				}
			} while (true);
			if (aCurrentMethod.DeclaringType.FullName == "System.String") {
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
			AssemblyDefinition xReferencedFieldAssembly;
			if (String.IsNullOrEmpty(aAssembly) || aAssembly == typeof(RuntimeEngine).Assembly.GetName().FullName) {
				xReferencedFieldAssembly = RuntimeEngineRefs.RuntimeAssemblyDef;
			} else {
				xReferencedFieldAssembly = mCurrent.mCrawledAssembly.Resolver.Resolve(aAssembly);
			}
			if (xReferencedFieldAssembly != null) {
				foreach (ModuleDefinition xModule in xReferencedFieldAssembly.Modules) {
					var xReferencedType = xModule.Types[aType];
					if (xReferencedType != null) {
						var xFieldDef = xReferencedType.Fields.GetField(aField);
						if (xFieldDef != null) {
							QueueStaticField(xFieldDef);
							aFieldName = DataMember.GetStaticFieldName(xFieldDef);
							return;
						}
					}
				}
			}
			throw new Exception("Field not found!");
		}

		public static void QueueStaticField(FieldReference aFieldRef) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			AssemblyNameReference xAssemblyNameReference = aFieldRef.DeclaringType.Scope as AssemblyNameReference;
			if (xAssemblyNameReference != null) {
				AssemblyDefinition xReferencedFieldAssembly;
				if (xAssemblyNameReference.FullName == typeof(RuntimeEngine).Assembly.GetName().FullName) {
					xReferencedFieldAssembly = RuntimeEngineRefs.RuntimeAssemblyDef;
				} else {
					xReferencedFieldAssembly = mCurrent.mCrawledAssembly.Resolver.Resolve(xAssemblyNameReference);
				}
				if (xReferencedFieldAssembly != null) {
					foreach (ModuleDefinition xModule in xReferencedFieldAssembly.Modules) {
						var xReferencedType = xModule.Types[aFieldRef.DeclaringType.FullName];
						if (xReferencedType != null) {
							var xFieldDef = xReferencedType.Fields.GetField(aFieldRef.Name);
							if (xFieldDef != null) {
								QueueStaticField(xFieldDef);
							}
							break;
						}
					}
				}
			} else {
				ModuleDefinition xReferencedModule = aFieldRef.DeclaringType.Scope as ModuleDefinition;
				if (xReferencedModule != null) {
					var xReferencedType = xReferencedModule.Types[aFieldRef.DeclaringType.FullName];
					if (xReferencedType != null) {
						var xFieldDef = xReferencedType.Fields.GetField(aFieldRef.Name);
						if (xFieldDef != null) {
							QueueStaticField(xFieldDef);
						}
					}
				} else {
					mCurrent.OnDebugLog("Error: Unhandled scope: " + aFieldRef.DeclaringType.Scope == null ? "**NULL**" : aFieldRef.DeclaringType.Scope.GetType().FullName);
				}
			}
		}

		// MtW: 
		//		Right now, we only support one engine at a time per AppDomain. This might be changed
		//		later. See for example NHibernate does this with the ICurrentSessionContext interface
		public static void QueueMethod(MethodDefinition aMethod) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			MethodDefinition xActualMethod = aMethod;
			if (!mCurrent.mMethods.ContainsKey(xActualMethod)) {
				mCurrent.mMethods.Add(xActualMethod, false);
			}
		}

		public static void QueueMethodRef(MethodReference aMethod) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			AssemblyNameReference xAssemblyNameReference = aMethod.DeclaringType.Scope as AssemblyNameReference;
			if (xAssemblyNameReference != null) {
				AssemblyDefinition xAssemblyDef;
				if (xAssemblyNameReference.FullName == typeof(Engine).Assembly.GetName().FullName) {
					xAssemblyDef = AssemblyFactory.GetAssembly(typeof(Engine).Assembly.Location);
				} else {
					xAssemblyDef = mCurrent.mCrawledAssembly.Resolver.Resolve(xAssemblyNameReference.FullName);
				}
				if (xAssemblyDef != null) {
					foreach (ModuleDefinition xModule in xAssemblyDef.Modules) {
						var xReferencedType = xModule.Types[aMethod.DeclaringType.FullName];
						if (xReferencedType != null) {
							var xMethodDef = xReferencedType.Methods.GetMethod(aMethod.Name, aMethod.Parameters);
							if (xMethodDef != null) {
								QueueMethod(xMethodDef);
								return;
							}
							var xCtorDef = xReferencedType.Constructors.GetConstructor(false, aMethod.Parameters);
							if (xCtorDef != null) {
								QueueMethod(xCtorDef);
								return;
							}
						}
					}
				}
			} else {
				ModuleDefinition xReferencedModule = aMethod.DeclaringType.Scope as ModuleDefinition;
				if (xReferencedModule != null) {
					var xReferencedType = xReferencedModule.Types[aMethod.DeclaringType.FullName];
					if (xReferencedType != null) {
						var xMethodDef = xReferencedType.Methods.GetMethod(aMethod.Name, aMethod.Parameters);
						if (xMethodDef != null) {
							QueueMethod(xMethodDef);
							return;
						}
						var xCtorDef = xReferencedType.Constructors.GetConstructor(false, aMethod.Parameters);
						if (xCtorDef != null) {
							QueueMethod(xCtorDef);
							return;
						}
					}
				} else {
					mCurrent.OnDebugLog("Error: Unhandled scope: " + aMethod.DeclaringType.Scope == null ? "**NULL**" : aMethod.DeclaringType.Scope.GetType().FullName);
				}
			}
			throw new Exception("Method not found: '" + aMethod.ToString() + "'");
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

		private void OnDebugLog(string aMessage, params object[] args) {
			if (mDebugLog != null) {
				mDebugLog(String.Format(aMessage, args));
			}
		}

		public static TypeDefinition GetTypeDefinition(string aAssembly, string aType) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			AssemblyDefinition xAssemblyDef;
			if (String.IsNullOrEmpty(aAssembly) || aAssembly == typeof(Engine).Assembly.GetName().Name || aAssembly == typeof(Engine).Assembly.GetName().FullName) {
				xAssemblyDef = AssemblyFactory.GetAssembly(typeof(Engine).Assembly.Location);
			} else {
				xAssemblyDef = mCurrent.mCrawledAssembly.Resolver.Resolve(aAssembly);
			}
			TypeDefinition xTypeDef = null;
			foreach (ModuleDefinition xModDef in xAssemblyDef.Modules) {
				if (xModDef.Types.Contains(aType)) {
					xTypeDef = xModDef.Types[aType];
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