using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Specialized;
using Cosmos.Debug.Common;
using Cosmos.Compiler.Debug;
using Cosmos.Debug.Common;
using Cosmos.Build.Common;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Cosmos.Debug.VSDebugEngine {
  public class AD7Process : IDebugProcess2 {
    public Guid mID = Guid.NewGuid();
    protected Process mProcess;
    protected ProcessStartInfo mProcessStartInfo;
    protected EngineCallback mCallback;
    public AD7Thread mThread;
    protected AD7Engine mEngine;
    public DebugConnector mDbgConnector;
    public ReverseSourceInfos mReverseSourceMappings;
    public SourceInfos mSourceMappings;
    public uint? mCurrentAddress = null;
    protected readonly NameValueCollection mDebugInfo;
    protected TargetHost mTargetHost;
    protected VMwareFlavor mVMWareFlavor = VMwareFlavor.Player;
    internal DebugInfo mDebugInfoDb;
    internal IDictionary<uint, string> mAddressLabelMappings;
    internal IDictionary<string, uint> mLabelAddressMappings;

    private int mProcessExitEventSent = 0;

    protected void LaunchVMWare(bool aGDB) {
      string xPath = Path.Combine(PathUtilities.GetBuildDir(), @"VMWare\Workstation") + @"\";
      string xDebugVmx = "Debug.vmx";

      using (var xSrc = new StreamReader(xPath + "Cosmos.vmx")) {
        try {
          // This copy process also leaves the VMX writeable. VMWare doesnt like them read only.
          using (var xDest = new StreamWriter(xPath + xDebugVmx)) {
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
          if (e.Message.Contains(xDebugVmx))
            throw new Exception("The Vmware image " + xDebugVmx + " is still in use! Please exit current Vmware session with Cosmos and try again!", e);
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
      DebugWindows.SendCommand(DwMsgType.Registers, aData);
    }

    protected void DbgCmdFrame(byte[] aData)
    {
        DebugWindows.SendCommand(DwMsgType.Frame, aData);
    }

    protected void DbgCmdStack(byte[] aData)
    {
        DebugWindows.SendCommand(DwMsgType.Stack, aData);
    }

    internal AD7Process(NameValueCollection aDebugInfo, EngineCallback aCallback, AD7Engine aEngine, IDebugPort2 aPort)
    {
        System.Diagnostics.Debug.WriteLine("In AD7Process..ctor");
        mCallback = aCallback;

        // Load passed in values
        mDebugInfo = aDebugInfo;

        //
        mISO = mDebugInfo["ISOFile"];
        mProjectFile = mDebugInfo["ProjectFile"];
        //
        var xGDBDebugStub = false;
        Boolean.TryParse(mDebugInfo["EnableGDB"], out xGDBDebugStub);
        //
        var xGDBClient = false;
        Boolean.TryParse(mDebugInfo["StartCosmosGDB"], out xGDBClient);

        mProcessStartInfo = new ProcessStartInfo(Path.Combine(PathUtilities.GetVSIPDir(), "Cosmos.Debug.HostProcess.exe"));
        if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["BuildTarget"], "VMWare"))
        {
            mTargetHost = TargetHost.VMWare;
            if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["VMWareFlavor"], "Player"))
            {
                mVMWareFlavor = VMwareFlavor.Player;
            }
            else if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["VMWareFlavor"], "Workstation"))
            {
                mVMWareFlavor = VMwareFlavor.Workstation;
            }
            else
            {
                throw new Exception("VMWare Flavor '" + mDebugInfo["VMWareFlavor"] + "' not implemented!");
            }
            LaunchVMWare(xGDBDebugStub);
        }
        else
        {
            throw new Exception("Invalid BuildTarget value: '" + mDebugInfo["BuildTarget"] + "'!");
        }

        mProcessStartInfo.UseShellExecute = false;
        mProcessStartInfo.RedirectStandardInput = true;
        mProcessStartInfo.RedirectStandardError = true;
        mProcessStartInfo.RedirectStandardOutput = true;
        mProcessStartInfo.CreateNoWindow = true;

        string xCpdbPath = Path.ChangeExtension(mISO, "cpdb");
        if (!File.Exists(xCpdbPath))
        {
            throw new Exception("Debug data file " + xCpdbPath + " not found! Could be a omitted build process of Cosmos project so that not created.");
        }

        mDebugInfoDb = new DebugInfo();
        mDebugInfoDb.OpenCPDB(xCpdbPath);
        mDebugInfoDb.ReadLabels(out mAddressLabelMappings, out mLabelAddressMappings);
        if (mAddressLabelMappings.Count == 0)
        {
            throw new Exception("Debug data not found: LabelByAddressMapping");
        }

        mSourceMappings = Cosmos.Debug.Common.SourceInfo.GetSourceInfo(mAddressLabelMappings, mLabelAddressMappings, mDebugInfoDb);

        if (mSourceMappings.Count == 0)
        {
            throw new Exception("Debug data not found: SourceMappings");
        }
        mReverseSourceMappings = new ReverseSourceInfos(mSourceMappings);
        if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["BuildTarget"], "vmware"))
        {
            mDbgConnector = new Cosmos.Debug.Common.DebugConnectorPipeServer();
        }
        else
        {
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

        System.Threading.Thread.Sleep(250);
        System.Diagnostics.Debug.WriteLine(String.Format("Launching process: \"{0}\" {1}", mProcessStartInfo.FileName, mProcessStartInfo.Arguments).Trim());
        mProcess = Process.Start(mProcessStartInfo);

        mProcess.EnableRaisingEvents = true;
        mProcess.Exited += new EventHandler(mProcess_Exited);

        // Sleep 250 and see if it exited too quickly. Why do we do this? We have .Exited hooked. Is this in case it happens between start and hook?
        // if so, why not hook before start? 
        // MtW: we do this for the potential situation where it might exit before the Exited event is hooked. Iirc i had this situation before..
        System.Threading.Thread.Sleep(250);
        if (mProcess.HasExited)
        {
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
        if (xGDBDebugStub && xGDBClient)
        {
            if (File.Exists(Cosmos.Build.Common.CosmosPaths.GDBClientExe))
            {
                var xPSInfo = new ProcessStartInfo(Cosmos.Build.Common.CosmosPaths.GDBClientExe);
                xPSInfo.Arguments = "\"" + Path.ChangeExtension(mProjectFile, ".cgdb") + "\"" + @" /Connect";
                xPSInfo.UseShellExecute = false;
                xPSInfo.RedirectStandardInput = false;
                xPSInfo.RedirectStandardError = false;
                xPSInfo.RedirectStandardOutput = false;
                xPSInfo.CreateNoWindow = false;
                Process.Start(xPSInfo);
            }
            else
            {
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
      DebugMsg("RmtDbg: Started");

      // OK, now debugger is ready. Send it a list of breakpoints that were set before
      // program run.
      foreach (var xBP in mEngine.BPMgr.mPendingBPs) {
        foreach (var xBBP in xBP.mBoundBPs) {
          mDbgConnector.SetBreakpoint(xBBP.RemoteID, xBBP.mAddress);
        }
      }
      mDbgConnector.SendCommand(DsCommand.BatchEnd);
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
        case DsMsgType.BreakPoint: {
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
      // while this would work fine in a VM, it puts extra requirements on the setup
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
      mDbgConnector.SendCommand(DsCommand.Continue);
    }

    internal void Step(enum_STEPKIND aKind) {
      if (aKind == enum_STEPKIND.STEP_INTO) { // F11
        mDbgConnector.SendCommand(DsCommand.StepInto);

      } else if (aKind == enum_STEPKIND.STEP_OVER) { // F10
        mDbgConnector.SendCommand(DsCommand.StepOver);
      
      } else if (aKind == enum_STEPKIND.STEP_OUT) { // Shift-F11
        mDbgConnector.SendCommand(DsCommand.StepOut);

      } else if (aKind == enum_STEPKIND.STEP_BACKWARDS) {
        // STEP_BACKWARDS - Supported at all by VS?
        MessageBox.Show("Step backwards is not supported.");
        mCallback.OnStepComplete(); // Have to call this otherwise VS gets "stuck"

      } else {
        MessageBox.Show("Unknown step type requested.");
        mCallback.OnStepComplete(); // Have to call this otherwise VS gets "stuck"
      }
    }

    public void SendAssembly() {
      // Scan and make a list of labels that belong to this line of code
      int xIdx = mSourceMappings.Keys.IndexOf((uint)mCurrentAddress);
      string xFile = mSourceMappings.Values[xIdx].SourceFile;
      int xLineNo = mSourceMappings.Values[xIdx].Line;
      int xCol = mSourceMappings.Values[xIdx].Column;

      var xLabels = new List<string>();
      xLabels.Add(mAddressLabelMappings[(uint)mCurrentAddress].Trim().ToUpper());
      for (int i = xIdx; i < mSourceMappings.Values.Count; i++) {
        var xSI = mSourceMappings.Values[i];
        if ((xSI.SourceFile != xFile) || (xSI.Line != xLineNo) || (xSI.Column != xCol)) {
          break;
        }
        xLabels.Add(mAddressLabelMappings[mSourceMappings.Keys[i]].Trim().ToUpper());
      }

      // Extract relevant lines from .ASM file.
      var xLines = File.ReadAllLines(Path.ChangeExtension(mISO, ".asm"));

      // Fine line in ASM that starts the code block
      int xStart = -1;
      for (int i = 0; i < xLines.Length; i++) {
        string xLine = xLines[i].Trim();
        if (xLine.EndsWith(":")) {
          if (xLabels.Contains(xLine.Substring(0, xLine.Length - 1).ToUpper())) {
            // Found the first match, store and break.
            xStart = i;
            break;
          }
        }
      }

      var xCode = new StringBuilder();
      if (xStart > -1) {
        // Extract the actual lines
        for (int i = xStart; i < xLines.Length; i++) {
          string xLine = xLines[i].TrimEnd();
          if (xLine.EndsWith(":")) {
            string xTest = xLine.Trim().ToUpper();
            if (xLabels.Contains(xTest.Substring(0, xTest.Length - 1))) {
              xCode.AppendLine(xLine);
            } else {
              // Found the first non-match, stop here
              break;
            }
          } else {
            xCode.AppendLine(xLine);
          }
        }
      }

      DebugWindows.SendCommand(DwMsgType.AssemblySource, Encoding.ASCII.GetBytes(xCode.ToString()));
    }

  }
}