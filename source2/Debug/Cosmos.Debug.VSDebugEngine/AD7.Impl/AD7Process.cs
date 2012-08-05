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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.Win32;

namespace Cosmos.Debug.VSDebugEngine {
  public class AD7Process : IDebugProcess2 {
    public Guid ID = Guid.NewGuid();
    protected EngineCallback mCallback;
    public AD7Thread mThread;
    protected AD7Engine mEngine;
    public ReverseSourceInfos mReverseSourceMappings;
    public SourceInfos mSourceInfos;
    public UInt32? mCurrentAddress = null;
    protected readonly NameValueCollection mDebugInfo;
    protected LaunchType mLaunch;
    internal DebugInfo mDebugInfoDb;
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

    Host.Base mHost;

    public string mISO;
    public string mProjectFile;

    protected void DbgCmdRegisters(byte[] aData) {
      mDebugDownPipe.SendCommand(Debugger2Windows.Registers, aData);
    }

    protected void DbgCmdFrame(byte[] aData) {
      mDebugDownPipe.SendCommand(Debugger2Windows.Frame, aData);
    }

    protected void DbgCmdPong(byte[] aData) {
      mDebugDownPipe.SendCommand(Debugger2Windows.PongDebugStub, aData);
    }

    protected void DbgCmdStack(byte[] aData) {
      mDebugDownPipe.SendCommand(Debugger2Windows.Stack, aData);
    }

    void mDebugUpPipe_DataPacketReceived(byte aCmd, byte[] aData) {
      switch (aCmd) {
        case Windows2Debugger.Noop:
          // do nothing
          break;

        case Windows2Debugger.PingVSIP:
          mDebugDownPipe.SendCommand(Debugger2Windows.PongVSIP);
          break;

        case Windows2Debugger.PingDebugStub:
          mDbgConnector.Ping();
          break;

        case Windows2Debugger.SetAsmBreak:
          string xLabel = Encoding.UTF8.GetString(aData);
          UInt32 xAddress = mDebugInfoDb.AddressOfLabel(xLabel);
          mDbgConnector.SetAsmBreakpoint(xAddress);
          mDbgConnector.Continue();
          //mDebugDownPipe.SendCommand(VsipUi.OutputPane, xAddress.ToString());
          break;

        default:
          throw new Exception(String.Format("Command value '{0}' not supported in method AD7Process.mDebugUpPipe_DataPacketReceived.", aCmd));
      }
    }

    protected void DebugConnectorConnected() {
      OutputText("Connected to DebugStub.");
    }

    void CreateDebugConnector() {
      mDbgConnector = null;

      string xPort = mDebugInfo[BuildProperties.VisualStudioDebugPortString];
      var xParts = xPort.Split(' ');
      string xPortType = xParts[0].ToLower();
      string xPortParam = xParts[1].ToLower();

      OutputText("Starting debug connector.");
      if (xPortType == "pipe:") {
        mDbgConnector = new Cosmos.Debug.Common.DebugConnectorPipeServer(xPortParam);
      } else if (xPortType == "serial:") {
        mDbgConnector = new Cosmos.Debug.Common.DebugConnectorSerial(xPortParam);
      }

      if (mDbgConnector == null) {
        throw new Exception("No debug connector found.");
      }
      mDbgConnector.Connected = DebugConnectorConnected;
      mDbgConnector.CmdBreak += new Action<UInt32>(DbgCmdBreak);
      mDbgConnector.CmdTrace += new Action<UInt32>(DbgCmdTrace);
      mDbgConnector.CmdText += new Action<string>(DbgCmdText);
      mDbgConnector.CmdStarted += new Action(DbgCmdStarted);
      mDbgConnector.OnDebugMsg += new Action<string>(DebugMsg);
      mDbgConnector.ConnectionLost += new Action<Exception>(DbgConnector_ConnectionLost);
      mDbgConnector.CmdRegisters += new Action<byte[]>(DbgCmdRegisters);
      mDbgConnector.CmdFrame += new Action<byte[]>(DbgCmdFrame);
      mDbgConnector.CmdStack += new Action<byte[]>(DbgCmdStack);
      mDbgConnector.CmdPong += new Action<byte[]>(DbgCmdPong);
    }

