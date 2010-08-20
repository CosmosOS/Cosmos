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

namespace Cosmos.Build.MSBuild
{
    public class IL2CPU : AppDomainIsolatedTask
    {
        private static bool mFirstTime = true;
        public IL2CPU()
        {
//            CheckFirstTime();

            
        }

        private static void CheckFirstTime()
        {
            if (mFirstTime)
            {
//                mFirstTime = false;
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

                Assembly.LoadWithPartialName("Cosmos.Sys");
                Assembly.LoadWithPartialName("Cosmos.Hardware");
                Assembly.LoadWithPartialName("Cosmos.Kernel");
                Assembly.LoadWithPartialName("Cosmos.Sys.FileSystem");
            }
        }

        private static string[] mSearchDirs = new string[0];

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

        private static Action<string> mStaticLog = null;

        private void DoInitTypes()
        {
            Type xType;
            // Old
            xType = typeof(Cosmos.Sys.Plugs.Deboot);
            var xName = xType.FullName;
            xType = typeof(Cosmos.Kernel.Plugs.ArrayListImpl);
            xName = xType.FullName;
            // New
            xType = typeof(Cosmos.Core.Plugs.CPU);
            xName = xType.FullName;
            xType = typeof(Cosmos.System.Plugs.System.ConsoleImpl);
            xName = xType.FullName;
            xType = typeof(Cosmos.Debug.Kernel.Plugs.Debugger);
            xName = xType.FullName;
        }

        #region properties
        [Required]
        public string DebugMode{
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

        [Required]
        public bool UseNAsm
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] References
        {
            get;
            set;
        }

        [Required]
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

        #endregion

        private bool Initialize()
        {
            CheckFirstTime();            
            DoInitTypes();
            // load searchpaths:
            Log.LogMessage("SearchPath: '{0}'", References);
            if (References!=null)
            {
                var xSearchPaths = new List<string>(mSearchDirs);
                foreach (var xRef in References)
                {
                    if (xRef.MetadataNames.OfType<string>().Contains("FullPath"))
                    {
                        var xDir = Path.GetDirectoryName(xRef.GetMetadata("FullPath"));
                        if (!xSearchPaths.Contains(xDir))
                        {
                            xSearchPaths.Add(xDir);
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
                    Log.LogError("Invalid DebugMode specified");
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
                    Log.LogError("Invalid TraceAssemblies specified");
                    return false;
                }
                mTraceAssemblies = (TraceAssemblies)Enum.Parse(typeof(TraceAssemblies), TraceAssemblies);
            }
            return true;
        }


        private DebugMode mDebugMode = Cosmos.Build.Common.DebugMode.None;
        private TraceAssemblies mTraceAssemblies = Cosmos.Build.Common.TraceAssemblies.All;
        private void LogTime(string message)
        {
            //
        }
        public override bool Execute()
        {
            try
            {
                Log.LogMessage("Executing IL2CPU on assembly");
                if (!Initialize())
                {
                    return false;
                }

                LogTime("Engine execute started");
                // find the kernel's entry point now. we are looking for a public class Kernel, with public static void Boot()
                MethodBase xInitMethod=null;
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
                                if (xType.Name == "Kernel")
                                {
                                    var xMethod = xType.GetMethod("Boot");
                                    if (xMethod != null)
                                    {
                                        if (!xMethod.IsStatic)
                                        {
                                            continue;
                                        }
                                        if (xInitMethod != null)
                                        {
                                            // already found an init method. log error.
                                            Log.LogError("Project has multiple Kernel.Boot methods!");
                                            return false;
                                        }
                                        xInitMethod = xMethod;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion detect entry point method
                if (xInitMethod == null)
                {
                    Log.LogError("No Kernel.Boot method found!");
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
                    xScanner.TempDebug += x => Log.LogMessage(x);
                    if(EnableLogging)
                    {
                        xScanner.EnableLogging(xOutputFilename + ".log.html");
                    }
                    xScanner.Execute(xInitMethod);

                    using (var xOut = new StreamWriter(OutputFilename, false))
                    {
                        if(EmitDebugSymbols)
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
                Log.LogMessage("Loaded assemblies: ");
                foreach (var xAsm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Log.LogMessage(xAsm.Location);
                }
                Log.LogErrorFromException(E, true);
                return false;
            }
        }
    }
}