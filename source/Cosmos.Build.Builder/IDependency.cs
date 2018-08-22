using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Build.Builder
{
    internal interface IDependency
    {
        string Name { get; }

        Task<bool> IsInstalledAsync(CancellationToken cancellationToken);
        Task InstallAsync(CancellationToken cancellationToken);
    }
}
