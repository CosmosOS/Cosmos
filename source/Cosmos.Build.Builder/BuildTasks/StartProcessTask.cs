using System.IO;

namespace Cosmos.Build.Builder.BuildTasks
{
    internal class StartProcessTask : ProcessBuildTaskBase
    {
        public override string Name => $"Run Process - {_processName ?? Path.GetFileNameWithoutExtension(_exePath)}";

        private string _exePath;
        private string _args;

        private string _processName;

        public StartProcessTask(
            string exePath,
            string args,
            string processName = null,
            bool waitForExit = false,
            bool createWindow = true)
            : base(waitForExit, false)
        {
            _exePath = exePath;
            _args = args;

            _processName = processName;
        }

        protected override string GetExePath() => _exePath;
        protected override string GetArguments() => _args;
    }
}
