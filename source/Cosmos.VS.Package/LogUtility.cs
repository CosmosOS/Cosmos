#define FULL_DEBUG
using System;
using System.Collections.Generic;
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
                File.AppendAllText(GetLogFilePath(), DateTime.Now.ToString() + " - " + String.Format(message, args) + Environment.NewLine);
            }
#endif
        }

        public static void LogException(Exception e, bool dontThrow = false)
        {
#if FULL_DEBUG
          if (null != e) {
            do {
              LogString("Error : {0}", e.Message);
              LogString("Stack : {0}", e.StackTrace);
              e = e.InnerException;
            } while (null != e);
          }
#endif
            if (!dontThrow) { throw new Exception("Error occurred", e); }
        }

        private static string _logFilePath;
        private static string GetLogFilePath()
        {
          if (null != _logFilePath)
          {
              CreateDirectoryForFilePath(_logFilePath);
              return _logFilePath;
          }
          _logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Cosmos", "CosmosPkg.log");
          CreateDirectoryForFilePath(_logFilePath);
          return _logFilePath;
        }

        private static void CreateDirectoryForFilePath(string filePath)
        {
            DirectoryInfo scannedDirectory = new FileInfo(filePath).Directory;

            if (scannedDirectory.Exists)
                return;

            Stack<DirectoryInfo> missingDirectories = new Stack<DirectoryInfo>();

            do
            {
                missingDirectories.Push(scannedDirectory);
                scannedDirectory = scannedDirectory.Parent;
            }
            while (!scannedDirectory.Exists);

            do
            {
                try { Directory.CreateDirectory(missingDirectories.Pop().FullName); }
                catch { break; }
            }
            while (0 < missingDirectories.Count);
        }
    }
}