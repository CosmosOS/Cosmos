using Cosmos.Build.Common;
using Microsoft.Build.Framework;

namespace Cosmos.Build.MSBuild
{
    public class MakeISO : BaseToolTask
    {
        #region Properties

        [Required]
        public string InputFile
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

        [Required]
        public string CosmosBuildDir
        {
            get;
            set;
        }

        #endregion Properties

        public override bool Execute()
        {
            IsoMaker.Generate(CosmosBuildDir, InputFile, OutputFile);
            return true;
        }
    }
}