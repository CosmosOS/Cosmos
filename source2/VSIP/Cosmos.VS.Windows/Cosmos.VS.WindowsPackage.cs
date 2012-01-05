using System;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Threading;
using Cosmos.Debug.Consts;
using System.Windows.Threading;
using System.Collections.Generic;

namespace Cosmos.VS.Windows {
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
  [ProvideToolWindow(typeof(InternalTW))]

  [Guid(GuidList.guidCosmos_VS_WindowsPkgString)]
  public sealed class Cosmos_VS_WindowsPackage : Package {
    Queue<byte> mCommand;
    Queue<byte[]> mMessage;
    System.Timers.Timer mTimer = new System.Timers.Timer(100);
    Cosmos.Debug.Common.PipeServer mPipeDown;

    /// Default constructor of the package.
    /// Inside this method you can place any initialization code that does not require 
    /// any Visual Studio service because at this point the package object is created but 
    /// not sited yet inside Visual Studio environment. The place to do all the other 
    /// initialization is the Initialize method.
    public Cosmos_VS_WindowsPackage() {
      mCommand = new Queue<byte>();
      mMessage = new Queue<byte[]>();

      // There are a lot of threading issues in VSIP, and the WPF dispatchers do not work.
      // So instead we use a stack and a timer to poll it for data.
      mTimer.AutoReset = true;
      mTimer.Elapsed += new System.Timers.ElapsedEventHandler(ProcessMessage);
      mTimer.Start();

      mPipeDown = new Cosmos.Debug.Common.PipeServer(Cosmos.Debug.Consts.Pipes.DownName);
      mPipeDown.DataPacketReceived += new Action<byte, byte[]>(PipeThread_DataPacketReceived);
      mPipeDown.Start();
    }

    protected ToolWindowPane2 FindWindow(Type aWindowType) {
      // Get the instance number 0 of this tool window.
      // Our windows are single instance so this instance will be the only one.
      // The last flag is set to true so that if the tool window does not exists it will be created.
      var xWindow = FindToolWindow(aWindowType, 0, true);
      if ((xWindow == null) || (xWindow.Frame == null)) {
        throw new NotSupportedException(Resources.CanNotCreateWindow);
      }
      return xWindow as ToolWindowPane2;
    }

    protected void ShowWindow(Type aWindowType) {
      var xWindow = FindWindow(aWindowType);
      var xFrame = (IVsWindowFrame)xWindow.Frame;
      Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(xFrame.Show());
      //return xFrame.IsVisible() == 0;
    }

    protected void UpdateWindow(Type aWindowType, string aTag, byte[] aData) {
      var xWindow = FindWindow(aWindowType);
      xWindow.UserControl.Update(aTag, aData);
    }

    // This function is called when the user clicks the menu item that shows the 
    // tool window. See the Initialize method to see how the menu item is associated to 
    // this function using the OleMenuCommandService service and the MenuCommand class.
    private void ShowWindowAssembly(object sender, EventArgs e) {
      ShowWindow(typeof(AssemblyTW));
    }

    private void ShowWindowInternal(object sender, EventArgs e) {
      ShowWindow(typeof(InternalTW));
    }

    private void ShowWindowRegisters(object sender, EventArgs e) {
      ShowWindow(typeof(RegistersTW));
    }

    private void ShowWindowStack(object sender, EventArgs e) {
      ShowWindow(typeof(StackTW));
    }

    private void ShowWindowAll(object sender, EventArgs e) {
      ShowWindowAssembly(sender, e);
      ShowWindowRegisters(sender, e);
      ShowWindowStack(sender, e);
      ShowWindowInternal(sender, e);
    }

    protected void AddCommand(OleMenuCommandService aMcs, uint aCmdID, EventHandler aHandler) {
      // Create the command for the assembly tool window
      var xCmdID = new CommandID(GuidList.guidCosmos_VS_WindowsCmdSet, (int)aCmdID);
      var xMenuCmd = new MenuCommand(aHandler, xCmdID);
      aMcs.AddCommand(xMenuCmd);
    }

    // Initialization of the package; this method is called right after the package is sited, so this is the place
    // where you can put all the initilaization code that rely on services provided by VisualStudio.
    protected override void Initialize() {
      base.Initialize();

      // Add our command handlers for menu (commands must exist in the .vsct file)
      var xMcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      if (xMcs != null) {
        AddCommand(xMcs, PkgCmdIDList.cmdidCosmosAssembly, ShowWindowAssembly);
        AddCommand(xMcs, PkgCmdIDList.cmdidCosmosRegisters, ShowWindowRegisters);
        AddCommand(xMcs, PkgCmdIDList.cmdidCosmosStack, ShowWindowStack);
        AddCommand(xMcs, PkgCmdIDList.cmdidCosmosShowAll, ShowWindowAll);
      }
    }

    void ProcessMessage(object sender, EventArgs e) {
      byte xCmd;
      byte[] xMsg;
      while (true) {
        lock (mCommand) {
          if (mCommand.Count == 0) {
            break;
          }
          xCmd = mCommand.Dequeue();
          xMsg = mMessage.Dequeue();
        }

        switch (xCmd) {
          case DwMsg.Noop:
            break;

          case DwMsg.Stack:
            UpdateWindow(typeof(StackTW), "STACK", xMsg);
            break;

          case DwMsg.Frame:
            UpdateWindow(typeof(StackTW), "FRAME", xMsg);
            break;

          case DwMsg.Registers:
            UpdateWindow(typeof(RegistersTW), null, xMsg);
            break;

          case DwMsg.Quit:
            //Close();
            break;

          case DwMsg.AssemblySource:
            UpdateWindow(typeof(AssemblyTW), null, xMsg);
            break;

          case DwMsg.Pong:
            System.Windows.MessageBox.Show("Pong!");
            break;
        }
      }
    }

    void PipeThread_DataPacketReceived(byte aCmd, byte[] aMsg) {
      lock (mCommand) {
        mCommand.Enqueue(aCmd);
        mMessage.Enqueue(aMsg);
      }
    }

  }
}
