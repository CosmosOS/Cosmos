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

    private string _harddiskFile;

    private string _isoFile;

    private string _debugPortString;

    private string _projectName;

    private string _launchExe;
    public bool RedirectOutput = false;

    public Action<string> LogOutput;

    public Action<string> LogError;
    public Qemu(Dictionary<string, string> aParams, bool aUseGDB, string aHarddisk = null)
      : base(aParams, aUseGDB)
    {
      if (String.IsNullOrWhiteSpace(aHarddisk))
      {
        _harddiskFile = Path.Combine(CosmosPaths.Build, @"VMware\Workstation\Filesystem.vmdk");
      }
      else
      {
        _harddiskFile = aHarddisk;
      }
      //This will be removed once Qemu is completly working!
      
      string Output = String.Empty;
      foreach (KeyValuePair<string, string> Pair in aParams)
      {
        Output += $" {Pair.Key} : {Pair.Value} \n";
      }
      using StreamWriter file = new(Path.Combine(CosmosPaths.Build, @"aParamsOut.txt"));
      file.WriteLine(Output);
      
      if (aParams.ContainsKey("ISOFile"))
      {
        _isoFile = aParams["ISOFile"];
        string _getFileName = Path.GetFileName(_isoFile);
        _projectName = _getFileName.Replace(".iso", "");
      }
      else
      {
        _projectName = String.Empty;
      }
      if (aParams.ContainsKey("QemuLocationParameters"))
      {
        if (File.Exists(aParams["QemuLocationParameters"]))
        {
          _launchExe = aParams["QemuLocationParameters"];
        }
      }
      else if(!Directory.Exists(Path.GetDirectoryName(_launchExe)))
      {
        throw new Exception($"Path {Path.GetDirectoryName(_launchExe)} does not exist at the specified Location!");
      }
      else
      {
        if (!Directory.Exists(Path.GetDirectoryName(QemuSupport.QemuExe.FullName)))
        {
          throw new Exception($"Path {Path.GetDirectoryName(QemuSupport.QemuExe.FullName)} does not Exist!");
        }
        else
        {
          _launchExe = QemuSupport.QemuExe.FullName;
        }
      }
      _debugPortString = @"Cosmos\Serial";
    }

    public override void Start()
    {
      qemuProcess = new Process();
      var qemuStartInfo = qemuProcess.StartInfo;

      //QemuSupport.QemuExe.FullName;
        qemuStartInfo.FileName = _launchExe;

      string xQemuArguments = "-m 512";
      xQemuArguments += $" -cdrom {_isoFile}";

      if (!string.IsNullOrWhiteSpace(_harddiskFile))
      {
        xQemuArguments += $" -hda \"{_harddiskFile}\"";
      }

      if (!string.IsNullOrWhiteSpace(_debugPortString))
      {
        xQemuArguments += @" -chardev pipe,path="+_debugPortString+",id=Cosmos -device isa-serial,chardev=Cosmos";
      }

      xQemuArguments += " -name \"Cosmos Project: " + _projectName + "\"  -device pcnet,netdev=n1 -netdev user,id=n1 -vga std -boot d -no-shutdown -no-reboot";

      qemuStartInfo.Arguments = xQemuArguments;
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
      if (qemuProcess != null)
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
