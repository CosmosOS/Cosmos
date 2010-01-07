using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Reflection;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.X86;
using System.IO;

namespace Cosmos.Build.MSBuild
{
    public class IL2CPU : AppDomainIsolatedTask
    {
        public IL2CPU()
        {
            var xType = typeof(Cosmos.Hardware.Plugs.FCL.System.Console);
            xType = typeof(Cosmos.Sys.Plugs.Deboot);
            xType = typeof(Cosmos.Kernel.Plugs.ArrayListImpl);
        }

        #region properties
        [Required]
        public string InputAssembly
        {
            get;
            set;
        }

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

        public string DebugSymbolsFile
        {
            get;
            set;
        }

        public string LogFile
        {
            get;
            set;
        }

        [Required]
        public string OutputFile
        {
            get;
            set;
        }

        #endregion

        private bool Initialize()
        {
            if (String.IsNullOrEmpty(DebugMode))
            {
                mDebugMode = Cosmos.IL2CPU.DebugMode.None;
            }
            else
            {
                if (!Enum.GetNames(typeof(DebugMode)).Contains(DebugMode, StringComparer.InvariantCultureIgnoreCase))
                {
                    Log.LogError("Invalid DebugMode specified");
                    return false;
                }
            }
            if (String.IsNullOrEmpty(TraceAssemblies))
            {
                mTraceAssemblies = Cosmos.IL2CPU.TraceAssemblies.All;
            }
            else
            {
                if (!Enum.GetNames(typeof(TraceAssemblies)).Contains(TraceAssemblies, StringComparer.InvariantCultureIgnoreCase))
                {
                    Log.LogError("Invalid TraceAssemblies specified");
                    return false;
                }
            }
            return true;
        }

        
        private Cosmos.IL2CPU.DebugMode  mDebugMode = Cosmos.IL2CPU.DebugMode.None;
        private TraceAssemblies mTraceAssemblies = Cosmos.IL2CPU.TraceAssemblies.All;
        private void LogTime(string message)
        {
            //
        }
        public override bool Execute()
        {
            Log.LogMessage("Executing IL2CPU on assembly");
            if (!Initialize())
            {
                return false;
            }
            
            LogTime("Engine execute started");
            var xEntryAsm = Assembly.LoadFrom(InputAssembly);
            var xInitMethod = xEntryAsm.EntryPoint.DeclaringType.GetMethod("Init", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var xAsm = new AssemblerNasm(DebugCom);
            xAsm.DebugMode = mDebugMode;
            xAsm.TraceAssemblies = mTraceAssemblies;
#if OUTPUT_ELF
                xAsm.EmitELF = true;
#endif
            xAsm.Initialize();
            using (var xScanner = new ILScanner(xAsm))
            {
                if (!String.IsNullOrEmpty(LogFile))
                {
                    xScanner.EnableLogging(LogFile);
                }
                xScanner.Execute(xInitMethod);

                using (var xOut = new StreamWriter(OutputFile, false))
                {
                    if (!String.IsNullOrEmpty(DebugSymbolsFile))
                    {
                        xAsm.FlushText(xOut, DebugSymbolsFile);
                    }
                    else
                    {
                        xAsm.FlushText(xOut);
                    }
                }
            }
            LogTime("Engine execute finished");
            return true;
        }
    }
}