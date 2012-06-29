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
        string xProfile = GetConfigurationProperty(BuildProperties.ProfileString, true);
        var xTarget = (Profile)Enum.Parse(typeof(Profile), xProfile);

        string xOutputAsm = ProjectMgr.GetOutputAssembly(ConfigName);
        string xOutputPath = Path.GetDirectoryName(xOutputAsm);
        string xIsoFile = Path.ChangeExtension(xOutputAsm, ".iso");
        string xBinFile = Path.ChangeExtension(xOutputAsm, ".bin");

        if (xTarget == Profile.ISO) {
          IsoMaker.Generate(CosmosPaths.Build, xBinFile, xIsoFile);
          Process.Start(xOutputPath);

        } else if (xTarget == Profile.USB) {
          Process.Start(Path.Combine(CosmosPaths.Tools, "Cosmos.Deploy.USB.exe"), "\"" + xBinFile + "\"");

        } else if (xTarget == Profile.PXE) {
          string xPxePath = Path.Combine(CosmosPaths.Build, "PXE");
          File.Copy(xBinFile, Path.Combine(xPxePath, "Cosmos.bin"), true);
          Process.Start(Path.Combine(CosmosPaths.Tools, "Cosmos.Deploy.Pixie.GUI.exe"), "192.168.42.1 \"" + xPxePath + "\"");

        } else if (xTarget == Profile.VMware) {
          // TODO - only make ISO if not PXE
          IsoMaker.Generate(CosmosPaths.Build, xBinFile, xIsoFile);

          // http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.shell.interop.vsdebugtargetinfo_members.aspx
          var xInfo = new VsDebugTargetInfo();
          xInfo.cbSize = (uint)Marshal.SizeOf(xInfo);

          xInfo.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
          xInfo.fSendStdoutToOutputWindow = 0; // App keeps its stdout
          xInfo.grfLaunch = aLaunch; // Just pass through for now.
          xInfo.bstrRemoteMachine = null; // debug locally

          var xValues = new NameValueCollection();
          xValues.Add("ISOFile", xIsoFile);
          xValues.Add("BinFormat", GetConfigurationProperty("BinFormat", false));
          xValues.Add(BuildProperties.EnableGDBString, GetConfigurationProperty(BuildProperties.EnableGDBString, false));
          xValues.Add(BuildProperties.DebugModeString, GetConfigurationProperty(BuildProperties.DebugModeString, false));
          xValues.Add(BuildProperties.TraceAssembliesString, GetConfigurationProperty(BuildProperties.TraceAssembliesString, false));
          xValues.Add(BuildProperties.ProfileString, GetConfigurationProperty(BuildProperties.ProfileString, false));
          xValues.Add("ProjectFile", Path.Combine(ProjectMgr.ProjectFolder, ProjectMgr.ProjectFile));
          xValues.Add(BuildProperties.VMwareEditionString, GetConfigurationProperty(BuildProperties.VMwareEditionString, false));
          xValues.Add(BuildProperties.VMwareEditionString, GetConfigurationProperty(BuildProperties.VMwareEditionString, false));
          xValues.Add(BuildProperties.StartCosmosGDBString, GetConfigurationProperty(BuildProperties.StartCosmosGDBString, false));

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