using System;
using System.Windows;

using Cosmos.Build.Builder.Models;

namespace Cosmos.Build.Builder.ViewModels
{
    internal class MainWindowLogger : ILogger
    {
        private static readonly string[] NewLineStringArray = new string[] { Environment.NewLine };

        private readonly MainWindowViewModel _viewModel;

        public MainWindowLogger(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void LogMessage(string text) => Application.Current.Dispatcher.Invoke(
            () =>
            {
                _viewModel.CurrentSection.LogMessage(text);

                foreach (var line in text.Split(NewLineStringArray, StringSplitOptions.None))
                {
                    _viewModel.TailItems.Push(line);
                }
            });

        public void NewSection(string name) => Application.Current.Dispatcher.Invoke(
            () =>
            {
                _viewModel.Sections.Add(new Section(name));
                _viewModel.TailItems.Clear();
            });

        public void SetError() => Application.Current.Dispatcher.Invoke(
            () =>
            {
                _viewModel.CurrentSection.SetError();
                _viewModel.CloseWhenCompleted = false;
            });
    }
}
