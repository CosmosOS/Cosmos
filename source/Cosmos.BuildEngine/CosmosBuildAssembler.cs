using System;
using System.Collections.Generic;

namespace Cosmos.BuildEngine
{
    /// <summary>
    /// An assembler with which Cosmos may be assembled.
    /// </summary>
    public abstract class CosmosBuildAssembler
    {
        /// <summary>
        /// Assembles a master assembly file.
        /// </summary>
        /// <param name="MasterFilePath">Path (relative to build directory) of the Cosmos master assembly file.</param>
        public abstract void AssembleMaster(String MasterFilePath, IEnumerable<BuildOption> Options);
        public abstract IEnumerable<BuildOption> GetBuildOptions();
        public abstract IEnumerable<CosmosBuildTarget> GetSupportedTargets();
        public abstract String GetDisplayName();
    }
}