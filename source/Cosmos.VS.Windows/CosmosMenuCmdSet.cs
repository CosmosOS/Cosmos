using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

using Cosmos.VS.Windows.ToolWindows;
using Microsoft;

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
        private readonly JoinableTaskFactory _joinableTaskFactory;

        private CosmosMenuCmdSet(CosmosWindowsPackage package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _joinableTaskFactory = package.JoinableTaskFactory;

            _ = AddCommandsAsync();
        }

        private async Task AddCommandsAsync()
        {
            await Task.WhenAll(
                AddCommandAsync(CosmosAssemblyCmdID, ShowWindowAssemblyAsync),
                AddCommandAsync(CosmosRegistersCmdID, ShowWindowRegistersAsync),
                AddCommandAsync(CosmosStackCmdID, ShowWindowStackAsync),
                AddCommandAsync(CosmosInternalCmdID, ShowWindowInternalAsync),
                AddCommandAsync(CosmosShowAllCmdID, ShowWindowAllAsync)
            );
        }

        private async Task AddCommandAsync(int cmdId, AsyncEventHandler handler) => await AddCommandInternalAsync(cmdId, (object sender, EventArgs e) => _joinableTaskFactory.Run(async () => await handler?.InvokeAsync(sender, e)));

        private async Task AddCommandInternalAsync(int cmdId, EventHandler handler)
        {
            if (await _package.GetServiceAsync(typeof(IMenuCommandService)) is IMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(Guids.CosmosMenuCmdSetGuid, cmdId);
                var menuItem = new MenuCommand(handler, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

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
