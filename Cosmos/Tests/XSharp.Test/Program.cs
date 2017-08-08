using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace XSharp.Test
{
    public static class Program
    {
        private static void DisplayUsage()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0} [-h] [-d <directory path>]\r\n",
                Assembly.GetExecutingAssembly().GetName().Name);
            builder.AppendFormat("-h : display this notice.\r\n");
            builder.AppendFormat("-a : Launch NASM on generated source code.\r\n");
            builder.AppendFormat("-d : names a directory that will be searched for .xs file.\r\n");
            MessageBox.Show(builder.ToString());
            return;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            if (!ParseArgs(args) || _displayUsage)
            {
                DisplayUsage();
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm form = new MainForm();
            form.RootDirectory = _rootDirectory;
            form.Compile = _launchNasm;
            Application.Run(form);
        }

        private static bool ParseArgs(string[] args)
        {
            bool result = true;

            for (int index = 0; index < args.Length; index++)
            {
                string scannedArgument = args[index];
                // Defensive programming.
                if (string.IsNullOrEmpty(scannedArgument)) { scannedArgument = " "; }
                switch (scannedArgument[0])
                {
                    case '-':
                    case '/':
                        if (1 == scannedArgument.Length) { goto default; }
                        scannedArgument = scannedArgument.Substring(1);
                        break;
                    default:
                        MessageBox.Show(string.Format("Unrecognized command line argument '{0}'.", scannedArgument));
                        result = false;
                        break;
                }
                switch (scannedArgument.ToLower())
                {
                    case "a":
                        _launchNasm = true;
                        break;
                    case "d":
                        if (++index >= args.Length)
                        {
                            MessageBox.Show("The -d command line argument must be followed by a directory name.");
                            result = false;
                        }
                        else
                        {
                            string directoryPath = args[index] ?? "";
                            try { _rootDirectory = new DirectoryInfo(args[index]); }
                            catch
                            {
                                MessageBox.Show(string.Format("'{0}' is not a valid directory path for -d command line argument.",
                                    directoryPath));
                                result = false;
                                break;
                            }
                            if (!_rootDirectory.Exists)
                            {
                                MessageBox.Show(string.Format("The target directory '{0} doesn't exist.", _rootDirectory.FullName));
                                result = false;
                            }
                        }
                        break;
                    case "h":
                        _displayUsage = true;
                        break;
                    default:
                        // Must reinitialize scannedArgument to its original value.
                        scannedArgument = args[index];
                        MessageBox.Show(string.Format("Unrecognized command line argument '{0}'.", scannedArgument));
                        result = false;
                        break;
                }
            }
            return result;
        }

        private static bool _displayUsage = false;
        private static bool _launchNasm = false;
        private static DirectoryInfo _rootDirectory;
    }
}
