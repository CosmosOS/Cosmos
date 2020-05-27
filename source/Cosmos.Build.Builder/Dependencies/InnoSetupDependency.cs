using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Cosmos.Build.Builder.Services;

namespace Cosmos.Build.Builder.Dependencies
{
    internal class InnoSetupDependency : IDependency
    {
        private const string InnoSetupInstallerUrl = "http://www.jrsoftware.org/download.php/is.exe";

        public string Name => "Inno Setup";

        private readonly IInnoSetupService _innoSetupService;

        public InnoSetupDependency(IInnoSetupService innoSetupService)
        {
            _innoSetupService = innoSetupService;
        }

        public Task<bool> IsInstalledAsync(CancellationToken cancellationToken) =>
            Task.FromResult(_innoSetupService.GetInnoSetupInstallationPath() != null);

        public async Task InstallAsync(CancellationToken cancellationToken)
        {
            string setupFilePath;

            using (var httpClient = new HttpClient())
            {
                var httpMessage = await httpClient.GetAsync(InnoSetupInstallerUrl, cancellationToken).ConfigureAwait(false);

                var setupFileName = Path.GetFileName(httpMessage.RequestMessage.RequestUri.LocalPath);
                var setupFileDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                setupFilePath = Path.Combine(setupFileDirectoryPath, setupFileName);

                Directory.CreateDirectory(setupFileDirectoryPath);

                using (var fileStream = File.Create(setupFilePath))
                {
                    await httpMessage.Content.CopyToAsync(fileStream).ConfigureAwait(false);
                }
            }

            var process = Process.Start(setupFilePath);
            await Task.Run(process.WaitForExit, cancellationToken).ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                throw new Exception("The process failed to execute!");
            }
        }
    }
}
