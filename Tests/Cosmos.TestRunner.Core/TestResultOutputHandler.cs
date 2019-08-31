using System.Text;

namespace Cosmos.TestRunner.Core
{
    internal class TestResultOutputHandler : OutputHandlerFullTextBase
    {
        public string TestLog => _logBuilder.ToString();

        private StringBuilder _logBuilder;

        public TestResultOutputHandler()
        {
            _logBuilder = new StringBuilder();
        }

        protected override void Log(string message)
        {
            _logBuilder.AppendLine(message);
        }
    }
}
