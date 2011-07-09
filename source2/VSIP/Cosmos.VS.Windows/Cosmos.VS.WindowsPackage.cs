using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Threading;
using Cosmos.Compiler.Debug;
using System.Windows.Threading;
using Cosmos.VS.Debug;
using System.Collections.Generic;

namespace Cosmos.Cosmos_VS_Windows
{
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// 
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]

    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]

    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]

    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(AssemblyTW))]
    [ProvideToolWindow(typeof(RegistersTW))]
    [ProvideToolWindow(typeof(StackTW))]

    [Guid(GuidList.guidCosmos_VS_WindowsPkgString)]
    public sealed class Cosmos_VS_WindowsPackage : Package
    {
        Queue<byte> mCommand;
        Queue<byte[]> mMessage;
        System.Timers.Timer mTimer = new System.Timers.Timer(250);

        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        public Cosmos_VS_WindowsPackage()
        {
            mCommand = new Queue<byte>();
            mMessage = new Queue<byte[]>();

            // There are a lot of threading issues in VSIP, and the WPF dispatchers do not work
            // So instead we use a stack and a timer to poll it for data.
            mTimer.AutoReset = true;
            mTimer.Elapsed += new System.Timers.ElapsedEventHandler(ProcessMessage);
            mTimer.Start();

            PipeThread.DataPacketReceived += new Action<byte, byte[]>(PipeThread_DataPacketReceived);
            var xServerThread = new Thread(PipeThread.ThreadStartServer);
            xServerThread.Start();

            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        private void ShowCosmosVSAssemblyToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(AssemblyTW), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private void ShowCosmosVSRegistersToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(RegistersTW), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private void ShowCosmosVSStackToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(StackTW), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
      
        // Overriden Package Implementation

        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the assembly tool window
                CommandID CosmosVSAssemblyToolWindowCommandID = new CommandID(GuidList.guidCosmos_VS_WindowsCmdSet, (int)PkgCmdIDList.cmdidCosmosAssembly);
                MenuCommand CosmosVSAssemblyToolWindowMenuCommand = new MenuCommand(ShowCosmosVSAssemblyToolWindow, CosmosVSAssemblyToolWindowCommandID);
                mcs.AddCommand(CosmosVSAssemblyToolWindowMenuCommand);

                // Create the command for the registers tool window
                CommandID CosmosVSRegistersToolWindowCommandID = new CommandID(GuidList.guidCosmos_VS_WindowsCmdSet, (int)PkgCmdIDList.cmdidCosmosRegisters);
                MenuCommand CosmosVSRegistersToolWindowMenuCommand = new MenuCommand(ShowCosmosVSRegistersToolWindow, CosmosVSRegistersToolWindowCommandID);
                mcs.AddCommand(CosmosVSRegistersToolWindowMenuCommand);

                // Create the command for the stack tool window
                CommandID CosmosVSStackToolWindowCommandID = new CommandID(GuidList.guidCosmos_VS_WindowsCmdSet, (int)PkgCmdIDList.cmdidCosmosStack);
                MenuCommand CosmosVSStackToolWindowMenuCommand = new MenuCommand(ShowCosmosVSStackToolWindow, CosmosVSStackToolWindowCommandID);
                mcs.AddCommand(CosmosVSStackToolWindowMenuCommand);
            }
        }

        void ProcessMessage(object sender, EventArgs e)
        {
            byte xCmd = 0x0;
            byte[] xMsg = {0x0};
            if ((mCommand.Count > 0) && (mMessage.Count > 0))
            {
                xCmd = mCommand.Dequeue();
                xMsg = mMessage.Dequeue();
            }
            switch (xCmd)
            {
                case DwMsgType.Noop:
                    break;

                case DwMsgType.Stack:
                    if (StackTW.mUC != null) {
                      StackTW.mUC.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
                        StackTW.mUC.UpdateStack(xMsg);
                      });
                    }
                    break;

                case DwMsgType.Frame:
                    if (StackTW.mUC != null) {
                      StackTW.mUC.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
                        StackTW.mUC.UpdateFrame(xMsg);
                      });
                    }
                    break;

                case DwMsgType.Registers:
                    if (RegistersTW.mUC != null) {
                      RegistersTW.mUC.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
                        RegistersTW.mUC.Update(xMsg);
                      });
                    }
                    break;

                case DwMsgType.Quit:
                    //Close();
                    break;

                case DwMsgType.AssemblySource:
                    if (AssemblyTW.mUC != null) {
                      AssemblyTW.mUC.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
                        AssemblyTW.mUC.Update(xMsg);
                      });
                    }
                    break;
            }
        }

        void PipeThread_DataPacketReceived(byte aCmd, byte[] aMsg)
        {
            mCommand.Enqueue(aCmd);
            mMessage.Enqueue(aMsg);
        }
    }
}
