using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Build.Windows;
using System.Collections;

namespace MatthijsTest
{
    public class Program
    {
        #region Cosmos Builder logic

        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        private static void Main(string[] args)
        {
            BuildUI.Run();
        }

        #endregion

        public static void GetResumeAndResume(ref uint aSuspend)
        {
            aSuspend = 0;
            throw new NotImplementedException();
        }

        public static void Init()
        {
            var xTest = new List<string>() { "String1", "String2", "String3", "String4" };
            foreach (string xItem in xTest) { Console.WriteLine(xItem); }
        }
    }
}