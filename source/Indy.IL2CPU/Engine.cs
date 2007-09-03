using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.IL;
using Indy.IL2CPU.IL.X86;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU {
	public class MethodDefinitionComparer: IComparer<MethodDefinition> {
		#region IComparer<MethodDefinition> Members
		public int Compare(MethodDefinition x, MethodDefinition y) {
			return GenerateFullName(x).CompareTo(GenerateFullName(y));
		}
		#endregion

		private static string GenerateFullName(MethodReference aDefinition) {
			StringBuilder sb = new StringBuilder();
			sb.Append(aDefinition.DeclaringType.FullName + "." + aDefinition.Name);
			sb.Append("(");
			foreach (ParameterDefinition param in aDefinition.Parameters) {
				sb.Append(param.ParameterType.FullName);
				sb.Append(",");
			}
			return sb.ToString().TrimEnd(',') + ")";
		}
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
		/// Compiles an assembly to CPU-specific code. The entrypoint of the assembly will be 
		/// crawled to see what is neccessary, same goes for all dependencies.
		/// </summary>
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
								mMap = new X86OpCodeMap();
								break;
							}
						default:
							throw new NotSupportedException("TargetPlatform '" + aTargetPlatform + "' not supported!");
					}
					mAssembler.OutputType = Indy.IL2CPU.Assembler.Assembler.OutputTypeEnum.Console;
					mMap.Initialize(mAssembler);
					IL.Op.QueueMethod += QueueMethod;
					try {
						mMethods.Add(mCrawledAssembly.EntryPoint, false);
						// first instructions are for calling the entrypoint
						mAssembler.Add(new JumpAlways(new Label(mCrawledAssembly.EntryPoint).Name));
						ProcessAllMethods();
					} finally {
						mAssembler.Flush();
						IL.Op.QueueMethod -= QueueMethod;
					}
				}
			} finally {
				mCurrent = null;
			}
		}

		private void ProcessAllMethods() {
			MethodDefinition xCurrentMethod;
			while ((xCurrentMethod = (from item in mMethods.Keys
									  where !mMethods[item]
									  select item).FirstOrDefault()) != null) {
				OnDebugLog("Processing method '{0}'", xCurrentMethod.DeclaringType.FullName + "." + xCurrentMethod.Name);
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
					MethodInformation.Argument[] xArgs = new MethodInformation.Argument[xCurrentMethod.Parameters.Count];
					xCurOffset = 0;
					for (int i = 0; i < xArgs.Length; i++) {
						int xArgSize = 4;
						xArgs[i] = new MethodInformation.Argument(xArgSize, xCurOffset);
						xCurOffset += xArgSize;
					}
					xMethodInfo = new MethodInformation(new Label(xCurrentMethod).Name, xVars, xArgs);
				}
				IL.Op xOp = GetOpFromType(mMap.MethodHeaderOp, null, xMethodInfo);
				xOp.Assembler = mAssembler;
				xOp.Assemble();
				// what to do if a method doesn't have a body?
				if (xCurrentMethod.HasBody) {
					// todo: add support for types which need different stack size
					foreach (Instruction xInstruction in xCurrentMethod.Body.Instructions) {
						MethodReference xMethodReference = xInstruction.Operand as MethodReference;
						if (xMethodReference != null) {
							#region add methods so that they get processed
							// TODO: find a more efficient way to get the MethodDefinition from a MethodReference
							AssemblyNameReference xAssemblyNameReference = xMethodReference.DeclaringType.Scope as AssemblyNameReference;
							if (xAssemblyNameReference != null) {
								AssemblyDefinition xReferencedMethodAssembly = mCrawledAssembly.Resolver.Resolve(xAssemblyNameReference);
								if (xReferencedMethodAssembly != null) {
									foreach (ModuleDefinition xModule in xReferencedMethodAssembly.Modules) {
										var xReferencedType = xModule.Types[xMethodReference.DeclaringType.FullName];
										if (xReferencedType != null) {
											var xMethodDef = xReferencedType.Methods.GetMethod(xMethodReference.Name, xMethodReference.Parameters);
											if (xMethodDef != null) {
												QueueMethod(xMethodDef);
											}
											var xCtorDef = xReferencedType.Constructors.GetConstructor(false, xMethodReference.Parameters);
											if (xCtorDef != null) {
												QueueMethod(xCtorDef);
											}
											break;
										}
									}
								}
							} else {
								ModuleDefinition xReferencedModule = xMethodReference.DeclaringType.Scope as ModuleDefinition;
								if (xReferencedModule != null) {
									var xReferencedType = xReferencedModule.Types[xMethodReference.DeclaringType.FullName];
									if (xReferencedType != null) {
										var xMethodDef = xReferencedType.Methods.GetMethod(xMethodReference.Name, xMethodReference.Parameters);
										if (xMethodDef != null) {
											QueueMethod(xMethodDef);
										}
									}
								} else {
									OnDebugLog("Error: Unhandled scope: " + xMethodReference.DeclaringType.Scope == null ? "**NULL**" : xMethodReference.DeclaringType.Scope.GetType().FullName);
								}
							}
							#endregion
						}
						mAssembler.Add(new Literal("; IL: " + xInstruction.OpCode.Code + " " + xInstruction.Operand));
						xOp = GetOpFromType(mMap.GetOpForOpCode(xInstruction.OpCode.Code), xInstruction, xMethodInfo);
						xOp.Assembler = mAssembler;
						xOp.Assemble();
					}
				} else {
					mAssembler.Add(new Literal("; Method not being generated yet, as it's handled by an iCall"));
				}
				xOp = GetOpFromType(mMap.MethodFooterOp, null, xMethodInfo);
				xOp.Assembler = mAssembler;
				xOp.Assemble();
				mMethods[xCurrentMethod] = true;
			}
		}

		private static IL.Op GetOpFromType(Type aType, Instruction aInstruction, MethodInformation aMethodInfo) {
			return (IL.Op)Activator.CreateInstance(aType, aInstruction, aMethodInfo);
		}

		// MtW: 
		//		Right now, we only support one engine at a time per AppDomain. This might be changed
		//		later. See for example NHibernate does this with the ICurrentSessionContext interface
		public static void QueueMethod(MethodDefinition aMethod) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			if (!mCurrent.mMethods.ContainsKey(aMethod)) {
				mCurrent.mMethods.Add(aMethod, false);
			}
		}

		public static void QueueMethod(string aAssembly, string aType, string aMethod) {
			if (mCurrent == null) {
				throw new Exception("ERROR: No Current Engine found!");
			}
			var xAssemblyDef = mCurrent.mCrawledAssembly.Resolver.Resolve(aAssembly);
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
			// todo: find a way to specify one overload of a method
			int xCount = 0;
			foreach (MethodDefinition xMethodDef in xTypeDef.Methods) {
				if (xMethodDef.Name == aMethod) {
					QueueMethod(xMethodDef);
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
	}
}