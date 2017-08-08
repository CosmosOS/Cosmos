using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PlugViewer
{
    public static class Log
    {
        private static StreamWriter streamWriter = new StreamWriter(System.Windows.Forms.Application.StartupPath + "\\log.txt");

        public static void WriteLine(string s)
        {
            streamWriter.WriteLine(s);
        }

        public static void Close()
        {
            streamWriter.Flush();
            streamWriter.Close();
            streamWriter.Dispose();
        }
    }
}
