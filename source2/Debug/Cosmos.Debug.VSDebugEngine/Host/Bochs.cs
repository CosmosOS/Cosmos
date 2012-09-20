using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Win32;

using Cosmos.Build.Common;
using Cosmos.Debug.Common;

namespace Cosmos.Debug.VSDebugEngine.Host
{
  /// <summary>This class handles interactions with the Bochs emulation environment.</summary>
  public class Bochs : Base
  {
    /// <summary>The emulator process once started.</summary>
    private static Process _bochsProcess;
    /// <summary>The configuration file to be used when launching the Bochs virtual machine.</summary>
    private FileInfo _bochsConfigurationFile;
    private bool _useDebugVersion;

    /// <summary>Instanciation occurs when debugging engine is invoked to launch the process in suspended
    /// mode. Bochs process will eventually be launched later when debugging engine is instructed to
    /// Attach to the debugged process.</summary>
    public Bochs(NameValueCollection aParams, bool aUseGDB, FileInfo configurationFile)
      : base(aParams, aUseGDB)
    {
      if (null == configurationFile) { throw new ArgumentNullException("configurationFile"); }
      if (!configurationFile.Exists) { throw new FileNotFoundException("Configuration file doesn't exist."); }
      _bochsConfigurationFile = configurationFile;
      bool parseSucceeded = bool.TryParse(aParams[BuildProperties.EnableBochsDebugString], out _useDebugVersion);
      return;
    }

    /// <summary>Fix the content of the configuration file, replacing each of the symbolic variable occurence
    /// with its associated value.</summary>
    /// <param name="symbols">A set of key/value pairs where the key is the name of a variable. The value is
    /// used for variable replacement. Variables are case sensistive.</param>
    internal void FixBochsConfiguration(KeyValuePair<string, string>[] symbols)
    {
        if ((null == symbols) || (0 == symbols.Length)) { return; }
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

    /// <summary>Initialize and start the Bochs process.</summary>
    public override void Start()
    {
      _bochsProcess = new Process();
      ProcessStartInfo _bochsStartInfo = _bochsProcess.StartInfo;
      _bochsStartInfo.FileName = (_useDebugVersion && BochsSupport.BochsDebugExe.Exists)
        ? BochsSupport.BochsDebugExe.FullName
        : BochsSupport.BochsExe.FullName;
      // Start Bochs without displaying the configuration interface (-q) and using the specified
      // configuration file (-f). The user is intended to edit the configuration file coming with
      // the Cosmos project whenever she wants to modify the environment.
      _bochsStartInfo.Arguments = string.Format("-q -f \"{0}\"", _bochsConfigurationFile.FullName);
      _bochsStartInfo.WorkingDirectory = _bochsConfigurationFile.Directory.FullName;
      _bochsStartInfo.UseShellExecute = false;

      // Register for process completion event so that we can funnel it to any code that
      // subscribed to this event in our base class.
      _bochsProcess.EnableRaisingEvents = true;
      _bochsProcess.Exited += ExitCallback;
      _bochsProcess.Start();
      return;
    }

    private void ExitCallback(object sender, EventArgs e)
    {
        if (null != OnShutDown) { try { OnShutDown(sender, e); } catch { } }
    }

    public override void Stop()
    {
        if (null != _bochsProcess) { try { _bochsProcess.Kill(); } catch { } }
        CleanUp();
    }

    private void CleanUp()
    {
        _bochsProcess.Exited -= ExitCallback;
        // TODO BlueSkeye : What kind of garbage may Bochs have left for us to clean ?
    }
  }
}
