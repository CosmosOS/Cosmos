//#define COSMOSDEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

using Cosmos.Build.Common;
using Cosmos.Debug.Symbols;

namespace Cosmos.IL2CPU {
    // http://blogs.msdn.com/b/visualstudio/archive/2010/07/06/debugging-msbuild-script-with-visual-studio.aspx
    public class CompilerEngine {
        public Action<string> OnLogMessage;
        public Action<string> OnLogError;
        public Action<string> OnLogWarning;
        public Action<Exception> OnLogException;
        protected static Action<string> mStaticLog = null;

        public static string KernelPkg { get; set; }
        public static bool UseGen3Kernel
        {
            get
            {
                return string.Equals(KernelPkg, "X86G3", StringComparison.CurrentCultureIgnoreCase);
            }
        }
        public string DebugMode { get; set; }
        public string TraceAssemblies { get; set; }
        public byte DebugCom { get; set; }
        public bool UseNAsm { get; set; }
        public string[] References { get; set; }
        public string OutputFilename { get; set; }
        public bool EnableLogging { get; set; }
        public bool EmitDebugSymbols { get; set; }
        public bool IgnoreDebugStubAttribute { get; set; }
        public string StackCorruptionDetectionLevel { get; set; }
        public string[] AssemblySearchDirs { get; set; }

        private List<CompilerExtensionBase> mLoadedExtensions;

        public bool DebugEnabled = false;
        public bool StackCorruptionDetectionEnabled = false;
        protected StackCorruptionDetectionLevel mStackCorruptionDetectionLevel = Cosmos.Build.Common.StackCorruptionDetectionLevel.MethodFooters;
        protected DebugMode mDebugMode = Cosmos.Build.Common.DebugMode.Source;
        protected TraceAssemblies mTraceAssemblies = Cosmos.Build.Common.TraceAssemblies.All;

        public string AssemblerLog = "Cosmos.Assembler.log";

        protected void LogTime(string message) {
        }

        protected void LogMessage(string aMsg) {
            OnLogMessage?.Invoke(aMsg);
        }

        protected void LogWarning(string aMsg) {
            OnLogWarning?.Invoke(aMsg);
        }

        protected void LogError(string aMsg) {
            OnLogError?.Invoke(aMsg);
        }

        protected void LogException(Exception e) {
            OnLogException?.Invoke(e);
        }

        private bool EnsureCosmosPathsInitialization() {
            try {
                CosmosPaths.Initialize();
                return true;
            } catch (Exception e) {
                StringBuilder builder = new StringBuilder();
                builder.Append("Error while initializing Cosmos paths");
                for (Exception scannedException = e; null != scannedException; scannedException = scannedException.InnerException) {
                    builder.Append(" | " + scannedException.Message);
                }
                LogError(builder.ToString());
                return false;
            }
        }

        protected bool Initialize() {
            if (!EnsureCosmosPathsInitialization()) {
                return false;
            }

            mDebugMode = (DebugMode)Enum.Parse(typeof(DebugMode), DebugMode);
            if (string.IsNullOrEmpty(TraceAssemblies)) {
                mTraceAssemblies = Cosmos.Build.Common.TraceAssemblies.User;
            } else {
                if (!Enum.GetNames(typeof(TraceAssemblies)).Contains(TraceAssemblies, StringComparer.OrdinalIgnoreCase)) {
                    LogError("Invalid TraceAssemblies specified");
                    return false;
                }
                mTraceAssemblies = (TraceAssemblies)Enum.Parse(typeof(TraceAssemblies), TraceAssemblies);
            }

            if (string.IsNullOrEmpty(StackCorruptionDetectionLevel)) {
                mStackCorruptionDetectionLevel = Cosmos.Build.Common.StackCorruptionDetectionLevel.MethodFooters;
            } else {
                mStackCorruptionDetectionLevel = (StackCorruptionDetectionLevel)Enum.Parse(typeof(StackCorruptionDetectionLevel), StackCorruptionDetectionLevel);
            }

            return true;
        }

