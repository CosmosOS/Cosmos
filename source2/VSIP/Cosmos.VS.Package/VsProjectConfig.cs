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
using Cosmos.Debug.VSDebugEngine;

namespace Cosmos.VS.Package {

  public class VsProjectConfig : ProjectConfig {

    public VsProjectConfig(ProjectNode project, string configuration) : base(project, configuration) { }

    public override int DebugLaunch(uint aLaunch) {
      try {
        // http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.shell.interop.vsdebugtargetinfo_members.aspx
        var xInfo = new VsDebugTargetInfo();
        xInfo.cbSize = (uint)Marshal.SizeOf(xInfo);

        xInfo.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
        xInfo.fSendStdoutToOutputWindow = 0; // App keeps its stdout
        xInfo.grfLaunch = aLaunch; // Just pass through for now.
        xInfo.bstrRemoteMachine = null; // debug locally

        // On first call, reset the cache, following calls will use the cached values
        // Think we will change this to a dummy program when we get our debugger working
        // This is the program that gest launched after build

        xInfo.bstrExe = ProjectMgr.GetOutputAssembly(this.ConfigName);
        xInfo.bstrExe = Path.Combine(Path.GetDirectoryName(xInfo.bstrExe), Path.GetFileNameWithoutExtension(xInfo.bstrExe) + ".iso");

        // Select the debugger
        // Managed debugger
        //xInfo.clsidCustom = VSConstants.CLSID_ComPlusOnlyDebugEngine;
        // Our debugger - a work in progress
        xInfo.clsidCustom = new Guid(AD7Engine.ID);
        xInfo.clsidPortSupplier = new Guid("{708C1ECA-FF48-11D2-904F-00C04FA302A1}");
        // Sample Debug Engine
        //xInfo.clsidCustom = new Guid("{D951924A-4999-42a0-9217-1EB5233D1D5A}"); 

        VsShellUtilities.LaunchDebugger(ProjectMgr.Site, xInfo);
        return VSConstants.S_OK;
      } catch (Exception e) {
        return Marshal.GetHRForException(e);
      }
    }

  }

}
