using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Microsoft.Win32;

using Cosmos.Build.Common;

namespace XSharp.Test {
  public partial class MainForm : Form {
    // D:\source\Cosmos\source2\Tests\XSharpCompilerTester\bin\Debug
    // D:\source\Cosmos\source2\Users\Matthijs\MatthijsPlayground

    public MainForm() {
      InitializeComponent();
    }

    internal bool Compile { get; set; }

    internal DirectoryInfo RootDirectory { get; set; }

    /// <summary>Attempt to retrieve the NASM path.</summary>
    /// <returns>The descriptor for the exe or a null reference if not found.</returns>
    private FileInfo GetNasmPath()
    {
        try
        {
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (RegistryKey cosmos = hklm.OpenSubKey(@"Software\Cosmos", false))
                {
                    if (null == cosmos) { return null; }
                    string userKit = cosmos.GetValue("UserKit") as string;

                    if (null == userKit) { return null; }
                    FileInfo result = new FileInfo(Path.Combine(userKit, "Build", "Tools", "Nasm", "Nasm.exe"));

                    return result.Exists ? result : null;
                }
            }
        }
        catch { return null; }
    }

    /// <summary>Launch NASM on the given input file, generating result into a temporary
    /// file wwith ELF format. Errors are writen back to the given <paramref name="resultCollector"/>
    /// </summary>
    /// <param name="inputFile">Input file to be compiled.</param>
    /// <param name="resultCollector">A writer where to redirect errors and warnings.</param>
    /// <returns>true on successfull compilation, false otherwise.</returns>
    protected bool LaunchNasm(string inputFile, StringWriter resultCollector)
    {
        FileInfo outputFile = new FileInfo(Path.GetTempFileName());
        bool errorEncountered = false;

        try
        {
            if (outputFile.Exists) { outputFile.Delete(); }
            var xProcessStartInfo = new ProcessStartInfo();
            xProcessStartInfo.WorkingDirectory = outputFile.Directory.FullName;
            xProcessStartInfo.FileName = _nasmPath.FullName;
            xProcessStartInfo.Arguments = string.Format("-g -f elf -o \"{0}\" -DELF_COMPILATION \"{1}\"",
                outputFile.FullName, inputFile);
            xProcessStartInfo.UseShellExecute = false;
            xProcessStartInfo.RedirectStandardOutput = true;
            xProcessStartInfo.RedirectStandardError = true;
            xProcessStartInfo.CreateNoWindow = true;
            using (var xProcess = new Process())
            {
                xProcess.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    if (null != e.Data)
                    {
                        errorEncountered = true;
                        resultCollector.WriteLine("ERROR : " + e.Data);
                    }
                };
                xProcess.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    if (null != e.Data)
                    {
                        if (e.Data.StartsWith("error")) { errorEncountered = true; }
                        resultCollector.WriteLine(e.Data);
                    }
                };
                xProcess.StartInfo = xProcessStartInfo;
                xProcess.Start();
                xProcess.BeginErrorReadLine();
                xProcess.BeginOutputReadLine();
                xProcess.WaitForExit(15 * 60 * 1000); // wait 15 minutes
                if (xProcess.ExitCode != 0)
                {
                    if (!xProcess.HasExited)
                    {
                        xProcess.Kill();
                        resultCollector.WriteLine("Process timed out.");
                    }
                    else { resultCollector.WriteLine("Error occurred while invoking NASM."); }
                }
                return !errorEncountered && (0 == xProcess.ExitCode);
            }
        }
        finally
        {
            outputFile.Refresh();
            if (outputFile.Exists)
            {
                try { File.Delete(outputFile.FullName); }
                catch { }
            }
        }
    }

    protected void Test(string aFilename) {
      tabsMain.TabPages.Add(Path.GetFileNameWithoutExtension(aFilename));
      var xTab = tabsMain.TabPages[tabsMain.TabPages.Count - 1];

      var xTbox = new TextBox();
      xTab.Controls.Add(xTbox);
      xTbox.Dock = DockStyle.Fill;
      xTbox.Multiline = true;
      xTbox.Font = new Font("Consolas", 12);
      xTbox.ScrollBars = ScrollBars.Both;

      using (var xInput = new StreamReader(aFilename)) {
        using (var xOutput = new StringWriter()) {
            try {
              var xGenerator = new XSharp.Compiler.AsmGenerator();

              xOutput.WriteLine("ORIGIN:");
              xGenerator.Generate(xInput, xOutput);
              if (Compile)
              {
                  if (null == _nasmPath)
                  {
                      xOutput.WriteLine("Can't compile. NASM not found.");
                  }
                  else
                  {
                      FileInfo inputFile = new FileInfo(Path.GetTempFileName());
                      bool compilationError = false;

                      try
                      {
                          // UTF8 stream without a BOM.
                          using (StreamWriter writer = new StreamWriter(inputFile.FullName, true))
                          {
                              writer.WriteLine(xOutput.ToString());
                          }
                          xOutput.WriteLine("============================");
                          xOutput.WriteLine("Compiling");
                          compilationError = !LaunchNasm(inputFile.FullName, xOutput);
                          if (compilationError) { xOutput.WriteLine("Some compilation error."); }
                          else { xOutput.WriteLine("Successfully compiled."); }
                      }
                      finally
                      {
                          inputFile.Refresh();
                          if (!compilationError && inputFile.Exists) { inputFile.Delete(); }
                      }
                  }
              }
              xTbox.Text = xOutput.ToString() + "\r\n" + xOutput.ToString();
            } catch (Exception ex) {
              xTab.Text = "* " + xTab.Text;
              StringBuilder builder = new StringBuilder();

              builder.AppendLine(xOutput.ToString());
              Exception innerMostException = null;
              for (Exception e = ex; null != e; e = e.InnerException)
              {
                  builder.AppendLine(e.Message);
                  innerMostException = e;
              }
              if (null != innerMostException)
              {
                  builder.AppendLine(innerMostException.StackTrace);
              }
              xTbox.Text = builder.ToString();
            }
        }
      }
    }

    private void MainForm_Load(object sender, EventArgs e) {
      if (null == RootDirectory) { RootDirectory = new DirectoryInfo(CosmosPaths.DebugStubSrc); }
      // For testing
      // Test(Path.Combine(RootDirectory.FullName, "Serial.xs"));

      if (Compile) { _nasmPath = GetNasmPath(); }
      var xFiles = Directory.GetFiles(RootDirectory.FullName, "*.xs");
      foreach (var xFile in xFiles) {
        Test(xFile);
      }
    }

    private FileInfo _nasmPath;
  }
}
