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
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Diagnostics.SymbolStore;
using Microsoft.Samples.Debugging.CorSymbolStore;
using System.Threading;

namespace Indy.IL2CPU {
    public enum DebugMode { None, IL, Source, MLUsingGDB }

    public class MethodBaseComparer : IComparer<MethodBase> {
        #region IComparer<MethodBase> Members

        public int Compare(MethodBase x,
                           MethodBase y) {
            return x.GetFullName().CompareTo(y.GetFullName());
        }

        #endregion
    }

    public class FieldInfoComparer : IComparer<FieldInfo> {
        #region IComparer<FieldInfo> Members

        public int Compare(FieldInfo x,
                           FieldInfo y) {
            return x.GetFullName().CompareTo(y.GetFullName());
        }

        #endregion
    }

    public class TypeComparer : IComparer<Type> {
        public int Compare(Type x,
                           Type y) {
            return x.AssemblyQualifiedName.CompareTo(y.AssemblyQualifiedName);
        }
    }

    public class TypeEqualityComparer : IEqualityComparer<Type> {
        public bool Equals(Type x,
                           Type y) {
            return x.FullName.Equals(y.FullName);
        }

        public int GetHashCode(Type obj) {
            return obj.FullName.GetHashCode();
        }
    }

    public class AssemblyEqualityComparer : IEqualityComparer<Assembly> {
        public bool Equals(Assembly x,
                           Assembly y) {
            return x.GetName().FullName.Equals(y.GetName().FullName);
        }

        public int GetHashCode(Assembly obj) {
            return obj.GetName().FullName.GetHashCode();
        }
    }

    public enum LogSeverityEnum { Warning, Error }

    public delegate void DebugLogHandler(LogSeverityEnum aSeverity, string aMessage);

    public enum TargetPlatformEnum { X86 }

    public enum TraceAssemblies {All, Cosmos, User};

    public class QueuedMethodInformation {
        public bool Processed;
        public bool PreProcessed;
        public int Index;
        public MLDebugSymbol[] Instructions;
        public readonly SortedList<string, object> Info = new SortedList<string, object>(StringComparer.InvariantCultureIgnoreCase);
        public MethodBase Implementation;
    }

    public class QueuedStaticFieldInformation {
        public bool Processed;
    }

    public class Engine {
        protected static Engine mCurrent;
        protected Assembly mCrawledAssembly;
        protected DebugLogHandler mDebugLog;
        protected OpCodeMap mMap;
        protected Assembler.Assembler mAssembler;
        
        public TraceAssemblies TraceAssemblies { get; set; }

        private SortedList<string, MethodBase> mPlugMethods;
        private SortedList<Type, Dictionary<string, PlugFieldAttribute>> mPlugFields;

        /// <summary>
        /// Contains a list of all methods. This includes methods to be processed and already processed.
        /// </summary>
        protected IDictionary<MethodBase, QueuedMethodInformation> mMethods = new SortedList<MethodBase, QueuedMethodInformation>(new MethodBaseComparer());
        protected ReaderWriterLocker mMethodsLocker = new ReaderWriterLocker();

        /// <summary>
        /// Contains a list of all static fields. This includes static fields to be processed and already processed.
        /// </summary>
        protected IDictionary<FieldInfo, QueuedStaticFieldInformation> mStaticFields = new SortedList<FieldInfo, QueuedStaticFieldInformation>(new FieldInfoComparer());
        protected ReaderWriterLocker mStaticFieldsLocker = new ReaderWriterLocker();
        protected IList<Type> mTypes = new List<Type>();
        protected ReaderWriterLocker mTypesLocker = new ReaderWriterLocker();
        protected TypeEqualityComparer mTypesEqualityComparer = new TypeEqualityComparer();
        private byte mDebugComport;
        private DebugMode mDebugMode;
        private List<MLDebugSymbol> mSymbols = new List<MLDebugSymbol>();
        private ReaderWriterLocker mSymbolsLocker = new ReaderWriterLocker();
        private string mOutputDir;

        public event Action<int, int> CompilingMethods;
        public event Action<int, int> CompilingStaticFields;

        /// <summary>
        /// Compiles an assembly to CPU-specific code. The entrypoint of the assembly will be 
        /// crawled to see what is neccessary, same goes for all dependencies.
        /// </summary>
        /// <remarks>For now, only entrypoints without params are supported!</remarks>
        /// <param name="aAssembly">The assembly of which to crawl the entry-point method.</param>
        /// <param name="aTargetPlatform">The platform to target when assembling the code.</param>
        /// <param name="aOutput"></param>
        /// <param name="aInMetalMode">Whether or not the output is metalmode only.</param>
        
