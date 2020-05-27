using System;
using System.Threading;

using ReactiveUI;

using Cosmos.TestRunner.Core;
using Cosmos.TestRunner.Full;

namespace Cosmos.TestRunner.UI.ViewModels
{
    internal class MainWindowViewModel : ReactiveObject
    {
        private string _log;

        public MainWindowViewModel(IEngineConfiguration aEngineConfiguration)
        {
            var xEngine = new FullEngine(aEngineConfiguration);

            xEngine.SetOutputHandler(
                new OutputHandler(
                    m => TestRunnerLog += m + Environment.NewLine));

            new Thread(() => xEngine.Execute()).Start();
        }

        public string TestRunnerLog
        {
            get => _log;
            set => this.RaiseAndSetIfChanged(ref _log, value);
        }
        
        internal class OutputHandler : OutputHandlerFullTextBase
        {
            private Action<string> mLog;

            public OutputHandler(Action<string> aLog)
            {
                mLog = aLog;
            }

            protected override void Log(string message) => mLog(message);
        }
    }
}
