using System.Threading.Tasks;

namespace Cosmos.VS.ProjectSystem
{
    public interface IBootableProperties
    {
        Task<string> GetBinFileAsync();
        Task<string> GetIsoFileAsync();
    }
}
