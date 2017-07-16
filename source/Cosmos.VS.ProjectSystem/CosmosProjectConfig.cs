using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Cosmos.Build.Common;
using Cosmos.Debug.Common;

namespace Cosmos.VS.ProjectSystem
{
    public class CosmosProjectConfig : ProjectConfig
    {
        public CosmosProjectConfig(ProjectNode project, string configuration)
            : base(project, configuration)
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());
        }

        public override int DebugLaunch(uint aLaunch)
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());

            try
            {
                var xDeployment = (DeploymentType)Enum.Parse(typeof(DeploymentType), GetConfigurationProperty(BuildPropertyNames.DeploymentString, true));
                var xLaunch = (LaunchType)Enum.Parse(typeof(LaunchType), GetConfigurationProperty(BuildPropertyNames.LaunchString, false));
                var xVSDebugPort = GetConfigurationProperty(BuildPropertyNames.VisualStudioDebugPortString, false);

                string xOutputAsm = ProjectMgr.GetOutputAssembly(ConfigName);
                string xOutputPath = Path.GetDirectoryName(xOutputAsm);
                string xIsoFile = Path.ChangeExtension(xOutputAsm, ".iso");
                string xBinFile = Path.ChangeExtension(xOutputAsm, ".bin");

                if (xDeployment == DeploymentType.ISO)
                {
                    IsoMaker.Generate(xBinFile, xIsoFile);

                }
                else if (xDeployment == DeploymentType.USB)
                {
                    Process.Start(Path.Combine(CosmosPaths.Tools, "Cosmos.Deploy.USB.exe"), "\"" + xBinFile + "\"");

                }
                else if (xDeployment == DeploymentType.PXE)
                {
                    string xPxePath = Path.Combine(CosmosPaths.Build, "PXE");
                    string xPxeIntf = GetConfigurationProperty(BuildPropertyNames.PxeInterfaceString, false);
                    File.Copy(xBinFile, Path.Combine(xPxePath, "Cosmos.bin"), true);
                    Process.Start(Path.Combine(CosmosPaths.Tools, "Cosmos.Deploy.Pixie.exe"), xPxeIntf + " \"" + xPxePath + "\"");
                }
                else if (xDeployment == DeploymentType.BinaryImage)
                {
                    // prepare?
                }
                else
                {
                    throw new Exception("Unknown deployment type.");
                }

                if (xLaunch == LaunchType.None
                    && xDeployment == DeploymentType.ISO)
                {
                    Process.Start(xOutputPath);
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

                    var xValues = new Dictionary<string, string>();
                    xValues.Add("ProjectFile", Path.Combine(ProjectMgr.ProjectFolder, ProjectMgr.ProjectFile));
                    xValues.Add("ISOFile", xIsoFile);
                    //xValues.Add("BinFormat", GetConfigurationProperty("BinFormat", false));
                    foreach (var xName in BuildProperties.PropNames)
                    {
                        xValues.Add(xName, GetConfigurationProperty(xName, false));
                    }

                    xInfo.bstrExe = DictionaryHelper.DumpToString(xValues);

                    // Select the debugger
                    xInfo.clsidCustom = Cosmos.VS.DebugEngine.AD7.Impl.AD7Engine.EngineID; // Debug engine identifier.
                    // ??? This identifier doesn't seems to appear anywhere else in souce code.
                    //xInfo.clsidPortSupplier = new Guid("{708C1ECA-FF48-11D2-904F-00C04FA302A1}");

                    VsShellUtilities.LaunchDebugger(ProjectMgr.Site, xInfo);
                }
            }
            catch (Exception ex)
            {
                return Marshal.GetHRForException(ex);
            }
            return VSConstants.S_OK;
        }
    }
}