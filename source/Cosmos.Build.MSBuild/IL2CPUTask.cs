using Cosmos.Build.Common;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU;
using Microsoft.Win32;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data;
using System.Configuration;
using System.Data.SQLite;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Cosmos.Debug.Common;

namespace Cosmos.Build.MSBuild
{
    // http://blogs.msdn.com/b/visualstudio/archive/2010/07/06/debugging-msbuild-script-with-visual-studio.aspx
    public class IL2CPUTask
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
        public ITaskItem[] References { get; set; }
        public string OutputFilename { get; set; }
        public bool EnableLogging { get; set; }
        public bool EmitDebugSymbols { get; set; }
        public bool IgnoreDebugStubAttribute { get; set; }

        protected void LogMessage(string aMsg)
        {
            if (OnLogMessage != null)
            {
                OnLogMessage(aMsg);
            }
        }

        protected void LogWarning(string aMsg)
        {
            if (OnLogWarning != null)
            {
                OnLogWarning(aMsg);
            }
        }

        protected void LogError(string aMsg)
        {
            if (OnLogError != null)
            {
                OnLogError(aMsg);
            }
        }

        protected void LogException(Exception e)
        {
            if (OnLogException != null)
            {
                OnLogException(e);
            }
        }

        protected static List<string> mSearchDirs = new List<string>();
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var xShortName = args.Name;
            if (xShortName.Contains(','))
            {
                xShortName = xShortName.Substring(0, xShortName.IndexOf(','));
            }
            foreach (var xDir in mSearchDirs)
            {
                var xPath = Path.Combine(xDir, xShortName + ".dll");
                if (File.Exists(xPath))
                {
                    return Assembly.LoadFrom(xPath);
                }
                xPath = Path.Combine(xDir, xShortName + ".exe");
                if (File.Exists(xPath))
                {
                    return Assembly.LoadFrom(xPath);
                }
            }
            // check for path in as requested dll is stored, this makes refrenced dll project working
            var xPathAsRequested = Path.Combine(Path.GetDirectoryName(args.RequestingAssembly.Location), xShortName + ".dll");
            if (File.Exists(xPathAsRequested))
            {
                return Assembly.LoadFrom(xPathAsRequested);
            }
            if (mStaticLog != null)
            {
                mStaticLog(string.Format("Assembly '{0}' not resolved!", args.Name));
            }
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
            // Add UserKit dirs for asms to load from.
            mSearchDirs.Add(Path.GetDirectoryName(typeof(IL2CPU).Assembly.Location));
            if (!EnsureCosmosPathsInitialization()) { return false; }
            mSearchDirs.Add(CosmosPaths.UserKit);
            mSearchDirs.Add(CosmosPaths.Kernel);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            // This seems to be to try to load plugs on demand from their own dirs, but 
            // it often just causes load conflicts, and weird errors like "implementation not found" 
            // for a method, even when both the output user kit dir and local bin dir have up to date
            // and same assemblies. 
            // So its removed for now and we should find a better way to dynamically load plugs in 
            // future.
            // Furthermore, it only scanned plugs/asms reffed from the boot proj, not the kernel proj 
            // so it was bugged there too.
            //if (References != null) {
            //  foreach (var xRef in References) {
            //    if (xRef.MetadataNames.OfType<string>().Contains("FullPath")) {
            //      var xName = xRef.GetMetadata("FullPath");
            //      var xDir = Path.GetDirectoryName(xName);
            //      if (!mSearchPaths.Contains(xDir)) {
            //        mSearchPaths.Insert(0, xDir);
            //      }
            //    }
            //  }
            //}

            mDebugMode = (DebugMode)Enum.Parse(typeof(DebugMode), DebugMode);
            if (String.IsNullOrEmpty(TraceAssemblies))
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

            return true;
        }

        public bool DebugEnabled = false;
        public bool StackCorruptionDetectionEnabled = false;
        protected DebugMode mDebugMode = Cosmos.Build.Common.DebugMode.Source;
        protected TraceAssemblies mTraceAssemblies = Cosmos.Build.Common.TraceAssemblies.All;

        protected void LogTime(string message)
        {
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

                using (var xAsm = new AppAssembler(DebugCom))
                {
                    using (var xDebugInfo = new DebugInfo(xOutputFilename + ".cdb", true))
                    {
                        xAsm.DebugInfo = xDebugInfo;
                        xAsm.DebugEnabled = DebugEnabled;
                        xAsm.StackCorruptionDetection = StackCorruptionDetectionEnabled;
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
                            xScanner.TempDebug += x => LogMessage(x);
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

                            using (var xOut = new StreamWriter(OutputFilename, false, Encoding.ASCII , 128*1024))
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
                foreach (var xAsm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    // HACK: find another way to skip dynamic assemblies (which belong to dynamic methods)
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

            Type xKernelType = null;
            foreach (var xRef in References)
            {
                if (xRef.MetadataNames.OfType<string>().Contains("FullPath"))
                {
                    var xFile = xRef.GetMetadata("FullPath");
                    if (File.Exists(xFile))
                    {
                        var xAssembly = Assembly.LoadFile(xFile);
                        foreach (var xType in xAssembly.GetExportedTypes())
                        {
                            if (!xType.IsGenericTypeDefinition && !xType.IsAbstract)
                            {
                                if (xType.BaseType.FullName == FULLASSEMBLYNAME_KERNEL)
                                {
                                    // found kernel?
                                    if (xKernelType != null)
                                    {
                                        // already a kernel found, which is not supported.
                                        LogError(string.Format("Two kernels found! '{0}' and '{1}'", xType.AssemblyQualifiedName, xKernelType.AssemblyQualifiedName));
                                        return null;
                                    }
                                    xKernelType = xType;
                                }
                            }
                        }
                    }
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
    }
}