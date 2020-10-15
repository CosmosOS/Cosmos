using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Cosmos.Build.Builder.Models;
using Cosmos.Build.Builder.Services;

namespace Cosmos.Build.Builder.ViewModels
{
    internal sealed class VisualStudioInstanceDialogViewModel : ViewModelBase
    {
        public IEnumerable<VisualStudioInstance> VisualStudioInstances { get; }

        public VisualStudioInstance SelectedVisualStudioInstance
        {
            get => _selectedVisualStudioInstance;
            set => SetAndRaiseIfChanged(ref _selectedVisualStudioInstance, value);
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        private readonly IVisualStudioService _visualStudioService;

        private VisualStudioInstance _selectedVisualStudioInstance;

        public VisualStudioInstanceDialogViewModel(IVisualStudioService visualStudioService)
        {
            _visualStudioService = visualStudioService;

            VisualStudioInstances = _visualStudioService.GetInstances().Select(i => new VisualStudioInstance(i)).ToList();
            SelectedVisualStudioInstance = VisualStudioInstances.FirstOrDefault();

            OkCommand = new RelayCommand(p => Close(p as Window, true));
            CancelCommand = new RelayCommand(p => Close(p as Window, false));
        }

        private static void Close(Window window, bool? dialogResult)
        {
#if DEBUG
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }
#endif

            window.DialogResult = dialogResult;
            window.Close();
        }
    }
}
