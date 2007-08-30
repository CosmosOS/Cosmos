using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU {
	public class MethodDefinitionComparer: IComparer<MethodDefinition> {
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

		public int Compare(MethodDefinition x, MethodDefinition y) {
			return GenerateFullName(x).CompareTo(GenerateFullName(y));
		}
	}

	public delegate void DebugLogHandler(string aMessage);

	public class Engine {
		protected OpCodeMap mMap = new OpCodeMap();
		private AssemblyDefinition mCrawledAssembly;
		private DebugLogHandler mDebugLog;

		/// <summary>
		/// Contains a list of all methods. This includes methods to be processed and already processed.
		/// </summary>
		protected SortedList<MethodDefinition, bool> mMethods = new SortedList<MethodDefinition, bool>(new MethodDefinitionComparer());

		/// <summary>
		/// Compiles an assembly to CPU-specific code. The entrypoint of the assembly will be 
		/// crawled to see what is neccessary, same goes for all dependencies.
		/// </summary>
		/// <param name="aAssembly">The assembly of which to crawl the entry-point method.</param>
		/// <param name="opAssembly">The assembly containing the architecture-specific implementation (x86, AMD64, etc)</param>
		/// <param name="aOutput"></param>
		public void Execute(string aAssembly, string opAssembly, BinaryWriter aOutput) {
			if (aOutput == null) {
				throw new ArgumentNullException("aOutput");
			}
			mMap.LoadOpMapFromAssembly(opAssembly);
			mCrawledAssembly = AssemblyFactory.GetAssembly(aAssembly);
			if (mCrawledAssembly.EntryPoint == null) {
				throw new NotSupportedException("Libraries are not yet supported!");
			}
			mMethods.Add(mCrawledAssembly.EntryPoint, false);
			ProcessAllMethods(aOutput);
		}

		private void ProcessAllMethods(BinaryWriter aOutput) {
			MethodDefinition xCurrentMethod;
			while ((xCurrentMethod = (from item in mMethods.Keys
																where !mMethods[item]
																select item).FirstOrDefault()) != null) {
				OnDebugLog("Processing method '{0}'", xCurrentMethod.DeclaringType.FullName + "." + xCurrentMethod.Name);
				if (xCurrentMethod.HasBody) {
					// what to do if a method doesn't have a body?
					foreach (Instruction xInstruction in xCurrentMethod.Body.Instructions) {
						if (xInstruction.OpCode.Code == Code.Callvirt) {
							//Debugger.Break();
						}
						MethodReference xMethodReference = xInstruction.Operand as MethodReference;
						if (xMethodReference != null) {
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
												AddMethodForProcessing(xMethodDef);
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
											AddMethodForProcessing(xMethodDef);
										}
									}
								} else {
									OnDebugLog("Error: Unhandled scope: " + xMethodReference.DeclaringType.Scope == null ? "**NULL**" : xMethodReference.DeclaringType.Scope.GetType().FullName);
								}
							}
						}
					}
				}
				mMethods[xCurrentMethod] = true;
			}
		}

		private void AddMethodForProcessing(MethodDefinition aMethod) {
			if (!mMethods.ContainsKey(aMethod)) {
				mMethods.Add(aMethod, false);
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