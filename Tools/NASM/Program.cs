using System;
using System.Diagnostics;
using System.IO;

namespace NASM
{
    public class Program
    {
        public static string InputFile;
        public static string OutputFile;
        public static string ExePath;
        public static bool IsELF;
        public static Action<string> LogError;
        public static Action<string> LogMessage;

        public static int Main(string[] args)
        {
            return Run(args, Console.WriteLine, s =>
                                                {
                                                    Console.Write("Error: ");
                                                    Console.WriteLine(s);
                                                });
        }

        public static int Run(string[] args, Action<string> logMessage, Action<string> logError)
        {
            if (string.IsNullOrWhiteSpace(InputFile) || string.IsNullOrWhiteSpace(OutputFile) ||
                string.IsNullOrWhiteSpace(ExePath))
            {
                if (args == null)
                {
                    throw new ArgumentNullException("args");
                }
                if (logMessage == null)
                {
                    throw new ArgumentNullException("logMessage");
                }
                if (logError == null)
                {
                    throw new ArgumentNullException("logError");
                }
            }

            LogError = logError;
            LogMessage = logMessage;

            try
            {
                if (string.IsNullOrWhiteSpace(InputFile) || string.IsNullOrWhiteSpace(OutputFile) ||
                    string.IsNullOrWhiteSpace(ExePath))
                {
                    var tmp = "";
                    foreach (var s in args)
                    {
                        tmp += s;
                        string[] s1 = s.Split(':');
                        string argID = s1[0].ToLower();

                        if (argID == "InputFile".ToLower())
                        {
                            InputFile = (s.Replace(s1[0] + ":", ""));
                        }
                        else if (argID == "OutputFile".ToLower())
                        {
                            OutputFile = (s.Replace(s1[0] + ":", ""));
                        }
                        else if (argID == "ExePath".ToLower())
                        {
                            ExePath = (s.Replace(s1[0] + ":", ""));
                        }
                        else if (argID == "IsELF".ToLower())
                        {
                            IsELF = bool.Parse(s.Replace(s1[0] + ":", ""));
                        }
                    }
                }

                if (File.Exists(OutputFile))
                {
                    File.Delete(OutputFile);
                }
                if (!File.Exists(InputFile))
                {
                    LogError("Input file \"" + InputFile + "\" does not exist!");
                    return 0;
                }
                if (!File.Exists(ExePath))
                {
                    LogError("Exe file not found! (File = \"" + ExePath + "\")");
                    return 0;
                }

                string xFormat = IsELF ? "elf" : "bin";
                string xArgs = string.Format("-g -f {0} -o \"{1}\" -D{3}_COMPILATION -O0 \"{2}\"",
                                             xFormat,
                                             Path.Combine(Directory.GetCurrentDirectory(), OutputFile),
                                             Path.Combine(Directory.GetCurrentDirectory(), InputFile),
                                             xFormat.ToUpper());

                var xProcess = Process.Start(ExePath, xArgs);

                xProcess.WaitForExit();

                int xResult = xProcess.ExitCode;
                if (xResult != 0)
                {
                    logMessage(string.Format("{0} -> {1}", InputFile, OutputFile));
                }
                return xResult;
            }
            catch (Exception E)
            {
                logError(String.Format("Error occurred: " + E.ToString()));
                return 0;
            }
        }
    }
}
