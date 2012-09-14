// DO NOT remove this line. Consider commenting it out. When the symbol is enabled the Bochs debug
// enbabled version will be launched instead of the regular one. The debug enabled version breaks
// in Bochs internal debugger as soon as the emulator starts. You must use the 'c' (continue) command
// in the Bochs console for the emulation to proceed.
// #define USE_BOCHSDBG
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Win32;

namespace Cosmos.Debug.VSDebugEngine.Host
{
  /// <summary>This class handles interactions with the Bochs emulation environment.</summary>
  public class Bochs : Base
  {
    /// <summary>The Bochs runtime.</summary>
    private static FileInfo _bochsExe;
#if USE_BOCHSDBG
    /// <summary>The Bochs runtime with internal debugger enabled.</summary>
    private static FileInfo _bochsDebugExe;
#endif
    /// <summary>The emulator process once started.</summary>
    private static Process _bochsProcess;
    /// <summary>The configuration file to be used when launching the Bochs virtual machine.</summary>
    private FileInfo _bochsConfigurationFile;

    static Bochs()
    {
      FindBochsExe();
    }

    /// <summary>Instanciation occurs when debugging engine is invoked to launch the process in suspended
    /// mode. Bochs process will eventually be launched later when debugging engine is instructed to
    /// Attach to the debugged process.</summary>
    public Bochs(NameValueCollection aParams, bool aUseGDB, FileInfo configurationFile)
      : base(aParams, aUseGDB)
    {
      if (null == configurationFile) { throw new ArgumentNullException("configurationFile"); }
      if (!configurationFile.Exists) { throw new FileNotFoundException("Configuration file doesn't exist."); }
      _bochsConfigurationFile = configurationFile;
    }

    /// <summary>Get a flag that tell whether Bochs is enabled on this system.</summary>
    public bool BochsEnabled
    {
      get { return (null != _bochsExe); }
    }

    /// <summary>Retrieve installation path for Bochs and initialize the <see cref="_bochsExe"/> field.
    /// Search is performed using the registry and rely on the shell command defined for the
    /// BochsConfigFile.</summary>
    private static void FindBochsExe()
    {
      using (var runCommandRegistryKey = Registry.ClassesRoot.OpenSubKey(@"BochsConfigFile\shell\Run\command", false)) {
        if (null == runCommandRegistryKey) { return; }
        string commandLine = (string)runCommandRegistryKey.GetValue(null, null);
        if (null != commandLine) { commandLine = commandLine.Trim(); }
        if (string.IsNullOrEmpty(commandLine)) { return; }
        // Now perform some parsing on command line to discover full exe path.
        string candidateFilePath;
        int commandLineLength = commandLine.Length;
        if ('"' == commandLine[0]) {
          // Seek for a non escaped double quote.
          int lastDoubleQuoteIndex = 1;
          for (; lastDoubleQuoteIndex < commandLineLength; lastDoubleQuoteIndex++) {
            if ('"' != commandLine[lastDoubleQuoteIndex]) { continue; }
            if ('\\' != commandLine[lastDoubleQuoteIndex - 1]) { break; }
          }
          if (lastDoubleQuoteIndex >= commandLineLength) { return; }
          candidateFilePath = commandLine.Substring(1, lastDoubleQuoteIndex - 1);
        }
        else {
          // Seek for first separator character.
          int firstSeparatorIndex = 0;
          for(; firstSeparatorIndex < commandLineLength; firstSeparatorIndex++) {
            if (char.IsSeparator(commandLine[firstSeparatorIndex])) { break; }
          }
          if (firstSeparatorIndex >= commandLineLength) { return; }
          candidateFilePath = commandLine.Substring(0, firstSeparatorIndex);
        }
        if (!File.Exists(candidateFilePath)) { return; }
        _bochsExe = new FileInfo(candidateFilePath);
#if USE_BOCHSDBG
        _bochsDebugExe = new FileInfo(Path.Combine(_bochsExe.Directory.FullName, "bochsdbg.exe"));
#endif
        return;
      }
    }

    /// <summary>Initialize and start the Bochs process.</summary>
    public override void Start()
    {
      _bochsProcess = new Process();
      ProcessStartInfo _bochsStartInfo = _bochsProcess.StartInfo;
#if USE_BOCHSDBG
      _bochsStartInfo.FileName = _bochsDebugExe.FullName;
#else
      _bochsStartInfo.FileName = _bochsExe.FullName;
#endif
      // Start Bochs without displaying the configuration interface (-q) and using the specified
      // configuration file (-f). The user is intended to edit the configuration file coming with
      // the Cosmos project whenever she wants to modify the environment.
      _bochsStartInfo.Arguments = string.Format("-q -f \"{0}\"", _bochsConfigurationFile.FullName);
      _bochsStartInfo.WorkingDirectory = _bochsConfigurationFile.Directory.FullName;

      // Register for process completion event so that we can funnel it to any code that
      // subscribed to this event in our base class.
      _bochsProcess.EnableRaisingEvents = true;
      _bochsProcess.Exited += delegate(object sender, EventArgs e)
      {
        if (null != OnShutDown) { OnShutDown(sender, e); }
      };
      _bochsProcess.Start();
      return;
    }

    public override void Stop()
    {
      // TODO BlueSkeye : How are we supposed to stop the bochs process ?
      CleanUp();
    }

    private void CleanUp()
    {
      // TODO BlueSkeye : What kind of garbage may Bochs have left for us to clean ?
    }
  }
}
