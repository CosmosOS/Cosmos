using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Cosmos.Build.Common;

namespace Cosmos.Debug.Hosts
{
  public class Qemu : Host
  {
    private static Process qemuProcess;

    private string harddiskFile;

    public bool RedirectOutput = false;

    public Action<string> LogOutput;

    public Action<string> LogError;

    public Qemu(Dictionary<string, string> aParams, bool aUseGDB, string aHarddisk = null)
      : base(aParams, aUseGDB)
    {
      if (String.IsNullOrWhiteSpace(aHarddisk))
      {
        harddiskFile = Path.Combine(CosmosPaths.Build, @"VMWare\Workstation\Filesystem.vmdk");
      }
      else
      {
        harddiskFile = aHarddisk;
      }
    }

    public override void Start()
    {
      qemuProcess = new Process();
      ProcessStartInfo qemuStartInfo = qemuProcess.StartInfo;
      qemuStartInfo.FileName = QemuSupport.QemuExe.FullName;

      string biosPath = ".";
      string isoPath = "";
      int memorySize = 32;
      qemuStartInfo.Arguments = $"-L {biosPath} -cdrom {isoPath} -m {memorySize}";
      qemuStartInfo.CreateNoWindow = true;
      if (RedirectOutput)
      {
        if (LogOutput == null)
        {
          throw new Exception("No LogOutput handler specified!");
        }

        if (LogError == null)
        {
          throw new Exception("No LogError handler specified!");
        }
      }

      qemuProcess.EnableRaisingEvents = true;
      qemuProcess.Exited += ExitCallback;
      qemuProcess.Start();
      if (RedirectOutput)
      {
        qemuProcess.BeginErrorReadLine();
        qemuProcess.BeginOutputReadLine();
      }
    }

    private void ExitCallback(object sender, EventArgs e)
    {
      if (OnShutDown != null)
      {
        try
        {
          OnShutDown(sender, e);
        }
        catch
        {
        }
      }
    }

    public override void Stop()
    {
      if (null != qemuProcess)
      {
        try
        {
          qemuProcess.Kill();
        }
        catch
        {
        }
      }

      Cleanup();
    }

    private void Cleanup()
    {
      OnShutDown(this, null);
      qemuProcess.Exited -= ExitCallback;
    }
  }
}
