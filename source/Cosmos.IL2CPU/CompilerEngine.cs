using Cosmos.Build.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cosmos.Debug.Common;

namespace Cosmos.IL2CPU
{
    // http://blogs.msdn.com/b/visualstudio/archive/2010/07/06/debugging-msbuild-script-with-visual-studio.aspx
    public class CompilerEngine
    {
        const string FULLASSEMBLYNAME_KERNEL = "Cosmos.System.Kernel";

        public Action<string> OnLogMessage;
        public Action<string> OnLogError;
        public Action<string> OnLogWarning;
        public Action<Exception> OnLogException;
        protected static Action<string> mStaticLog = null;

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
        public string[] AdditionalSearchDirs { get; set; }
        public string[] AdditionalReferences { get; set; }

        public bool DebugEnabled = false;
        public bool StackCorruptionDetectionEnabled = false;
        protected StackCorruptionDetectionLevel mStackCorruptionDetectionLevel = Cosmos.Build.Common.StackCorruptionDetectionLevel.MethodFooters;
        protected DebugMode mDebugMode = Cosmos.Build.Common.DebugMode.Source;
        protected TraceAssemblies mTraceAssemblies = Cosmos.Build.Common.TraceAssemblies.All;
        protected static List<string> mSearchDirs = new List<string>();

        public string AssemblerLog = "Cosmos.Assembler.log";

        protected void LogTime(string message)
        {
        }

        protected void LogMessage(string aMsg)
        {
            OnLogMessage?.Invoke(aMsg);
        }

        protected void LogWarning(string aMsg)
        {
            OnLogWarning?.Invoke(aMsg);
        }

        protected void LogError(string aMsg)
        {
            OnLogError?.Invoke(aMsg);
        }

        protected void LogException(Exception e)
        {
            OnLogException?.Invoke(e);
        }

        private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var xShortName = args.Name;
            if (xShortName.Contains(','))
            {
                xShortName = xShortName.Substring(0, xShortName.IndexOf(','));
            }

            // Check already loaded assemblies.
            foreach (var assembly in AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies())
            {
                var xLoadedShortName = assembly.GetName().Name;
                if (xLoadedShortName == xShortName)
                {
                    return assembly;
                }
            }

            // Check nuget packages.
            foreach (var xRef in AdditionalReferences)
            {
                var xAssemblyName = AssemblyName.GetAssemblyName(xRef);
                if (xAssemblyName.Name == xShortName)
                {
                    return Assembly.ReflectionOnlyLoadFrom(xRef);
                }
            }

            // Check search directories.
            foreach (var xDir in mSearchDirs)
            {
                var xPath = Path.Combine(xDir, xShortName + ".dll");
                if (File.Exists(xPath))
                {
                    return Assembly.ReflectionOnlyLoadFrom(xPath);
                }
                xPath = Path.Combine(xDir, xShortName + ".exe");
                if (File.Exists(xPath))
                {
                    return Assembly.ReflectionOnlyLoadFrom(xPath);
                }
            }
            // check for path in as requested dll is stored, this makes refrenced dll project working
            var xPathAsRequested = Path.Combine(Path.GetDirectoryName(args.RequestingAssembly.Location), xShortName + ".dll");
            if (File.Exists(xPathAsRequested))
            {
                return Assembly.ReflectionOnlyLoadFrom(xPathAsRequested);
            }
            mStaticLog?.Invoke($"Assembly '{args.Name}' not resolved!");
            return null;
        }