        public bool Execute() {
            try {
                LogMessage("Executing IL2CPU on assembly");
                if (!Initialize()) {
                    return false;
                }
                LogTime("Engine execute started");

                // Find the kernel's entry point. We are looking for a public class Kernel, with public static void Boot()
                var xKernelCtor = LoadAssemblies();
                if (xKernelCtor == null) {
                    return false;
                }

                var xOutputFilename = Path.Combine(Path.GetDirectoryName(OutputFilename), Path.GetFileNameWithoutExtension(OutputFilename));
                if (!DebugEnabled) {
                    // Default of 1 is in Cosmos.Targets. Need to change to use proj props.
                    DebugCom = 0;
                }

                using (var xAsm = GetAppAssembler()) {
                    using (var xDebugInfo = new DebugInfo(xOutputFilename + ".cdb", true, false)) {
                        xAsm.DebugInfo = xDebugInfo;
                        xAsm.DebugEnabled = DebugEnabled;
                        xAsm.StackCorruptionDetection = StackCorruptionDetectionEnabled;
                        xAsm.StackCorruptionDetectionLevel = mStackCorruptionDetectionLevel;
                        xAsm.DebugMode = mDebugMode;
                        xAsm.TraceAssemblies = mTraceAssemblies;
                        xAsm.IgnoreDebugStubAttribute = IgnoreDebugStubAttribute;
                        if (DebugEnabled == false) {
                            xAsm.ShouldOptimize = true;
                        }

                        xAsm.Assembler.Initialize();
                        using (var xScanner = new ILScanner(xAsm)) {
                            xScanner.LogException = LogException;
                            xScanner.LogWarning = LogWarning;
                            CompilerHelpers.DebugEvent += LogMessage;
                            if (EnableLogging) {
                                var xLogFile = xOutputFilename + ".log.html";
                                if (false == xScanner.EnableLogging(xLogFile)) {
                                    // file creation not possible
                                    EnableLogging = false;
                                    LogWarning("Could not create the file \"" + xLogFile + "\"! No log will be created!");
                                }
                            }
                            xScanner.QueueMethod(xKernelCtor.DeclaringType.GetTypeInfo().BaseType.GetTypeInfo().GetMethod(UseGen3Kernel ? "EntryPoint" : "Start"));
                            xScanner.Execute(xKernelCtor);

                            AppAssemblerRingsCheck.Execute(xScanner, xKernelCtor.DeclaringType.GetTypeInfo().Assembly);

                            using (var xOut = new StreamWriter(File.Create(OutputFilename), Encoding.ASCII, 128 * 1024)) {
                                //if (EmitDebugSymbols) {
                                xAsm.Assembler.FlushText(xOut);
                                xAsm.FinalizeDebugInfo();
                                //// for now: write debug info to console
                                //Console.WriteLine("Wrote {0} instructions and {1} datamembers", xAsm.Assembler.Instructions.Count, xAsm.Assembler.DataMembers.Count);
                                //var dict = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
                                //foreach (var instr in xAsm.Assembler.Instructions)
                                //{
                                //    var mn = instr.Mnemonic ?? "";
                                //    if (dict.ContainsKey(mn))
                                //    {
                                //        dict[mn] = dict[mn] + 1;
                                //    }
                                //    else
                                //    {
                                //        dict[mn] = 1;
                                //    }
                                //}
                                //foreach (var entry in dict)
                                //{
                                //    Console.WriteLine("{0}|{1}", entry.Key, entry.Value);
                                //}
                            }
                        }
                        // If you want to uncomment this line make sure to enable PERSISTANCE_PROFILING symbol in
                        // DebugInfo.cs file.
                        //LogMessage(string.Format("DebugInfo flatening {0} seconds, persistance : {1} seconds",
                        //    (int)xDebugInfo.FlateningDuration.TotalSeconds,
                        //    (int)xDebugInfo.PersistanceDuration.TotalSeconds));
                    }
                }
                LogTime("Engine execute finished");
                return true;
            } catch (Exception ex) {
                LogException(ex);
                LogMessage("Loaded assemblies: ");
                foreach (var xAsm in AssemblyLoadContext.Default.GetLoadedAssemblies()) {
                    if (xAsm.IsDynamic) {
                        continue;
                    }

                    try {
                        LogMessage(xAsm.Location);
                    } catch {
                    }
                }
                return false;
            }
        }

        private AppAssembler GetAppAssembler() {
            if (mLoadedExtensions == null) {
                throw new InvalidOperationException("Extensions have not been loaded!");
            }
            foreach (var xExt in mLoadedExtensions) {
                if (xExt.TryCreateAppAssembler(DebugCom, AssemblerLog, out var xResult)) {
                    return xResult;
                }
            }

            return new AppAssembler(DebugCom, AssemblerLog);
        }

