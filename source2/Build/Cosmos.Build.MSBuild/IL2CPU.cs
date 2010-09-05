using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Cosmos.Build.Common;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU;

namespace Cosmos.Build.MSBuild
{
    public class IL2CPU: AppDomainIsolatedTask
    {
        #region properties
        [Required]
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

        private static bool mFirstTime = true;
        private DebugMode mDebugMode = Cosmos.Build.Common.DebugMode.None;
        private TraceAssemblies mTraceAssemblies = Cosmos.Build.Common.TraceAssemblies.All;

        private static void CheckFirstTime()
        {
            if (mFirstTime)
            {
                mFirstTime = false;
                var xSearchDirs = new List<string>();
                xSearchDirs.Add(Path.GetDirectoryName(typeof(Cosmos.Build.MSBuild.IL2CPU).Assembly.Location));

                using (var xReg = Registry.LocalMachine.OpenSubKey("Software\\Cosmos", false))
                {
                    var xPath = (string)xReg.GetValue(null);
                    xSearchDirs.Add(xPath);
                    xSearchDirs.Add(Path.Combine(xPath, "Kernel"));
                }
                mSearchDirs = xSearchDirs.ToArray();

                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
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

        private bool Initialize()
        {
            CheckFirstTime();
            // load searchpaths:
            Log.LogMessage("SearchPath: '{0}'", References);
            if (References != null)
            {
                var xSearchPaths = new List<string>(mSearchDirs);
                foreach (var xName in mParameter.References)
                {
                    var xDir = Path.GetDirectoryName(xName);
                    if (!xSearchPaths.Contains(xDir))
                        xSearchPaths.Insert(0, xDir);
                    Assembly.LoadFile(xName);
                }
                mSearchDirs = xSearchPaths.ToArray();
            }
            if (String.IsNullOrEmpty(mParameter.DebugMode))
            {
                mDebugMode = Cosmos.Build.Common.DebugMode.None;
            }
            else
            {
                if (!Enum.GetNames(typeof(DebugMode)).Contains(mParameter.DebugMode, StringComparer.InvariantCultureIgnoreCase))
                {
                    Log.LogError("Invalid DebugMode specified");
                    return false;
                }
                mDebugMode = (DebugMode)Enum.Parse(typeof(DebugMode), mParameter.DebugMode);
            }
            if (String.IsNullOrEmpty(mParameter.TraceAssemblies))
            {
                mTraceAssemblies = Cosmos.Build.Common.TraceAssemblies.User;
            }
            else
            {
                if (!Enum.GetNames(typeof(TraceAssemblies)).Contains(mParameter.TraceAssemblies, StringComparer.InvariantCultureIgnoreCase))
                {
                    Log.LogError("Invalid TraceAssemblies specified");
                    return false;
                }
                mTraceAssemblies = (TraceAssemblies)Enum.Parse(typeof(TraceAssemblies), mParameter.TraceAssemblies);
            }
            return true;
        }

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

               Log.LogMessage("Engine execute started");
               // find the kernel's entry point now. we are looking for a public class Kernel, with public static void Boot()
               var xInitMethod = RetrieveEntryPoint();
               if (xInitMethod == null)
               {
                   return false;
               }
               var xOutputFilename = Path.Combine(Path.GetDirectoryName(mParameter.OutputFilename), Path.GetFileNameWithoutExtension(mParameter.OutputFilename));
               if (mDebugMode == Common.DebugMode.None)
               {
                   mParameter.DebugCom = 0;
               }
               var xAsm = new AppAssemblerNasm(mParameter.DebugCom);
               xAsm.DebugMode = mDebugMode;
               xAsm.TraceAssemblies = mTraceAssemblies;
#if OUTPUT_ELF
                xAsm.EmitELF = true;
#endif

               var xNasmAsm = (AssemblerNasm)xAsm.Assembler;
               xAsm.Assembler.Initialize();
               using (var xScanner = new ILScanner(xAsm))
               {
                   xScanner.TempDebug += x => Console.WriteLine(x);
                   if (mParameter.EnableLogging)
                   {
                       xScanner.EnableLogging(xOutputFilename + ".log.html");
                   }
                   // TODO: shouldn't be here?
                   xScanner.QueueMethod(xInitMethod.DeclaringType.BaseType.GetMethod("Start"));
                   xScanner.Execute(xInitMethod);

                   using (var xOut = new StreamWriter(mParameter.OutputFilename, false))
                   {
                       if (mParameter.EmitDebugSymbols)
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
               Console.Error.WriteLine(E.Message + ":\r\n" + E.StackTrace);
               //Log.LogErrorFromException(E, true);
               Console.WriteLine("Loaded assemblies: ");
               foreach (var xAsm in AppDomain.CurrentDomain.GetAssemblies())
               {
                   // HACK: find another way to skip dynamic assemblies (which belong to dynamic methods)
                   try
                   {
                       Console.WriteLine(xAsm.Location);
                   }
                   catch
                   {
                   }
               }
               return false;
           }
        }

        private MethodBase RetrieveEntryPoint()
        {
            Type xFoundType = null;
            #region detect entry point method
            
            foreach (var xFile in mParameter.References)
            {
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
                        if (xType.BaseType == null)
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
                                Log.LogError("Two kernels found! '{0}' and '{1}'", xType.AssemblyQualifiedName, xFoundType.AssemblyQualifiedName);
                                return null;
                            }
                            xFoundType = xType;
                        }
                    }
                }
            }
            #endregion detect entry point method
            if (xFoundType == null)
            {
                Log.LogError("No Kernel found!");
                return null;
            }
            var xCtor = xFoundType.GetConstructor(Type.EmptyTypes);
            if (xCtor == null)
            {
                Log.LogError("Kernel has no public default constructor");
                return null;
            }
            return xCtor;
        }
    }
}