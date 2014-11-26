using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Debug.VSDebugEngine
{
    public static class AD7Util
    {
        public static void Log(string message, params object[] args)
        {
          // this method doesn't do anything normally, but keep it for debugging
          //  File.AppendAllText(@"c:\data\sources\ad7.log", DateTime.Now.ToString("HH:mm:ss.ffffff: ") + String.Format(message, args) + Environment.NewLine);
        }
    }
}