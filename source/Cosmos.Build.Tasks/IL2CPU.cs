using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Indy.IL2CPU;
using Microsoft.Build.Framework;
using Cosmos.Build.Tasks.Properties;
using System.IO;
using System.Reflection;

namespace Cosmos.Build.Tasks
{
    public class IL2CPU : Task
    {
        private string[] plugs;
        /// <summary>
        /// Gets or sets the plugs files to include in the build.
        /// </summary>
        [Required]
        public string[] Plugs
        {
            get { return plugs; }
            set { plugs = value; }
        }

        private string sourceFile;
        /// <summary>
        /// Gets or sets the source assembly.
        /// </summary>
        public string SourceFile
        {
            get { return sourceFile; }
            set { sourceFile = value; }
        }

        public override bool Execute()
        {
            bool success = true;
            try
            {
                Engine e = new Engine();

                e.DebugLog += new DebugLogHandler(e_DebugLog);

                string path = Path.GetTempFileName();
                string asmDir = Path.Combine(Path.GetDirectoryName(sourceFile), "asm");
                if (!Directory.Exists(asmDir))
                    Directory.CreateDirectory(asmDir);

                Func<string, string> getFilenameForGroup = group => Path.Combine(asmDir, group + ".asm");

                List<string> ps = new List<string>(plugs.Length);
                foreach (string item in plugs)
                    ps.Add(item);

                e.Execute(sourceFile, TargetPlatformEnum.NativeX86, getFilenameForGroup, false, true, null, ps);
            }
            catch (ReflectionTypeLoadException e)
            {
                Log.LogError(null, "IL2", null, "IL2CPU", 0, 0, 0, 0, Resources.UnknownExceptionMessage, e.GetType().ToString());
                for (int i = 0; i < e.LoaderExceptions.Length; i++)
                {
                    Log.LogError(null, "IL2", null, "IL2CPU", 0, 0, 0, 0, Resources.UnknownExceptionMessage, e.LoaderExceptions[i].ToString());
                    Log.LogError(null, "IL2", null, "IL2CPU", 0, 0, 0, 0, Resources.UnknownExceptionMessage, e.StackTrace);
                }
                success = false;
            }
            catch (Exception e)
            {
                Log.LogError(null, "IL1", null, "IL2CPU", 0, 0, 0, 0, Resources.UnknownExceptionMessage, e.ToString());
                Log.LogError(null, "IL2", null, "IL2CPU", 0, 0, 0, 0, Resources.UnknownExceptionMessage, e.StackTrace);
                success = false;
            }

            return success;
        }

        void e_DebugLog(LogSeverityEnum aSeverity, string aMessage)
        {
            switch (aSeverity)
            {
                case LogSeverityEnum.Error:
                    Log.LogError(null, "IL0", null, "IL2CPU", 0, 0, 0, 0, aMessage);
                    break;
                case LogSeverityEnum.Informational:
                    //Log.LogMessage(MessageImportance.Normal, aMessage);
                    Log.LogWarning(null, "IL0", null, "IL2CPU", 0, 0, 0, 0, aMessage);
                    break;
                case LogSeverityEnum.Warning:
                    Log.LogWarning(null, "IL0", null, "IL2CPU", 0, 0, 0, 0, aMessage);
                    break;
            }
        }
    }
}