        //TODO: Way too many params, these should be properties
        public void Execute(string aAssembly,
                            TargetPlatformEnum aTargetPlatform,
                            Func<string, string> aGetFileNameForGroup,
                            bool aInMetalMode,
                            IEnumerable<string> aPlugs,
                            DebugMode aDebugMode,
                            byte aDebugComNumber,
                            string aOutputDir) {
            mCurrent = this;
            try {
                if (aGetFileNameForGroup == null) {
                    throw new ArgumentNullException("aGetFileNameForGroup");
                }
                mCrawledAssembly = Assembly.LoadFile(aAssembly);
                mDebugMode = aDebugMode;
                MethodInfo xEntryPoint = (MethodInfo)mCrawledAssembly.EntryPoint;
                if (xEntryPoint == null) {
                    throw new NotSupportedException("No EntryPoint found!");
                }
                mOutputDir = aOutputDir;

                Type xEntryPointType = xEntryPoint.DeclaringType;
                xEntryPoint = xEntryPointType.GetMethod("Init", new Type[0]);
                mDebugComport = aDebugComNumber;
                //List<string> xSearchDirs = new List<string>(new string[] { Path.GetDirectoryName(aAssembly), aAssemblyDir });
                //xSearchDirs.AddRange((from item in aPlugs
                //                      select Path.GetDirectoryName(item)).Distinct());
                switch (aTargetPlatform) {
                    case TargetPlatformEnum.X86: {
                        mMap = (OpCodeMap)Activator.CreateInstance(Type.GetType("Indy.IL2CPU.IL.X86.X86OpCodeMap, Indy.IL2CPU.IL.X86",
                                                                                true));
                        mAssembler = new Assembler.X86.Assembler(aGetFileNameForGroup,
                                                                 aInMetalMode,
                                                                 ((aDebugMode != DebugMode.None) && (aDebugMode != DebugMode.MLUsingGDB))
                                                                     ? aDebugComNumber
                                                                     : (byte)0);
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
                    //!String.IsNullOrEmpty(aDebugSymbols);
                    IL.Op.QueueMethod += QueueMethod;
                    IL.Op.QueueStaticField += QueueStaticField;
                    try {
                        using (mTypesLocker.AcquireWriterLock()) {
                            mTypes.Add(typeof(object));
                        }
                        using (mMethodsLocker.AcquireWriterLock()) {
                            mMethods.Add(RuntimeEngineRefs.InitializeApplicationRef,
                                         new QueuedMethodInformation() {
                                                                           Processed = false,
                                                                           Index = mMethods.Count
                                                                       });
                            mMethods.Add(RuntimeEngineRefs.FinalizeApplicationRef,
                                         new QueuedMethodInformation() {
                                                                           Processed = false,
                                                                           Index = mMethods.Count
                                                                       });
                            mMethods.Add(typeof(Assembler.Assembler).GetMethod("PrintException"),
                                         new QueuedMethodInformation() {
                                                                           Index = mMethods.Count
                                                                       });
                            if (!aInMetalMode) {
                                mMethods.Add(VTablesImplRefs.LoadTypeTableRef,
                                             new QueuedMethodInformation() {
                                                                               Processed = false,
                                                                               Index = mMethods.Count
                                                                           });
                                mMethods.Add(VTablesImplRefs.SetMethodInfoRef,
                                             new QueuedMethodInformation() {
                                                                               Processed = false,
                                                                               Index = mMethods.Count
                                                                           });
                                mMethods.Add(VTablesImplRefs.IsInstanceRef,
                                             new QueuedMethodInformation() {
                                                                               Processed = false,
                                                                               Index = mMethods.Count
                                                                           });
                                mMethods.Add(VTablesImplRefs.SetTypeInfoRef,
                                             new QueuedMethodInformation() {
                                                                               Processed = false,
                                                                               Index = mMethods.Count
                                                                           });
                                mMethods.Add(VTablesImplRefs.GetMethodAddressForTypeRef,
                                             new QueuedMethodInformation() {
                                                                               Processed = false,
                                                                               Index = mMethods.Count
                                                                           });
                                mMethods.Add(GCImplementationRefs.IncRefCountRef,
                                             new QueuedMethodInformation() {
                                                                               Processed = false,
                                                                               Index = mMethods.Count
                                                                           });
                                mMethods.Add(GCImplementationRefs.DecRefCountRef,
                                             new QueuedMethodInformation() {
                                                                               Processed = false,
                                                                               Index = mMethods.Count
                                                                           });
                                mMethods.Add(GCImplementationRefs.AllocNewObjectRef,
                                             new QueuedMethodInformation() {
                                                                               Processed = false,
                                                                               Index = mMethods.Count
                                                                           });
                            }
                            mMethods.Add(xEntryPoint,
                                         new QueuedMethodInformation() {
                                                                           Processed = false,
                                                                           Index = mMethods.Count
                                                                       });
                        }
                        ScanAllMethods();
                        ScanAllStaticFields(); 
                        mMap.PreProcess(mAssembler);
                        if (!aInMetalMode) {
                            do {
                                int xOldCount;
                                using (mMethodsLocker.AcquireReaderLock()) {
                                    xOldCount = mMethods.Count;
                                }
                                ScanAllMethods();
                                ScanAllStaticFields();
                                ScanForMethodsToIncludeForVMT();
                                int xNewCount; 
                                using (mMethodsLocker.AcquireReaderLock())
                                {
                                    xNewCount= mMethods.Count;
                                }
                                if (xOldCount == xNewCount) {
                                    break;
                                }
                            } while (true);
                            mAssembler.CurrentGroup = "main";
                        }
                        // initialize the runtime engine
                        mAssembler.CurrentGroup = "main";
                        MainEntryPointOp xEntryPointOp = (MainEntryPointOp)GetOpFromType(mMap.MainEntryPointOp,
                                                                                         null,
                                                                                         null);
                        xEntryPointOp.Assembler = mAssembler;
                        xEntryPointOp.Enter(Assembler.Assembler.EntryPointName);
                        xEntryPointOp.Call(RuntimeEngineRefs.InitializeApplicationRef);
                        if (!aInMetalMode) {
                            xEntryPointOp.Call("____INIT__VMT____");
                        }
                        using (mTypesLocker.AcquireWriterLock()) {
                            foreach (Type xType in mTypes) {
                                foreach (MethodBase xMethod in xType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)) {
                                    if (xMethod.IsStatic) {
                                        xEntryPointOp.Call(xMethod);
                                    }
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
                        using (mMethodsLocker.AcquireWriterLock()) {
                            mMethods = new ReadOnlyDictionary<MethodBase, QueuedMethodInformation>(mMethods);
                        }
                        using (mStaticFieldsLocker.AcquireWriterLock()) {
                            mStaticFields = new ReadOnlyDictionary<FieldInfo, QueuedStaticFieldInformation>(mStaticFields);
                        }
                        ProcessAllMethods();
                        mMap.PostProcess(mAssembler);
                        ProcessAllStaticFields();
                        if (!aInMetalMode) {
                            GenerateVMT(mDebugMode != DebugMode.None);
                        }
                        using (mSymbolsLocker.AcquireReaderLock()) {
                            if (mSymbols != null) {
                                string xOutputFile = Path.Combine(mOutputDir, "debug.cxdb");
                                MLDebugSymbol.WriteSymbolsListToFile(mSymbols, xOutputFile);
                            }
                        }
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

        // EDIT BELOW TO CHANGE THREAD COUNT:
        private int mThreadCount = Environment.ProcessorCount;
        private AutoResetEvent[] mThreadEvents = new AutoResetEvent[Environment.ProcessorCount];

        private void ScanAllMethods() {
            for (int i = 0; i < mThreadCount; i++) {
                mThreadEvents[i] = new AutoResetEvent(false);
                var xThread = new Thread(DoScanMethods);
                xThread.Start(i);
            }
            int xFinishedThreads = 0;
            while (xFinishedThreads < mThreadCount) {
                for (int i = 0; i < mThreadCount; i++) {
                    if (mThreadEvents[i] != null) {
                        if (mThreadEvents[i].WaitOne(10, false)) {
                            mThreadEvents[i].Close();
                            mThreadEvents[i] = null;
                            xFinishedThreads++;
                        }
                    }
                }
            }
        }

        private void DoScanMethods(object aData) {
            //ProgressChanged.Invoke("Scanning methods");
            int xThreadIndex = (int)aData;
            try {
                int xIndex = -1;
                MethodBase xCurrentMethod;
                while (true) {
                    xIndex++;
                    if ((xIndex % mThreadCount) != xThreadIndex) {
                        continue;
                    }
                    using (mMethodsLocker.AcquireReaderLock()) {
                        xCurrentMethod = (from item in mMethods.Keys
                                          where !mMethods[item].PreProcessed
                                          select item).FirstOrDefault();
                    }
                    if (xCurrentMethod == null) {
                        break;
                    }
                    //ProgressChanged.Invoke(String.Format("Scanning method: {0}", xCurrentMethod.GetFullName()));
                    EmitDependencyGraphLine(true, xCurrentMethod.GetFullName());
                    try {
                        mAssembler.CurrentGroup = GetGroupForType(xCurrentMethod.DeclaringType);
                        RegisterType(xCurrentMethod.DeclaringType);
                        using (mMethodsLocker.AcquireReaderLock()) {
                            mMethods[xCurrentMethod].PreProcessed = true;
                        }
                        if (xCurrentMethod.IsAbstract) {
                            continue;
                        }
                        string xMethodName = Label.GenerateLabelName(xCurrentMethod);
                        TypeInformation xTypeInfo = null;
                        if (!xCurrentMethod.IsStatic) {
                            xTypeInfo = GetTypeInfo(xCurrentMethod.DeclaringType);
                        }
                        MethodInformation xMethodInfo;
                        using (mMethodsLocker.AcquireReaderLock()) {
                            xMethodInfo = GetMethodInfo(xCurrentMethod,
                                                        xCurrentMethod,
                                                        xMethodName,
                                                        xTypeInfo,
                                                        mDebugMode != DebugMode.None,
                                                        mMethods[xCurrentMethod].Info);
                        }
                        MethodBase xCustomImplementation = GetCustomMethodImplementation(xMethodName);
                        if (xCustomImplementation != null) {
                            QueueMethod(xCustomImplementation);
                            using (mMethodsLocker.AcquireReaderLock()) {
                                mMethods[xCurrentMethod].Implementation = xCustomImplementation;
                            }
                            continue;
                        }
                        Type xOpType = mMap.GetOpForCustomMethodImplementation(xMethodName);
                        if (xOpType != null) {
                            Op xMethodOp = GetOpFromType(xOpType, null, xMethodInfo);
                            if (xMethodOp != null) {
                                continue;
                            }
                        }
                        if (mMap.HasCustomAssembleImplementation(xMethodInfo, false)) {
                            mMap.ScanCustomAssembleImplementation(xMethodInfo, false);
                            continue;
                        }

                        //xCurrentMethod.GetMethodImplementationFlags() == MethodImplAttributes.
                        MethodBody xBody = xCurrentMethod.GetMethodBody();
                        // todo: add better detection of implementation state
                        if (xBody != null) {
                            mInstructionsToSkip = 0;
                            mAssembler.StackContents.Clear();
                            ILReader xReader = new ILReader(xCurrentMethod);
                            var xInstructionInfos = new List<DebugSymbolsAssemblyTypeMethodInstruction>();
                            while (xReader.Read()) {
                                SortedList<string, object> xInfo = null;
                                using (mMethodsLocker.AcquireReaderLock()) {
                                    xInfo = mMethods[xCurrentMethod].Info;
                                }
                                mMap.ScanILCode(xReader, xMethodInfo, xInfo);
                            }
                        }
                    } catch (Exception e) {
                        OnDebugLog(LogSeverityEnum.Error, xCurrentMethod.GetFullName());
                        OnDebugLog(LogSeverityEnum.Warning, e.ToString());
                        throw;
                    }
                }
                using (mTypesLocker.AcquireReaderLock()) {
                    foreach (Type xType in mTypes) {
                        foreach (MethodBase xMethod in xType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)) {
                            if (xMethod.IsStatic) {
                                QueueMethod(xMethod);
                            }
                        }
                    }
                }
            } finally {
                mThreadEvents[xThreadIndex].Set();
            }
        }

        private void ScanAllStaticFields() { 
        }

        private void GenerateDebugSymbols() {
            /*var xAssemblyComparer = new AssemblyEqualityComparer();
			var xTypeComparer = new TypeEqualityComparer();
			var xDbgAssemblies = new List<DebugSymbolsAssembly>();
			int xTypeCount = mTypes.Count;
			try {
				foreach (var xAssembly in (from item in mTypes
										   select item.Assembly).Distinct(xAssemblyComparer)) {
					var xDbgAssembly = new DebugSymbolsAssembly();
					var xDbgAssemblyTypes = new List<DebugSymbolsAssemblyType>();
					xDbgAssembly.FileName = xAssembly.Location;
					xDbgAssembly.FullName = xAssembly.GetName().FullName;
					//if (xDbgAssembly.FullName == "Cosmos.Hardware, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5ae71220097cb983") {
					//    System.Diagnostics.Debugger.Break();
					//}
					for (int xIdxTypes = 0; xIdxTypes < mTypes.Count; xIdxTypes++) {
						var xType = mTypes[xIdxTypes];
						if (!xAssemblyComparer.Equals(xAssembly, xType.Assembly)) {
							continue;
						}
						var xDbgType = new DebugSymbolsAssemblyType();
						//if (xType.FullName == "Cosmos.Hardware.Screen.Text") {
						//    System.Diagnostics.Debugger.Break();
						//}
						if (xType.BaseType != null) {
							xDbgType.BaseTypeId = GetTypeId(xType.BaseType);
						}
						xDbgType.TypeId = xIdxTypes;
						xDbgType.FullName = xType.FullName;
						var xTypeFields = new List<DebugSymbolsAssemblyTypeField>();
						var xTypeInfo = GetTypeInfo(xType);
						xDbgType.StorageSize = GetFieldStorageSize(xType);
						foreach (var xField in xType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
							var xDbgField = new DebugSymbolsAssemblyTypeField();
							xDbgField.Name = xField.Name;
							xDbgField.IsStatic = xField.IsStatic;
							if (xField.IsPublic) {
								xDbgField.Visibility = "Public";
							} else {
								if (xField.IsPrivate) {
									xDbgField.Visibility = "Private";
								} else {
									if (xField.IsFamily) {
										xDbgField.Visibility = "Protected";
									} else {
										xDbgField.Visibility = "Internal";
									}
								}
							}
							xDbgField.FieldType = GetTypeId(xField.FieldType);
							if (xDbgField.IsStatic) {
								xDbgField.Address = DataMember.GetStaticFieldName(xField);
							} else {
								xDbgField.Address = "+" + xTypeInfo.Fields[xField.GetFullName()].Offset;
							}
							xTypeFields.Add(xDbgField);
						}
						xDbgType.Field = xTypeFields.ToArray();
						var xTypeMethods = new List<DebugSymbolsAssemblyTypeMethod>();
						foreach (var xMethod in xType.GetMethods(BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Cast<MethodBase>().Union(xType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))) {
							var xIdxMethods = mMethods.IndexOfKey(xMethod);
							if (xIdxMethods == -1) {
								continue;
							}
							//var xMethod = mMethods.Keys[xIdxMethods];
							//if (!xTypeComparer.Equals(xMethod.DeclaringType, xType)) {
							//    continue;
							//}
							var xDbgMethod = new DebugSymbolsAssemblyTypeMethod();
							xDbgMethod.Name = xMethod.Name;
							xDbgMethod.MethodId = xIdxMethods;
							xDbgMethod.Address = Label.GenerateLabelName(xMethod);
							if (xMethod is ConstructorInfo) {
								xDbgMethod.ReturnTypeId = GetTypeId(typeof(void));
							} else {
								var xTheMethod = xMethod as MethodInfo;
								if (xTheMethod != null) {
									xDbgMethod.ReturnTypeId = GetTypeId(xTheMethod.ReturnType);
								} else {
									xDbgMethod.ReturnTypeId = GetTypeId(typeof(void));
								}
							}
							if (xMethod.IsPublic) {
								xDbgMethod.Visibility = "Public";
							} else {
								if (xMethod.IsPrivate) {
									xDbgMethod.Visibility = "Private";
								} else {
									if (xMethod.IsFamily) {
										xDbgMethod.Visibility = "Protected";
									} else {
										xDbgMethod.Visibility = "Internal";
									}
								}
							}
							xTypeMethods.Add(xDbgMethod);
							MethodBody xBody = xMethod.GetMethodBody();
							if (xBody != null) {
								var xDbgLocals = new List<DebugSymbolsAssemblyTypeMethodLocal>();
								var xMethodInfo = GetMethodInfo(xMethod, xMethod, Label.GenerateLabelName(xMethod), xTypeInfo);
								if (xBody.LocalVariables != null) {
									foreach (var xLocal in xBody.LocalVariables) {
										var xDbgLocal = new DebugSymbolsAssemblyTypeMethodLocal();
										xDbgLocal.Name = xLocal.LocalIndex.ToString();
										xDbgLocal.LocalTypeId = GetTypeId(xLocal.LocalType);
										xDbgLocal.RelativeStartAddress = xMethodInfo.Locals[xLocal.LocalIndex].VirtualAddresses.First();
										xDbgLocals.Add(xDbgLocal);
									}
								}
								xDbgMethod.Local = xDbgLocals.ToArray();
							}
							xDbgMethod.Body = mMethods.Values[xIdxMethods].Instructions;
						}
						xDbgType.Method = xTypeMethods.ToArray();
						xDbgAssemblyTypes.Add(xDbgType);
					}
					xDbgAssembly.Type = xDbgAssemblyTypes.ToArray();
					xDbgAssemblies.Add(xDbgAssembly);
				}
			} finally {
				if (xTypeCount != mTypes.Count) {
					Console.WriteLine("TypeCount changed (was {0}, new {1})", xTypeCount, mTypes.Count);
					Console.WriteLine("Last Type: {0}", mTypes.Last().FullName);
				}
			}*/
        }

        private void GenerateVMT(bool aDebugMode) {
            Op xOp = GetOpFromType(mMap.MethodHeaderOp,
                                   null,
                                   new MethodInformation("____INIT__VMT____",
                                                         new MethodInformation.Variable[0],
                                                         new MethodInformation.Argument[0],
                                                         0,
                                                         false,
                                                         null,
                                                         null,
                                                         typeof(void),
                                                         aDebugMode,
                                                         new Dictionary<string, object>()));
            xOp.Assembler = mAssembler;
            xOp.Assemble();
            InitVmtImplementationOp xInitVmtOp = (InitVmtImplementationOp)GetOpFromType(mMap.InitVmtImplementationOp,
                                                                                        null,
                                                                                        null);
            xInitVmtOp.Assembler = mAssembler;
            xInitVmtOp.Types = mTypes;
            xInitVmtOp.SetTypeInfoRef = VTablesImplRefs.SetTypeInfoRef;
            xInitVmtOp.SetMethodInfoRef = VTablesImplRefs.SetMethodInfoRef;
            xInitVmtOp.LoadTypeTableRef = VTablesImplRefs.LoadTypeTableRef;
            xInitVmtOp.TypesFieldRef = VTablesImplRefs.VTablesImplDef.GetField("mTypes",
                                                                               BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            using (mMethodsLocker.AcquireReaderLock()) {
                xInitVmtOp.Methods = mMethods.Keys.ToList();
            }
            xInitVmtOp.VTableEntrySize = GetFieldStorageSize(GetType("",
                                                                     typeof(VTable).FullName.Replace('+',
                                                                                                     '.')));
            xInitVmtOp.GetMethodIdentifier += delegate(MethodBase aMethod) {
                                                  if (aMethod.GetFullName() == "System.Reflection.Cache.InternalCache  System.Reflection.MemberInfo.get_Cache()") {
                                                      System.Diagnostics.Debugger.Break();
                                                  }
                                                  ParameterInfo[] xParams = aMethod.GetParameters();
                                                  Type[] xParamTypes = new Type[xParams.Length];
                                                  for (int i = 0; i < xParams.Length; i++) {
                                                      xParamTypes[i] = xParams[i].ParameterType;
                                                  }
                                                  MethodBase xMethod = GetUltimateBaseMethod(aMethod,
                                                                                             xParamTypes,
                                                                                             aMethod.DeclaringType);
                                                  return GetMethodIdentifier(xMethod);
                                              };
            using (mTypesLocker.AcquireWriterLock()) {
                xInitVmtOp.Assemble();
            }
            xOp = GetOpFromType(mMap.MethodFooterOp,
                                null,
                                new MethodInformation("____INIT__VMT____",
                                                      new MethodInformation.Variable[0],
                                                      new MethodInformation.Argument[0],
                                                      0,
                                                      false,
                                                      null,
                                                      null,
                                                      typeof(void),
                                                      aDebugMode,
                                                      new Dictionary<string, object>()));
            xOp.Assembler = mAssembler;
            xOp.Assemble();
        }

        private void ScanForMethodsToIncludeForVMT() {
            List<Type> xCheckedTypes = new List<Type>();
            int i = -1;
            while(true){
                i++;
                MethodBase xMethod;
                using (mMethodsLocker.AcquireReaderLock()) {
                    if (i == mMethods.Count) {
                        break;
                    }
                    xMethod = mMethods.ElementAt(i).Key;
                }
                if (xMethod.IsStatic) {
                    continue;
                }
                Type xCurrentType = xMethod.DeclaringType;
                if (!xCheckedTypes.Contains(xCurrentType,
                                            mTypesEqualityComparer)) {
                    xCheckedTypes.Add(xCurrentType);
                }
                QueueMethod(GetUltimateBaseMethod(xMethod,
                                                  (from item in xMethod.GetParameters()
                                                   select item.ParameterType).ToArray(),
                                                  xCurrentType));
            }
            using (mTypesLocker.AcquireReaderLock()) {
                foreach (Type xType in mTypes) {
                    if (!xCheckedTypes.Contains(xType,
                                                mTypesEqualityComparer)) {
                        xCheckedTypes.Add(xType);
                    }
                }
            }
            for (i = 0; i < xCheckedTypes.Count; i++) {
                Type xCurrentType = xCheckedTypes[i];
                while (xCurrentType != null) {
                    if (!xCheckedTypes.Contains(xCurrentType,
                                                mTypesEqualityComparer)) {
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
                        if (xMethod.IsVirtual && !xMethod.IsConstructor) {
                            Type xCurrentInspectedType = xTD.BaseType;
                            ParameterInfo[] xParams = xMethod.GetParameters();
                            Type[] xMethodParams = new Type[xParams.Length];
                            for (int k = 0; k < xParams.Length; k++) {
                                xMethodParams[k] = xParams[k].ParameterType;
                            }
                            MethodBase xBaseMethod = GetUltimateBaseMethod(xMethod,
                                                                           xMethodParams,
                                                                           xTD);
                            if (xBaseMethod != null && xBaseMethod != xMethod) {
                                bool xNeedsRegistering = false;
                                using(mMethodsLocker.AcquireReaderLock()) {
                                    xNeedsRegistering = mMethods.ContainsKey(xBaseMethod);
                                }
                                if (xNeedsRegistering) {
                                    QueueMethod(xMethod);
                                }
                            }
                        }
                    }
                }
            }
            int j = -1;
            while (true)
            {
                j++;
                KeyValuePair<MethodBase,QueuedMethodInformation> xMethod;
                using (mMethodsLocker.AcquireReaderLock())
                {
                    if (j == mMethods.Count)
                    {
                        break;
                    }
                    xMethod = mMethods.Skip(j).First();
                }
                if (xMethod.Key.DeclaringType.IsInterface) {
                    var xInterface = xMethod.Key.DeclaringType;
                    i = -1;
                    while(true) {
                        Type xImplType;
                        i++;
                        using (mTypesLocker.AcquireReaderLock()) {
                            if(i == mTypes.Count) {
                                break;
                            }
                            xImplType = mTypes.ElementAt(i);
                        }
                        if (xImplType.IsInterface) {
                            continue;
                        }
                        if (!xInterface.IsAssignableFrom(xImplType)) {
                            continue;
                        }

                        var xActualMethod = xImplType.GetMethod(xInterface.FullName + "." + xMethod.Key.Name,
                                                                (from xParam in xMethod.Key.GetParameters()
                                                                 select xParam.ParameterType).ToArray());

                        if (xActualMethod == null) {
                            // get private implemenation
                            xActualMethod = xImplType.GetMethod(xMethod.Key.Name,
                                                                (from xParam in xMethod.Key.GetParameters()
                                                                 select xParam.ParameterType).ToArray());
                        }
                        if (xActualMethod == null) {
                            try {
                                var xMap = xImplType.GetInterfaceMap(xInterface);
                                for (int k = 0; k < xMap.InterfaceMethods.Length; k++) {
                                    if (xMap.InterfaceMethods[k] == xMethod.Key) {
                                        xActualMethod = xMap.TargetMethods[k];
                                        break;
                                    }
                                }
                            } catch {
                            }
                        }
                        if (xActualMethod != null) {
                            QueueMethod(xActualMethod);
                        }
                    }
                }
            }
        }

        private static MethodBase GetUltimateBaseMethod(MethodBase aMethod,
                                                        Type[] aMethodParams,
                                                        Type aCurrentInspectedType) {
            MethodBase xBaseMethod = null;
            //try {
            while (true) {
                if (aCurrentInspectedType.BaseType == null) {
                    break;
                }
                aCurrentInspectedType = aCurrentInspectedType.BaseType;
                MethodBase xFoundMethod = aCurrentInspectedType.GetMethod(aMethod.Name,
                                                                          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                                                                          Type.DefaultBinder,
                                                                          aMethodParams,
                                                                          new ParameterModifier[0]);
                if (xFoundMethod == null) {
                    break;
                }
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
                if (xFoundMethod != null)
                {
                    xBaseMethod = xFoundMethod;

                    if (xFoundMethod.IsVirtual == aMethod.IsVirtual && xFoundMethod.IsPrivate == false && xFoundMethod.IsPublic == aMethod.IsPublic && xFoundMethod.IsFamily == aMethod.IsFamily && xFoundMethod.IsFamilyAndAssembly == aMethod.IsFamilyAndAssembly && xFoundMethod.IsFamilyOrAssembly == aMethod.IsFamilyOrAssembly && xFoundMethod.IsFinal == false)
                    {
                        var xFoundMethInfo = xFoundMethod as MethodInfo;
                        var xBaseMethInfo = xBaseMethod as MethodInfo;
                        if (xFoundMethInfo == null && xBaseMethInfo == null)
                        {
                            xBaseMethod = xFoundMethod;
                        }
                        if (xFoundMethInfo != null && xBaseMethInfo != null)
                        {
                            if (xFoundMethInfo.ReturnType.AssemblyQualifiedName.Equals(xBaseMethInfo.ReturnType.AssemblyQualifiedName))
                            {
                                xBaseMethod = xFoundMethod;
                            }
                        }
                        //xBaseMethod = xFoundMethod;
                    }
                }
                //else
                //{
                //    xBaseMethod = xFoundMethod;
                //}
            }
            //} catch (Exception) {
            // todo: try to get rid of the try..catch
            //}
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
                    xMethod = xTypeDef.GetMethod("GetValue",
                                                 xParams);
                }
                if (aRef.Name == "Set") {
                    xMethod = xTypeDef.GetMethod("SetValue",
                                                 xParams);
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
            GetTypeFieldInfo(aType,
                             out xResult);
            return xResult;
        }

        private static string GetGroupForType(Type aType) {
            return aType.Module.Assembly.GetName().Name;
        }

        private void ProcessAllStaticFields() {
            int i = -1;
            int xCount = 0;
            while (true) {
                i++;
                FieldInfo xCurrentField;
                using(mStaticFieldsLocker.AcquireReaderLock()) {
                    xCount = mStaticFields.Count;
                    if (i == xCount) {
                        break;
                    }
                    xCurrentField = mStaticFields.Keys.ElementAt(i);
                }
                CompilingStaticFields(i, xCount);
                //ProgressChanged.Invoke(String.Format("Processing static field: {0}", xCurrentField.GetFullName()));
                mAssembler.CurrentGroup = GetGroupForType(xCurrentField.DeclaringType);
                string xFieldName = xCurrentField.GetFullName();
                xFieldName = DataMember.GetStaticFieldName(xCurrentField);
                if (mAssembler.DataMembers.Count(x => x.Value.Name == xFieldName) == 0) {
                    var xItem = (from item in xCurrentField.GetCustomAttributes(false)
                                 where item.GetType().FullName == "ManifestResourceStreamAttribute"
                                 select item).FirstOrDefault();
                    string xManifestResourceName = null;
                    if (xItem != null) {
                        var xItemType = xItem.GetType();
                        xManifestResourceName = (string)xItemType.GetField("ResourceName").GetValue(xItem);
                    }
                    if (xManifestResourceName != null) {
                        RegisterType(xCurrentField.FieldType);
                        string xFileName = Path.Combine(mOutputDir,
                                                        (xCurrentField.DeclaringType.Assembly.FullName + "__" + xManifestResourceName).Replace(",",
                                                                                                                                               "_") + ".res");
                        using (var xStream = xCurrentField.DeclaringType.Assembly.GetManifestResourceStream(xManifestResourceName)) {
                            if (xStream == null) {
                                throw new Exception("Resource '" + xManifestResourceName + "' not found!");
                            }
                            using (var xTarget = File.Create(xFileName)) {
                                // todo: abstract this array code out.
                                xTarget.Write(BitConverter.GetBytes(Engine.RegisterType(Engine.GetType("mscorlib",
                                                                                                       "System.Array"))),
                                              0,
                                              4);
                                xTarget.Write(BitConverter.GetBytes((uint)InstanceTypeEnum.StaticEmbeddedArray),
                                              0,
                                              4);
                                xTarget.Write(BitConverter.GetBytes((int)xStream.Length), 0, 4);
                                xTarget.Write(BitConverter.GetBytes((int)1), 0, 4);
                                var xBuff = new byte[128];
                                while (xStream.Position < xStream.Length) {
                                    int xBytesRead = xStream.Read(xBuff, 0, 128);
                                    xTarget.Write(xBuff, 0, xBytesRead);
                                }
                            }
                        }
                        mAssembler.DataMembers.Add(new KeyValuePair<string, DataMember>("ManifestResourceStreams",
                                                                                        new DataMember("___" + xFieldName + "___Contents",
                                                                                                       "incbin",
                                                                                                       "\"" + xFileName + "\"")));
                        mAssembler.DataMembers.Add(new KeyValuePair<string, DataMember>("ManifestResourceStreams",
                                                                                        new DataMember(xFieldName,
                                                                                                       "dd",
                                                                                                       "___" + xFieldName + "___Contents")));
                    } else {
                        RegisterType(xCurrentField.FieldType);
                        int xTheSize;
                        string theType = "db";
                        Type xFieldTypeDef = xCurrentField.FieldType;
                        if (!xFieldTypeDef.IsClass || xFieldTypeDef.IsValueType) {
                            xTheSize = GetFieldStorageSize(xCurrentField.FieldType);
                        } else {
                            xTheSize = 4;
                        }
                        if (xTheSize == 4) {
                            theType = "dd";
                            xTheSize = 1;
                        } else if (xTheSize == 2) {
                            theType = "dw";
                            xTheSize = 1;
                        }
                        string xTheData = "";
                        try {
                            object xValue = xCurrentField.GetValue(null);
                            if (xValue != null) {
                                try {
                                    if (xValue.GetType().IsValueType) {
                                        StringBuilder xSB = new StringBuilder(xTheSize * 3);
                                        for (int x = 0; x < xTheSize; x++) {
                                            xSB.Append(Marshal.ReadByte(xValue, x) + ",");
                                        }
                                        xTheData = xSB.Remove(xSB.Length - 1, 1).ToString();
                                    }
                                } catch {
                                }
                            }
                        } catch {
                        }
                        if (xTheSize == 0) {
                            throw new Exception("Field '" + xCurrentField.ToString() + "' doesn't have a valid size!");
                        }
                        if (String.IsNullOrEmpty(xTheData)) {
                            for (uint x = 0; x < xTheSize; x++) {
                                xTheData += "0,";
                            }
                        }
                        xTheData = xTheData.TrimEnd(',');
                        mAssembler.DataMembers.Add(new KeyValuePair<string, DataMember>(
                         mAssembler.CurrentGroup, new DataMember(xFieldName, theType, xTheData)));
                    }
                }
                using (mStaticFieldsLocker.AcquireReaderLock()) {
                    mStaticFields[xCurrentField].Processed = true;
                }
            }
            CompilingStaticFields(i, xCount);
        }

        private ISymbolReader GetSymbolReaderForAssembly(Assembly aAssembly) {
            return SymbolAccess.GetReaderForFile(aAssembly.Location);
        }

        private void ProcessAllMethods() {
            int i = -1;
            int xCount = 0;
            while (true) {
                i++;
                MethodBase xCurrentMethod;
                using(mMethodsLocker.AcquireReaderLock()) {
                    xCount = mMethods.Count;
                    if (i == xCount) {
                        break;
                    }
                    xCurrentMethod = mMethods.Keys.ElementAt(i);
                }
                CompilingMethods(i, xCount);
                try {
                    EmitDependencyGraphLine(true, xCurrentMethod.GetFullName());
                    mAssembler.CurrentGroup = GetGroupForType(xCurrentMethod.DeclaringType);
                    RegisterType(xCurrentMethod.DeclaringType);
                    if (xCurrentMethod.IsAbstract) {
                        using (mMethodsLocker.AcquireReaderLock()) {
                            mMethods[xCurrentMethod].Processed = true;
                        }
                        continue;
                    }
                    string xMethodName = Label.GenerateLabelName(xCurrentMethod);
                    TypeInformation xTypeInfo = null;
                    if (!xCurrentMethod.IsStatic) {
                        xTypeInfo = GetTypeInfo(xCurrentMethod.DeclaringType);
                    }
                    SortedList<string, object> xMethodScanInfo;
                    using(mMethodsLocker.AcquireReaderLock()) {
                        xMethodScanInfo = mMethods[xCurrentMethod].Info;
                    }
                    MethodInformation xMethodInfo = GetMethodInfo(xCurrentMethod, xCurrentMethod
                     , xMethodName, xTypeInfo, mDebugMode != DebugMode.None, xMethodScanInfo);
                    IL.Op xOp = GetOpFromType(mMap.MethodHeaderOp, null, xMethodInfo);
                    xOp.Assembler = mAssembler;
#if VERBOSE_DEBUG
                    string comment = "(No Type Info available)";
                    if (xMethodInfo.TypeInfo != null) {
                        comment = "Type Info:\r\n \r\n" + xMethodInfo.TypeInfo;
                    }
                    foreach (string s in comment.Trim().Split(new string[] {"\r\n"}
                     , StringSplitOptions.RemoveEmptyEntries)) {
                        new Comment(s);
                    }
                    comment = xMethodInfo.ToString();
                    foreach (string s in comment.Trim().Split(new string[] {"\r\n"}
                     , StringSplitOptions.RemoveEmptyEntries)) {
                        new Comment(s);
                    }
#endif
                    xOp.Assemble();
                    MethodBase xCustomImplementation = GetCustomMethodImplementation(xMethodName);
                    bool xIsCustomImplementation = (xCustomImplementation != null);
                    // what to do if a method doesn't have a body?
                    bool xContentProduced = false;
                    if (xIsCustomImplementation) {
                        // this is for the support for having extra fields on types, and being able to use
                        // them in custom implementation methods
                        CustomMethodImplementationProxyOp xProxyOp 
                         = (CustomMethodImplementationProxyOp)GetOpFromType(
                         mMap.CustomMethodImplementationProxyOp, null, xMethodInfo);
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
                        if (mMap.HasCustomAssembleImplementation(xMethodInfo, false)) {
                            mMap.DoCustomAssembleImplementation(false, mAssembler, xMethodInfo);
                        // No plugs, we need to compile the IL from the method
                        } else {
                            MethodBody xBody = xCurrentMethod.GetMethodBody();
                            // todo: add better detection of implementation state
                            if (xBody != null) {
                                mInstructionsToSkip = 0;
                                mAssembler.StackContents.Clear();
                                var xReader = new ILReader(xCurrentMethod);
                                var xInstructionInfos = new List<DebugSymbolsAssemblyTypeMethodInstruction>();

                                // Section currently is dead code. Working on matching it up 
                                // with contents from inside the read
                                int[] xCodeOffsets = null;
                                if (mDebugMode == DebugMode.Source) {
                                    var xSymbolReader = GetSymbolReaderForAssembly(xCurrentMethod.DeclaringType.Assembly);
                                    if (xSymbolReader != null) {
                                        var xSmbMethod = xSymbolReader.GetMethod(new SymbolToken(xCurrentMethod.MetadataToken));
                                        // This gets the Sequence Points.
                                        // Sequence Points are spots that identify what the compiler/debugger says is a spot
                                        // that a breakpoint can occur one. Essentially, an atomic source line in C#
                                        if (xSmbMethod != null) {
                                            xCodeOffsets = new int[xSmbMethod.SequencePointCount];
                                            var xCodeDocuments = new ISymbolDocument[xSmbMethod.SequencePointCount];
                                            var xCodeLines = new int[xSmbMethod.SequencePointCount];
                                            var xCodeColumns = new int[xSmbMethod.SequencePointCount];
                                            var xCodeEndLines = new int[xSmbMethod.SequencePointCount];
                                            var xCodeEndColumns = new int[xSmbMethod.SequencePointCount];
                                            xSmbMethod.GetSequencePoints(xCodeOffsets, xCodeDocuments
                                             , xCodeLines, xCodeColumns, xCodeEndLines, xCodeEndColumns);
                                        }
                                    }
                                }

                                // Scan each IL op in the method
                                while (xReader.Read()) {
                                    ExceptionHandlingClause xCurrentHandler = null;

                                    #region Exception handling support code
                                    // todo: add support for nested handlers using a stack or so..
                                    foreach (ExceptionHandlingClause xHandler in xBody.ExceptionHandlingClauses) {
                                        if (xHandler.TryOffset > 0) {
                                            if (xHandler.TryOffset <= xReader.NextPosition && (xHandler.TryLength + xHandler.TryOffset) > xReader.NextPosition) {
                                                if (xCurrentHandler == null) {
                                                    xCurrentHandler = xHandler;
                                                    continue;
                                                } else if (xHandler.TryOffset > xCurrentHandler.TryOffset && (xHandler.TryLength + xHandler.TryOffset) < (xCurrentHandler.TryLength + xCurrentHandler.TryOffset)) {
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
                                                } else if (xHandler.HandlerOffset > xCurrentHandler.HandlerOffset && (xHandler.HandlerOffset + xHandler.HandlerLength) < (xCurrentHandler.HandlerOffset + xCurrentHandler.HandlerLength)) {
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
                                                    } else if (xHandler.FilterOffset > xCurrentHandler.FilterOffset) {
                                                        // only replace if the current found handler is narrower
                                                        xCurrentHandler = xHandler;
                                                        continue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    xMethodInfo.CurrentHandler = xCurrentHandler;
                                    xOp = GetOpFromType(mMap.GetOpForOpCode(xReader.OpCode), xReader
                                     , xMethodInfo);
                                    xOp.Assembler = mAssembler;
                                    new Comment("StackItems = " + mAssembler.StackContents.Count);
                                    foreach (var xStackContent in mAssembler.StackContents) {
                                        new Comment("    " + xStackContent.Size);
                                    }
                                    string xLabel = Op.GetInstructionLabel(xReader);
                                    if (xLabel.StartsWith(".")) {
                                        xLabel = Label.LastFullLabel + "__DOT__" + xLabel.Substring(1);
                                        xLabel = DataMember.FilterStringForIncorrectChars(xLabel);
                                    }
                                    
                                    // Determine if a new DebugStub should be emitted
                                    bool xEmitTracer = false;
                                    // Set based on TracedAssemblies
                                    // NOTE - These if statemens can be optimized down - but clarity is
                                    // more importnat the optimizations would not offer much benefit
                                    if (TraceAssemblies != TraceAssemblies.All) {
                                        string xNS = xCurrentMethod.DeclaringType.Namespace;
                                        if (xNS.StartsWith("System.", StringComparison.InvariantCultureIgnoreCase)) {
                                        } else if (xNS.ToLower() == "system") {
                                        } else if (xNS.StartsWith("Microsoft.", StringComparison.InvariantCultureIgnoreCase)) {
                                        } else if (TraceAssemblies == TraceAssemblies.Cosmos) {
                                            xEmitTracer = true;
                                        } else {
                                            //TODO: Currently for User we only include the entry assembly
                                            //we need to somehow flag Cosmos assemblies specifically
                                            //Maybe an attribute that could be used to turn tracing on and off
                                            if (xNS.StartsWith("Cosmos.Demo.", StringComparison.InvariantCultureIgnoreCase)) {
                                                xEmitTracer = true;
                                            } else if (xNS.StartsWith("Cosmos.Playground.", StringComparison.InvariantCultureIgnoreCase)) {
                                                xEmitTracer = true;
                                            // Must come 2nd becuase it is a substring of previous comparisons
                                            } else if (xNS.StartsWith("Cosmos.", StringComparison.InvariantCultureIgnoreCase)) {
                                            // No . on this one. We might need Indy. in future, but not Indy.IL2CPU, but there is 
                                            // an asm of just Indy.IL2CPU
                                            } else if (xNS.StartsWith("Indy.IL2CPU", StringComparison.InvariantCultureIgnoreCase)) {
                                            } else {
                                                xEmitTracer = true;
                                            }
                                        }
                                    }
                                    // Check options for Debug Level
                                    if (xEmitTracer) {
                                        if (mDebugMode == DebugMode.IL) {
                                            // For IL, we emit for every one
                                            xEmitTracer = true;
                                        } else if (mDebugMode == DebugMode.Source) {
                                            // If the current position equals one of the offsets, then we have
                                            // reached a new atomic C# statement
                                            if (xCodeOffsets != null) {
                                                xEmitTracer = xCodeOffsets.Contains(xReader.Position);
                                            }
                                        } else if (mDebugMode == DebugMode.None) {
                                            xEmitTracer = false;
                                        }

                                        if (xEmitTracer) { 
                                            mMap.EmitOpDebugHeader(mAssembler, 0, xLabel);
                                        }
                                    }
                                    
                                    using (mSymbolsLocker.AcquireWriterLock()) {
                                        if (mSymbols != null) {
                                            var xMLSymbol = new MLDebugSymbol();
                                            xMLSymbol.LabelName = xLabel;
                                            int xStackSize = (from item in mAssembler.StackContents
                                                              let xSize = (item.Size % 4 == 0)
                                                                              ? item.Size
                                                                              : (item.Size + (4 - (item.Size % 4)))
                                                              select xSize).Sum();
                                            xMLSymbol.StackDifference = xMethodInfo.LocalsSize + xStackSize;
                                            xMLSymbol.AssemblyFile = xCurrentMethod.DeclaringType.Assembly.Location;
                                            xMLSymbol.MethodToken = xCurrentMethod.MetadataToken;
                                            xMLSymbol.TypeToken = xCurrentMethod.DeclaringType.MetadataToken;
                                            xMLSymbol.ILOffset = xReader.Position;
                                            mSymbols.Add(xMLSymbol);
                                        }
                                    }
                                    xOp.Assemble();
                                    //if (xInstructionInfo != null) {
                                    //    int xNewStack = (from item in mAssembler.StackContents
                                    //                     let xSize = (item.Size % 4 == 0) ? item.Size : (item.Size + (4 - (item.Size % 4)))
                                    //                     select xSize).Sum();
                                    //    xInstructionInfo.StackResult = xNewStack - xCurrentStack;
                                    //    xInstructionInfo.StackResultSpecified = true;
                                    //    xInstructionInfos.Add(xInstructionInfo);
                                    //}
                                }
                                if (mSymbols != null) {
                                    MLDebugSymbol[] xSymbols;
                                    using(mSymbolsLocker.AcquireReaderLock()) {
                                        xSymbols = mSymbols.ToArray();
                                    }
                                    using (mMethodsLocker.AcquireReaderLock()) {
                                        mMethods[xCurrentMethod].Instructions = xSymbols;
                                    }
                                }
                            } else {
                                if ((xCurrentMethod.Attributes & MethodAttributes.PinvokeImpl) != 0) {
                                    OnDebugLog(LogSeverityEnum.Error,
                                               "Method '{0}' not generated!",
                                               xCurrentMethod.GetFullName());
                                    new Comment("Method not being generated yet, as it's handled by a PInvoke");
                                } else {
                                    OnDebugLog(LogSeverityEnum.Error,
                                               "Method '{0}' not generated!",
                                               xCurrentMethod.GetFullName());
                                    new Comment("Method not being generated yet, as it's handled by an iCall");
                                }
                            }
                        }
                    }
                    xOp = GetOpFromType(mMap.MethodFooterOp, null, xMethodInfo);
                    xOp.Assembler = mAssembler;
                    xOp.Assemble();
                    mAssembler.StackContents.Clear();
                    using (mMethodsLocker.AcquireReaderLock()) {
                        mMethods[xCurrentMethod].Processed = true;
                    }
                } catch (Exception e) {
                    OnDebugLog(LogSeverityEnum.Error, xCurrentMethod.GetFullName());
                    OnDebugLog(LogSeverityEnum.Warning, e.ToString());
                    throw;
                }
            }
            CompilingMethods(i, xCount);
        }

        private IList<Assembly> GetPlugAssemblies() {
            var xResult = this.mMap.GetPlugAssemblies();
            xResult.Add(typeof(Engine).Assembly);
            return xResult;
        }

        /// <summary>
        /// Gets the full name of a method, without the defining type included
        /// </summary>
        /// <param name="aMethod"></param>
        /// <returns></returns>
        private static string GetStrippedMethodBaseFullName(MethodBase aMethod,
                                                            MethodBase aRefMethod) {
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
                if (i == 0 && (aRefMethod != null && !aRefMethod.IsStatic)) {
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

            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            foreach (var xAsm in AppDomain.CurrentDomain.GetAssemblies()) {
                CheckAssemblyForPlugAssemblies(xAsm);
            }
            List<Assembly> xPlugs = new List<Assembly>();
            var xComparer = new AssemblyEqualityComparer();

            foreach (string s in aPlugs) {
                Assembly a = Assembly.LoadFrom(s);
                a.GetTypes();
                if (!xPlugs.Contains(a,
                                     xComparer)) {
                    xPlugs.Add(a);
                }
            }

            foreach (var item in GetPlugAssemblies()) {
                if (!xPlugs.Contains(item,
                                     xComparer)) {
                    xPlugs.Add(item);
                }
            }

            foreach (Assembly xAssemblyDef in xPlugs) {
                LoadPlugAssembly(xAssemblyDef);
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender,
                                                       ResolveEventArgs args) {
            if (File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                         args.Name + ".dll"))) {
                return Assembly.ReflectionOnlyLoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                    args.Name + ".dll"));
            }
            return null;
        }

        private void CurrentDomain_AssemblyLoad(object sender,
                                                AssemblyLoadEventArgs args) {
            CheckAssemblyForPlugAssemblies(args.LoadedAssembly);
        }

        /// <summary>
        /// Load any plug assemblies referred to in this assembly's .config file.
        /// </summary>
        private void CheckAssemblyForPlugAssemblies(Assembly aAssembly) {
            //If in the GAC, then ignore assembly
            if (aAssembly.GlobalAssemblyCache) {
                return;
            }

            //Search for related .config file
            string configFile = aAssembly.Location + ".cosmos-config";
            if (System.IO.File.Exists(configFile)) {
                //Load and parse all PlugAssemblies referred to in the .config file
                foreach (Assembly xAssembly in GetAssembliesFromConfigFile(configFile)) {
                    LoadPlugAssembly(xAssembly);
                }
            }
        }

        /// <summary>
        /// Retrieves a list of plug assemblies from the given .config file.
        /// </summary>
        /// <param name="configFile"></param>
        private IEnumerable<Assembly> GetAssembliesFromConfigFile(string configFile) {
            //Parse XML and get all the PlugAssembly names
            XmlDocument xml = new XmlDocument();
            xml.Load(configFile);
            // do version check:
            if (xml.DocumentElement.Attributes["version"] == null || xml.DocumentElement.Attributes["version"].Value != "1") {
                throw new Exception(".DLL configuration version mismatch!");
            }

            string xHintPath = null;
            if (xml.DocumentElement.Attributes["hintpath"] != null) {
                xHintPath = xml.DocumentElement.Attributes["hintpath"].Value;
            }
            foreach (XmlNode assemblyName in xml.GetElementsByTagName("plug-assembly")) {
                string xName = assemblyName.InnerText;
                if (xName.EndsWith(".dll",
                                   StringComparison.InvariantCultureIgnoreCase) || xName.EndsWith(".exe",
                                                                                                  StringComparison.InvariantCultureIgnoreCase)) {
                    if (!String.IsNullOrEmpty(xHintPath)) {
                        yield return Assembly.LoadFile(Path.Combine(xHintPath,
                                                                    xName));
                        continue;
                    }
                }
                yield return Assembly.Load(assemblyName.InnerText);
            }
        }

        /// <summary>
        /// Searches assembly for methods or fields marked with custom attributes PlugMethodAttribute or PlugFieldAttribute.
        /// Matches found are inserted in SortedLists mPlugMethods and mPlugFields.
        /// </summary>
        private void LoadPlugAssembly(Assembly aAssemblyDef) {
            foreach (var xType in (from item in aAssemblyDef.GetTypes()
                                   let xCustomAttribs = item.GetCustomAttributes(typeof(PlugAttribute),
                                                                                 false)
                                   where xCustomAttribs != null && xCustomAttribs.Length > 0
                                   select new KeyValuePair<Type, PlugAttribute>(item,
                                                                                (PlugAttribute)xCustomAttribs[0]))) {
                PlugAttribute xPlugAttrib = xType.Value;
                Type xTypeRef = xPlugAttrib.Target;
                if (xTypeRef == null) {
                    xTypeRef = Type.GetType(xPlugAttrib.TargetName,
                                            true);
                }

                PlugFieldAttribute[] xTypePlugFields = xType.Key.GetCustomAttributes(typeof(PlugFieldAttribute),
                                                                                     false) as PlugFieldAttribute[];
                if (xTypePlugFields != null && xTypePlugFields.Length > 0) {
                    Dictionary<string, PlugFieldAttribute> xPlugFields;
                    if (mPlugFields.ContainsKey(xTypeRef)) {
                        xPlugFields = mPlugFields[xTypeRef];
                    } else {
                        mPlugFields.Add(xTypeRef,
                                        xPlugFields = new Dictionary<string, PlugFieldAttribute>());
                    }
                    foreach (var xPlugField in xTypePlugFields) {
                        if (!xPlugFields.ContainsKey(xPlugField.FieldId)) {
                            xPlugFields.Add(xPlugField.FieldId,
                                            xPlugField);
                        }
                    }
                }

                foreach (MethodBase xMethod in xType.Key.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
                    PlugMethodAttribute xPlugMethodAttrib = xMethod.GetCustomAttributes(typeof(PlugMethodAttribute),
                                                                                        true).Cast<PlugMethodAttribute>().FirstOrDefault();
                    string xSignature = String.Empty;
                    if (xPlugMethodAttrib != null) {
                        xSignature = xPlugMethodAttrib.Signature;
                        if (!xPlugMethodAttrib.Enabled) {
                            continue;
                        }
                        if (!xPlugMethodAttrib.InNormalMode) {
                            continue;
                        }
                        if (!String.IsNullOrEmpty(xSignature)) {
                            if (!mPlugMethods.ContainsKey(xSignature)) {
                                mPlugMethods.Add(xSignature,
                                                 xMethod);
                            }
                            continue;
                        }
                    }
                    foreach (MethodBase xOrigMethodDef in xTypeRef.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic)) {
                        string xStrippedSignature = GetStrippedMethodBaseFullName(xMethod,
                                                                                  xOrigMethodDef);
                        string xOrigStrippedSignature = GetStrippedMethodBaseFullName(xOrigMethodDef,
                                                                                      null);
                        if (xOrigStrippedSignature == xStrippedSignature) {
                            if (!mPlugMethods.ContainsKey(Label.GenerateLabelName(xOrigMethodDef))) {
                                mPlugMethods.Add(Label.GenerateLabelName(xOrigMethodDef),
                                                 xMethod);
                            }
                        }
                    }
                    foreach (MethodBase xOrigMethodDef in xTypeRef.GetConstructors(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic)) {
                        string xStrippedSignature = GetStrippedMethodBaseFullName(xMethod,
                                                                                  xOrigMethodDef);
                        string xOrigStrippedSignature = GetStrippedMethodBaseFullName(xOrigMethodDef,
                                                                                      null);
                        if (xOrigStrippedSignature == xStrippedSignature) {
                            if (mPlugMethods.ContainsKey(Label.GenerateLabelName(xOrigMethodDef))) {
                                System.Diagnostics.Debugger.Break();
                            }
                            mPlugMethods.Add(Label.GenerateLabelName(xOrigMethodDef),
                                             xMethod);
                        }
                    }
                }
            }
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
            Dictionary<string, TypeInformation.Field> xTypeFields = GetTypeFieldInfo(aType,
                                                                                     out xObjectStorageSize);
            xTypeInfo = new TypeInformation(xObjectStorageSize,
                                            xTypeFields,
                                            aType,
                                            (!aType.IsValueType) && aType.IsClass);
            return xTypeInfo;
        }

        public static MethodInformation GetMethodInfo(MethodBase aCurrentMethodForArguments,
                                                      MethodBase aCurrentMethodForLocals,
                                                      string aMethodName,
                                                      TypeInformation aTypeInfo,
                                                      bool aDebugMode) {
            return GetMethodInfo(aCurrentMethodForArguments,
                                 aCurrentMethodForLocals,
                                 aMethodName,
                                 aTypeInfo,
                                 aDebugMode,
                                 null);
        }

        public static MethodInformation GetMethodInfo(MethodBase aCurrentMethodForArguments,
                                                      MethodBase aCurrentMethodForLocals,
                                                      string aMethodName,
                                                      TypeInformation aTypeInfo,
                                                      bool aDebugMode,
                                                      IDictionary<string, object> aMethodData) {
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
                                                                      xVars[xVarDef.LocalIndex] = new MethodInformation.Variable(xCurOffset,
                                                                                                                                 xVarSize,
                                                                                                                                 !xVarDef.LocalType.IsValueType,
                                                                                                                                 xVarDef.LocalType);
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
                                                                      xArgs[i] = new MethodInformation.Argument(xArgSize,
                                                                                                                xCurOffset,
                                                                                                                xKind,
                                                                                                                !xParamDef.ParameterType.IsValueType,
                                                                                                                GetTypeInfo(xParamDef.ParameterType),
                                                                                                                xParamDef.ParameterType);
                                                                      xCurOffset += xArgSize;
                                                                  }
                                                                  xArgSize = 4;
                                                                  // this
                                                                  xArgs[0] = new MethodInformation.Argument(xArgSize,
                                                                                                            xCurOffset,
                                                                                                            MethodInformation.Argument.KindEnum.In,
                                                                                                            !aCurrentMethodForArguments.DeclaringType.IsValueType,
                                                                                                            GetTypeInfo(aCurrentMethodForArguments.DeclaringType),
                                                                                                            aCurrentMethodForArguments.DeclaringType);
                                                              } else {
                                                                  ParameterInfo[] xParameters = aCurrentMethodForArguments.GetParameters();
                                                                  xArgs = new MethodInformation.Argument[xParameters.Length];
                                                                  xCurOffset = 0;
                                                                  for (int i = xArgs.Length - 1; i >= 0; i--) {
                                                                      ParameterInfo xParamDef = xParameters[i]; //xArgs.Length - i - 1];
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
                                                                      xArgs[i] = new MethodInformation.Argument(xArgSize,
                                                                                                                xCurOffset,
                                                                                                                xKind,
                                                                                                                !xParamDef.ParameterType.IsValueType,
                                                                                                                GetTypeInfo(xParamDef.ParameterType),
                                                                                                                xParamDef.ParameterType);
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
                                                              xMethodInfo = new MethodInformation(aMethodName,
                                                                                                  xVars,
                                                                                                  xArgs,
                                                                                                  xResultSize,
                                                                                                  !aCurrentMethodForArguments.IsStatic,
                                                                                                  aTypeInfo,
                                                                                                  aCurrentMethodForArguments,
                                                                                                  xReturnType,
                                                                                                  aDebugMode,
                                                                                                  aMethodData);
                                                          }
            return xMethodInfo;
        }

        public static Dictionary<string, TypeInformation.Field> GetTypeFieldInfo(MethodBase aCurrentMethod,
                                                                                 out int aObjectStorageSize) {
            Type xCurrentInspectedType = aCurrentMethod.DeclaringType;
            return GetTypeFieldInfo(xCurrentInspectedType,
                                    out aObjectStorageSize);
        }

        private static void GetTypeFieldInfoImpl(List<KeyValuePair<string, TypeInformation.Field>> aTypeFields,
                                                 Type aType,
                                                 ref int aObjectStorageSize) {
            Type xActualType = aType;
            Dictionary<string, PlugFieldAttribute> xCurrentPlugFieldList = new Dictionary<string, PlugFieldAttribute>();
            do {
                if (mCurrent.mPlugFields.ContainsKey(aType)) {
                    var xOrigList = mCurrent.mPlugFields[aType];
                    foreach (var item in xOrigList) {
                        xCurrentPlugFieldList.Add(item.Key,
                                                  item.Value);
                    }
                }
                foreach (FieldInfo xField in aType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
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
                    if ((from item in aTypeFields
                         where item.Key == xFieldId
                         select item).Count() > 0) {
                        continue;
                    }
                    int xOffset = aObjectStorageSize;
                    FieldOffsetAttribute xOffsetAttrib = xField.GetCustomAttributes(typeof(FieldOffsetAttribute),
                                                                                    true).FirstOrDefault() as FieldOffsetAttribute;
                    if (xOffsetAttrib != null) {
                        xOffset = xOffsetAttrib.Value;
                    } else {
                        aObjectStorageSize += xFieldSize;
                        xOffset = -1;
                    }
                    aTypeFields.Insert(0,
                                       new KeyValuePair<string, TypeInformation.Field>(xField.GetFullName(),
                                                                                       new TypeInformation.Field(xFieldSize,
                                                                                                                 xFieldType.IsClass && !xFieldType.IsValueType,
                                                                                                                 xFieldType,
                                                                                                                 (xPlugFieldAttr != null && xPlugFieldAttr.IsExternalValue)) {
                                                                                                                                                                                 Offset = xOffset
                                                                                                                                                                             }));
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
                    aTypeFields.Insert(0,
                                       new KeyValuePair<string, TypeInformation.Field>(xItem.FieldId,
                                                                                       new TypeInformation.Field(xFieldSize,
                                                                                                                 xFieldType.IsClass && !xFieldType.IsValueType,
                                                                                                                 xFieldType,
                                                                                                                 xItem.IsExternalValue)));
                }
                if (aType.FullName != "System.Object" && aType.BaseType != null) {
                    aType = aType.BaseType;
                } else {
                    break;
                }
            } while (true);
        }

        public static Dictionary<string, TypeInformation.Field> GetTypeFieldInfo(Type aType,
                                                                                 out int aObjectStorageSize) {
            var xTypeFields = new List<KeyValuePair<string, TypeInformation.Field>>();
            aObjectStorageSize = 0;
            GetTypeFieldInfoImpl(xTypeFields,
                                 aType,
                                 ref aObjectStorageSize);
            if (aType.IsExplicitLayout) {
                var xStructLayout = aType.StructLayoutAttribute;
                if (xStructLayout.Size == 0) {
                    aObjectStorageSize = (from item in xTypeFields
                                          let xSize = item.Value.Offset + item.Value.Size
                                          orderby xSize descending
                                          select xSize).FirstOrDefault();
                } else {
                    aObjectStorageSize = xStructLayout.Size;
                }
            }
            int xOffset = 0;
            Dictionary<string, TypeInformation.Field> xResult = new Dictionary<string, TypeInformation.Field>();
            foreach (var item in xTypeFields) {
                var xItem = item.Value;
                if (item.Value.Offset == -1) {
                    xItem.Offset = xOffset;
                    xOffset += xItem.Size;
                }
                xResult.Add(item.Key,
                            xItem);
            }
            return xResult;
        }

        private static Op GetOpFromType(Type aType,
                                        ILReader aReader,
                                        MethodInformation aMethodInfo) {
            return (IL.Op)Activator.CreateInstance(aType,
                                                   aReader,
                                                   aMethodInfo);
        }

        public static void QueueStaticField(FieldInfo aField) {
            if (mCurrent == null) {
                throw new Exception("ERROR: No Current Engine found!");
            }
            using (mCurrent.mStaticFieldsLocker.AcquireReaderLock()) {
                if (mCurrent.mStaticFields.ContainsKey(aField)) {
                    return;
                }
            }
            using (mCurrent.mStaticFieldsLocker.AcquireWriterLock()) {
                if (!mCurrent.mStaticFields.ContainsKey(aField)) {
                    mCurrent.mStaticFields.Add(aField,
                                               new QueuedStaticFieldInformation());
                }
            }
        }

        public static void QueueStaticField(string aAssembly,
                                            string aType,
                                            string aField,
                                            out string aFieldName) {
            if (mCurrent == null) {
                throw new Exception("ERROR: No Current Engine found!");
            }
            Type xTypeDef = GetType(aAssembly,
                                    aType);
            var xFieldDef = xTypeDef.GetField(aField);
            if (xFieldDef != null) {
                QueueStaticField(xFieldDef);
                aFieldName = DataMember.GetStaticFieldName(xFieldDef);
                return;
            }
            throw new Exception("Field not found!(" + String.Format("{0}/{1}/{2}",
                                                                    aAssembly,
                                                                    aType,
                                                                    aField));
        }

        public static void QueueStaticField(FieldInfo aField,
                                            out string aDataName) {
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
            using(mCurrent.mMethodsLocker.AcquireReaderLock()) {
                if(mCurrent.mMethods.ContainsKey(aMethod)) {
                    return;
                }
            }
            using (mCurrent.mMethodsLocker.AcquireWriterLock()) {
                if (!mCurrent.mMethods.ContainsKey(aMethod)) {
                    if (mCurrent.mMethods is ReadOnlyDictionary<MethodBase, QueuedMethodInformation>) {
                        EmitDependencyGraphLine(false,
                                                aMethod.GetFullName());
                        throw new Exception("Cannot queue " + aMethod.GetFullName());
                    }
                    EmitDependencyGraphLine(false,
                                            aMethod.GetFullName());
                    mCurrent.mMethods.Add(aMethod,
                                          new QueuedMethodInformation() {
                                                                            Processed = false,
                                                                            PreProcessed = false,
                                                                            Index = mCurrent.mMethods.Count
                                                                        });
                }
            }
        }

        public static int GetMethodIdentifier(MethodBase aMethod) {
            QueueMethod(aMethod);
            using (mCurrent.mMethodsLocker.AcquireReaderLock()) {
                return mCurrent.mMethods[aMethod].Index;
            }
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
                if (aType.IsArray) {
                    aType = typeof(Array);
                } else {
                    aType = aType.GetElementType();
                }
            }
            using(mCurrent.mTypesLocker.AcquireReaderLock()) {
                var xItem = mCurrent.mTypes.FirstOrDefault(x => x.FullName.Equals(aType.FullName));
                if (xItem != null) {
                    return mCurrent.mTypes.IndexOf(xItem);
                }
            }
            Type xFoundItem;
            using (mCurrent.mTypesLocker.AcquireWriterLock()) {
                xFoundItem = mCurrent.mTypes.FirstOrDefault(x => x.FullName.Equals(aType.FullName));

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
        }

        public static Assembly GetCrawledAssembly() {
            if (mCurrent == null) {
                throw new Exception("ERROR: No Current Engine found!");
            }
            return mCurrent.mCrawledAssembly;
        }

        public static void QueueMethod2(string aAssembly,
                                        string aType,
                                        string aMethod) {
            MethodBase xMethodDef;
            QueueMethod2(aAssembly,
                         aType,
                         aMethod,
                         out xMethodDef);
        }

        public static void QueueMethod2(string aAssembly,
                                        string aType,
                                        string aMethod,
                                        out MethodBase aMethodDef) {
            Type xTypeDef = GetType(aAssembly,
                                    aType);
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

        private void OnDebugLog(LogSeverityEnum aSeverity,
                                string aMessage,
                                params object[] args) {
            if (mDebugLog != null) {
                mDebugLog(aSeverity,
                          String.Format(aMessage,
                                        args));
            }
        }

        private SortedList<string, Assembly> mAssemblyDefCache = new SortedList<string, Assembly>();

        public static Type GetType(string aAssembly,
                                   string aType) {
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
            return GetType(xAssemblyDef,
                           aType);
        }

        public static Type GetType(Assembly aAssembly,
                                   string aType) {
            if (mCurrent == null) {
                throw new Exception("ERROR: No Current Engine found!");
            }
            string xActualTypeName = aType;
            if (xActualTypeName.Contains("<") && xActualTypeName.Contains(">")) {
                xActualTypeName = xActualTypeName.Substring(0,
                                                            xActualTypeName.IndexOf("<"));
            }
            Type xResult = aAssembly.GetType(aType,
                                             false);
            if (xResult != null) {
                RegisterType(xResult);
                return xResult;
            }
            throw new Exception("Type '" + aType + "' not found in assembly '" + aAssembly + "'!");
        }

        public static MethodBase GetMethodBase(Type aType,
                                               string aMethod,
                                               params string[] aParamTypes) {
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
        public static IEnumerable<Assembly> GetAllAssemblies() {
            using (mCurrent.mMethodsLocker.AcquireReaderLock()) {
                return (from item in mCurrent.mMethods.Keys
                        select item.DeclaringType.Module.Assembly).Distinct(new AssemblyEqualityComparer()).ToArray();
            }
        }

        private int mInstructionsToSkip = 0;

        public static void SetInstructionsToSkip(int aCount) {
            if (mCurrent == null) {
                throw new Exception("No Current Engine!");
            }
            mCurrent.mInstructionsToSkip = aCount;
        }

        #region Dependency graph code

        private static bool mEmitDependencyGraph = false;

        public static void EmitDependencyGraphLine(bool aIsContainer, string aMessage) {
        }

        static Engine() {
            mEmitDependencyGraph = Environment.GetEnvironmentVariables().Contains("CosmosDependencyGraph") || Environment.MachineName.Equals("laptop-matthijs",
                                                                                                                                             StringComparison.InvariantCultureIgnoreCase);
            if (mEmitDependencyGraph) {
                File.Delete(@"d:\dependencygraph.txt");
            }
        }

        #endregion

    }
}