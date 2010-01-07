using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

namespace Cosmos.Build.MSBuild
{
    public class MakeISO: BaseToolTask
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
        #endregion
        public override bool Execute()
        {
            string xPath = Path.Combine(CosmosBuildDir, @"ISO\");
            if (File.Exists(OutputFile))
            {
                File.Delete(OutputFile);
            }
            if (File.Exists(Path.Combine(xPath, "output.bin")))
            {
                File.Delete(Path.Combine(xPath, "output.bin"));
            }
            File.Copy(InputFile, Path.Combine(xPath, "output.bin"));
            File.SetAttributes(Path.Combine(xPath, "isolinux.bin"), FileAttributes.Normal);
            
            Log.LogMessage("xPath = '{0}'", xPath);

            return ExecuteTool(
                xPath,
                Path.Combine(CosmosBuildDir, "Tools\\mkisofs.exe"),
                String.Format("-R -b isolinux.bin -no-emul-boot -boot-load-size 4 -boot-info-table -o {0} .", Path.Combine(Environment.CurrentDirectory, OutputFile)),
                "mkisofs");
        }
    }
}
