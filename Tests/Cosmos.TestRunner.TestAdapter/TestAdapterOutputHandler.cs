using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using Cosmos.TestRunner.Core;

namespace Cosmos.TestRunner.TestAdapter
{
    internal class TestAdapterOutputHandler : OutputHandlerFullTextBase
    {
        public IReadOnlyList<string> Messages => _messages;

        private readonly IFrameworkHandle _frameworkHandle;
        private readonly List<string> _messages = new List<string>();

        public TestAdapterOutputHandler(IFrameworkHandle frameworkHandle)
        {
            _frameworkHandle = frameworkHandle;
        }

        protected override void Log(string message)
        {
            _frameworkHandle.SendMessage(TestMessageLevel.Informational, message);
            _messages.Add(message);
        }
    }
}
