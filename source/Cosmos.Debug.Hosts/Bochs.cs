using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Cosmos.Build.Common;
using Cosmos.Debug.Common;

namespace Cosmos.Debug.Hosts
{
  /// <summary>This class handles interactions with the Bochs emulation environment.</summary>
  public partial class Bochs : Host
  {
    /// <summary>The emulator process once started.</summary>
    private static Process _bochsProcess;

    /// <summary>The configuration file to be used when launching the Bochs virtual machine.</summary>
    private FileInfo _bochsConfigurationFile;

    /// <summary>Instanciation occurs when debugging engine is invoked to launch the process in suspended
    /// mode. Bochs process will eventually be launched later when debugging engine is instructed to
    /// Attach to the debugged process.</summary>
    public Bochs(Dictionary<string, string> aParams, bool aUseGDB, FileInfo configurationFile, string harddisk = null)
        : base(aParams, aUseGDB)
    {
      if (null == configurationFile)
      {
        throw new ArgumentNullException("configurationFile");
      }

      bool parseSucceeded = bool.TryParse(aParams[BuildPropertyNames.EnableBochsDebugString], out _useDebugVersion);
      parseSucceeded = bool.TryParse(aParams[BuildPropertyNames.StartBochsDebugGui], out startDebugGui);

      if (String.IsNullOrWhiteSpace(harddisk))
      {
        mHarddiskFile = Path.Combine(CosmosPaths.Build, @"VMWare\Workstation\Filesystem.vmdk");
      }
      else
      {
        mHarddiskFile = harddisk;
      }

      InitializeKeyValues();
      GenerateConfiguration(configurationFile.FullName);
      _bochsConfigurationFile = configurationFile;
    }

    private bool _useDebugVersion;

    private bool startDebugGui;


    /// <summary>Fix the content of the configuration file, replacing each of the symbolic variable occurence
    /// with its associated value.</summary>
    /// <param name="symbols">A set of key/value pairs where the key is the name of a variable. The value is
    /// used for variable replacement. Variables are case sensistive.</param>
    internal void FixBochsConfiguration(KeyValuePair<string, string>[] symbols)
    {
      if ((null == symbols) || (0 == symbols.Length))
      {
        return;
      }
      string content;
      using (StreamReader reader = new StreamReader(File.Open(_bochsConfigurationFile.FullName, FileMode.Open, FileAccess.Read)))
      {
        content = reader.ReadToEnd();
      }
      foreach (KeyValuePair<string, string> pair in symbols)
      {
        string variableName = string.Format("$({0})", pair.Key);

        content.Replace(variableName, pair.Value);
      }
      using (StreamWriter writer = new StreamWriter(File.Open(_bochsConfigurationFile.FullName, FileMode.Create, FileAccess.Write)))
      {
        writer.Write(content);
      }
    }

    public bool RedirectOutput = false;

    public Action<string> LogOutput;

    public Action<string> LogError;

    /// <summary>Initialize and start the Bochs process.</summary>
    public override void Start()
    {
      BochsSupport.ExtractBochsDebugSymbols(Path.ChangeExtension(mParams["ISOFile"], "map"), Path.ChangeExtension(mParams["ISOFile"], "sym"));
      _bochsProcess = new Process();
      ProcessStartInfo _bochsStartInfo = _bochsProcess.StartInfo;
      _bochsStartInfo.FileName = (_useDebugVersion && BochsSupport.BochsDebugExe.Exists)
                                     ? BochsSupport.BochsDebugExe.FullName
                                     : BochsSupport.BochsExe.FullName;
      // Start Bochs without displaying the configuration interface (-q) and using the specified
      // configuration file (-f). The user is intended to edit the configuration file coming with
      // the Cosmos project whenever she wants to modify the environment.
      var xExtraLog = "";
      if (_useDebugVersion)
      {
        //xExtraLog = "-dbglog \"bochsdbg.log\"";
      }
      _bochsStartInfo.Arguments = string.Format("-q {1} -f \"{0}\"", _bochsConfigurationFile.FullName, xExtraLog);
      _bochsStartInfo.WorkingDirectory = _bochsConfigurationFile.Directory.FullName;
      _bochsStartInfo.CreateNoWindow = true; // when ProcessStartInfo.UseShellExecute is supported in .net core, maybe this line isn't needed
      //_bochsStartInfo.UseShellExecute = true;
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
        //_bochsStartInfo.RedirectStandardOutput = true;
        //_bochsStartInfo.RedirectStandardError = true;
        // _bochsProcess.OutputDataReceived += (sender, args) => LogOutput(args.Data);
        // _bochsProcess.ErrorDataReceived += (sender, args) => LogError(args.Data);
      }
      // Register for process completion event so that we can funnel it to any code that
      // subscribed to this event in our base class.
      _bochsProcess.EnableRaisingEvents = true;
      _bochsProcess.Exited += ExitCallback;
      _bochsProcess.Start();
      if (RedirectOutput)
      {
        _bochsProcess.BeginErrorReadLine();
        _bochsProcess.BeginOutputReadLine();
      }
      return;
    }

    private void ExitCallback(object sender, EventArgs e)
    {
      if (null != OnShutDown)
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
      if (null != _bochsProcess)
      {
        try
        {
          _bochsProcess.Kill();
        }
        catch
        {
        }
      }
      CleanUp();
    }

    private void CleanUp()
    {
      OnShutDown(this, null);
      _bochsProcess.Exited -= ExitCallback;
      // TODO BlueSkeye : What kind of garbage may Bochs have left for us to clean ?
    }
  }
}
