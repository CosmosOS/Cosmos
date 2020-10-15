using System;
using System.Collections.Generic;
using System.IO;

using Cosmos.Build.Builder.Services;

namespace Cosmos.Build.Builder.BuildTasks
{
    internal class CreateSetupTask : ProcessBuildTaskBase
    {
        public override string Name => "Create Setup";

        private readonly IInnoSetupService _innoSetupService;

        private readonly string _scriptFilePath;
        private readonly Dictionary<string, string> _defines;

        public CreateSetupTask(
            IInnoSetupService innoSetupService,
            string scriptFilePath,
            string configuration,
            string releaseVersion)
            : base(true, false)
        {
            _innoSetupService = innoSetupService;

            _scriptFilePath = scriptFilePath;

            _defines = new Dictionary<string, string>()
            {
                ["BuildConfiguration"] = configuration,
                ["ChangeSetVersion"] = releaseVersion
            };
        }

        protected override string GetExePath()
        {
            var innoSetupInstallationPath = _innoSetupService.GetInnoSetupInstallationPath();
            var innoSetupCompilerPath = Path.Combine(innoSetupInstallationPath, "ISCC.exe");

            if (!File.Exists(innoSetupCompilerPath))
            {
                throw new InvalidOperationException("Inno Setup installation detected, but the compiler doesn't exist!");
            }

            return innoSetupCompilerPath;
        }

        protected override string GetArguments()
        {
            var args = $"/Q \"{_scriptFilePath}\"";

            if (_defines != null)
            {
                foreach (var define in _defines)
                {
                    args += $" \"/d{define.Key}={define.Value}\"";
                }
            }

            return args;
        }
    }
}
