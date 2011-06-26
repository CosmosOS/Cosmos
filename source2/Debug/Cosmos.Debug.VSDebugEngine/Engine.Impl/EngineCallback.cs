using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Cosmos.Compiler.Debug;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace Cosmos.Debug.VSDebugEngine
{
    public class EngineCallback //: ISampleEngineCallback
    {
        readonly IDebugEventCallback2 m_ad7Callback;
        readonly AD7Engine m_engine;

        public EngineCallback(AD7Engine engine, IDebugEventCallback2 ad7Callback)
        {
            m_ad7Callback = ad7Callback;
            m_engine = engine;
        }

        public void Send(IDebugEvent2 eventObject, string iidEvent, IDebugProgram2 program, IDebugThread2 thread)
        {
            uint attributes; 
            Guid riidEvent = new Guid(iidEvent);

            EngineUtils.RequireOk(eventObject.GetAttributes(out attributes));

            EngineUtils.RequireOk(m_ad7Callback.Event(m_engine, null, program, thread, eventObject, ref riidEvent, attributes));
        }

        public void Send(IDebugEvent2 eventObject, string iidEvent, IDebugThread2 thread)
        {
            Send(eventObject, iidEvent, m_engine, thread);
        }

        public void OnError(int hrErr)
        {
            //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);

            // IDebugErrorEvent2 is used to report error messages to the user when something goes wrong in the debug engine.
            // The sample engine doesn't take advantage of this.
        }

        public void OnModuleLoad(AD7Module aModule)
        {
            // This will get called when the entrypoint breakpoint is fired because the engine sends a mod-load event
            // for the exe.
            //if (m_engine.DebuggedProcess != null)
            //{
            //    System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);
            //}

            AD7ModuleLoadEvent eventObject = new AD7ModuleLoadEvent(aModule, true /* this is a module load */);

            Send(eventObject, AD7ModuleLoadEvent.IID, null);
        }

        public void OnModuleUnload()//DebuggedModule debuggedModule)
        {
            //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);

            //AD7Module ad7Module = (AD7Module)debuggedModule.Client;
            //System.Diagnostics.Debug.Assert(ad7Module != null);

            //AD7ModuleLoadEvent eventObject = new AD7ModuleLoadEvent(ad7Module, false /* this is a module unload */);

            //Send(eventObject, AD7ModuleLoadEvent.IID, null);
        }

        // Call this one for internal Cosmos dev.
        // Can be turned off and should be turned off by default. Use an IFDEF or something.
        public void OnOutputString(string outputString) {
            if (false) {
                //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);
                var eventObject = new AD7OutputDebugStringEvent(outputString);
                Send(eventObject, AD7OutputDebugStringEvent.IID, null);
            }
        }

        // This is the user version, messages directly from Cosmos user code
        public void OnOutputStringUser(string outputString) {
            //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);
            var eventObject = new AD7OutputDebugStringEvent(outputString);
            Send(eventObject, AD7OutputDebugStringEvent.IID, null);
        }

        public void OnProcessExit(uint exitCode)
        {
            //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);

            AD7ProgramDestroyEvent eventObject = new AD7ProgramDestroyEvent(exitCode);

            Send(eventObject, AD7ProgramDestroyEvent.IID, null);
        }

        public void OnThreadExit()//DebuggedThread debuggedThread, uint exitCode)
        {
            //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);

            //AD7Thread ad7Thread = (AD7Thread)debuggedThread.Client;
            //System.Diagnostics.Debug.Assert(ad7Thread != null);

            //AD7ThreadDestroyEvent eventObject = new AD7ThreadDestroyEvent(exitCode);

            //Send(eventObject, AD7ThreadDestroyEvent.IID, ad7Thread);
        }

        public void OnThreadStart(AD7Thread debuggedThread)
        {
            // This will get called when the entrypoint breakpoint is fired because the engine sends a thread start event
            // for the main thread of the application.
            //if (m_engine.DebuggedProcess != null)
            //{
            //    System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);
            //}

            AD7ThreadCreateEvent eventObject = new AD7ThreadCreateEvent();
            Send(eventObject, AD7ThreadCreateEvent.IID, debuggedThread);
        }

        public void OnBreak(AD7Thread aThread)
        {
            var mBreak = new AD7BreakEvent();
            Send(mBreak, AD7BreakEvent.IID, aThread);
        }

        public void OnBreakpoint(AD7Thread thread, IList<IDebugBoundBreakpoint2> clients) {
          var boundBreakpoints = new IDebugBoundBreakpoint2[clients.Count];
            int i = 0;
            foreach (var objCurrentBreakpoint in clients) {
                boundBreakpoints[i] = objCurrentBreakpoint;
                i++;
            }
            
            ////
            IDebugPendingBreakpoint2 ppend;
            IDebugBreakpointRequest2 req;
            BP_REQUEST_INFO[] reqinf = new BP_REQUEST_INFO[i];
            ((IDebugBoundBreakpoint2)boundBreakpoints[0]).GetPendingBreakpoint(out ppend);
            ppend.GetBreakpointRequest(out req);
            req.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_BPLOCATION, reqinf);
            IDebugDocumentPosition2 docPosition = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(reqinf[0].bpLocation.unionmember2));

            // Get the name of the document that the breakpoint was put in
            string documentName;
            EngineUtils.CheckOk(docPosition.GetFileName(out documentName));

            // Get the location in the document that the breakpoint is in.
            TEXT_POSITION[] startPosition = new TEXT_POSITION[1];
            TEXT_POSITION[] endPosition = new TEXT_POSITION[1];
            EngineUtils.CheckOk(docPosition.GetRange(startPosition, endPosition));

            // Get C# lines
            TextReader xTR = new StreamReader(documentName);
            string xFile = xTR.ReadToEnd();
            xTR.Close();
            xFile = xFile.Replace('\r', ' ');
            xFile = xFile.Trim();
            string[] xFileLines = xFile.Split('\n');
            string xMethod = xFileLines[startPosition[0].dwLine - 1];
            int xstart = 0, xstop = 0;
            string[] xMethodParts = xMethod.Split(' ');
            for (int j = 0; j < xMethodParts.Length; j++)
            {
                if (xMethodParts[j].Contains("()"))
                {
                    xMethod = xMethodParts[j];
                    break;
                }
            }
            xMethod = "_" + xMethod;
            xMethod = xMethod.Replace("()", "__:");

            // Get ASM lines
            xstart = 0; 
            xstop = 0;
            xstart = documentName.LastIndexOf('\\');
            xstop = documentName.LastIndexOf('.');
            string xFileName = documentName.Substring(0, xstart);
            xFileName = Path.GetDirectoryName(documentName);
            xFileName = Path.Combine(xFileName, "bin", "Debug");
            // TODO: Error checking for no return files.
            string[] xFiles = Directory.GetFiles(xFileName, "*.asm");
            xFileName = Path.Combine(xFileName, xFiles[0]);
            xTR = new StreamReader(xFileName);
            xFile = xTR.ReadToEnd();
            xTR.Close();
            xFile = xFile.Replace('\r', ' ');
            xFile = xFile.Trim();
            xFileLines = xFile.Split('\n');
            int k = 0, l = 0;
            for (int j = 0; j < xFileLines.Length; j++)
            {
                if (xFileLines[j].Contains(xMethod))
                {
                    k = j;
                    j++;
                }
                if ((k != 0) && (xFileLines[j].Contains(":")))
                {
                    l = j - 2;
                    break;
                }
            }
            //MessageBox.Show(xFileLines[k]);
            //MessageBox.Show(xFileLines[l]);
            //MessageBox.Show(k.ToString());
            //MessageBox.Show(l.ToString());
            //MessageBox.Show("Name: " + documentName);
            //MessageBox.Show("Start Pos: " + startPosition[0].dwLine + " " + startPosition[0].dwColumn);
            //MessageBox.Show("End Pos: " + endPosition[0].dwLine + " " + endPosition[0].dwColumn);
            for (int j = k; j < l; j++)
            {
              DebugWindows.SendCommand(DwMsgType.AssemblySource, Encoding.ASCII.GetBytes(xFileLines[j]));
            }
                ////

            // An engine that supports more advanced breakpoint features such as hit counts, conditions and filters
            // should notify each bound breakpoint that it has been hit and evaluate conditions here.
            // The sample engine does not support these features.
            var boundBreakpointsEnum = new AD7BoundBreakpointsEnum(boundBreakpoints);
            var eventObject = new AD7BreakpointEvent(boundBreakpointsEnum);
            var ad7Thread = (AD7Thread)thread;
            Send(eventObject, AD7BreakpointEvent.IID, ad7Thread);
        }

        public void OnException()//DebuggedThread thread, uint code)
        {
            // Exception events are sent when an exception occurs in the debuggee that the debugger was not expecting.
            // The sample engine does not support these.
            throw new Exception("The method or operation is not implemented.");
        }

        public void OnStepComplete()//DebuggedThread thread)
        {
            AD7StepCompletedEvent.Send(m_engine);
        }

        public void OnAsyncBreakComplete(AD7Thread aThread)
        {
            // This will get called when the engine receives the breakpoint event that is created when the user
            // hits the pause button in vs.
            //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);

            var xEvent = new AD7AsyncBreakCompleteEvent();
            Send(xEvent, AD7AsyncBreakCompleteEvent.IID, aThread);
            //AD7Thread ad7Thread = (AD7Thread)thread.Client;
            //AD7AsyncBreakCompleteEvent eventObject = new AD7AsyncBreakCompleteEvent();
            //Send(eventObject, AD7AsyncBreakCompleteEvent.IID, ad7Thread);
        }

        public void OnLoadComplete(AD7Thread aThread)
        {
            var xMsg = new AD7LoadCompleteEvent();
            Send(xMsg, AD7LoadCompleteEvent.IID, aThread);
            //AD7Thread ad7Thread = (AD7Thread)thread.Client;
            //AD7LoadCompleteEvent eventObject = new AD7LoadCompleteEvent();
            //Send(eventObject, AD7LoadCompleteEvent.IID, ad7Thread);
        }

        public void OnProgramDestroy(uint exitCode)
        {                     
            AD7ProgramDestroyEvent eventObject = new AD7ProgramDestroyEvent(exitCode);
            Send(eventObject, AD7ProgramDestroyEvent.IID, null);
        }

        // Engines notify the debugger about the results of a symbol serach by sending an instance
        // of IDebugSymbolSearchEvent2
        public void OnSymbolSearch(AD7Module module, string status, enum_MODULE_INFO_FLAGS dwStatusFlags)
        {
            string statusString = (dwStatusFlags == enum_MODULE_INFO_FLAGS.MIF_SYMBOLS_LOADED ? "Symbols Loaded - " : "No symbols loaded") + status;

            AD7SymbolSearchEvent eventObject = new AD7SymbolSearchEvent(module, statusString, dwStatusFlags);
            Send(eventObject, AD7SymbolSearchEvent.IID, null);
        }

        // Engines notify the debugger that a breakpoint has bound through the breakpoint bound event.
        public void OnBreakpointBound(object objBoundBreakpoint, uint address)
        {
            AD7BoundBreakpoint boundBreakpoint = (AD7BoundBreakpoint)objBoundBreakpoint;
            IDebugPendingBreakpoint2 pendingBreakpoint;
            ((IDebugBoundBreakpoint2)boundBreakpoint).GetPendingBreakpoint(out pendingBreakpoint);

            AD7BreakpointBoundEvent eventObject = new AD7BreakpointBoundEvent((AD7PendingBreakpoint)pendingBreakpoint, boundBreakpoint);
            Send(eventObject, AD7BreakpointBoundEvent.IID, null);
        }

    }

    internal class AD7BreakEvent : AD7StoppingEvent, IDebugBreakEvent2
    {
        public const string IID = "C7405D1D-E24B-44E0-B707-D8A5A4E1641B";
    }
}