        private Assembly Default_Resolving(AssemblyLoadContext aContext, AssemblyName aName) {
            foreach (var xRef in References) {
                var xName = AssemblyLoadContext.GetAssemblyName(xRef);
                if (xName.Name == aName.Name) {
                    return aContext.LoadFromAssemblyPath(xRef);
                }
            }

            foreach (var xRef in References)
            {
                var xKernelAssemblyDir = Path.GetDirectoryName(xRef);
                var xAssemblyPath = Path.Combine(xKernelAssemblyDir, aName.Name);
                if (File.Exists(xAssemblyPath + ".dll"))
                {
                    return aContext.LoadFromAssemblyPath(xAssemblyPath + ".dll");
                }
                if (File.Exists(xAssemblyPath + ".exe"))
                {
                    return aContext.LoadFromAssemblyPath(xAssemblyPath + ".exe");
                }
            }

            //var xRequestingAssembly = Assembly.GetEntryAssembly();
            //if (xRequestingAssembly != null) {
            //    // check for path in as requested dll is stored, this makes referenced dll project working
            //    var xPathAsRequested = Path.Combine(Path.GetDirectoryName(xRequestingAssembly.Location), aName.Name + ".dll");
            //    if (File.Exists(xPathAsRequested)) {
            //        return aContext.LoadFromAssemblyPath(xPathAsRequested);
            //    }
            //}

            // check for assembly in working directory
            var xPathToCheck = Path.Combine(Directory.GetCurrentDirectory(), aName.Name + ".dll");
            if (File.Exists(xPathToCheck)) {
                return aContext.LoadFromAssemblyPath(xPathToCheck);
            }

            foreach (var xDir in AssemblySearchDirs) {
                var xPath = Path.Combine(xDir, aName.Name + ".dll");
                if (File.Exists(xPath)) {
                    return aContext.LoadFromAssemblyPath(xPath);
                }
                xPath = Path.Combine(xDir, aName.Name + ".exe");
                if (File.Exists(xPath)) {
                    return aContext.LoadFromAssemblyPath(xPath);
                }
            }

            return null;
        }

        /// <summary>Load every refernced assemblies that have an associated FullPath property and seek for
        /// the kernel default constructor.</summary>
        /// <returns>The kernel default constructor or a null reference if either none or several such
        /// constructor could be found.</returns>
        private MethodBase LoadAssemblies() {
            // Try to load explicit path references.
            // These are the references of our boot project. We dont actually ever load the boot
            // project asm. Instead the references will contain plugs, and the kernel. We load
            // them then find the entry point in the kernel.
            //
            // Plugs and refs in this list will be loaded absolute (or as proj refs) only. Asm resolution
            // will not be tried on them, but will on ASMs they reference.

            AssemblyLoadContext.Default.Resolving += Default_Resolving;
            mLoadedExtensions = new List<CompilerExtensionBase>();

            string xKernelBaseName = "Cosmos.System.Boot";
            if (!UseGen3Kernel) {
                xKernelBaseName = "Cosmos.System.Kernel";
                LogMessage("Kernel is Gen2.");
            }
            LogMessage("Kernel Base: " + xKernelBaseName);

            Type xKernelType = null;
            foreach (string xRef in References) {
                LogMessage("Checking Reference: " + xRef);
                if (File.Exists(xRef)) {
                    LogMessage("  Exists");
                    var xAssembly = AssemblyLoadContext.Default.LoadFromAssemblyCacheOrPath(xRef);

                    CompilerHelpers.Debug($"Looking for kernel in {xAssembly}");

                    foreach (var xType in xAssembly.ExportedTypes) {
                        var xTypeInfo = xType.GetTypeInfo();

                        if (!xTypeInfo.IsGenericTypeDefinition && !xTypeInfo.IsAbstract) {
                            CompilerHelpers.Debug($"Checking type {xType.FullName}");

                            // We used to resolve with this:
                            //   if (xType.GetTypeInfo().IsSubclassOf(typeof(Cosmos.System.Kernel))) {
                            // But this caused a single dependency on Cosmos.System which is bad.
                            // We could use an attribute, or maybe an interface would be better in this limited case. Interface
                            // will force user to implement what is needed if replacing our core. But in the end this is a "not needed" feature
                            // and would only complicate things.
                            // So for now at least, we look by name so we dont have a dependency since the method returns a MethodBase and not a Kernel instance anyway.
                            if (xTypeInfo.BaseType.FullName == xKernelBaseName) {
                                if (xKernelType != null) {
                                    LogError($"Two kernels found: {xType.FullName} and {xKernelType.FullName}");
                                    return null;
                                }
                                xKernelType = xType;
                            }
                        }
                    }

                    var xCompilerExtensionsMetas = xAssembly.GetCustomAttributes<CompilerExtensionAttribute>();
                    foreach (var xMeta in xCompilerExtensionsMetas) {
                        mLoadedExtensions.Add((CompilerExtensionBase)Activator.CreateInstance(xMeta.Type));
                    }
                }
            }

            if (xKernelType == null) {
                LogError("No kernel found.");
                return null;
            }
            var xCtor = xKernelType.GetTypeInfo().GetConstructor(Type.EmptyTypes);
            if (xCtor == null) {
                LogError("Kernel has no public parameterless constructor.");
                return null;
            }
            return xCtor;
        }
    }
}
