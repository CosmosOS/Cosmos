using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Reflection;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;
using System.IO;
using Cosmos.Build.Common;
using Microsoft.Win32;
using Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU;
using System.Reflection.Emit;

namespace Cosmos.Build.MSBuild {
    // Class is separated from MSBuild task so we can call it from debugging and standalone applications.
    public class IL2CPUTask {
        //public Action<string> OnLog(string aMsg); 

        protected void Log(string aMsg) {
            //OnLog(aMsg);
        }

        protected static void CheckFirstTime()
        {
            var xSearchDirs = new List<string>();
            xSearchDirs.Add(Path.GetDirectoryName(typeof(IL2CPU).Assembly.Location));

            using (var xReg = Registry.LocalMachine.OpenSubKey("Software\\Cosmos", false))
            {
                var xPath = (string)xReg.GetValue(null);
                xSearchDirs.Add(xPath);
                xSearchDirs.Add(Path.Combine(xPath, "Kernel"));
            }
            mSearchDirs = xSearchDirs.ToArray();

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        protected static string[] mSearchDirs = new string[0];

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var xShortName = args.Name;
            if (xShortName.Contains(','))
            {
                xShortName = xShortName.Substring(0, xShortName.IndexOf(','));
                // TODO: remove following statement if it proves unnecessary
                if (xShortName.Contains(','))
                {
                    throw new Exception("Algo error");
                }
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
            if (mStaticLog != null)
            {
                mStaticLog("Assembly '" + args.Name + "' not resolved!");
            }
            return null;
        }

        protected static Action<string> mStaticLog = null;

        public string DebugMode
        {
            get;
            set;
        }

        public string TraceAssemblies
        {
            get;
            set;
        }

        public byte DebugCom
        {
            get;
            set;
        }

        public bool UseNAsm
        {
            get;
            set;
        }

        public ITaskItem[] References
        {
            get;
            set;
        }

        public string OutputFilename
        {
            get;
            set;
        }

        public bool EnableLogging
        {
            get;
            set;
        }

        public bool EmitDebugSymbols
        {
            get;
            set;
        }

        protected bool Initialize()
        {
            CheckFirstTime();
            // load searchpaths:
            //Log.LogMessage("SearchPath: '{0}'", References);
            if (References != null)
            {
                var xSearchPaths = new List<string>(mSearchDirs);
                foreach (var xRef in References)
                {
                    if (xRef.MetadataNames.OfType<string>().Contains("FullPath"))
                    {
                        var xDir = Path.GetDirectoryName(xRef.GetMetadata("FullPath"));
                        if (!xSearchPaths.Contains(xDir))
                        {
                            xSearchPaths.Insert(0, xDir);
                        }
                        var xName = xRef.GetMetadata("FullPath");
                        if (xName.Length > 0)
                        {
                            Assembly.LoadFile(xName);
                        }
                    }
                }
                mSearchDirs = xSearchPaths.ToArray();
            }
            if (String.IsNullOrEmpty(DebugMode))
            {
                mDebugMode = Cosmos.Build.Common.DebugMode.None;
            }
            else
            {
                if (!Enum.GetNames(typeof(DebugMode)).Contains(DebugMode, StringComparer.InvariantCultureIgnoreCase))
                {
                    //Log.LogError("Invalid DebugMode specified");
                    return false;
                }
                mDebugMode = (DebugMode)Enum.Parse(typeof(DebugMode), DebugMode);
            }
            if (String.IsNullOrEmpty(TraceAssemblies))
            {
                mTraceAssemblies = Cosmos.Build.Common.TraceAssemblies.User;
            }
            else
            {
                if (!Enum.GetNames(typeof(TraceAssemblies)).Contains(TraceAssemblies, StringComparer.InvariantCultureIgnoreCase))
                {
                    //Log.LogError("Invalid TraceAssemblies specified");
                    return false;
                }
                mTraceAssemblies = (TraceAssemblies)Enum.Parse(typeof(TraceAssemblies), TraceAssemblies);
            }
            return true;
        }


        protected DebugMode mDebugMode = Cosmos.Build.Common.DebugMode.None;
        protected TraceAssemblies mTraceAssemblies = Cosmos.Build.Common.TraceAssemblies.All;
        protected void LogTime(string message)
        {
            //
        }
        public bool Execute()
        {
            try
            {
                //Log.LogMessage("Executing IL2CPU on assembly");
                if (!Initialize())
                {
                    return false;
                }

                LogTime("Engine execute started");
                // find the kernel's entry point now. we are looking for a public class Kernel, with public static void Boot()
                var xInitMethod = RetrieveEntryPoint();
                if (xInitMethod == null)
                {
                    return false;
                }
                var xOutputFilename = Path.Combine(Path.GetDirectoryName(OutputFilename), Path.GetFileNameWithoutExtension(OutputFilename));
                if (mDebugMode == Common.DebugMode.None)
                {
                    DebugCom = 0;
                }
                var xAsm = new AppAssemblerNasm(DebugCom);
                xAsm.DebugMode = mDebugMode;
                xAsm.TraceAssemblies = mTraceAssemblies;
#if OUTPUT_ELF
                xAsm.EmitELF = true;
#endif

                var xNasmAsm = (AssemblerNasm)xAsm.Assembler;
                xAsm.Assembler.Initialize();
                using (var xScanner = new ILScanner(xAsm))
                {
                    //xScanner.TempDebug += x => Log.LogMessage(x);
                    if (EnableLogging)
                    {
                        xScanner.EnableLogging(xOutputFilename + ".log.html");
                    }
                    // TODO: shouldn't be here?
                    xScanner.QueueMethod(xInitMethod.DeclaringType.BaseType.GetMethod("Start"));
                    xScanner.Execute(xInitMethod);

                    using (var xOut = new StreamWriter(OutputFilename, false))
                    {
                        if (EmitDebugSymbols)
                        {
                            xNasmAsm.FlushText(xOut);
                            xAsm.WriteDebugSymbols(xOutputFilename + ".cxdb");
                        }
                        else
                        {
                            xAsm.Assembler.FlushText(xOut);
                        }
                    }
                }
                LogTime("Engine execute finished");
                return true;
            }
            catch (Exception E)
            {
                //Log.LogErrorFromException(E, true);
                //Log.LogMessage("Loaded assemblies: ");
                foreach (var xAsm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    // HACK: find another way to skip dynamic assemblies (which belong to dynamic methods)
                    try
                    {
                        //Log.LogMessage(xAsm.Location);
                    }
                    catch
                    {
                    }
                }
                return false;
            }
        }

        protected MethodBase RetrieveEntryPoint()
        {
            Type xFoundType = null;
            #region detect entry point method
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
                            if (xType.IsGenericTypeDefinition)
                            {
                                continue;
                            }
                            if (xType.IsAbstract)
                            {
                                continue;
                            }
                            // FIX THIS: when the kernel class changes, fix the name below
                            if (xType.BaseType.FullName == "Cosmos.System.Kernel")
                            {
                                // found kernel?
                                if (xFoundType != null)
                                {
                                    // already a kernel found, which is not supported.
                                    //Log.LogError("Two kernels found! '{0}' and '{1}'", xType.AssemblyQualifiedName, xFoundType.AssemblyQualifiedName);
                                    return null;
                                }
                                xFoundType = xType;
                            }
                        }
                    }
                }
            }
            #endregion detect entry point method
            if (xFoundType == null)
            {
                //Log.LogError("No Kernel found!");
                return null;
            }
            var xCtor = xFoundType.GetConstructor(Type.EmptyTypes);
            if (xCtor == null)
            {
                //Log.LogError("Kernel has no public default constructor");
                return null;
            }
            return xCtor;
        }
    }
}
