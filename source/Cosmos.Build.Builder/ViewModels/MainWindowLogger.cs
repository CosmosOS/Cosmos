using Cosmos.Build.Builder.Models;

namespace Cosmos.Build.Builder.ViewModels
{
    internal class MainWindowLogger : ILogger
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindowLogger(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void LogMessage(string text)
        {
            _viewModel.CurrentSection.LogMessage(text);
            _viewModel.TailItems.Push(text);
        }

        public void NewSection(string name)
        {
            _viewModel.Sections.Add(new Section(name));
            _viewModel.TailItems.Clear();
        }

        public void SetError()
        {
            _viewModel.CurrentSection.SetError();
        }
    }
}
