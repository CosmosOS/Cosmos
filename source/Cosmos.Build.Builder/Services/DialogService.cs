using System;
using System.Windows;

namespace Cosmos.Build.Builder.Services
{
    internal class DialogService<TView, TViewModel> : IDialogService<TViewModel> where TView : Window
    {
        private readonly Func<TView> _dialogFactory;
        private readonly Window _owner;

        public DialogService(Func<TView> dialogFactory, Window owner = null)
        {
            _dialogFactory = dialogFactory;
            _owner = owner;
        }

        public bool? ShowDialog(TViewModel viewModel)
        {
            var dialog = _dialogFactory?.Invoke();

            if (dialog != null)
            {
                if (_owner != null)
                {
                    dialog.Owner = _owner;
                }

                dialog.DataContext = viewModel;
                return dialog.ShowDialog();
            }

            return null;
        }
    }
}
