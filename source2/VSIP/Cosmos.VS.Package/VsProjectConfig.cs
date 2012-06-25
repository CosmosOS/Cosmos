using System;
using Path = System.IO.Path;
using Marshal = System.Runtime.InteropServices.Marshal;
using Microsoft.VisualStudio.Project;
using VSConstants = Microsoft.VisualStudio.VSConstants;
using Microsoft.VisualStudio.OLE.Interop;
using VsShellUtilities = Microsoft.VisualStudio.Shell.VsShellUtilities;
using Microsoft.VisualStudio.Shell.Interop;
using NameValueCollection = System.Collections.Specialized.NameValueCollection;
using NameValueCollectionHelper = Cosmos.Debug.Common.NameValueCollectionHelper;
using Cosmos.Build.Common;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace Cosmos.VS.Package {

  public class VsProjectConfig : ProjectConfig {
    public VsProjectConfig(ProjectNode project, string configuration)
      : base(project, configuration) {
    }

    public override int DebugLaunch(uint aLaunch) {
      try {
        // Second param is ResetCache. Must be called one time. Dunno why.
        // Also dunno if this comment is still valid/needed:
        // On first call, reset the cache, following calls will use the cached values
        // Think we will change this to a dummy program when we get our debugger working
        // This is the program that gest launched after build
        string xBuildTarget = GetConfigurationProperty("BuildTarget", true).ToUpper();
        //
        var xEnumValues = (BuildTarget[])Enum.GetValues(typeof(BuildTarget));
        var xTarget = xEnumValues.Where(q => q.ToString().ToUpper() == xBuildTarget).First();

        string xOutputAsm = ProjectMgr.GetOutputAssembly(ConfigName);
        string xOutputPath = Path.GetDirectoryName(xOutputAsm);
        string xIsoPathname = Path.ChangeExtension(xOutputAsm, ".iso");

        if (xTarget == BuildTarget.ISO) {
          Process.Start(xOutputPath);
        } else if (xTarget == BuildTarget.USB) {
          var xDrives = DriveInfo.GetDrives().Where(q => q.DriveType == DriveType.Removable).ToArray();
          string xDrive = "G";
          UsbMaker.Generate(xDrive, @"d:\source\Cosmos\source2\Demos\Guess\bin\Debug\Guess.obj");
          Process.Start(xDrive + @":\");
        } else {
          // http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.shell.interop.vsdebugtargetinfo_members.aspx
          var xInfo = new VsDebugTargetInfo();
          xInfo.cbSize = (uint)Marshal.SizeOf(xInfo);

          xInfo.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
          xInfo.fSendStdoutToOutputWindow = 0; // App keeps its stdout
          xInfo.grfLaunch = aLaunch; // Just pass through for now.
          xInfo.bstrRemoteMachine = null; // debug locally

          var xValues = new NameValueCollection();
          xValues.Add("ISOFile", xIsoPathname);
          xValues.Add("BinFormat", GetConfigurationProperty("BinFormat", false));
          xValues.Add("EnableGDB", GetConfigurationProperty("EnableGDB", false));
          xValues.Add("DebugMode", GetConfigurationProperty("DebugMode", false));
          xValues.Add("TraceAssemblies", GetConfigurationProperty("TraceAssemblies", false));
          xValues.Add("BuildTarget", GetConfigurationProperty("BuildTarget", false));
          xValues.Add("ProjectFile", Path.Combine(ProjectMgr.ProjectFolder, ProjectMgr.ProjectFile));
          xValues.Add("VMwareEdition", GetConfigurationProperty("VMwareEdition", false));
          xValues.Add("VMwareDeploy", GetConfigurationProperty("VMwareDeploy", false));
          xValues.Add("StartCosmosGDB", GetConfigurationProperty("StartCosmosGDB", false));

          xInfo.bstrExe = NameValueCollectionHelper.DumpToString(xValues);

          // Select the debugger
          xInfo.clsidCustom = new Guid("{FA1DA3A6-66FF-4c65-B077-E65F7164EF83}");
          xInfo.clsidPortSupplier = new Guid("{708C1ECA-FF48-11D2-904F-00C04FA302A1}");

          VsShellUtilities.LaunchDebugger(ProjectMgr.Site, xInfo);
        }
      } catch (Exception ex) {
        return Marshal.GetHRForException(ex);
      }
      return VSConstants.S_OK;
    }
  }
}