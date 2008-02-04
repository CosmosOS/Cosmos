using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Indy.IL2CPU;
using Microsoft.Build.Framework;
using Cosmos.Build.Tasks.Properties;
using System.IO;

namespace Cosmos.Build.Tasks
{
    public class IL2CPU : Task
    {
        private TaskItem[] plugs;
        /// <summary>
        /// Gets or sets the plugs files to include in the build.
        /// </summary>
        public TaskItem[] Plugs
        {
            get { return plugs; }
            set { plugs = value; }
        }

        private TaskItem sourceFile;
        /// <summary>
        /// Gets or sets the source assembly.
        /// </summary>
        public TaskItem SourceFile
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
                string sourceFile = SourceFile.GetMetadata("assembly");
                string asmDir = Path.Combine(Path.GetDirectoryName(sourceFile), "asm");
                if (!Directory.Exists(asmDir))
                    Directory.CreateDirectory(asmDir);

                Func<string, string> getFilenameForGroup = group => Path.Combine(asmDir, group + ".asm");

                List<string> ps = new List<string>(plugs.Length);
                foreach (TaskItem item in plugs)
                    ps.Add(item.GetMetadata("assembly"));

                e.Execute(sourceFile, TargetPlatformEnum.NativeX86, getFilenameForGroup, false, true, null, ps);
            }
            catch (Exception e)
            {
                Log.LogMessage(MessageImportance.High, Resources.UnknownExceptionMessage, e.GetType().FullName);
                success = false;
            }

            return success;
        }

        void e_DebugLog(LogSeverityEnum aSeverity, string aMessage)
        {
            switch (aSeverity)
            {
                case LogSeverityEnum.Error:
                    Log.LogError(aMessage);
                    break;
                case LogSeverityEnum.Informational:
                    Log.LogMessage(aMessage);
                    break;
                case LogSeverityEnum.Warning:
                    Log.LogWarning(aMessage);
                    break;
            }
        }
    }
}

