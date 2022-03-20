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

    private string _networkDevice;

    private string _videoDriver;

    private string _audioDriver;

    private string _customArgs;

    private string _memoryAssign;

    private string _hardwareAccel;

    private string _useUSBMouse;

    private string _useUSBKeyboard;

    private string _useSerialOutput;

    public bool RedirectOutput = false;

    public Action<string> LogOutput;
    string Output = String.Empty;
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
      

      foreach (KeyValuePair<string, string> Pair in aParams)
      {
        Output += $" {Pair.Key} : {Pair.Value} \n";
      }

      
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
      if(aParams.ContainsKey("QemuMemory"))
      {
        _memoryAssign = aParams["QemuMemory"];
      }
      else
      {
        _memoryAssign = "512";
      }
      if(aParams.ContainsKey("QemuUseCustomParameters"))
      {

          bool Check;
          Boolean.TryParse(aParams["QemuUseCustomParameters"], out Check);
          if (Check)
          {
            _customArgs = " " + aParams["QemuCustomParameters"] + " ";
          }
          else
          {
            _customArgs = String.Empty;
          }
        
      }
      if(aParams.ContainsKey("QemuHWAccel"))
      {
        bool Accel;
        Boolean.TryParse(aParams["QemuHWAccel"], out Accel);
        if (Accel)
        {
          _hardwareAccel = "-accel hax";
        }
      }
      if (aParams.ContainsKey("QemuUSBKeyboard"))
      {
        bool Keyboard;
        Boolean.TryParse(aParams["QemuUSBKeyboard"], out Keyboard);
        if (Keyboard)
        {
          _useUSBKeyboard = "-usbdevice keyboard";
        }
      }
      if (aParams.ContainsKey("QemuUSBMouse"))
      {
        bool Mouse;
        Boolean.TryParse(aParams["QemuUSBMouse"], out Mouse);
        if (Mouse)
        {
          _useUSBMouse = "-usbdevice mouse";
        }
      }
      if (aParams.ContainsKey("QemuUseSerial"))
      {
        bool Serial;
        Boolean.TryParse(aParams["QemuUseSerial"], out Serial);
        if (Serial)
        {
          _useSerialOutput = "-serial stdio";
        }
      }
      if (aParams.ContainsKey("QemuNetworkDevice"))
      {
        _networkDevice = aParams["QemuNetworkDevice"].ToLower();
      }
      if (aParams.ContainsKey("QemuAudioDriver"))
      {
        if (aParams["QemuAudioDriver"] == "SoundBlaster16")
        {
          _audioDriver = "-soundhw sb16";
        }
        if (aParams["QemuAudioDriver"] == "AC97")
        {
          _audioDriver = "-soundhw ac97";
        }

      }
          if (aParams.ContainsKey("QemuVideoDriver"))
      {
        if (aParams["QemuVideoDriver"] == "VGA")
        {
          _videoDriver = String.Empty;
        }
        else if (aParams["QemuVideoDriver"] == "VBE")
        {
          _videoDriver = "-vga std";
        }
        else if (aParams["QemuVideoDriver"] == "VMWare")
        {
          _videoDriver = "-vga vmware";
        }
        else if (aParams["QemuVideoDriver"] == "Cirrus")
        {
          _videoDriver = "-vga cirrus";
        }
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

        qemuStartInfo.FileName = _launchExe;

      string xQemuArguments = "-m " + _memoryAssign;
      xQemuArguments += $" -cdrom {_isoFile}";

      if (!string.IsNullOrWhiteSpace(_harddiskFile))
      {
        xQemuArguments += $" -hda \"{_harddiskFile}\"";
      }

      if (!string.IsNullOrWhiteSpace(_debugPortString))
      {
        xQemuArguments += @" -chardev pipe,path="+_debugPortString+",id=Cosmos -device isa-serial,chardev=Cosmos";
      }

      xQemuArguments += " -name \"Cosmos Project: " + _projectName + "\"  -device "+_networkDevice+",netdev=n1 -netdev user,id=n1 "+_videoDriver+" "+_audioDriver+ " -boot d -no-shutdown -no-reboot " + _customArgs + " " + _useUSBKeyboard + " " + _useUSBMouse + " " + _hardwareAccel;
      Output += _launchExe + " ";
      Output += xQemuArguments;
      using StreamWriter file = new(Path.Combine(CosmosPaths.Build, @"aParamsOut.txt"));
      file.WriteLine(Output);
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
