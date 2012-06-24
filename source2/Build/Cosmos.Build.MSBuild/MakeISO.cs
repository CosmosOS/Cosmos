using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

using Mosa.Utility.IsoImage;

namespace Cosmos.Build.MSBuild {
  public class MakeISO : BaseToolTask {

    #region Properties
    [Required]
    public string InputFile {
      get;
      set;
    }

    [Required]
    public string OutputFile {
      get;
      set;
    }

    [Required]
    public string CosmosBuildDir {
      get;
      set;
    }
    #endregion

    public override bool Execute() {
      string xPath = Path.Combine(CosmosBuildDir, @"ISO\");
      if (File.Exists(OutputFile)) {
        File.Delete(OutputFile);
      }
      if (File.Exists(Path.Combine(xPath, "output.bin"))) {
        File.Delete(Path.Combine(xPath, "output.bin"));
      }

      File.Copy(InputFile, Path.Combine(xPath, "output.bin"));
      File.SetAttributes(Path.Combine(xPath, "isolinux.bin"), FileAttributes.Normal);

      Log.LogMessage("xPath = '{0}'", xPath);

      var options = new Options();
      options.BootLoadSize = 4;
      options.IsoFileName = Path.Combine(Environment.CurrentDirectory, OutputFile);
      options.BootFileName = Path.Combine(xPath, "isolinux.bin");
      options.BootInfoTable = true;
      options.IncludeFiles.Add(xPath);

      var xISO = new Iso9660Generator(options);
      xISO.Generate();

      return true;
    }
  }
}
