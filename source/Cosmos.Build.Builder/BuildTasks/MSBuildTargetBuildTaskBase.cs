using System;
using System.Collections.Generic;

using Cosmos.Build.Builder.Services;

namespace Cosmos.Build.Builder.BuildTasks
{
    internal abstract class MSBuildTargetBuildTaskBase : ProcessBuildTaskBase
    {
        public abstract string ProjectFilePath { get; }
        public abstract IEnumerable<string> Targets { get; }

        protected abstract IReadOnlyDictionary<string, string> Properties { get; }

        private readonly IMSBuildService _msBuildService;

        protected MSBuildTargetBuildTaskBase(IMSBuildService msBuildService)
            : base(true, false)
        {
            _msBuildService = msBuildService;
        }

        protected override string GetExePath() => _msBuildService.GetMSBuildExePath();

        protected override string GetArguments()
        {
            if (ProjectFilePath == null)
            {
                throw new InvalidOperationException("ProjectFilePath is null!");
            }

            var args = $"\"{ProjectFilePath}\" /nologo /maxcpucount /nodeReuse:False /verbosity:minimal /t:\"{String.Join(";", Targets)}\"";

            if (Properties != null)
            {
                foreach (var property in Properties)
                {
                    var value = property.Value;

                    if (value.EndsWith("\\"))
                    {
                        value += '\\';
                    }

                    args += $" /p:\"{property.Key}={value}\"";
                }
            }

            args += " /p:DeployExtension=False";

            return args;
        }
    }
}
