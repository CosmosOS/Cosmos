using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Indy.IL2CPU {
    public class PassedEngineValue {
        public string aAssembly;
        public TargetPlatformEnum aTargetPlatform;
        public IEnumerable<string> aPlugs;
        public DebugMode aDebugMode;
        public byte aDebugComNumber;
        public string aOutputDir;
        public TraceAssemblies TraceAssemblies;
        public bool GDBDebug;
        
        public PassedEngineValue(string aAssembly,
         TargetPlatformEnum aTargetPlatform,
         IEnumerable<string> aPlugs,
         DebugMode aDebugMode,
         byte aDebugComNumber,
            bool aGDBDebug,
         string aOutputDir
         , TraceAssemblies aTraceAssemblies) {
            this.aAssembly = aAssembly;
            this.aTargetPlatform = aTargetPlatform;
            this.aPlugs = aPlugs;
            this.aDebugMode = aDebugMode;
            this.aDebugComNumber = aDebugComNumber;
            this.aOutputDir = aOutputDir;
            GDBDebug= aGDBDebug;
            this.TraceAssemblies = aTraceAssemblies;
        }
    }
}
