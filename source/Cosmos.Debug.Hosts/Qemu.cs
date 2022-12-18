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

    private string _debugACPIEnable;

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
      if (aParams.ContainsKey("QemuMemory"))
      {
        if (!String.IsNullOrWhiteSpace(aParams["QemuMemory"]))
        {
          _memoryAssign = aParams["QemuMemory"];
        }
        else
        {
          _memoryAssign = "512";
        }
      }
      else
      {
        _memoryAssign = "512";
      }
      if (aParams.ContainsKey("QemuUseCustomParameters"))
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
      if (aParams.ContainsKey("QemuHWAccel"))
      {
        bool Accel;
        Boolean.TryParse(aParams["QemuHWAccel"], out Accel);
        if (Accel)
        {
          _hardwareAccel = " -accel hax";
        }
      }
      if (aParams.ContainsKey("QemuUSBKeyboard"))
      {
        bool Keyboard;
        Boolean.TryParse(aParams["QemuUSBKeyboard"], out Keyboard);
        if (Keyboard)
        {
          _useUSBKeyboard = " -usbdevice keyboard";
        }
      }
      if (aParams.ContainsKey("QemuUSBMouse"))
      {
        bool Mouse;
        Boolean.TryParse(aParams["QemuUSBMouse"], out Mouse);
        if (Mouse)
        {
          _useUSBMouse = " -usbdevice mouse";
        }
      }
      if (aParams.ContainsKey("QemuUseSerial"))
      {
        bool Serial;
        Boolean.TryParse(aParams["QemuUseSerial"], out Serial);
        if (Serial)
        {
          _useSerialOutput = " -serial file:CON";
        }
      }
      if (aParams.ContainsKey("QemuNetworkDevice"))
      {
        if (!String.IsNullOrWhiteSpace(aParams["QemuNetworkDevice"]))
        {
          _networkDevice = aParams["QemuNetworkDevice"].ToLower();
        }
        else
        {
          _networkDevice = "e1000";
        }
      }
      if (aParams.ContainsKey("QemuAudioDriver"))
      {
        if (aParams["QemuAudioDriver"] == "SoundBlaster16")
        {
          _audioDriver = " -device sb16";
        }
        if (aParams["QemuAudioDriver"] == "AC97")
        {
          _audioDriver = " -device ac97";
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
          _videoDriver = " -vga std";
        }
        else if (aParams["QemuVideoDriver"] == "VMWare")
        {
          _videoDriver = " -vga vmware";
        }
        else if (aParams["QemuVideoDriver"] == "Cirrus")
        {
          _videoDriver = " -vga cirrus";
        }
        else if (aParams["QemuVideoDriver"] == "Bochs")
        {
          _videoDriver = " -vga none -device bochs-display";
        }
      }
      if(aParams.ContainsKey("DebugEnabled"))
      {
        bool EnableAcpi;
        Boolean.TryParse(aParams["DebugEnabled"], out EnableAcpi);
          if(EnableAcpi)
          {
            _debugACPIEnable = " -no-acpi";
          }
          else
          {
            _debugACPIEnable = String.Empty;
          }
      }
      if(aParams.ContainsKey("QemuHWAccelWHPX"))
      {
        bool WHPX;
        Boolean.TryParse(aParams["QemuHWAccelWHPX"],out WHPX);
        if(WHPX)
        {
          _hardwareAccel = " -accel whpx";
        }
      }
      if (aParams.ContainsKey("QemuUseCustomLocation"))
      {
        bool UseCustomExe;
        Boolean.TryParse(aParams["QemuUseCustomLocation"], out UseCustomExe);

        if (UseCustomExe)
        {
          _launchExe = aParams["QemuLocationParameters"];
        }
        else
        {
          SetDefaultPath();
        }
      }
      else
      {
        SetDefaultPath();
      }

      _debugPortString = @"Cosmos\Serial";
    }

    private void SetDefaultPath()
    {
      string defaultPath = @"C:\qemu\qemu-system-i386.exe";

      if (!File.Exists(defaultPath))
      {
        defaultPath = @"C:\Program Files\qemu\qemu-system-i386.exe";

        if (!File.Exists(defaultPath))
        {
          throw new Exception("Checked paths C:\\Program Files\\qemu and C:\\qemu and could not find QEMU.");
        }
      }

      _launchExe = defaultPath;
    }

    public override void Start()
    {
      qemuProcess = new Process();
      var qemuStartInfo = qemuProcess.StartInfo;
      qemuStartInfo.FileName = _launchExe;

      StringBuilder xQemuArguments = new StringBuilder();
      xQemuArguments.Append("-m ");
      xQemuArguments.Append(_memoryAssign);
      xQemuArguments.Append(" -cdrom \"");
      xQemuArguments.Append(_isoFile);
      xQemuArguments.Append("\" -name \"Cosmos Project: ");
      xQemuArguments.Append(_projectName);
      xQemuArguments.Append('"');
      xQemuArguments.Append(_videoDriver);
      xQemuArguments.Append(_audioDriver);
      xQemuArguments.Append(" -boot d");
      xQemuArguments.Append(_debugACPIEnable);
      xQemuArguments.Append(_useSerialOutput);
      xQemuArguments.Append(_useUSBKeyboard);
      xQemuArguments.Append(_useUSBMouse);
      xQemuArguments.Append(_hardwareAccel);
      xQemuArguments.Append(' ');
      xQemuArguments.Append(_customArgs);

      if (!string.IsNullOrWhiteSpace(_harddiskFile))
      {
        xQemuArguments.Append(" -hda \"");
        xQemuArguments.Append(_harddiskFile);
        xQemuArguments.Append('"');
      }

      if (!string.IsNullOrWhiteSpace(_debugPortString))
      {
        xQemuArguments.Append(" -chardev pipe,path=");
        xQemuArguments.Append(_debugPortString);
        xQemuArguments.Append(",id=Cosmos -device isa-serial,chardev=Cosmos");
      }

      if (!string.IsNullOrWhiteSpace(_networkDevice))
      {
        xQemuArguments.Append(" -device ");
        xQemuArguments.Append(_networkDevice);
        xQemuArguments.Append(",netdev=n1 -netdev user,id=n1");
      }

      qemuStartInfo.Arguments = xQemuArguments.ToString();
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
    }
  }
}
