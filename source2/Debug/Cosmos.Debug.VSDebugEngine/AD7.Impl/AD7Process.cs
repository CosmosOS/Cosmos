using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Cosmos.Build.Common;
using Cosmos.Debug.Common;
using Cosmos.Debug.Consts;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.Win32;

namespace Cosmos.Debug.VSDebugEngine {
  public class AD7Process : IDebugProcess2 {
    public Guid mID = Guid.NewGuid();
    protected Process mProcess;
    protected ProcessStartInfo mProcessStartInfo;
    protected EngineCallback mCallback;
    public AD7Thread mThread;
    protected AD7Engine mEngine;
    public ReverseSourceInfos mReverseSourceMappings;
    public SourceInfos mSourceMappings;
    public uint? mCurrentAddress = null;
    protected readonly NameValueCollection mDebugInfo;
    protected TargetHost mTargetHost;
    protected VMwareFlavor mVMWareFlavor = VMwareFlavor.Player;
    internal DebugInfo mDebugInfoDb;
    internal List<KeyValuePair<uint, string>> mAddressLabelMappings;
    internal IDictionary<string, uint> mLabelAddressMappings;
    private int mProcessExitEventSent = 0;

    // Connection to target environment. Usually serial but is
    // abstracted to allow other transports (ethernet, etc)
    public DebugConnector mDbgConnector;
    //
    // These are static because we need them persistent between debug
    // sessions to avoid reconnection issues. But they are not created
    // until the debug session is ready the first time so that we know
    // the debug window pipes are already reayd.
    //
    // Pipe to communicate with Cosmos.VS.Windows
    static private Cosmos.Debug.Common.PipeClient mDebugDownPipe = null;
    // Pipe to receive messages from Cosmos.VS.Windows
    static private Cosmos.Debug.Common.PipeServer mDebugUpPipe = null;

    protected void DeleteFiles(string aPath, string aPattern) {
      var xFiles = Directory.GetFiles(aPath, aPattern);
      foreach (var xFile in xFiles) {
        File.Delete(xFile);
      }
    }

    protected void CleanupVMWare(string aPath, string aVmxFile) {
      try {
        // Delete old Debug.vmx and other files that might be left over from previous run
        // Especially important with newer versions of VMWare player which defaults to suspend
        // when the close button is used.
        File.Delete(Path.Combine(aPath, aVmxFile));
        File.Delete(Path.Combine(aPath, Path.ChangeExtension(aVmxFile, ".nvram")));
        // Delete the auto snapshots that latest vmware players create as default
        // It creates them with suffixes though, so we need to wild card find them
        DeleteFiles(aPath, "*.vmxf");
        DeleteFiles(aPath, "*.vmss");
        DeleteFiles(aPath, "*.vmsd");
        DeleteFiles(aPath, "*.vmem");
        // Delete log files so that logged data is only from last boot
        File.Delete(Path.Combine(aPath, "vmware.log"));
        File.Delete(Path.Combine(aPath, "vmware-0.log"));
        File.Delete(Path.Combine(aPath, "vmware-1.log"));
        File.Delete(Path.Combine(aPath, "vmware-2.log"));
      } catch (Exception e) {
        // Ignore errors, users can stop VS while VMWare is still running and files
        // will be locked.
      }
    }

