using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FrodeTest.Shell;
using System.IO;
using Cosmos.Hardware;

namespace FrodeTest.Application
{
    class cd : IConsoleApplication
    {
        #region IConsoleApplication Members

        public int Execute(string[] args)
        {
            try
            {
                DebugUtil.SendMessage("cd.cs", "Executing cd command");
                if (args == null)
                    return 0;

                if (String.IsNullOrEmpty(args[0]))
                    return 0;

                if (args[0] == "..")
                {
                    //Go up one directory

                    //TODO: Use DirectoryInfo.GetParent
                    string xCurDir = Directory.GetCurrentDirectory();
                    xCurDir = xCurDir.TrimEnd(Path.DirectorySeparatorChar);
                    //EnvironmentVariables.GetCurrent().CurrentDirectory = xCurDir.Substring(0, xCurDir.LastIndexOf(Path.DirectorySeparatorChar));
                    Directory.SetCurrentDirectory(
                        Directory.GetCurrentDirectory().Substring(
                            0, 
                            Directory.GetCurrentDirectory().LastIndexOf(Path.DirectorySeparatorChar)));
                    
                    //Directory.SetCurrentDirectory(Directory.GetParent); //TODO: Use this when GetParent works

                    return 0;
                }

                string xNewPath = Directory.GetCurrentDirectory() + args[0] + "/"; //EnvironmentVariables.GetCurrent().CurrentDirectory + args[0] + "/";
                DebugUtil.SendMessage("cd.cs", "Checking path " + xNewPath);
                if (Directory.Exists(xNewPath))
                {
                    //EnvironmentVariables.GetCurrent().CurrentDirectory = xNewPath;
                    Directory.SetCurrentDirectory(xNewPath);
                }
                else if (Directory.Exists(args[0] + "/"))
                {
                    //EnvironmentVariables.GetCurrent().CurrentDirectory += args[0] + "/";
                    Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + args[0] + "/");
                }
                else
                {
                    Console.WriteLine("Unknown path, didn't match " + xNewPath + " or " + args[0] + "/");
                    return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
             
            return 0;
        }

        public string CommandName
        {
            get { return "cd"; }
        }

        public string Description
        {
            get { return "Change the current directory"; }
        }

        #endregion
    }
}
