using System;
using System.ComponentModel;
using System.Threading;

using Cosmos.TestRunner.Core;
using Cosmos.TestRunner.Full;

namespace Cosmos.TestRunner.UI.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(IEngineConfiguration aEngineConfiguration)
        {
            var xEngine = new FullEngine(aEngineConfiguration);

            xEngine.SetOutputHandler(
                new OutputHandler(
                    m =>
                    {
                        TestRunnerLog += m + Environment.NewLine;
                        OnPropertyChanged(nameof(TestRunnerLog));
                    }));

            new Thread(() => xEngine.Execute()).Start();
        }

        public string TestRunnerLog { get; set; }

        private void OnPropertyChanged(string aPropertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(aPropertyName));

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
