using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using Cosmos.Build.Common;

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
      IsoMaker.Generate(InputFile, OutputFile);
      return true;
    }
  }
}
