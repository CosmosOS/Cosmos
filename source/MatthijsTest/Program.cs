using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Build.Windows;

namespace MatthijsTest {
    public class Program {
        #region Cosmos Builder logic

        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        private static void Main(string[] args) {
            BuildUI.Run();
        }

        #endregion

        public static void GetResumeAndResume(ref uint aSuspend)
        {
            aSuspend = 0;
            throw new NotImplementedException();
        }

        public static void Init() {
            //Cosmos.Kernel.Boot.Default();
            var xTest = new Object();
        }
    }
}