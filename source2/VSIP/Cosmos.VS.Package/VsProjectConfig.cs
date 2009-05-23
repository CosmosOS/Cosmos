using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.Package {

  public class VsProjectConfig : ProjectConfig {

    public VsProjectConfig(ProjectNode project, string configuration) : base(project, configuration) { }

    public override int DebugLaunch(uint aLaunch) {
      try {
        // http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.shell.interop.vsdebugtargetinfo_members.aspx
        var xInfo = new VsDebugTargetInfo();
        xInfo.cbSize = (uint)Marshal.SizeOf(xInfo);
        xInfo.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;

        // On first call, reset the cache, following calls will use the cached values
        // Think we will change this to a dummy program when we get our debugger working
        // This is the program that gest launched after build
        string xProp = GetConfigurationProperty("StartProgram", true);
        if (string.IsNullOrEmpty(xProp)) {
          xInfo.bstrExe = ProjectMgr.GetOutputAssembly(this.ConfigName);
        } else {
          xInfo.bstrExe = xProp;
        }

        xInfo.fSendStdoutToOutputWindow = 0;
        // Managed debugger
        xInfo.clsidCustom = VSConstants.CLSID_ComPlusOnlyDebugEngine;
        // Just pass through for now.
        xInfo.grfLaunch = aLaunch;

        VsShellUtilities.LaunchDebugger(ProjectMgr.Site, xInfo);
        return VSConstants.S_OK;
      } catch (Exception e) {
        return Marshal.GetHRForException(e);
      }
    }

  }

}
