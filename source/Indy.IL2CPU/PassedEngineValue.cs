using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Indy.IL2CPU
{
    public class PassedEngineValue
    {
        public string aAssembly;
        public TargetPlatformEnum aTargetPlatform;
        public Func<string, string> aGetFileNameForGroup;
        public bool aInMetalMode;
        public IEnumerable<string> aPlugs;
        public DebugModeEnum aDebugMode;
        public byte aDebugComNumber;
        public string aOutputDir;
        public PassedEngineValue(string aAssembly,
                            TargetPlatformEnum aTargetPlatform,
                            Func<string, string> aGetFileNameForGroup,
                            bool aInMetalMode,
                            IEnumerable<string> aPlugs,
                            DebugModeEnum aDebugMode,
                            byte aDebugComNumber,
                            string aOutputDir)
        {
            this.aAssembly = aAssembly;
            this.aTargetPlatform = aTargetPlatform;
            this.aGetFileNameForGroup = aGetFileNameForGroup;
            this.aInMetalMode = aInMetalMode;
            this.aPlugs = aPlugs;
            this.aDebugMode = aDebugMode;
            this.aDebugComNumber = aDebugComNumber;
            this.aOutputDir = aOutputDir;
        }
    }
}
