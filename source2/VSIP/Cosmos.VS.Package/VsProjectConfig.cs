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

namespace Cosmos.VS.Package
{

    public class VsProjectConfig : ProjectConfig
    {
        public VsProjectConfig(ProjectNode project, string configuration) : base(project, configuration) {
            LogUtility.LogString("Entering Cosmos.VS.Package.VsProjectConfig.ctor(project, '{0}')", configuration);
            LogUtility.LogString("Exiting Cosmos.VS.Package.VsProjectConfig.ctor(project, configuration)");
        }

        public override int DebugLaunch(uint aLaunch)
        {
            LogUtility.LogString("Entering Cosmos.VS.Package.VsProjectConfig.DebugLaunch({0})", aLaunch);
            try
            {
                if (String.Equals(this.GetConfigurationProperty("BuildTarget", true), "ISO", StringComparison.InvariantCultureIgnoreCase))
                {
                    return VSConstants.S_OK;
                }
                else
                {
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

                    var xValues = new NameValueCollection();
                    xValues.Add("ISOFile", Path.Combine(Path.GetDirectoryName(ProjectMgr.GetOutputAssembly(this.ConfigName)), Path.GetFileNameWithoutExtension(ProjectMgr.GetOutputAssembly(this.ConfigName)) + ".iso"));
                    xValues.Add("BinFormat", this.GetConfigurationProperty("BinFormat", false)); // only first needs to force cache reset (in upper if statement)
                    xValues.Add("EnableGDB", this.GetConfigurationProperty("EnableGDB", false));
					xValues.Add("DebugMode", this.GetConfigurationProperty("DebugMode", false));
					xValues.Add("TraceAssemblies", this.GetConfigurationProperty("TraceAssemblies", false));
					xValues.Add("BuildTarget", this.GetConfigurationProperty("BuildTarget", false));
                    xValues.Add("ProjectFile", Path.Combine(ProjectMgr.ProjectFolder, ProjectMgr.ProjectFile));
					xValues.Add("VMWareFlavor", this.GetConfigurationProperty("VMWareFlavor", false));
					xValues.Add("StartCosmosGDB", this.GetConfigurationProperty("StartCosmosGDB", false));

                    xInfo.bstrExe = NameValueCollectionHelper.DumpToString(xValues);

                    LogUtility.LogString("Parameters = '{0}'", xInfo.bstrExe);

                    // Select the debugger
                    xInfo.clsidCustom = new Guid("{FA1DA3A6-66FF-4c65-B077-E65F7164EF83}");
                    xInfo.clsidPortSupplier = new Guid("{708C1ECA-FF48-11D2-904F-00C04FA302A1}");

                    VsShellUtilities.LaunchDebugger(ProjectMgr.Site, xInfo);
                    LogUtility.LogString("Returning VSConstants.S_OK");
                    return VSConstants.S_OK;
                }
            }
            catch (Exception e)
            {
                LogUtility.LogString("Error: {0}", e.ToString());
                return Marshal.GetHRForException(e);
            }
            finally
            {
                LogUtility.LogString("Exiting Cosmos.VS.Package.VsProjectConfig.DebugLaunch");
            }
        }
    }
}