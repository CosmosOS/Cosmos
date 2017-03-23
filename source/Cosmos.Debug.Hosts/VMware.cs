using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32;

//using Vestris.VMWareLib;
using Cosmos.Build.Common;

namespace Cosmos.Debug.Hosts {
  public class VMware : Host {
    protected VMwareEdition mEdition;
    protected string mDir;
    protected string mVmxPath;
    protected Process mProcess;
    protected string mWorkstationPath;
    protected string mPlayerPath;
    protected string mHarddisk;

    public VMware(Dictionary<string, string> aParams, bool aUseGDB,string harddisk = "Filesystem.vmdk") : base(aParams, aUseGDB) {
      mHarddisk = harddisk;
      mDir = Path.Combine(CosmosPaths.Build, @"VMWare\Workstation\");
      mVmxPath = Path.Combine(mDir, @"Debug.vmx");

      mWorkstationPath = GetPathname("VMware Workstation", "vmware.exe");
      mPlayerPath = GetPathname("VMware Player", "vmplayer.exe");
      if (mWorkstationPath == null && mPlayerPath == null) {
        throw new Exception("VMware not found.");
      }

      string xFlavor = aParams[BuildPropertyNames.VMwareEditionString].ToUpper();
      mEdition = VMwareEdition.Player;
      if (xFlavor == "WORKSTATION") {
        mEdition = VMwareEdition.Workstation;
      }

      // Try alternate if selected one is not installed
      if (mEdition == VMwareEdition.Player && mPlayerPath == null && mWorkstationPath != null) {
        mEdition = VMwareEdition.Workstation;
      } else if (mEdition == VMwareEdition.Workstation && mWorkstationPath == null) {
        mEdition = VMwareEdition.Player;
      }
    }

    //protected void ConnectToVMWare(VMWareVirtualHost aHost) {
    //  if (mEdition != VMwareEdition.Player) {
    //    aHost.ConnectToVMWareWorkstation();
    //  } else {
    //    aHost.ConnectToVMWarePlayer();
    //  }
    //}

    protected string GetPathname(string aKey, string aEXE) {
      using (var xRegKey = Registry.LocalMachine.OpenSubKey(@"Software\VMware, Inc.\" + aKey, false)) {
        if (xRegKey != null) {
          string xResult = Path.Combine(((string)xRegKey.GetValue("InstallPath")), aEXE);
          if (File.Exists(xResult)) {
            return xResult;
          }
        }
        return null;
      }
    }

    public override void Start() {
      Cleanup();
      CreateDebugVmx();

      // Target exe or file
      mProcess = new Process();
      var xPSI = mProcess.StartInfo;
      if (mEdition == VMwareEdition.Player) {
        xPSI.FileName = mPlayerPath;
      } else {
        xPSI.FileName = mWorkstationPath;
      }
      var xArgSB = new StringBuilder();

      string xVmxPath = "\"" + mVmxPath + "\"";
      if (mEdition == VMwareEdition.Player) {
        xPSI.Arguments = xVmxPath;
      } else {
        // -x: Auto power on VM. Must be small x, big X means something else.
        // -q: Close VMWare when VM is powered off.
        // Options must come beore the vmx, and cannot use shellexecute
        xPSI.Arguments = "-x -q " + xVmxPath;
      }
      xPSI.UseShellExecute = false;  //must be true to allow elevate the process, sometimes needed if vmware only runs with admin rights
      mProcess.EnableRaisingEvents = true;
      mProcess.Exited += delegate(Object aSender, EventArgs e) {
        if (OnShutDown != null) {
          OnShutDown(aSender, e);
        }
      };
      mProcess.Start();
    }

    public override void Stop() {
      //using (var xHost = new VMWareVirtualHost()) {
      //  ConnectToVMWare(xHost);
      //  using (var xMachine = xHost.Open(mVmxPath)) {
      //    xMachine.PowerOff();
      //  }
      //  xHost.Close();
      //}
      Cleanup();
    }

    protected void DeleteFiles(string aPath, string aPattern) {
      var xFiles = Directory.GetFiles(aPath, aPattern);
      foreach (var xFile in xFiles) {
        File.Delete(xFile);
      }
    }

    protected void Cleanup() {
      try {
        string xPath = Path.GetDirectoryName(mVmxPath);
        // Delete old Debug.vmx and other files that might be left over from previous run.
        // Especially important with newer versions of VMWare player which defaults to suspend
        // when the close button is used.
        File.Delete(mVmxPath);
        File.Delete(Path.ChangeExtension(mVmxPath, ".nvram"));
        // Delete the auto snapshots that latest vmware players create as default.
        // It creates them with suffixes though, so we need to wild card find them.
        DeleteFiles(xPath, "*.vmxf");
        DeleteFiles(xPath, "*.vmss");
        DeleteFiles(xPath, "*.vmsd");
        DeleteFiles(xPath, "*.vmem");
        // Delete log files so that logged data is only from last boot
        File.Delete(Path.Combine(xPath, "vmware.log"));
        File.Delete(Path.Combine(xPath, "vmware-0.log"));
        File.Delete(Path.Combine(xPath, "vmware-1.log"));
        File.Delete(Path.Combine(xPath, "vmware-2.log"));
      } catch (Exception) {
        // Ignore errors, users can stop VS while VMware is still running and files will be locked.
      }
    }

    protected void CreateDebugVmx() {
      // VMWare doesn't like to boot a read only VMX.
      // We also need to make changes based on project / debug settings.
      // Finally we do not want to create VCS checkins based on local user changes.
      // Because of this we use Cosmos.vmx as a template and output a Debug.vmx on every run.
      using (var xSrc = new StreamReader(File.Open(Path.Combine(mDir, "Cosmos.vmx"), FileMode.OpenOrCreate))) {
        try {
          // Write out Debug.vmx
          using (var xDest = new StreamWriter(File.Open(mVmxPath, FileMode.Create))) {
            string xLine;
            while ((xLine = xSrc.ReadLine()) != null) {
              var xParts = xLine.Split('=');
              if (xParts.Length == 2) {
                string xName = xParts[0].Trim();
                string xValue = xParts[1].Trim();

                if ((xName == "uuid.location") || (xName == "uuid.bios")) {
                  // We delete uuid entries so VMWare doesnt ask the user "Did you move or copy" the file
                  xValue = null;

                } else if (xName == "ide1:0.fileName")
                {
                  // Set the ISO file for booting
                  xValue = "\"" + mParams["ISOFile"] + "\"";
                } else if (xName == "ide0:0.fileName") {
                  xValue = "\"" + mHarddisk + "\"";
                } else if (xName == "nvram") {
                  // Point it to an initially non-existent nvram.
                  // This has the effect of disabling PXE so the boot is faster.
                  xValue = "\"Debug.nvram\"";
                }

                if (xValue != null) {
                  xDest.WriteLine(xName + " = " + xValue);
                }
              }
            }

            if (mUseGDB) {
              xDest.WriteLine();
              xDest.WriteLine("debugStub.listen.guest32 = \"TRUE\"");
              xDest.WriteLine("debugStub.hideBreakpoints = \"TRUE\"");
              xDest.WriteLine("monitor.debugOnStartGuest32 = \"TRUE\"");
              xDest.WriteLine("debugStub.listen.guest32.remote = \"TRUE\"");
            }
          }
        } catch (IOException ex) {
          if (ex.Message.Contains(Path.GetFileName(mDir))) {
            throw new Exception("The VMware image " + mDir + " is still in use. Please exit current Vmware session with Cosmos and try again.", ex);
          }
          throw;
        }
      }
    }
  }
}
