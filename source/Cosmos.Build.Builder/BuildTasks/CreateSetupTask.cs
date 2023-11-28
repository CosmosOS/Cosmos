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
            string releaseVersion,
            bool InstallExtensions)
            : base(true, false)
        {
            _innoSetupService = innoSetupService;

            _scriptFilePath = scriptFilePath;

            _defines = new Dictionary<string, string>()
            {
                ["BuildConfiguration"] = configuration,
                ["ChangeSetVersion"] = releaseVersion,
            };

            if(!InstallExtensions)
            {
                _defines.Add("DoNotInstallExtensions", "1");
            }

            // when building the userkit we want to let innosetup determine the installation location
            // see https://github.com/CosmosOS/Cosmos/issues/2329
            if (configuration == "DevKit")
            {
                _defines["RealPath"] = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
        }

        protected override string GetExePath()
        {
            var innoSetupInstallationPath = _innoSetupService.GetInnoSetupInstallationPath();
            var innoSetupCompilerPath = Path.Combine(innoSetupInstallationPath, "ISCC.exe");

            if (!File.Exists(innoSetupCompilerPath))
            {
                throw new InvalidOperationException($"An Inno Setup installation was detected, but no compiler exists at {innoSetupCompilerPath}");
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