    internal AD7Process(NameValueCollection aDebugInfo, EngineCallback aCallback, AD7Engine aEngine, IDebugPort2 aPort) {
      mCallback = aCallback;
      mDebugInfo = aDebugInfo;

      mLaunch = (LaunchType)Enum.Parse(typeof(LaunchType), aDebugInfo[BuildProperties.LaunchString]);

      if (mDebugDownPipe == null) {
        mDebugDownPipe = new Cosmos.Debug.Common.PipeClient(Pipes.DownName);

        mDebugUpPipe = new Cosmos.Debug.Common.PipeServer(Pipes.UpName);
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
      bool xUseGDB = string.Equals(mDebugInfo[BuildProperties.EnableGDBString], "true", StringComparison.InvariantCultureIgnoreCase);
      OutputText("GDB " + (xUseGDB ? "Enabled" : "Disabled") + ".");
      //
      var xGDBClient = false;
      Boolean.TryParse(mDebugInfo[BuildProperties.StartCosmosGDBString], out xGDBClient);

      string xHostArgs = "";
      if (mLaunch == LaunchType.VMware) {
        mHost = new Host.VMware(mDebugInfo, xUseGDB);
      } else if (mLaunch == LaunchType.Slave) {
        mHost = new Host.Slave(mDebugInfo, xUseGDB);
      } else {
        throw new Exception("Invalid Launch value: '" + mLaunch + "'.");
      }
      mHost.OnShutDown += HostShutdown;

      string xDbPath = Path.ChangeExtension(mISO, "mdf");
      if (!File.Exists(xDbPath)) {
        throw new Exception("Debug data file " + xDbPath + " not found. Could be a omitted build process of Cosmos project so that not created.");
      }

      mDebugInfoDb = new DebugInfo(xDbPath);
      mSourceInfos = SourceInfo.GetSourceInfo(mDebugInfoDb);
      if (mSourceInfos.Count == 0) {
        throw new Exception("Debug data not found: SourceMappings");
      }
      mReverseSourceMappings = new ReverseSourceInfos(mSourceInfos);

      CreateDebugConnector();
      aEngine.BPMgr.SetDebugConnector(mDbgConnector);

      mEngine = aEngine;
      mThread = new AD7Thread(aEngine, this);
      mCallback.OnThreadStart(mThread);
      mPort = aPort;

      if (xUseGDB && xGDBClient) {
        LaunchGdbClient();
      }
    }

    protected void LaunchGdbClient() {
      OutputText("Launching GDB client.");
      if (File.Exists(Cosmos.Build.Common.CosmosPaths.GdbClientExe)) {
        var xPSInfo = new ProcessStartInfo(Cosmos.Build.Common.CosmosPaths.GdbClientExe);
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
            Cosmos.Build.Common.CosmosPaths.GdbClientExe), "GDB-Client", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
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
      mDbgConnector.SendCmd(Vs2Ds.BatchEnd);
    }

    void DbgCmdText(string obj) {
      mCallback.OnOutputStringUser(obj + "\r\n");
    }

    internal AD7Thread Thread {
      get {
        return mThread;
      }
    }

    void DbgCmdTrace(UInt32 aAddress) {
      DebugMsg("TraceReceived: " + aAddress);
    }

    void DbgCmdBreak(UInt32 aAddress) {
      // aAddress will be actual address. Call and other methods push return to (after op), but DS
      // corrects for us and sends us actual op address.
      DebugMsg("DbgCmdBreak " + aAddress + " / " + aAddress.ToString("X8").ToUpper());

      var xActionPoints = new List<object>();
      var xBoundBreakpoints = new List<IDebugBoundBreakpoint2>();

      // Search the BPs and find ones that match our address.
      foreach (var xBP in mEngine.BPMgr.mPendingBPs) {
        foreach (var xBBP in xBP.mBoundBPs) {
          if (xBBP.mAddress == aAddress) {
            xBoundBreakpoints.Add(xBBP);
          }
        }
      }

      mCurrentAddress = aAddress;
      if (xBoundBreakpoints.Count == 0) {
        // if no matching breakpoints are found then its one of the following:
        //   - Stepping operation
        //   - Code based break
        //   - Asm stepping

        if (mStepping) {
          mCallback.OnStepComplete();
          mStepping = false;
        } else {
          mCallback.OnBreakpoint(mThread, new List<IDebugBoundBreakpoint2>());
        }
      } else {
        // Found a bound breakpoint
        mCallback.OnBreakpoint(mThread, xBoundBreakpoints.AsReadOnly());
      }
      RequestFullDebugStubUpdate();
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

    public readonly Guid PhysID = Guid.NewGuid();
    public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId) {
      // http://blogs.msdn.com/b/jacdavis/archive/2008/05/01/what-to-do-if-your-debug-engine-doesn-t-create-real-processes.aspx
      // http://social.msdn.microsoft.com/Forums/en/vsx/thread/fe809686-e5f9-439d-9e52-00017e12300f
      pProcessId[0].guidProcessId = PhysID;
      pProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID;

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
      pguidProcessId = ID;
      return VSConstants.S_OK;
    }

    public int GetServer(out IDebugCoreServer2 ppServer) {
      throw new NotImplementedException();
    }

    public int Terminate() {
      OutputText("Debugger terminating.");

      mHost.Stop();

      OutputText("Debugger terminated.");
      return VSConstants.S_OK;
    }

    internal void ResumeFromLaunch() {
      mHost.Start();
    }

    void HostShutdown(object sender, EventArgs e) {
      //AD7ThreadDestroyEvent.Send(mEngine, mThread, (uint)mProcess.ExitCode);
      //mCallback.OnProgramDestroy((uint)mProcess.ExitCode);

      // We dont use process info any more, but have to call this to tell
      // VS to stop debugging.
      if (Interlocked.CompareExchange(ref mProcessExitEventSent, 1, 0) == 0) {
        mCallback.OnProcessExit(0);
      }

      if (mDbgConnector != null) {
        mDbgConnector.Dispose();
        mDbgConnector = null;
      }
      if (mDebugInfoDb != null) {
        // Commented for debugging, so we can look at the DB after
        //mDebugInfoDb.DeleteDB();
        mDebugInfoDb.Dispose();
        mDebugInfoDb = null;
      }
    }

    internal void Continue() { // F5
      mCurrentAddress = null;
      mDbgConnector.Continue();
    }

    bool mStepping = false;
    internal void Step(enum_STEPKIND aKind) {
      if (aKind == enum_STEPKIND.STEP_INTO) { // F11
        mStepping = true;
        mDbgConnector.SendCmd(Vs2Ds.StepInto);

      } else if (aKind == enum_STEPKIND.STEP_OVER) { // F10
        mStepping = true;
        mDbgConnector.SendCmd(Vs2Ds.StepOver);

      } else if (aKind == enum_STEPKIND.STEP_OUT) { // Shift-F11
        mStepping = true;
        mDbgConnector.SendCmd(Vs2Ds.StepOut);

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
      UInt32 xAddress = mCurrentAddress.Value;
      var xSourceInfos = mDebugInfoDb.GetSourceInfos(xAddress);

      // Because of Asm breakpoints the address we have might be in the middle of a C# line.
      // So we find the closest address to ours that is less or equal to ours.
      var xQry = from x in xSourceInfos
                 where x.Key <= xAddress
                 orderby x.Key descending
                 select x.Value;
      var xValue = xQry.FirstOrDefault();
      if (xValue == null) {
        return;
      }

      // Create list of asm labels that belong to this line of C#.
      var xMappings = from x in xSourceInfos
                      where x.Value.SourceFile == xValue.SourceFile
                        && x.Value.Line == xValue.Line
                        && x.Value.Column == xValue.Column
                      select x.Key;
      var xLabels = new List<string>();
      foreach (uint xAddr in xMappings) {
        foreach (string xLabel in mDebugInfoDb.GetLabels(xAddr)) {
          xLabels.Add(xLabel + ":");
        }
      }

      // Get assembly source
      var xCode = AsmSource.GetSourceForLabels(Path.ChangeExtension(mISO, ".asm"), xLabels);

      // Get label for current address.
      // A single address can have multiple labels (IL, Asm). Because of this we search
      // for the one with the Asm tag. We dont have the tags in this debug info though,
      // so instead if there is more than one label we use the longest one which is the Asm tag.
      var xCurrentLabels = mDebugInfoDb.GetLabels(mCurrentAddress.Value);
      if (xCurrentLabels.Length > 0) {
        string xCurrentLabel = xCurrentLabels.OrderBy(q => q.Length).Last();
        // Insert it to the first line of our data stream
        xCode.Insert(0, xCurrentLabel + "\r\n");
        mDebugDownPipe.SendCommand(Debugger2Windows.AssemblySource, Encoding.UTF8.GetBytes(xCode.ToString()));
      }
    }

    //TODO: At some point this will probably need to be exposed for access outside of AD7Process
    protected void OutputText(string aText) {
      mDebugDownPipe.SendCommand(Debugger2Windows.OutputPane, Encoding.UTF8.GetBytes(aText + "\r\n"));
    }

    protected void OutputClear() {
      mDebugDownPipe.SendCommand(Debugger2Windows.OutputClear);
    }

  }
}