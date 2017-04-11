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

        private readonly CosmosVSWindowsPackage package;

        private CosmosMenuCmdSet(CosmosVSWindowsPackage package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            AddCommand(CosmosAssemblyCmdID);
            AddCommand(CosmosRegistersCmdID);
            AddCommand(CosmosStackCmdID);
            AddCommand(CosmosInternalCmdID);
            AddCommand(CosmosShowAllCmdID);
        }

        private void AddCommand(int id)
        {
            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(Guids.AsmToolbarCmdSetGuid, id);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static CosmosMenuCmdSet Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider => package;

        public static void Initialize(CosmosVSWindowsPackage package)
        {
            Instance = new CosmosMenuCmdSet(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "AssemblyCommand";

            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
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
            ShowWindowConsole(aCommand, e);
            // Dont show Internal Window, most Cosmos users wont use it.
        }

    }
}
