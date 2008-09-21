using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Indy.IL2CPU;

namespace Cosmos.Build.Windows {
 
    public class Options {
        public const string RegKey = @"Software\Cosmos\User Kit";

        public void Load() {
            using (var xKey = Registry.CurrentUser.OpenSubKey(Options.RegKey, false)) {
                object xValue;
                
                //TODO: Use attributes or just name and reflection to save/load
                xValue = xKey.GetValue("Debug Trace Assemblies");
                if (xValue != null) {
                    TraceAssemblies = (TraceAssemblies)Enum.Parse(typeof(TraceAssemblies), (string)xValue, true);
                } else {
                    TraceAssemblies = TraceAssemblies.Cosmos;
                }
            }
        }
        
        public void Save() {
            using (var xKey = Registry.CurrentUser.CreateSubKey(Options.RegKey)) {
                xKey.SetValue("Debug Trace Assemblies", TraceAssemblies.ToString());
            }
        }
        
        public TraceAssemblies TraceAssemblies { get; set; }
    }
}
