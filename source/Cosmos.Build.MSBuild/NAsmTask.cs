using System;
using System.IO;

namespace Cosmos.Build.MSBuild
{
    public class NAsmTask
    {
        public string InputFile;
        public string OutputFile;
        public string ExePath;
        public bool IsELF;
        public Action<string> LogError;
        public Action<string> LogMessage;

        public bool Execute()
        {
            if (File.Exists(OutputFile))
            {
                File.Delete(OutputFile);
            }
            if (!File.Exists(InputFile))
            {
                LogError("Input file \"" + InputFile + "\" does not exist!");
                return false;
            }
            else if (!File.Exists(ExePath))
            {
                LogError("Exe file not found! (File = \"" + ExePath + "\")");
                return false;
            }

            var xFormat = IsELF ? "elf" : "bin";
            var xResult = BaseToolTask.ExecuteTool(Path.GetDirectoryName(OutputFile), ExePath,
                                                   String.Format("-g -f {0} -o \"{1}\" -D{3}_COMPILATION -O0 \"{2}\"", xFormat, Path.Combine(Directory.GetCurrentDirectory(), OutputFile), Path.Combine(Directory.GetCurrentDirectory(), InputFile), xFormat.ToUpper()),
                                                   "NAsm", LogError, LogMessage);

            if (xResult)
            {
                LogMessage(String.Format("{0} -> {1}", InputFile, OutputFile));
            }
            return true;
        }
    }
}
