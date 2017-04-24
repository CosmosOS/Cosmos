using System.Reflection;

using NUnitLite;

namespace Cosmos.TestRunner.UnitTest
{
    public class Program
    {
        static int Main(string[] args)
        {
            return new AutoRun(typeof(Program).GetTypeInfo().Assembly).Execute(args);
        }
    }
}
