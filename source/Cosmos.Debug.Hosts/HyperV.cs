using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
#if false
using System.Management.Automation;
using System.Management.Automation.Runspaces;
#endif
using System.Security.Principal;

using Cosmos.Build.Common;

namespace Cosmos.Debug.Hosts
{
    public class HyperV : Host
    {
        protected string mHarddiskPath;
        protected Process mProcess;
        
#if false
        private static bool IsProcessAdministrator => (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
#else   
        private static bool IsProcessAdministrator => true;
#endif
        
        public HyperV(Dictionary<string, string> aParams, bool aUseGDB, string harddisk = "Filesystem.vhdx") : base(aParams, aUseGDB)
        {
            if (!IsProcessAdministrator)
            {
                throw new Exception("Visual Studio must be run as administrator for Hyper-V to work");
            }

            mHarddiskPath = Path.Combine(CosmosPaths.Build, harddisk);
        }
        
        public override void Start()
        {
            CreateVirtualMachine();

            // Target exe or file
            var info = new ProcessStartInfo(@"C:\Windows\sysnative\VmConnect.exe", @"""localhost"" ""Cosmos""")
            {
                UseShellExecute = false
            };

            mProcess = new Process();
            mProcess.StartInfo = info;
            mProcess.EnableRaisingEvents = true;
            mProcess.Exited += (Object aSender, EventArgs e) =>
            {
                OnShutDown?.Invoke(aSender, e);
            };
            mProcess.Start();

            RunPowershellScript("Start-VM -Name Cosmos");
        }
        
        public override void Stop()
        {
            RunPowershellScript("Stop-VM -Name Cosmos -TurnOff -ErrorAction Ignore");
            mProcess.Kill();
        }
        
        protected void CreateVirtualMachine()
        {
            RunPowershellScript("Stop-VM -Name Cosmos -TurnOff -ErrorAction Ignore");

            RunPowershellScript("Remove-VM -Name Cosmos -Force -ErrorAction Ignore");
            RunPowershellScript("New-VM -Name Cosmos -MemoryStartupBytes 268435456 -BootDevice CD");
            if (!File.Exists(mHarddiskPath))
            {
                RunPowershellScript($@"New-VHD -SizeBytes 268435456 -Dynamic -Path ""{mHarddiskPath}""");
            }

            RunPowershellScript($@"Add-VMHardDiskDrive -VMName Cosmos -ControllerNumber 0 -ControllerLocation 0 -Path ""{mHarddiskPath}""");
            RunPowershellScript($@"Set-VMDvdDrive -VMName Cosmos -ControllerNumber 1 -ControllerLocation 0 -Path ""{mParams["ISOFile"]}""");
            RunPowershellScript(@"Set-VMComPort -VMName Cosmos -Path \\.\pipe\CosmosSerial -Number 1");
        }
        
        private static void RunPowershellScript(string text)
        {
            // Workaround
            ProcessStartInfo xStartInfo = new ProcessStartInfo("powershell");
            xStartInfo.Arguments = text;

            var xProcess = Process.Start(xStartInfo);
            xProcess.WaitForExit();
        
#if false
            using (Runspace runspace = RunspaceFactory.CreateRunspace())
            {
                runspace.Open();

                Pipeline pipeline = runspace.CreatePipeline();

                pipeline.Commands.AddScript(text);
                pipeline.Commands.Add("Out-String");

                Collection<PSObject> results = pipeline.Invoke();
                foreach (PSObject obj in results)
                {
                    System.Diagnostics.Debug.WriteLine(obj.ToString());
                }
            }
#endif
        }
    }
}
