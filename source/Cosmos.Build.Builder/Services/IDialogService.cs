using System.Windows;

namespace Cosmos.Build.Builder.Services
{
    internal interface IDialogService<TViewModel>
    {
        void SetAnotherOwner(Window owner);
        bool? ShowDialog(TViewModel viewModel);
    }
}