        private bool EnsureCosmosPathsInitialization()
        {
            try
            {
                CosmosPaths.Initialize();
                return true;
            }
            catch (Exception e)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("Error while initializing Cosmos paths");
                for (Exception scannedException = e; null != scannedException; scannedException = scannedException.InnerException)
                {
                    builder.Append(" | " + scannedException.Message);
                }
                LogError(builder.ToString());
                return false;
            }
        }

        protected bool Initialize()
        {
            if (!EnsureCosmosPathsInitialization())
            {
                return false;
            }

            if (AdditionalSearchDirs != null)
            {
                mSearchDirs.AddRange(AdditionalSearchDirs);
            }

            // Add UserKit dirs for asms to load from.
            mSearchDirs.Add(Path.GetDirectoryName(typeof(CompilerEngine).Assembly.Location));
            mSearchDirs.Add(CosmosPaths.UserKit);
            mSearchDirs.Add(CosmosPaths.Kernel);


            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;

            mDebugMode = (DebugMode)Enum.Parse(typeof(DebugMode), DebugMode);
            if (string.IsNullOrEmpty(TraceAssemblies))
            {
                mTraceAssemblies = Cosmos.Build.Common.TraceAssemblies.User;
            }
            else
            {
                if (!Enum.GetNames(typeof(TraceAssemblies)).Contains(TraceAssemblies, StringComparer.InvariantCultureIgnoreCase))
                {
                    LogError("Invalid TraceAssemblies specified");
                    return false;
                }
                mTraceAssemblies = (TraceAssemblies)Enum.Parse(typeof(TraceAssemblies), TraceAssemblies);
            }

            if (string.IsNullOrEmpty(StackCorruptionDetectionLevel))
            {
                mStackCorruptionDetectionLevel = Cosmos.Build.Common.StackCorruptionDetectionLevel.MethodFooters;
            }
            else
            {
                mStackCorruptionDetectionLevel = (StackCorruptionDetectionLevel)Enum.Parse(typeof(StackCorruptionDetectionLevel), StackCorruptionDetectionLevel);
            }

            return true;
        }

        public bool Execute()
        {
            try
            {
                LogMessage("Executing IL2CPU on assembly");
                if (!Initialize())
                {
                    return false;
                }
                LogTime("Engine execute started");

                // Find the kernel's entry point. We are looking for a public class Kernel, with public static void Boot()
                var xInitMethod = LoadAssemblies();
                if (xInitMethod == null)
                {
                    return false;
                }

                var xOutputFilename = Path.Combine(Path.GetDirectoryName(OutputFilename), Path.GetFileNameWithoutExtension(OutputFilename));
                if (!DebugEnabled)
                {
                    // Default of 1 is in Cosmos.Targets. Need to change to use proj props.
                    DebugCom = 0;
                }

                using (var xAsm = GetAppAssembler())
                {
                    using (var xDebugInfo = new DebugInfo(xOutputFilename + ".cdb", true, false))
                    {
                        xAsm.DebugInfo = xDebugInfo;
                        xAsm.DebugEnabled = DebugEnabled;
                        xAsm.StackCorruptionDetection = StackCorruptionDetectionEnabled;
                        xAsm.StackCorruptionDetectionLevel = mStackCorruptionDetectionLevel;
                        xAsm.DebugMode = mDebugMode;
                        xAsm.TraceAssemblies = mTraceAssemblies;
                        xAsm.IgnoreDebugStubAttribute = IgnoreDebugStubAttribute;
                        if (DebugEnabled == false)
                        {
                            xAsm.ShouldOptimize = true;
                        }

                        xAsm.Assembler.Initialize();
                        using (var xScanner = new ILScanner(xAsm))
                        {
                            xScanner.LogException = LogException;
                            xScanner.LogWarning = LogWarning;
                            CompilerHelpers.DebugEvent += LogMessage;
                            if (EnableLogging)
                            {
                                var xLogFile = xOutputFilename + ".log.html";
                                if (false == xScanner.EnableLogging(xLogFile))
                                {
                                    // file creation not possible
                                    EnableLogging = false;
                                    LogWarning("Could not create the file \"" + xLogFile + "\"! No log will be created!");
                                }
                            }
                            xScanner.QueueMethod(xInitMethod.DeclaringType.BaseType.GetMethod("Start"));
                            xScanner.Execute(xInitMethod);

                            AppAssemblerRingsCheck.Execute(xScanner, xInitMethod.DeclaringType.Assembly);

                            using (var xOut = new StreamWriter(OutputFilename, false, Encoding.ASCII, 128 * 1024))
                            {
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
            }
            catch (Exception ex)
            {
                LogException(ex);
                LogMessage("Loaded assemblies: ");
                foreach (var xAsm in AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies())
                {
                    // HACK: find another way to skip dynamic assemblies (which belong to dynamic methods)
                    if (xAsm.IsDynamic)
                    {
                        continue;
                    }

                    try
                    {
                        LogMessage(xAsm.Location);
                    }
                    catch
                    {
                    }
                }
                return false;
            }
        }

        private AppAssembler GetAppAssembler()
        {
            if (mLoadedExtensions == null)
            {
                throw new InvalidOperationException("Extensions have not been loaded!");
            }
            foreach (var xExt in mLoadedExtensions)
            {
                AppAssembler xResult;
                if (xExt.TryCreateAppAssembler(DebugCom, AssemblerLog, out xResult))
                {
                    return xResult;
                }
            }

            return new AppAssembler(DebugCom, AssemblerLog);
        }

        /// <summary>Load every refernced assemblies that have an associated FullPath property and seek for
        /// the kernel default constructor.</summary>
        /// <returns>The kernel default constructor or a null reference if either none or several such
        /// constructor could be found.</returns>
        private MethodBase LoadAssemblies()
        {
            // Try to load explicit path references.
            // These are the references of our boot project. We dont actually ever load the boot
            // project asm. Instead the references will contain plugs, and the kernel. We load
            // them then find the entry point in the kernel.
            //
            // Plugs and refs in this list will be loaded absolute (or as proj refs) only. Asm resolution
            // will not be tried on them, but will on ASMs they reference.
            //
            // TODO - Update to use Load for Reflection only, but note that this wont load references
            // and we have to do it manually (Probably better for us anyways)

            mLoadedExtensions = new List<CompilerExtensionBase>();
            Type xKernelType = null;

            foreach (string xReference in References)
            {
                if (File.Exists(xReference))
                {
                    var xAssembly = Assembly.ReflectionOnlyLoadFrom(xReference);

                    LogMessage($"Looking for kernel in {xAssembly}");
                    foreach (var xType in xAssembly.ExportedTypes)
                    {
                        if (!xType.IsGenericTypeDefinition && !xType.IsAbstract)
                        {
                            LogMessage($"Checking type {xType}");
                            if (xType.BaseType.Name == "Kernel")
                            {
                                // found kernel?
                                if (xKernelType != null)
                                {
                                    // already a kernel found, which is not supported.
                                    LogError($"Two kernels found! '{xType.AssemblyQualifiedName}' and '{xKernelType.AssemblyQualifiedName}'");
                                    return null;
                                }
                                xKernelType = xType;
                            }
                        }
                    }

                    //var xCompilerExtensionsMetas = xRef.GetReflectionOnlyCustomAttributes<CompilerExtensionAttribute>();
                    //foreach (var xMeta in xCompilerExtensionsMetas)
                    //{
                    //    mLoadedExtensions.Add((CompilerExtensionBase)Activator.CreateInstance(xMeta.AttributeType));
                    //}
                }
            }

            if (xKernelType == null)
            {
                LogError("No Kernel found!");
                return null;
            }
            var xCtor = xKernelType.GetConstructor(Type.EmptyTypes);
            if (xCtor == null)
            {
                LogError("Kernel has no public default constructor");
                return null;
            }
            return xCtor;
        }

        private List<CompilerExtensionBase> mLoadedExtensions;
    }

    public static class AssemblyExtensions
    {
        public static IEnumerable<CustomAttributeData> GetReflectionOnlyCustomAttributes<T>(this Assembly aAssembly)
        {
            var xAttributes = aAssembly.GetCustomAttributesData();
            return xAttributes;
        }
    }
}
