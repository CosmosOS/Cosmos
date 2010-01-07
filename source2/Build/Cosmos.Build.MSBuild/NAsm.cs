using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using System.Diagnostics;

namespace Cosmos.Build.MSBuild
{
    public class NAsm : Task
    {
        #region Property
        [Required]
        public string InputFile
        {
            get;
            set;
        }

        [Required]
        public string OutputFile
        {
            get;
            set;
        }

        public bool IsELF
        {
            get;
            set;
        }


        [Required]
        public string ExePath
        {
            get;
            set;
        }
        #endregion

        public override bool Execute()
        {
            if (File.Exists(OutputFile))
            {
                File.Delete(OutputFile);
            }
            if (!File.Exists(InputFile))
            {
                Log.LogError("Input file does not exist!");
                return false;
            }
            if (!File.Exists(ExePath))
            {
                Log.LogError("Exe file not found!");
                return false;
            }
            var xFormat = "bin";
            if (IsELF)
            {
                xFormat = "elf";
            }

            var xProcessStartInfo = new ProcessStartInfo();
            xProcessStartInfo.WorkingDirectory = Path.GetDirectoryName(OutputFile);
            xProcessStartInfo.FileName = ExePath;
            xProcessStartInfo.Arguments = String.Format("-g -f {0} -o \"{1}\" \"{2}\"", xFormat, Path.Combine(Environment.CurrentDirectory, OutputFile), Path.Combine(Environment.CurrentDirectory, InputFile));
            xProcessStartInfo.UseShellExecute = false;
            xProcessStartInfo.RedirectStandardOutput = true;
            xProcessStartInfo.RedirectStandardError = true;
            using (var xProcess = Process.Start(xProcessStartInfo))
            {
                if (!xProcess.WaitForExit(30 * 1000) || xProcess.ExitCode != 0)
                {
                    if (!xProcess.HasExited)
                    {
                        xProcess.Kill();
                        Log.LogError("NAsm assembler timed out.");
                    }
                    else
                    {
                        Log.LogError("Error occurred while invoking nasm");
                    }
                    while (!xProcess.StandardOutput.EndOfStream)
                    {
                        Log.LogMessage("NAsm output: {0}", xProcess.StandardOutput.ReadLine());
                    }
                    while (!xProcess.StandardError.EndOfStream)
                    {
                        Log.LogMessage("NAsm error: {0}", xProcess.StandardError.ReadLine());
                    }
                    return false;
                }
            }

            return true;
        }


        //public void Execute()
        //{
        //    Init();

        //    buildFileUtils.RemoveFile(BuildPath + "output.obj");
        //    var xFormat = "bin";
        //    if (IsELF)
        //    {
        //        xFormat = "elf";
        //    }
        //    Global.Call(ToolsPath + @"nasm\nasm.exe", String.Format("-g -f {0} -o \"{1}\" \"{2}\"", xFormat, BuildPath + "output.obj", AsmPath + "main.asm"), BuildPath);

        //    Finish();
        //}




    }
}