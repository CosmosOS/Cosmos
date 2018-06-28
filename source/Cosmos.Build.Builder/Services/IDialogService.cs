namespace Cosmos.Build.Builder.Services
{
    internal interface IDialogService<TViewModel>
    {
        bool? ShowDialog(TViewModel viewModel);
    }
}
