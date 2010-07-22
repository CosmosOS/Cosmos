#define FULL_DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Cosmos.VS.Package
{
    public static class LogUtility
    {
        private static readonly object mLockObj = new object();
        [Conditional("FULL_DEBUG")]
        public static void LogString(string message, params object[] args)
        {
#if FULL_DEBUG
            lock (mLockObj)
            {
                File.AppendAllText(@"m:\vsip.log", String.Format(message, args) + "\r\n");
            }
#endif
        }

        public static void LogException(Exception e)
        {
#if FULL_DEBUG
            LogString("Error: " + e.ToString());
#endif
            throw new Exception("Error occurred", e);
        }
    }
}