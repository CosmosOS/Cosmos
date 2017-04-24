using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.Windows
{
    internal sealed class CosmosMenuCmdSet
    {
        public const int CosmosAssemblyCmdID = 0x0100;
        public const int CosmosRegistersCmdID = 0x0101;
        public const int CosmosStackCmdID = 0x0102;
        public const int CosmosInternalCmdID = 0x0103;
        public const int CosmosShowAllCmdID = 0x0104;

        private readonly CosmosWindowsPackage package;

        private CosmosMenuCmdSet(CosmosWindowsPackage package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            AddCommand(CosmosAssemblyCmdID, ShowWindowAssembly);
            AddCommand(CosmosRegistersCmdID, ShowWindowRegisters);
            AddCommand(CosmosStackCmdID, ShowWindowStack);
            AddCommand(CosmosInternalCmdID, ShowWindowInternal);
            AddCommand(CosmosShowAllCmdID, ShowWindowAll);
        }

        private void AddCommand(int cmdId, EventHandler handler)
        {
            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(Guids.CosmosMenuCmdSetGuid, cmdId);
                var menuItem = new MenuCommand(handler, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static CosmosMenuCmdSet Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider => package;

        public static void Initialize(CosmosWindowsPackage package)
        {
            Instance = new CosmosMenuCmdSet(package);
        }

        private void ShowWindow(Type aWindowType)
        {
            var xWindow = package.FindWindow(aWindowType);
            var xFrame = (IVsWindowFrame)xWindow.Frame;
            ErrorHandler.ThrowOnFailure(xFrame.Show());
        }

        private void ShowChannelWindow(Type aWindowType)
        {
            var xWindow = package.FindChannelWindow(aWindowType);
            var xFrame = (IVsWindowFrame)xWindow.Frame;
            ErrorHandler.ThrowOnFailure(xFrame.Show());
        }

        private void ShowWindowAssembly(object aCommand, EventArgs e)
        {
            ShowWindow(typeof(AssemblyTW));
        }

        private void ShowWindowInternal(object aCommand, EventArgs e)
        {
            ShowWindow(typeof(InternalTW));
        }

        private void ShowWindowRegisters(object aCommand, EventArgs e)
        {
            ShowWindow(typeof(RegistersTW));
        }

        private void ShowWindowStack(object aCommand, EventArgs e)
        {
            ShowWindow(typeof(StackTW));
        }

        private void ShowWindowConsole(object aCommand, EventArgs e)
        {
            ShowChannelWindow(typeof(ConsoleTW));
        }

        private void ShowWindowAll(object aCommand, EventArgs e)
        {
            ShowWindowAssembly(aCommand, e);
            ShowWindowRegisters(aCommand, e);
            ShowWindowStack(aCommand, e);
            //ShowWindowConsole(aCommand, e);
            // Dont show Internal Window, most Cosmos users wont use it.
        }

    }
}
