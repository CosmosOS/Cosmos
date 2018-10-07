using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

using Cosmos.VS.Windows.ToolWindows;

namespace Cosmos.VS.Windows
{
    internal sealed class CosmosMenuCmdSet
    {
        public const int CosmosAssemblyCmdID = 0x0100;
        public const int CosmosRegistersCmdID = 0x0101;
        public const int CosmosStackCmdID = 0x0102;
        public const int CosmosInternalCmdID = 0x0103;
        public const int CosmosShowAllCmdID = 0x0104;

        private readonly CosmosWindowsPackage _package;

        private CosmosMenuCmdSet(CosmosWindowsPackage package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));

            AddCommand(CosmosAssemblyCmdID, ShowWindowAssemblyAsync);
            AddCommand(CosmosRegistersCmdID, ShowWindowRegistersAsync);
            AddCommand(CosmosStackCmdID, ShowWindowStackAsync);
            AddCommand(CosmosInternalCmdID, ShowWindowInternalAsync);
            AddCommand(CosmosShowAllCmdID, ShowWindowAllAsync);
        }

        private void AddCommand(int cmdId, EventHandler handler)
        {
            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(Guids.CosmosMenuCmdSetGuid, cmdId);
                var menuItem = new MenuCommand(handler, menuCommandID);

                commandService.AddCommand(menuItem);
            }
        }

        private void AddCommand(int cmdId, AsyncEventHandler handler) =>
            AddCommand(cmdId, (EventHandler)((sender, e) => handler?.InvokeAsync(sender, e)));

        public static CosmosMenuCmdSet Instance { get; private set; }

        private IServiceProvider ServiceProvider => _package;

        public static void Initialize(CosmosWindowsPackage package)
        {
            Instance = new CosmosMenuCmdSet(package);
        }

        private async Task ShowWindowAsync(Type aWindowType)
        {
            await _package.JoinableTaskFactory.SwitchToMainThreadAsync();

            var xWindow = await _package.FindWindowAsync(aWindowType);
            var xFrame = (IVsWindowFrame)xWindow.Frame;

            ErrorHandler.ThrowOnFailure(xFrame.Show());
        }

        private async Task ShowChannelWindowAsync(Type aWindowType)
        {
            await _package.JoinableTaskFactory.SwitchToMainThreadAsync();

            var xWindow = await _package.FindChannelWindowAsync(aWindowType);
            var xFrame = (IVsWindowFrame)xWindow.Frame;

            ErrorHandler.ThrowOnFailure(xFrame.Show());
        }

        private Task ShowWindowAssemblyAsync(object sender, EventArgs e) => ShowWindowAsync(typeof(AssemblyToolWindow));

        private Task ShowWindowInternalAsync(object sender, EventArgs e) => ShowWindowAsync(typeof(InternalTW));

        private Task ShowWindowRegistersAsync(object sender, EventArgs e) => ShowWindowAsync(typeof(RegistersToolWindow));

        private Task ShowWindowStackAsync(object sender, EventArgs e) => ShowWindowAsync(typeof(StackTW));

        private Task ShowWindowConsoleAsync(object sender, EventArgs e) => ShowChannelWindowAsync(typeof(ConsoleTW));

        private Task ShowWindowAllAsync(object sender, EventArgs e) =>
            Task.WhenAll(
                ShowWindowAssemblyAsync(sender, e),
                ShowWindowRegistersAsync(sender, e),
                ShowWindowStackAsync(sender, e)
                //ShowWindowConsole(sender, e)
                // Dont show Internal Window, most Cosmos users wont use it.
                );
    }
}
