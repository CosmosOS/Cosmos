using System;
using System.Reflection;
using System.Collections.Generic;

namespace Cosmos.BuildEngine
{
    /// <summary>
    /// A platform for which Cosmos may be built.
    /// </summary>
    public abstract class CosmosBuildPlatform
    {
        public abstract IEnumerable<CosmosBuildAssembler> GetSupportedAssemblers();
        public abstract String GetDisplayName();
        public IEnumerable<BuildOption> Options = null;

        public CosmosBuildPlatform()
        {
        }

        public void SetOptions(IEnumerable<BuildOption> options)
        {
            Options = options;
        }

        /// <summary>
        /// Translates a block of CIL into platform Assembly Code.
        /// </summary>
        /// <param name="CILCodeReader">CILReader from which the CIL block can be read.</param>
        /// <param name="DataMemberAssemblyCode">Platform Assembly Code which represents the data nessecary to assemble the specified block of CIL code.</param>
        /// <param name="InstructionAssemblyCode">Platform Assembly Code which represents the instructions nessecary to assemble the specified block of CIL code.</param>
        /// <returns>Null if successfull, else a string specifing the error which occured.</returns>
        public abstract String TranslateCIL(CILReader CILCodeReader, out String[] DataMemberAssemblyCode, out String[] InstructionAssemblyCode);
        public abstract String[] GetPreDataCode();
        public abstract String[] GetPostDataCode();
        public abstract String[] GetPostCodeCode();
        public abstract IEnumerable<BuildOption> GetBuildOptions();
        public abstract String GetPostfixForOptions();
    }
}