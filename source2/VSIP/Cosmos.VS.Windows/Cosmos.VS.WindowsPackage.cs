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

      Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
    }

    /// This function is called when the user clicks the menu item that shows the 
    /// tool window. See the Initialize method to see how the menu item is associated to 
    /// this function using the OleMenuCommandService service and the MenuCommand class.
    private void ShowWindowAssembly(object sender, EventArgs e) {
      if (ShowWindow(typeof(AssemblyTW))) {
        UpdateAssembly();
      }
    }

    private void ShowWindowInternal(object sender, EventArgs e) {
      if (ShowWindow(typeof(InternalTW))) {
        UpdateInternal();
      }
    }

    private void ShowWindowRegisters(object sender, EventArgs e) {
      if (ShowWindow(typeof(RegistersTW))) {
        UpdateRegisters();
      }
    }

    private void ShowWindowStack(object sender, EventArgs e) {
      if (ShowWindow(typeof(StackTW))) {
        UpdateStack();
        UpdateFrame();
      }
    }

    private bool ShowWindow(Type aWindowType) {
      // Get the instance number 0 of this tool window.
      // Our windows are single instance so this instance will be the only one.
      // The last flag is set to true so that if the tool window does not exists it will be created.
      var xWindow = FindToolWindow(aWindowType, 0, true);
      if ((xWindow == null) || (xWindow.Frame == null)) {
        throw new NotSupportedException(Resources.CanNotCreateWindow);
      }

      var xFrame = (IVsWindowFrame)xWindow.Frame;
      Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(xFrame.Show());

      return xFrame.IsVisible() == 0;
    }

    private void ShowWindowAll(object sender, EventArgs e) {
      ShowWindowAssembly(sender, e);
      ShowWindowRegisters(sender, e);
      ShowWindowStack(sender, e);
      ShowWindowInternal(sender, e);
    }

    // Overriden Package Implementation

    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initilaization code that rely on services provided by VisualStudio.
    protected override void Initialize() {
      Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
      base.Initialize();

      // Add our command handlers for menu (commands must exist in the .vsct file)
      OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      if (null != mcs) {
        // Create the command for the assembly tool window
        CommandID CosmosVSAssemblyToolWindowCommandID = new CommandID(GuidList.guidCosmos_VS_WindowsCmdSet, (int)PkgCmdIDList.cmdidCosmosAssembly);
        MenuCommand CosmosVSAssemblyToolWindowMenuCommand = new MenuCommand(ShowWindowAssembly, CosmosVSAssemblyToolWindowCommandID);
        mcs.AddCommand(CosmosVSAssemblyToolWindowMenuCommand);

        CommandID CosmosVSRegistersToolWindowCommandID = new CommandID(GuidList.guidCosmos_VS_WindowsCmdSet, (int)PkgCmdIDList.cmdidCosmosRegisters);
        MenuCommand CosmosVSRegistersToolWindowMenuCommand = new MenuCommand(ShowWindowRegisters, CosmosVSRegistersToolWindowCommandID);
        mcs.AddCommand(CosmosVSRegistersToolWindowMenuCommand);

        CommandID CosmosVSStackToolWindowCommandID = new CommandID(GuidList.guidCosmos_VS_WindowsCmdSet, (int)PkgCmdIDList.cmdidCosmosStack);
        MenuCommand CosmosVSStackToolWindowMenuCommand = new MenuCommand(ShowWindowStack, CosmosVSStackToolWindowCommandID);
        mcs.AddCommand(CosmosVSStackToolWindowMenuCommand);

        CommandID CosmosVSShowAllToolWindowsCommandID = new CommandID(GuidList.guidCosmos_VS_WindowsCmdSet, (int)PkgCmdIDList.cmdidCosmosShowAll);
        MenuCommand CosmosVSShowAllToolWindowMenuCommand = new MenuCommand(ShowWindowAll, CosmosVSShowAllToolWindowsCommandID);
        mcs.AddCommand(CosmosVSShowAllToolWindowMenuCommand);
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
            StackUC.mStackData = xMsg;
            UpdateStack();
            break;

          case DwMsg.Frame:
            StackUC.mFrameData = xMsg;
            UpdateFrame();
            break;

          case DwMsg.Registers:
            RegistersUC.mData = xMsg;
            UpdateRegisters();
            break;

          case DwMsg.Quit:
            //Close();
            break;

          case DwMsg.AssemblySource:
            AssemblyUC.mData = xMsg;
            UpdateAssembly();
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

    private void UpdateRegisters() {
      if ((RegistersUC.mData != null) && (RegistersUC.mData.Length > 0)) {
        if (RegistersTW.mUC != null) {
          RegistersTW.mUC.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
            RegistersTW.mUC.Update(RegistersUC.mData);
          });
        }
      }
    }

    private void UpdateStack() {
      if (StackTW.mUC != null) {
        if ((StackUC.mStackData != null) && (StackUC.mStackData.Length > 0)) {
          StackTW.mUC.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
            StackTW.mUC.UpdateStack(StackUC.mStackData);
          });
        }
      }
    }

    private void UpdateFrame() {
      if (StackTW.mUC != null) {
        if ((StackUC.mFrameData != null) && (StackUC.mFrameData.Length > 0)) {
          StackTW.mUC.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
            StackTW.mUC.UpdateFrame(StackUC.mFrameData);
          });
        }
      }
    }

    private void UpdateAssembly() {
      if ((AssemblyUC.mData != null) && (AssemblyUC.mData.Length > 0)) {
        if (AssemblyTW.mUC != null) {
          AssemblyTW.mUC.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
            AssemblyTW.mUC.Update(AssemblyUC.mData);
          });
        }
      }
    }

    private void UpdateInternal() {
    }
  }
}
