using System.Threading.Tasks;

namespace Cosmos.Build.Builder
{
    internal interface IBuildTask
    {
        string Name { get; }

        Task RunAsync(ILogger logger);
    }
}