    protected const string mDebugVmxFile = "Debug.vmx";
    protected void LaunchVMWare(bool aGDB) {
      OutputText("Preparing VMWare.");

      string xPath = Path.Combine(PathUtilities.GetBuildDir(), @"VMWare\Workstation") + @"\";
      CleanupVMWare(xPath, mDebugVmxFile);

      // VMWare doesn't like to boot a read only VMX.
      // We also need to make changes based on project / debug settings.
      // Finally we do not want to create VCS checkins based on local user changes.
      // Because of this we use Cosmos.vmx as a template and output a Debug.vmx on
      // every run.
      using (var xSrc = new StreamReader(xPath + "Cosmos.vmx")) {
        try {
          // Write out Debug.vmx
          using (var xDest = new StreamWriter(Path.Combine(xPath, mDebugVmxFile))) {
            string xLine;
            while ((xLine = xSrc.ReadLine()) != null) {
              var xParts = xLine.Split('=');
              if (xParts.Length == 2) {
                string xName = xParts[0].Trim();
                string xValue = xParts[1].Trim();

                // We delete uuid entries so VMWare doenst ask the user "Did you move or copy" the file
                if ((xName == "uuid.location") || (xName == "uuid.bios")) {
                  xValue = null;
                } else if (xName == "ide1:0.fileName") {
                  xValue = "\"" + mDebugInfo["ISOFile"] + "\"";
                } else if (xName == "nvram") {
                  xValue = "\"Debug.nvram\"";
                }

                if (xValue != null) {
                  xDest.WriteLine(xName + " = " + xValue);
                }
              }
            }
            if (aGDB) {
              xDest.WriteLine();
              xDest.WriteLine("debugStub.listen.guest32 = \"TRUE\"");
              xDest.WriteLine("debugStub.hideBreakpoints = \"TRUE\"");
              xDest.WriteLine("monitor.debugOnStartGuest32 = \"TRUE\"");
              xDest.WriteLine("debugStub.listen.guest32.remote = \"TRUE\"");
            }
          }
        } catch (IOException e) {
          if (e.Message.Contains(mDebugVmxFile)) {
            throw new Exception("The Vmware image " + mDebugVmxFile + " is still in use! Please exit current Vmware session with Cosmos and try again!", e);
          }
          throw e;
        }
      }

      string xVmwarePath;
      switch (mVMWareFlavor) {
        case VMwareFlavor.Workstation:
          xVmwarePath = GetVMWareWorkstationPath();
          if (String.IsNullOrEmpty(xVmwarePath)) {
            goto case VMwareFlavor.Player;
          }
          mProcessStartInfo.Arguments = "false \"" + xVmwarePath + "\" -x -q \"" + xPath + "Debug.vmx\"";
          break;
        case VMwareFlavor.Player:
          xVmwarePath = GetVMWarePlayerPath();
          mProcessStartInfo.Arguments = "false \"" + xVmwarePath + "\" \"" + xPath + "Debug.vmx\"";
          break;
        default:
          throw new NotImplementedException("VMWare flavor '" + mVMWareFlavor.ToString() + "' not implemented!");
      }
      //mProcessStartInfo.Arguments = "true \"" + xPath + "Debug.vmx\" -x -q";
      // -x: Auto power on VM. Must be small x, big X means something else.
      // -q: Close VMWare when VM is powered off.
      // Options must come beore the vmx, and cannot use shellexecute

      if (String.IsNullOrEmpty(xVmwarePath) || !File.Exists(xVmwarePath)) {
        OutputText("VMWare not found.");
        MessageBox.Show("VWMare is not installed, probably going to crash now!", "Cosmos DebugEngine", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private static string GetVMWareWorkstationPath() {
      using (var xRegKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VMware, Inc.\VMware Workstation", false)) {
        if (xRegKey == null) {
          return String.Empty;
        }
        return Path.Combine(((string)xRegKey.GetValue("InstallPath")), "vmware.exe");
      }
    }

    private static string GetVMWarePlayerPath() {
      using (var xRegKey = Registry.LocalMachine.OpenSubKey(@"Software\VMware, Inc.\VMware Player", false)) {
        if (xRegKey == null) {
          return String.Empty;
        }
        return Path.Combine(((string)xRegKey.GetValue("InstallPath")), "vmplayer.exe");
      }
    }

    public string mISO;
    public string mProjectFile;

    protected void DbgCmdRegisters(byte[] aData) {
      mDebugDownPipe.SendCommand(VsipUi.Registers, aData);
    }

    protected void DbgCmdFrame(byte[] aData) {
      mDebugDownPipe.SendCommand(VsipUi.Frame, aData);
    }

    protected void DbgCmdPong(byte[] aData) {
      mDebugDownPipe.SendCommand(VsipUi.PongDebugStub, aData);
    }

    protected void DbgCmdStack(byte[] aData) {
      mDebugDownPipe.SendCommand(VsipUi.Stack, aData);
    }

    void mDebugUpPipe_DataPacketReceived(byte aCmd, byte[] aData) {
      switch (aCmd) {
        case UiVsip.Noop:
          // do nothing
          break;

        case UiVsip.PingVSIP:
          mDebugDownPipe.SendCommand(VsipUi.PongVSIP);
          break;

        case UiVsip.PingDebugStub:
          mDbgConnector.Ping();
          break;

        case UiVsip.SetAsmBreak:
          //mDbgConnector.Ping();
          string xLabel = Encoding.UTF8.GetString(aData);
          uint xAddress = mLabelAddressMappings[xLabel];
          mDebugDownPipe.SendCommand(VsipUi.OutputPane, xAddress.ToString());
          break;

        default:
          throw new Exception(String.Format("Command value '{0}' not supported in method AD7Process.mDebugUpPipe_DataPacketReceived.", aCmd));
      }
    }

    protected void DebugConnectorConnected() {
      OutputText("Connected to DebugStub.");
    }

    internal AD7Process(NameValueCollection aDebugInfo, EngineCallback aCallback, AD7Engine aEngine, IDebugPort2 aPort) {
      System.Diagnostics.Debug.WriteLine("In AD7Process..ctor");
      mCallback = aCallback;
      mDebugInfo = aDebugInfo;

      if (mDebugDownPipe == null) {
        mDebugDownPipe = new Cosmos.Debug.Common.PipeClient(Cosmos.Debug.Consts.Pipes.DownName);

        mDebugUpPipe = new Cosmos.Debug.Common.PipeServer(Cosmos.Debug.Consts.Pipes.UpName);
        mDebugUpPipe.DataPacketReceived += new Action<byte, byte[]>(mDebugUpPipe_DataPacketReceived);
        mDebugUpPipe.Start();
      }
      // Must be after mDebugDownPipe is initialized
      OutputClear();
      OutputText("Debugger process initialized.");

      mISO = mDebugInfo["ISOFile"];
      OutputText("Using ISO file " + mISO + ".");
      mProjectFile = mDebugInfo["ProjectFile"];
      //
      var xGDBDebugStub = false;
      Boolean.TryParse(mDebugInfo["EnableGDB"], out xGDBDebugStub);
      OutputText("GDB " + (xGDBDebugStub ? "Enabled" : "Disabled") + ".");
      //
      var xGDBClient = false;
      Boolean.TryParse(mDebugInfo["StartCosmosGDB"], out xGDBClient);

      mProcessStartInfo = new ProcessStartInfo(Path.Combine(PathUtilities.GetVSIPDir(), "Cosmos.Debug.HostProcess.exe"));
      if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["BuildTarget"], "VMWare")) {
        mTargetHost = TargetHost.VMWare;
        if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["VMWareFlavor"], "Player")) {
          mVMWareFlavor = VMwareFlavor.Player;
        } else if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["VMWareFlavor"], "Workstation")) {
          mVMWareFlavor = VMwareFlavor.Workstation;
        } else {
          throw new Exception("VMWare Flavor '" + mDebugInfo["VMWareFlavor"] + "' not implemented!");
        }
        LaunchVMWare(xGDBDebugStub);
      } else {
        throw new Exception("Invalid BuildTarget value: '" + mDebugInfo["BuildTarget"] + "'!");
      }

      mProcessStartInfo.UseShellExecute = false;
      mProcessStartInfo.RedirectStandardInput = true;
      mProcessStartInfo.RedirectStandardError = true;
      mProcessStartInfo.RedirectStandardOutput = true;
      mProcessStartInfo.CreateNoWindow = true;

      string xCpdbPath = Path.ChangeExtension(mISO, "cpdb");
      if (!File.Exists(xCpdbPath)) {
        throw new Exception("Debug data file " + xCpdbPath + " not found! Could be a omitted build process of Cosmos project so that not created.");
      }

      mDebugInfoDb = new DebugInfo();
      mDebugInfoDb.OpenCPDB(xCpdbPath);
      mDebugInfoDb.ReadLabels(out mAddressLabelMappings, out mLabelAddressMappings);
      if (mAddressLabelMappings.Count == 0) {
        throw new Exception("Debug data not found: LabelByAddressMapping");
      }

      mSourceMappings = Cosmos.Debug.Common.SourceInfo.GetSourceInfo(mAddressLabelMappings, mLabelAddressMappings, mDebugInfoDb);
      if (mSourceMappings.Count == 0) {
        throw new Exception("Debug data not found: SourceMappings");
      }
      mReverseSourceMappings = new ReverseSourceInfos(mSourceMappings);

      mDbgConnector = null;
      if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["BuildTarget"], "vmware")) {
        OutputText("Starting serial debug listener.");
        mDbgConnector = new Cosmos.Debug.Common.DebugConnectorPipeServer();
        mDbgConnector.Connected = DebugConnectorConnected;
      }
      if (mDbgConnector == null) {
        throw new Exception("BuildTarget value not valid: '" + mDebugInfo["BuildTarget"] + "'!");
      }

      aEngine.BPMgr.SetDebugConnector(mDbgConnector);
      mDbgConnector.CmdTrace += new Action<byte, uint>(DbgCmdTrace);
      mDbgConnector.CmdText += new Action<string>(DbgCmdText);
      mDbgConnector.CmdStarted += new Action(DbgCmdStarted);
      mDbgConnector.OnDebugMsg += new Action<string>(DebugMsg);
      mDbgConnector.ConnectionLost += new Action<Exception>(DbgConnector_ConnectionLost);
      mDbgConnector.CmdRegisters += new Action<byte[]>(DbgCmdRegisters);
      mDbgConnector.CmdFrame += new Action<byte[]>(DbgCmdFrame);
      mDbgConnector.CmdStack += new Action<byte[]>(DbgCmdStack);
      mDbgConnector.CmdPong += new Action<byte[]>(DbgCmdPong);

      System.Threading.Thread.Sleep(250);
      System.Diagnostics.Debug.WriteLine(String.Format("Launching process: \"{0}\" {1}", mProcessStartInfo.FileName, mProcessStartInfo.Arguments).Trim());
      OutputText("Starting VMWare.");
      mProcess = Process.Start(mProcessStartInfo);

      mProcess.EnableRaisingEvents = true;
      mProcess.Exited += new EventHandler(mProcess_Exited);

      // Sleep 250 and see if it exited too quickly. Why do we do this? We have .Exited hooked. Is this in case it happens between start and hook?
      // if so, why not hook before start? 
      // MtW: we do this for the potential situation where it might exit before the Exited event is hooked. Iirc i had this situation before..
      System.Threading.Thread.Sleep(250);
      if (mProcess.HasExited) {
        Trace.WriteLine("Error while running: " + mProcess.StandardError.ReadToEnd());
        Trace.WriteLine(mProcess.StandardOutput.ReadToEnd());
        Trace.WriteLine("ExitCode: " + mProcess.ExitCode);
        throw new Exception("Error while starting application");
      }

      mEngine = aEngine;
      mThread = new AD7Thread(aEngine, this);
      mCallback.OnThreadStart(mThread);
      mPort = aPort;

      // Launch GDB Client
      if (xGDBDebugStub && xGDBClient) {
        OutputText("Launching GDB client.");
        if (File.Exists(Cosmos.Build.Common.CosmosPaths.GDBClientExe)) {
          var xPSInfo = new ProcessStartInfo(Cosmos.Build.Common.CosmosPaths.GDBClientExe);
          xPSInfo.Arguments = "\"" + Path.ChangeExtension(mProjectFile, ".cgdb") + "\"" + @" /Connect";
          xPSInfo.UseShellExecute = false;
          xPSInfo.RedirectStandardInput = false;
          xPSInfo.RedirectStandardError = false;
          xPSInfo.RedirectStandardOutput = false;
          xPSInfo.CreateNoWindow = false;
          Process.Start(xPSInfo);
        } else {
          MessageBox.Show(string.Format(
              "The GDB-Client could not be found at \"{0}\". Please deactivate it under \"Properties/Debug/Enable GDB\"",
              Cosmos.Build.Common.CosmosPaths.GDBClientExe), "GDB-Client", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
        }
      }
    }

    private void DbgConnector_ConnectionLost(Exception e) {
      if (Interlocked.CompareExchange(ref mProcessExitEventSent, 1, 0) == 1) {
        return;
      }
      if (mDbgConnector != null) {
        mEngine.Callback.OnProcessExit(0);
      }
    }

    // Shows a message in the output window of VS. Needs special treatment, 
    // because normally VS only shows msgs from debugged process, not internal
    // stuff like us.
    public void DebugMsg(string aMsg) {
      mCallback.OnOutputString(aMsg + "\n");
    }

    protected void DbgCmdStarted() {
      OutputText("DebugStub handshake completed.");
      DebugMsg("RmtDbg: Started");

      // OK, now debugger is ready. Send it a list of breakpoints that were set before
      // program run.
      foreach (var xBP in mEngine.BPMgr.mPendingBPs) {
        foreach (var xBBP in xBP.mBoundBPs) {
          mDbgConnector.SetBreakpoint(xBBP.RemoteID, xBBP.mAddress);
        }
      }
      mDbgConnector.SendCommand(VsipDs.BatchEnd);
    }

    void DbgCmdText(string obj) {
      mCallback.OnOutputStringUser(obj + "\r\n");
    }

    internal AD7Thread Thread {
      get {
        return mThread;
      }
    }

    void DbgCmdTrace(byte arg1, uint arg2) {
      DebugMsg("DbgCmdTrace");
      switch (arg1) {
        case DsVsip.BreakPoint: {
            // When doing a CALL, the return address is pushed, but that's the address of the next instruction, after CALL. call is 5 bytes (for now?)
            // Dont need to correct the address, becuase DebugStub does it for us.
            var xActualAddress = arg2;
            DebugMsg("BP hit @ " + xActualAddress.ToString("X8").ToUpper());

            var xActionPoints = new List<object>();
            var xBoundBreakpoints = new List<IDebugBoundBreakpoint2>();

            // Search the BPs and find the ones that match our address
            foreach (var xBP in mEngine.BPMgr.mPendingBPs) {
              foreach (var xBBP in xBP.mBoundBPs) {
                if (xBBP.mAddress == xActualAddress) {
                  xBoundBreakpoints.Add(xBBP);
                }
              }
            }

            mCurrentAddress = xActualAddress;
            // if no matching breakpoint, its either a stepping operation, or a code based break
            if (xBoundBreakpoints.Count == 0) {
              // Is it a result of stepping operation?
              if (mEngine.AfterBreak) {
                RequestFullDebugStubUpdate();
                mCallback.OnStepComplete();
              } else {
                RequestFullDebugStubUpdate();
                // Code based break. Tell VS to break.
                mCallback.OnBreakpoint(mThread, new ReadOnlyCollection<IDebugBoundBreakpoint2>(xBoundBreakpoints));
              }
            } else {
              // Found a bound breakpoint
              RequestFullDebugStubUpdate();
              mCallback.OnBreakpoint(mThread, new ReadOnlyCollection<IDebugBoundBreakpoint2>(xBoundBreakpoints));
              mEngine.AfterBreak = true;
            }
            break;
          }

        default: {
            DebugMsg("TraceReceived: " + arg1);
            break;
          }
      }
    }

    protected void RequestFullDebugStubUpdate() {
      // We catch and resend data rather than using a second serial port because
      // while this would work fine in a VM, it would require 2 serial ports
      // when real hardware is used.
      SendAssembly();
      mDbgConnector.SendRegisters();
      mDbgConnector.SendFrame();
      mDbgConnector.SendStack();
    }

    public int Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach) {
      Trace.WriteLine(new StackTrace(false).GetFrame(0).GetMethod().GetFullName());
      throw new NotImplementedException();
    }

    public int CanDetach() {
      throw new NotImplementedException();
    }

    public int CauseBreak() {
      throw new NotImplementedException();
    }

    public int Detach() {
      throw new NotImplementedException();
    }

    public int EnumPrograms(out IEnumDebugPrograms2 ppEnum) {
      throw new NotImplementedException();
    }

    public int EnumThreads(out IEnumDebugThreads2 ppEnum) {
      var xEnum = new AD7ThreadEnum(new IDebugThread2[] { mThread });
      ppEnum = xEnum;
      return VSConstants.S_OK;
    }

    public int GetAttachedSessionName(out string pbstrSessionName) {
      throw new NotImplementedException();
    }

    public int GetInfo(enum_PROCESS_INFO_FIELDS Fields, PROCESS_INFO[] pProcessInfo) {
      throw new NotImplementedException();
    }

    public int GetName(enum_GETNAME_TYPE gnType, out string pbstrName) {
      throw new NotImplementedException();
    }

    public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId) {
      Trace.WriteLine(new StackTrace(false).GetFrame(0).GetMethod().GetFullName());
      pProcessId[0].dwProcessId = (uint)mProcess.Id;
      pProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM;
      return VSConstants.S_OK;
    }

    private IDebugPort2 mPort = null;

    public int GetPort(out IDebugPort2 ppPort) {
      if (mPort == null) {
        throw new Exception("Error");
      }
      ppPort = mPort;
      return VSConstants.S_OK;
    }

    public int GetProcessId(out Guid pguidProcessId) {
      Trace.WriteLine(new StackTrace(false).GetFrame(0).GetMethod().GetFullName());
      pguidProcessId = mID;
      return VSConstants.S_OK;
    }

    public int GetServer(out IDebugCoreServer2 ppServer) {
      throw new NotImplementedException();
    }

    public int Terminate() {
      OutputText("Debugger terminating.");
      if (Interlocked.CompareExchange(ref mProcessExitEventSent, 1, 0) == 0) {
        mProcess.Kill();
        mProcess.Exited -= mProcess_Exited;
        if (mDbgConnector != null) {
          mDbgConnector.Dispose();
          mDbgConnector = null;
        }
        if (mDebugInfoDb != null) {
          mDebugInfoDb.Dispose();
          mDebugInfoDb = null;
        }
      }

      string xPath = Path.Combine(PathUtilities.GetBuildDir(), @"VMWare\Workstation") + @"\";
      CleanupVMWare(xPath, mDebugVmxFile);

      OutputText("Debugger terminated.");
      return VSConstants.S_OK;
    }

    internal void ResumeFromLaunch() {
      // This unpauses our debug host
      // We do this because VS requires a start, and then a resume after. So we have debughost which is a stub
      // that allows VS to "see" that. Here we resume it.
      mProcess.StandardInput.WriteLine();
    }

    void mProcess_Exited(object sender, EventArgs e) {
      Trace.WriteLine("Error while running: " + mProcess.StandardError.ReadToEnd());
      Trace.WriteLine(mProcess.StandardOutput.ReadToEnd());
      Trace.WriteLine(String.Format("Process Exit Code: {0}", mProcess.ExitCode));
      //AD7ThreadDestroyEvent.Send(mEngine, mThread, (uint)mProcess.ExitCode);
      //mCallback.OnProgramDestroy((uint)mProcess.ExitCode);
      mDbgConnector.Dispose();
      mDbgConnector = null;
      if (mDebugInfoDb != null) {
        mDebugInfoDb.Dispose();
        mDebugInfoDb = null;
      }
      if (Interlocked.CompareExchange(ref mProcessExitEventSent, 1, 0) == 0) {
        mCallback.OnProcessExit((uint)mProcess.ExitCode);
      }
    }

    internal void Continue() { // F5
      mCurrentAddress = null;
      mDbgConnector.SendCommand(VsipDs.Continue);
    }

    internal void Step(enum_STEPKIND aKind) {
      if (aKind == enum_STEPKIND.STEP_INTO) { // F11
        mDbgConnector.SendCommand(VsipDs.StepInto);

      } else if (aKind == enum_STEPKIND.STEP_OVER) { // F10
        mDbgConnector.SendCommand(VsipDs.StepOver);

      } else if (aKind == enum_STEPKIND.STEP_OUT) { // Shift-F11
        mDbgConnector.SendCommand(VsipDs.StepOut);

      } else if (aKind == enum_STEPKIND.STEP_BACKWARDS) {
        // STEP_BACKWARDS - Supported at all by VS?
        //
        // Possibly, by dragging the execution location up
        // or down through the source code? -Orvid
        MessageBox.Show("Step backwards is not supported.");
        mCallback.OnStepComplete(); // Have to call this otherwise VS gets "stuck"

      } else {
        MessageBox.Show("Unknown step type requested.");
        mCallback.OnStepComplete(); // Have to call this otherwise VS gets "stuck"
      }
    }

    public void SendAssembly() {
      // Because of Asm breakpoints the address we have might be in the middle of a C# line.
      // So we find the closest address to ours that is less or equal to ours.
      var xQry = from x in mSourceMappings
                 where x.Key <= (uint)mCurrentAddress
                 orderby x.Key descending
                 select x.Value;
      var xValue = xQry.FirstOrDefault();
      if (xValue == null) {
        return;
      }

      // Create list of asm labels that belong to this line of C#.
      //var xValue = mSourceMappings[(uint)mCurrentAddress];
      var xMappings = from x in mSourceMappings
                   where x.Value.SourceFile == xValue.SourceFile
                     && x.Value.Line == xValue.Line
                     && x.Value.Column == xValue.Column
                   select x.Key;
      var xLabels = new List<string>();
      foreach (uint xAddr in xMappings) {
        var xLabelsForAddr = from x in mAddressLabelMappings
                      where x.Key == xAddr
                      select x.Value;
        foreach (string xLabel in xLabelsForAddr) {
          xLabels.Add(xLabel + ":");
        }
      }

      // Get assembly source
      var xCode = AsmSource.GetSourceForLabels(Path.ChangeExtension(mISO, ".asm"), xLabels);

      // Get label for current address
      var xCurrentLabel = (from x in mAddressLabelMappings
                           where x.Key == (uint)mCurrentAddress
                           select x.Value).FirstOrDefault();
      if (xCurrentLabel == null) {
        return;
      }
      // Insert it to the first line of our data stream
      xCode.Insert(0, xCurrentLabel + "\r\n");                      

      mDebugDownPipe.SendCommand(VsipUi.AssemblySource, Encoding.UTF8.GetBytes(xCode.ToString()));
    }

    //TODO: At some point this will probably need to be exposed for access outside of AD7Process
    protected void OutputText(string aText) {
      mDebugDownPipe.SendCommand(VsipUi.OutputPane, Encoding.UTF8.GetBytes(aText + "\r\n"));
    }

    protected void OutputClear() {
      mDebugDownPipe.SendCommand(VsipUi.OutputClear);
    }

  }
}