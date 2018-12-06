using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Build.Builder.Dependencies
{
    internal class ReposDependency : IDependency
    {
        public string Name => "Repos: IL2CPU, XSharp and Common";

        private readonly string _cosmosDir;
        private readonly IEnumerable<Repo> _repos;

        public ReposDependency(string cosmosDir)
        {
            _cosmosDir = cosmosDir;

            _repos = new List<Repo>()
            {
                new Repo(
                    Path.GetFullPath(Path.Combine(cosmosDir, "..", "IL2CPU")),
                    "https://github.com/CosmosOS/IL2CPU"),
                new Repo(
                    Path.GetFullPath(Path.Combine(cosmosDir, "..", "XSharp")),
                    "https://github.com/CosmosOS/XSharp"),
                new Repo(
                    Path.GetFullPath(Path.Combine(cosmosDir, "..", "Common")),
                    "https://github.com/CosmosOS/Common")
            };
        }

        public Task<bool> IsInstalledAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_repos.All(r => Directory.Exists(r.LocalPath)));
        }

        public Task InstallAsync(CancellationToken cancellationToken)
        {
            var usesGit = Directory.Exists(Path.Combine(_cosmosDir, ".git"));
            return Task.WhenAll(_repos
                .Where(r => !Directory.Exists(r.LocalPath))
                .Select(r => DownloadRepoAsync(r, usesGit, cancellationToken)));
        }

        private static async Task DownloadRepoAsync(Repo repo, bool useGit, CancellationToken cancellationToken)
        {
            if (useGit)
            {
                var process = Process.Start("git", $"clone \"{repo.Url}.git\" \"{repo.LocalPath}\"");
                await Task.Run(process.WaitForExit).ConfigureAwait(false);

                if (process.ExitCode != 0)
                {
                    throw new Exception("The process failed to execute!");
                }
            }
            else
            {
                using (var httpClient = new HttpClient())
                {
                    var uri = new Uri(new Uri($"{repo.Url}/"), "archive/master.zip");
                    var httpMessage = await httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);

                    var zipStream = await httpMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var zipArchive = new ZipArchive(zipStream);

                    var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                    zipArchive.ExtractToDirectory(tempPath);
                    Directory.Move(Directory.GetFileSystemEntries(tempPath).Single(), repo.LocalPath);
                }
            }
        }

        private class Repo
        {
            public string LocalPath { get; }
            public string Url { get; }

            public Repo(string localPath, string url)
            {
                LocalPath = localPath;
                Url = url;
            }
        }
    }
}
