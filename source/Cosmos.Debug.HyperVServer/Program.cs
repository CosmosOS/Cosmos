using System.Threading.Tasks;

namespace Cosmos.Debug.HyperVServer
{
    internal class Program
    {
        private static Task<int> Main(string[] args)
        {
            try
            {
                var server = new Server();
                return server.RunAsync();
            }
            catch
            {
                return Task.FromResult(-1);
            }
        }
    }
}
