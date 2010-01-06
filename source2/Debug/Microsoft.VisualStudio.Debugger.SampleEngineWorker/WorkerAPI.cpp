#include "stdafx.h"

#pragma managed(on)

#include "ProjInclude.h"

// Worker API header files (in alphabetical order)
#include "DebuggedModule.h"
#include "DebuggedProcess.h"
#include "DebuggedThread.h"
#include "EngineCallback.h"
#include "ProcessLaunchInfo.h"
#include "ThreadContext.h"
#include "WorkerAPI.h"
#include "ModuleResolver.h"
#include "DiaStackWalkHelper.h"
#include "DiaFrameHolder.h"

// The x86 breakpoint instruction
const BYTE BreakpointInstruction = 0xCC;

/* static */
void Worker::Initialize()
{
	ASSERT(s_mainThreadId == 0 || s_mainThreadId == GetCurrentThreadId());
	s_mainThreadId = GetCurrentThreadId();
}


/* static */
DebuggedProcess^ Worker::AttachToProcess(ISampleEngineCallback^ callback, int processId)
{
	ASSERT(Worker::MainThreadId != Worker::CurrentThreadId);

	HANDLE hProcess = Win32HandleCall( ::OpenProcess(
		PROCESS_ALL_ACCESS, 
		FALSE, 
		processId
		));
	
	String^ nameFromHandle = GetProcessName(hProcess);

	String^ processName = System::IO::Path::GetFullPath(nameFromHandle);

	Win32BoolCall( ::DebugActiveProcess(
		processId
		) );

	DebuggedProcess^ process = gcnew DebuggedProcess(Attach, callback, hProcess, processId, processName);

	return process;
}

/* static */
DebuggedProcess^ Worker::LaunchProcess(ISampleEngineCallback^ callback, ProcessLaunchInfo ^processLaunchInfo)
{
	ASSERT(Worker::MainThreadId != Worker::CurrentThreadId);

	PROCESS_INFORMATION pi = { 0 };

	BOOL fInheritHandles = FALSE;

	String^ processName = System::IO::Path::GetFullPath(processLaunchInfo->Exe);

	STARTUPINFO si = { sizeof(si) };
    si.dwFlags = STARTF_USESHOWWINDOW;
    si.wShowWindow = SW_SHOW;

	if (processLaunchInfo->StdInput || processLaunchInfo->StdOutput || processLaunchInfo->StdError)
    {
        si.hStdInput = (HANDLE) (size_t) processLaunchInfo->StdInput;
        si.hStdOutput = (HANDLE) (size_t) processLaunchInfo->StdOutput;
        si.hStdError = (HANDLE) (size_t) processLaunchInfo->StdError;
        si.dwFlags |= STARTF_USESTDHANDLES;
        fInheritHandles = true;
    }

	const DWORD dwCreationFlags = DEBUG_ONLY_THIS_PROCESS;

	CAutoVectorPtr<wchar_t> commandLine;
	commandLine.Attach( wcsdup(processLaunchInfo->CommandLine) );

	pin_ptr<const wchar_t> szDir = PtrToStringChars(processLaunchInfo->Dir);

	Win32BoolCall( ::CreateProcess(
		NULL,
		commandLine,
		NULL,
		NULL,
		fInheritHandles,
		dwCreationFlags,
		NULL,
		szDir,
		&si,
		&pi
		) ); 

	VERIFY( CloseHandle(pi.hThread) );

	DebuggedProcess^ process = gcnew DebuggedProcess(Launch, callback, pi.hProcess, (int)pi.dwProcessId, processName);

	return process;
}



DebuggedProcess::DebuggedProcess(DEBUG_METHOD method, ISampleEngineCallback^ callback, HANDLE hProcess, int processId, String^ name) :
	Id(processId),
	m_debugMethod(method),
	m_callback(callback),
	m_hProcess(hProcess),
	m_dwPollThreadId( ::GetCurrentThreadId() ),
	m_lastDebugEvent(*(new DEBUG_EVENT())),
	m_lastStoppingEvent(StartDebugging),
	m_suspendCount(0),
	m_fIsPumpingDebugEvents(false),
	m_fSeenEntrypointBreakpoint(false),
	m_fExpectingAsyncBreak(false)
{
	ASSERT(Worker::MainThreadId != Worker::CurrentThreadId);

	memset(&m_lastDebugEvent, 0, sizeof(DEBUG_EVENT));
	bool fSuccess = false;

	try
	{

		m_resolver = gcnew ModuleResolver();
		m_pSymbolEngine = new SymbolEngine();
		m_pSymbolEngine->Initialize();
		
		m_moduleAddressMap = gcnew AddressDictionary<DebuggedModule^>();
		m_moduleList = gcnew Collections::Generic::LinkedList<DebuggedModule^>();
		
		m_threadIdMap = gcnew Collections::Generic::Dictionary<DWORD, DebuggedThread^>();
		m_threadList = gcnew Collections::Generic::LinkedList<DebuggedThread^>();

		m_breakpointMap = gcnew Collections::Generic::Dictionary<DWORD_PTR, BreakpointData^>();

		m_resolver->InitializeCache(name);

		WaitForDebugEvent();

		if (m_lastDebugEvent.dwProcessId != Id ||
			m_lastDebugEvent.dwDebugEventCode != CREATE_PROCESS_DEBUG_EVENT)
		{
			ASSERT(!"Why didn't we get a CREATE_PROCESS_DEBUG_EVENT");
			ThrowHR(E_UNEXPECTED);
		}
		LOAD_DLL_DEBUG_INFO loadDllEvent;
		loadDllEvent.hFile = m_lastDebugEvent.u.CreateProcessInfo.hFile;
		loadDllEvent.lpBaseOfDll = m_lastDebugEvent.u.CreateProcessInfo.lpBaseOfImage;
		loadDllEvent.dwDebugInfoFileOffset = m_lastDebugEvent.u.CreateProcessInfo.dwDebugInfoFileOffset;
		loadDllEvent.nDebugInfoSize = m_lastDebugEvent.u.CreateProcessInfo.nDebugInfoSize;
		loadDllEvent.lpImageName = m_lastDebugEvent.u.CreateProcessInfo.lpImageName;
		loadDllEvent.fUnicode = m_lastDebugEvent.u.CreateProcessInfo.fUnicode;

		DebuggedModule^ exeModule = CreateModule(loadDllEvent);

		this->Name = exeModule->Name;
		this->StartAddress = (DWORD_PTR)m_lastDebugEvent.u.CreateProcessInfo.lpStartAddress;

		fSuccess = true;
	
		DispatchDebugEvent();

        if (method == Attach)
        {
		    // Continue the process create event. If the debuggee is being launched, this occurs during the call to ResumeFromLaunch.
		    ContinueDebugEvent(true);

    		m_lastStoppingEvent = Invalid;
        }
	}
	finally 
	{
		if (!fSuccess)
		{
			::TerminateProcess(m_hProcess, MAXDWORD);

			while (true)
			{
				if (m_lastDebugEvent.dwDebugEventCode == LOAD_DLL_DEBUG_EVENT)
				{
					VERIFY( CloseHandle(m_lastDebugEvent.u.LoadDll.hFile) );
				}
				else if (m_lastDebugEvent.dwDebugEventCode == CREATE_PROCESS_DEBUG_EVENT)
				{
					VERIFY( CloseHandle(m_lastDebugEvent.u.CreateProcessInfo.hFile) );
				}
				else if (m_lastDebugEvent.dwDebugEventCode == EXIT_PROCESS_DEBUG_EVENT)
				{
					ContinueDebugEvent(false);
					break;
				}

				ContinueDebugEvent(false);
				WaitForDebugEvent();
			}

			Close();
		}
	}
}

DebuggedProcess::~DebuggedProcess()
{
	if (m_pSymbolEngine != NULL)
	{
		m_pSymbolEngine->Close();
	}
	
	// Close any threads that were yanked down as part of process exit
	for each (DebuggedThread^ thread in m_threadList)
	{
		thread->Close();
	}
	m_threadList->Clear();
	m_threadIdMap->Clear();
	m_moduleAddressMap->Clear();
	m_moduleList->Clear();
	m_resolver->Close();		
	this->!DebuggedProcess();	
}

DebuggedProcess::!DebuggedProcess()
{
 	CloseHandle(m_hProcess);
}

void DebuggedProcess::Close()
{
	this->~DebuggedProcess();
}

void DebuggedProcess::SetBreakpoint(DWORD_PTR address, Object^ client)
{
	// THREADING: Can be called on any thread
	msclr::lock lock(m_breakpointMap);

	BreakpointData^ bpData;
	if (m_breakpointMap->TryGetValue(address, bpData))
	{
		bpData->Clients->AddLast(client);
		return;
	}

	Suspend();

	try
	{
		array<byte>^ memory = ReadMemory(address, 1);
		BYTE originialData = memory[0];

		bpData = gcnew BreakpointData(address, originialData, client);	
		m_breakpointMap->Add(address, bpData);

		if (originialData != BreakpointInstruction)
		{
			memory[0] = BreakpointInstruction;
			WriteMemory(address, memory);
			Win32BoolCall(FlushInstructionCache(m_hProcess, NULL, NULL));
		}
	}
	finally
	{
		Resume();
	}

	m_callback->OnBreakpointBound(client, address);
}

void DebuggedProcess::RemoveBreakpoint(DWORD_PTR address, Object^ client)
{
	// THREADING: Can be called on any thread
	msclr::lock lock(m_breakpointMap);

	BreakpointData^ bpData;
	if (m_breakpointMap->TryGetValue(address, bpData))
	{	
		Suspend();

		try
		{
			array<byte>^ origData = gcnew array<byte>(1);
			origData[0] = bpData->OriginalData;
			WriteMemory(address, origData);
			Win32BoolCall(FlushInstructionCache(m_hProcess, NULL, NULL));

			bpData->Clients->Remove(client);

			if (bpData->Clients->Count == 0)
			{
				m_breakpointMap->Remove(address);
			}
		}
		finally
		{
			Resume();
		}
	}

	return;
}

BreakpointData^ DebuggedProcess::FindBreakpointAtAddress(DWORD_PTR address)
{
	// THREADING: Can be called on any thread
	ASSERT(this->IsStopped);

	msclr::lock lock(m_breakpointMap);

	BreakpointData^ bpData;
	if (m_breakpointMap->TryGetValue(address, bpData))
	{
		return bpData;
	}

	return nullptr;
}

void DebuggedProcess::WaitForAndDispatchDebugEvent(ResumeEventPumpFlags flags)
{
	ASSERT(GetCurrentThreadId() == this->m_dwPollThreadId);
	ASSERT(m_fIsPumpingDebugEvents);

	if (m_lastStoppingEvent != Invalid)
	{
		return;
	}

	bool fGotEvent = WaitForDebugEvent(50);
	if (fGotEvent)
	{
		if (!DispatchDebugEvent())
		{
			// DispatchDebugEvent return true if the event was a stopping event. Stop the event pump until the next continue.
			m_fIsPumpingDebugEvents = false;
		}
		else
		{
			// We sent an non-stopping event to the debugger. Continue now.
			ContinueDebugEvent(true);
		}
	}
}

void DebuggedProcess::ResumeEventPump()
{
	m_fIsPumpingDebugEvents = true;
}

// Perform an Async-Break in the debuggee
void DebuggedProcess::Break()
{
	if (IsStopped)
	{
		return; // We are already stopped, so nothing to do
	}

	m_fExpectingAsyncBreak = true;

	// NOTE: DebugBreakProcess will cause the target process to hit an int3 (breakpoint) instruction
	// by starting a new thread. For the purposes of a sample win32 debugger, this is a reasonable way 
	// to implement Debug->Break. A production quality Win32 debugger might not want to do this because 
	// if the target processs in hung with the loader lock taken, the debugger will be unable to break 
	// into the process.
	Win32BoolCall( ::DebugBreakProcess(m_hProcess) );
}

// Used to suspend all of the threads in the process.
void DebuggedProcess::Suspend()
{
	if (m_suspendCount > 0)
	{
		m_suspendCount++; // we are already suspended. Simply increase the counter.
		return;
	}

	// Once the m_threadIdMap has been entered, there will be no more thread creates or thread destroyies processsed
	Threading::Monitor::Enter(m_threadIdMap);
	int suspendIndex = 0;
	array<DebuggedThread^>^ threads = nullptr;
	bool success = false;

	try
	{
		// start by making a copy of the threads so that if something goes wrong we can undo the suspension
		threads = gcnew array<DebuggedThread^>(m_threadIdMap->Count);
		m_threadIdMap->Values->CopyTo(threads, 0);
		
		for (; suspendIndex < threads->Length; suspendIndex++)
		{
			Win32BoolCall( ::SuspendThread((HANDLE)threads[suspendIndex]->Handle) != MAXDWORD );
		}

		success = true;

		// Leave the function with the suspend count non-zero and the m_threadIdMap lock held
		Debug::Assert(m_suspendCount == 0);
		m_suspendCount = 1;
	}
	finally
	{
		if (!success)
		{
			// Something went wrong. Resume the threads.
			for (int resumeIndex = 0; resumeIndex < suspendIndex; resumeIndex++)
			{
				::ResumeThread((HANDLE)threads[resumeIndex]->Handle);
			}
		}
	}
}

// Resume the threads in the process that were suspended via a call to DebuggedProcess::Suspend
void DebuggedProcess::Resume()
{
	ASSERT(m_suspendCount > 0);

	m_suspendCount--;
	if (m_suspendCount > 0)
	{
		return; // we aren't ready to resume yet
	}

	int resumeIndex = 0;
	array<DebuggedThread^>^ threads = nullptr;
	bool success = false;

	try
	{
		// start by making a copy of the threads so that if something goes wrong we can undo the resume
		threads = gcnew array<DebuggedThread^>(m_threadIdMap->Count);
		m_threadIdMap->Values->CopyTo(threads, 0);
		
		for (; resumeIndex < threads->Length; resumeIndex++)
		{
			Win32BoolCall( ::ResumeThread((HANDLE)threads[resumeIndex]->Handle) != MAXDWORD );
		}

		success = true;
	}
	finally
	{
		if (!success)
		{
			for (int suspendIndex = 0; suspendIndex < resumeIndex; suspendIndex++)
			{
				::SuspendThread((HANDLE)threads[suspendIndex]->Handle);
			}
		}
	}

	// leave m_threadIdMap which allows for thread creates/destroys
	Threading::Monitor::Exit(m_threadIdMap);

	// the debuggee should no longer be suspended
	ASSERT(m_suspendCount == 0);
}

// Called during a debuggee launch to resume the first-thread in the process.
// The engine must wait until this point to start sending events to the debugger 
// so the faked up mod-load and thread create events are not sent until this time.
void DebuggedProcess::ResumeFromLaunch()
{
	assert(m_debugMethod == Launch);
	assert(m_entrypointModule != nullptr);
	assert(m_entrypointThread != nullptr);

	// Fake up a module load event for the entrypoint module
	m_callback->OnModuleLoad(m_entrypointModule);
	
	// Load symbols for the entrypoint module. This is the only module the sample will load symbols for.
	m_callback->OnSymbolSearch(m_entrypointModule, m_entrypointModule->SymbolPath, m_entrypointModule->SymbolsLoaded);
	
	// Send the thread create event for the main thread
	m_callback->OnThreadStart(m_entrypointThread);

	m_entrypointThread = nullptr;
	m_entrypointModule = nullptr;

    // Continue the Create process debug event.
    ContinueDebugEvent(true);
    m_lastStoppingEvent = Invalid;
}

// Return the integer and control context for the thread whose handle is passed.
X86ThreadContext^ DebuggedProcess::GetThreadContext(IntPtr hThread)
{
	// THREADING: Can be called from any thread
	ASSERT(m_lastDebugEvent.dwDebugEventCode != 0); // We should be stopped

	CONTEXT context = { 0 };
	context.ContextFlags = CONTEXT_INTEGER | CONTEXT_CONTROL;

	Win32BoolCall( ::GetThreadContext((HANDLE)hThread, &context) );

	return gcnew X86ThreadContext(context);
}

// Find the module the address falls in
DebuggedModule^ DebuggedProcess::ResolveAddress(DWORD_PTR address)
{
	msclr::lock lock(m_moduleAddressMap);

	return m_moduleAddressMap->FindAddress(address, 1);
}

// Read memory from thte process. 
array<byte>^ DebuggedProcess::ReadMemory(DWORD_PTR base, DWORD size)
{
	array<byte>^ result = gcnew array<byte>(size);
	pin_ptr<byte> pResult = &result[0];

	SIZE_T bytesRead;
	Win32BoolCall( ::ReadProcessMemory(m_hProcess, (LPCVOID)base, pResult, size, &bytesRead) );

	return result;
}

// Helper to make reading a single uint easier
unsigned int DebuggedProcess::ReadMemoryUInt(DWORD_PTR base)
{
	unsigned int val;
	pin_ptr<unsigned int> pResult = &val;

	SIZE_T bytesRead;
	Win32BoolCall( ::ReadProcessMemory(m_hProcess, (LPCVOID)base, pResult, sizeof(unsigned int), &bytesRead) );

	return val;
}

// Write memory to the debuggee process
void DebuggedProcess::WriteMemory(DWORD_PTR base, array<byte>^ data)
{
	pin_ptr<byte> pData = &data[0];

	SIZE_T bytesWritten;
	Win32BoolCall( ::WriteProcessMemory(m_hProcess, (LPVOID)base, pData, data->Length, &bytesWritten) );
}

// Detach from the debuggee
void DebuggedProcess::Detach()
{
	ASSERT(Id != 0);

	// If the debuggee is broken
	if (m_lastStoppingEvent != Invalid)
	{
		if (LastDebugEventWasBreakpoint())
		{
			// If the debuggee is still at a breakpoint, the IP must be rewound and the breakpoint exception continued.
			// The original instruction has already been restored because the bound breakpoints are removed before this call.
			RewindInstructionPointer(m_lastDebugEvent.dwThreadId, 1);
			ContinueDebugEvent(true);
		}
		else if (m_lastStoppingEvent == Exception)
		{
			// If the last event wasn't a breakpoint exception, then leave the exception unhandled.
			ContinueDebugEvent(false);
		}
		else
		{
			// All other events should be continued handled
			ContinueDebugEvent(true);
		}
	}

	// Actually perform the detach
	Win32BoolCall(::DebugActiveProcessStop(Id));

	// Send program destroy to let the UI know debugging has ended.
    m_callback->OnProgramDestroy(0);
}

// Terminate the debuggee process.
void DebuggedProcess::Terminate()	
{
	assert(m_hProcess != NULL);

	// Forcefully kill the process. 
	Win32BoolCall(::TerminateProcess(m_hProcess, 0));

	// Send program destroy to let the UI know debugging has ended.
    m_callback->OnProgramDestroy(0);
}

// Wrapper around wait for debug event with infinite delay
bool DebuggedProcess::WaitForDebugEvent()
{
	return this->WaitForDebugEvent(INFINITE);
}

// Wrapper around wait for debug event with a timeout
bool DebuggedProcess::WaitForDebugEvent(DWORD dwMilliSeconds)
{
	ASSERT(GetCurrentThreadId() == this->PollThreadId);
	ASSERT(m_lastDebugEvent.dwDebugEventCode == 0);

	bool toRet = ::WaitForDebugEvent(&m_lastDebugEvent, dwMilliSeconds);

	// Since the finializer for DebuggedProcess will delete m_lastDebugEvent, make sure that there 
	// is no way that the GC will this this object is dead while we are in the call.
	GC::KeepAlive(this);

	return toRet;
}

// Continue from a debug event that was given to the debugger via WaitForDebugEvent.
void DebuggedProcess::ContinueDebugEvent(bool fExceptionHandled)
{
	ASSERT(GetCurrentThreadId() == this->PollThreadId);

	const DWORD dwContinueStatus = fExceptionHandled ? DBG_EXCEPTION_HANDLED : DBG_CONTINUE;

	// The system passes an open handle to the binary in the CREATE_PROCESS_DEBUG_EVENT and the LOAD_DLL_DEBUG_EVENT
	// These must be closed before the event is continued. The other handles are closed by the system when the exit process event is sent.
	if (m_lastDebugEvent.dwDebugEventCode == CREATE_PROCESS_DEBUG_EVENT)
	{	
		VERIFY( CloseHandle(m_lastDebugEvent.u.CreateProcessInfo.hFile) );
	}
	else if (m_lastDebugEvent.dwDebugEventCode == LOAD_DLL_DEBUG_EVENT)
	{
		VERIFY( CloseHandle(m_lastDebugEvent.u.LoadDll.hFile) );
	}

	// Tell the OS to continue the event
	Win32BoolCall( ::ContinueDebugEvent(m_lastDebugEvent.dwProcessId, m_lastDebugEvent.dwThreadId, dwContinueStatus) );

	// Clear the last debug event and last stopping event.
	memset(&m_lastDebugEvent, 0, sizeof(DEBUG_EVENT));
	m_lastStoppingEvent = Invalid;

	// Restart the event pump if it is currently not pumping.
	m_fIsPumpingDebugEvents = true;

	// Since the finializer for DebuggedProcess will delete m_lastDebugEvent, make sure that there 
	// is no way that the GC will this this object is dead while we are in the call.
	GC::KeepAlive(this);
}

// Called when an exception event was the result of an AsyncBreak. See DebuggedProcess::Break for 
// who this is setup.
bool DebuggedProcess::HandleAsyncBreakException(const EXCEPTION_DEBUG_INFO* exceptionDebugInfo)
{
	// The sample debugger will assume the first breakpoint after issuing an async break is the async break completing.
	// A production debugger will want to walk the stack and make sure this is the async-break.
	m_fExpectingAsyncBreak = false;

	// Send the AyncBreakComplete event.
	m_lastStoppingEvent = AyncBreakComplete;
	msclr::lock lock(m_threadIdMap);
	{
		DebuggedThread^ thread = m_threadIdMap[m_lastDebugEvent.dwThreadId];						
		m_callback->OnAsyncBreakComplete(thread);				
	}

	// Don't continue this exception (enter break-mode)
	return false;
}

// Called when an exception is due to a breakpoint.
bool DebuggedProcess::HandleBreakpointException(const EXCEPTION_DEBUG_INFO* exceptionDebugInfo)
{
	// Determine if there is an expected breakpoint at this location.

	// hold this lock to ensure the clients collection does not change until this function is done.
	msclr::lock lock(m_breakpointMap);
	DWORD dwBreakpointAddress = (DWORD)(exceptionDebugInfo->ExceptionRecord.ExceptionAddress);
	BreakpointData^ bpData = FindBreakpointAtAddress(dwBreakpointAddress);
	if (bpData != nullptr)
	{
		// Send the breakpoint event.
		m_lastStoppingEvent = Breakpoint;	

		msclr::lock lock(m_threadIdMap);
		{
			DebuggedThread^ thread = m_threadIdMap[m_lastDebugEvent.dwThreadId];	

			// Copy the clients collection so that changes to the breakpoint collection will not be affected by the handler.
			System::Collections::Generic::List<System::Object^>^ objectList = gcnew System::Collections::Generic::List<System::Object^>();

			System::Collections::Generic::LinkedListNode<System::Object^>^ currNode = bpData->Clients->First;
			while (currNode != nullptr)
			{
				objectList->Add(currNode->Value);
				currNode = currNode->Next;
			}
			System::Diagnostics::Debug::Assert(objectList->Count > 0);

			typedef System::Collections::Generic::IList<System::Object^> ObjectListType;
			typedef System::Collections::ObjectModel::ReadOnlyCollection<System::Object^> ReadOnlyCollection;
			ReadOnlyCollection^ clientCollection = gcnew ReadOnlyCollection((ObjectListType^)objectList);

			m_callback->OnBreakpoint(thread, clientCollection, dwBreakpointAddress);				
		}

		// Don't continue this exception (enter break-mode)
		return false;
	}
	else
	{
		// The debuggee contained a unexpected breakpoint instruction. 
		// The sample debugger will not handle this case. A production debugger will want to enter break-mode and notify the UI.

		ASSERT(!L"Unexpected breakpoint event in the debugee. This is must likely a bug in the sample debugger");
		return true;
	}
}

// When recovering from a breakpoint, the sample debugger will enable the single-step (trap) flag on the processor.
// This will cause a single-step exception after the next instruction executes. The debugger will then restore
// the breakpoint instruction and continue handling the exception
bool DebuggedProcess::HandleBreakpointSingleStepException(DWORD dwThreadId, const EXCEPTION_DEBUG_INFO* exceptionDebugInfo)
{
	if (m_singleStepBreakpoint)
	{
		msclr::lock lock(m_threadIdMap);
		DebuggedThread^ thread = m_threadIdMap[dwThreadId];
		ASSERT(thread != nullptr);

		array<byte>^ data = gcnew array<byte>(1);
		data[0] = BreakpointInstruction;
		WriteMemory((DWORD_PTR)(m_singleStepBreakpoint->Address), data);
		Win32BoolCall(FlushInstructionCache(m_hProcess, NULL, NULL));

		// Continue this excecption (don't enter break-mode)
		return true;
	}
	
	// This was not the expected single-step exception. The sample debugger does not handle this case.
	ASSERT(!L"Unexpected breakpoint event in the debugee. This is must likely a bug in the sample debugger");
	return false;
}

// Parse a debug event and determine the correct action to take.
// The debug events come from WaitForDebugEvent.
bool DebuggedProcess::DispatchDebugEvent()
{
	ASSERT(GetCurrentThreadId() == this->PollThreadId);

	bool fContinue = true;	

	switch (m_lastDebugEvent.dwDebugEventCode)
	{
	case EXCEPTION_DEBUG_EVENT: /* 1 */
		{
			if (!m_fSeenEntrypointBreakpoint)
			{
				// The Win32 Debugger API sends a breakpoint event once all of the modules for the process are loaded but before any code runs.
                // this is the physical entrypoint not the logical user entrypoint. Send the Load Complete stopping event to the debugger.
                msclr::lock lock(m_threadIdMap);
                {
                    DWORD key = m_lastDebugEvent.dwThreadId;
				    DebuggedThread^ thread =  m_threadIdMap[key];

                    m_lastStoppingEvent = LoadComplete;
                    m_callback->OnLoadComplete(thread);
				    m_fSeenEntrypointBreakpoint = true;
                    fContinue = false;
                }
			}
			else
			{
				const EXCEPTION_DEBUG_INFO* exceptionDebugInfo = &(m_lastDebugEvent.u.Exception);

				if (IsBreakpointException(exceptionDebugInfo->ExceptionRecord.ExceptionCode))
				{
					if (IsExpectingAsyncBreak())
					{
						fContinue = HandleAsyncBreakException(exceptionDebugInfo);
					}
					else
					{
						fContinue = HandleBreakpointException(exceptionDebugInfo);
					}		
				}
				else if (IsSingleStepException(exceptionDebugInfo->ExceptionRecord.ExceptionCode) && m_fExpectingBreakpointSingleStep)
				{
					fContinue = HandleBreakpointSingleStepException(m_lastDebugEvent.dwThreadId, exceptionDebugInfo);
				}
				else
				{
					// An exception occured in the debuggee which was not an exception expected by the debugger.                   
					// The sample debugger does not handle this case. 
                    // A production debugger will want to enter break-mode and notify the UI.
					ASSERT(!L"Unexpected exception occurred in the debuggee. This could be an actual exception or a bug in the sample engine"); 
				}
			}
		}
		break;
	case CREATE_THREAD_DEBUG_EVENT: /* 2 */
		{
			DebuggedThread^ thread = CreateThread(m_lastDebugEvent.dwThreadId, m_lastDebugEvent.u.CreateThread.hThread, (DWORD_PTR)m_lastDebugEvent.u.CreateThread.lpStartAddress);
			m_callback->OnThreadStart(thread);
		}
		break;
	case CREATE_PROCESS_DEBUG_EVENT: /* 3 */
		{
			ASSERT(m_moduleList->Count == 1);

			DebuggedModule^ module = m_moduleList->First->Value;		

			CComBSTR bstrModuleName;
			CComBSTR bstrSymbolPath;
			bstrModuleName.Attach((BSTR)(System::Runtime::InteropServices::Marshal::StringToBSTR(module->Name).ToInt32()));

			// Load symbols for the application's exe. This is the only symbol file the sample engine will load.
			if (m_pSymbolEngine->LoadSymbolsForModule(bstrModuleName, &bstrSymbolPath))
			{
				module->SymbolsLoaded = true;
				module->SymbolPath = gcnew String(bstrSymbolPath);
			}	

			m_entrypointModule = module;
			DebuggedThread^ thread = CreateThread(m_lastDebugEvent.dwThreadId, m_lastDebugEvent.u.CreateProcessInfo.hThread, (DWORD_PTR)m_lastDebugEvent.u.CreateProcessInfo.lpStartAddress);

			if (m_debugMethod == Launch)
			{
				// Because of Com-re-entrancy, the engine must wait to send the fake mod-load and thread create events until after the
				// launch is complete. Save these references so the call to ResumeFromLaunch can send the events.
				m_entrypointModule = module;
				m_entrypointThread = thread;

                // Do not continue the create process event until after the call to ResumeFromLaunch.
                fContinue = false;
			}
			else
			{
				// This is an attach.
				// Fake up a thread create event for the entrypoint module and the first thread in the process for attach
				m_callback->OnModuleLoad(module);
				m_callback->OnSymbolSearch(module, module->SymbolPath, module->SymbolsLoaded);
				m_callback->OnThreadStart(thread);
			}
		}
		break;
	case EXIT_THREAD_DEBUG_EVENT: /* 4 */
		{
			DWORD key = m_lastDebugEvent.dwThreadId;
			
			DebuggedThread^ thread = nullptr;
			// remove from map
			{
				msclr::lock lock(m_threadIdMap);

				thread = m_threadIdMap[key];
				m_threadIdMap->Remove(key);
			}
			// remove from list
			{
				msclr::lock lock(m_threadList);
				m_threadList->Remove(thread->Node);
			}

			thread->Node = nullptr;

			m_callback->OnThreadExit(thread, m_lastDebugEvent.u.ExitThread.dwExitCode);
		}
		break;
	case EXIT_PROCESS_DEBUG_EVENT: /* 5 */
		{
			// Save the exit code before ContinueDebugEvent
			const DWORD exitCode = m_lastDebugEvent.u.ExitProcess.dwExitCode;
			
			// Continue the debug event now because we may get Closed anytime after we notify the callback.
			ContinueDebugEvent(false);

			m_callback->OnProcessExit(exitCode);

			return false; // stop the event pump
		}
		break;
	case LOAD_DLL_DEBUG_EVENT: /* 6 */
		{
			DebuggedModule^ module = CreateModule(m_lastDebugEvent.u.LoadDll);

			m_callback->OnModuleLoad(module);

			// The sample engine does not attempt to load symbols for modules that are not the exe. Real engines will want to load 
			// symbols in response to a LOAD_DLL_DEBUG_EVENT
			m_callback->OnSymbolSearch(module, nullptr, false);		
		}
		break;
	case UNLOAD_DLL_DEBUG_EVENT: /* 7 */
		{
			DWORD_PTR key = (DWORD_PTR)m_lastDebugEvent.u.UnloadDll.lpBaseOfDll;

			DebuggedModule^ module = nullptr;

			// Remove from the map
			{
				msclr::lock lock(m_moduleAddressMap);

				module = m_moduleAddressMap[key];
				m_moduleAddressMap->Remove(key);
			}

			// Decrement the load order for anything loaded after. No need to lock since we aren't updating here
			// and all updates happen on this thread.
			for (DebuggedModuleNode^ node = module->Node->Next; node != nullptr; node = node->Next)	
			{
				node->Value->DecrementLoadOrder();
			}

			// Remove from the list
			{
				msclr::lock lock(m_moduleList);
				m_moduleList->Remove(module->Node);
			}
			
			module->Node = nullptr;

			m_callback->OnModuleUnload(module);
		}
		break;
	case OUTPUT_DEBUG_STRING_EVENT: /* 8 */
		{
			size_t cbBuffer = (size_t)(m_lastDebugEvent.u.DebugString.fUnicode ? 
											m_lastDebugEvent.u.DebugString.nDebugStringLength * 2 : 
											m_lastDebugEvent.u.DebugString.nDebugStringLength);
			SIZE_T cbActual = 0;
			char* rgBuffer = new char[cbBuffer];
			
			try
			{
				if (::ReadProcessMemory(this->m_hProcess, (void*)m_lastDebugEvent.u.DebugString.lpDebugStringData, rgBuffer, cbBuffer, &cbActual))
				{
					String^ outputStr = gcnew String(rgBuffer);
					m_callback->OnOutputString(outputStr);
				}
			}
			finally
			{
				delete [] rgBuffer;
			}
		}
		break;
	case RIP_EVENT: /* 9 */
		{
			m_callback->OnError(HRESULT_FROM_WIN32(m_lastDebugEvent.u.RipInfo.dwError));
		}
		break;
	default:
		ThrowHR(E_UNEXPECTED);
	}

	return fContinue; 
}

DebuggedModule^ DebuggedProcess::CreateModule(const LOAD_DLL_DEBUG_INFO& loadEvent)
{
	ASSERT(GetCurrentThreadId() == this->PollThreadId);

	const DWORD_PTR moduleBase = (DWORD_PTR)loadEvent.lpBaseOfDll;
	String^ filePath = m_resolver->ResolveMappedFile(m_hProcess, moduleBase, loadEvent.hFile);;

	DWORD dwFileSize = GetImageSizeFromPEHeader(m_hProcess ,loadEvent.lpBaseOfDll);

	DebuggedModule^ loadedModule = gcnew DebuggedModule(moduleBase, dwFileSize, filePath, m_moduleList->Count + 1);
	
	// Add to the map
	{
		msclr::lock lock(m_moduleAddressMap);
		m_moduleAddressMap->Add(loadedModule->BaseAddress, loadedModule->Size, loadedModule);
	}

	// Add to the list
	{
		msclr::lock lock(m_moduleList);
		loadedModule->Node = m_moduleList->AddLast(loadedModule);
	}

	return loadedModule;
}

DebuggedThread^ DebuggedProcess::CreateThread(DWORD threadId, HANDLE hThread, DWORD startAddress)
{
	HANDLE hCurrentProcess = GetCurrentProcess();

	HANDLE hThreadCopy;
	Win32BoolCall( DuplicateHandle(hCurrentProcess, hThread, hCurrentProcess, &hThreadCopy, 0, FALSE, DUPLICATE_SAME_ACCESS) );

	DebuggedThread^ thread = gcnew DebuggedThread(hThreadCopy, threadId, startAddress);

	// add to the map
	{
		msclr::lock lock(m_threadIdMap);
		m_threadIdMap->Add(threadId, thread);
	}

	// add to the list
	{
		msclr::lock lock(m_threadList);
		thread->Node = m_threadList->AddLast(thread);
	}

	return thread;
}

array<DebuggedThread^>^ DebuggedProcess::GetThreads()
{
	{
		msclr::lock lock(m_threadList);

		array<DebuggedThread^, 1>^ threads = gcnew array<DebuggedThread^, 1>(m_threadList->Count);
		
		m_threadList->CopyTo(threads, 0);

		return threads;
	}
}

array<DebuggedModule^>^ DebuggedProcess::GetModules()
{
	{
		msclr::lock lock(m_moduleList);

		array<DebuggedModule^, 1>^ modules = gcnew array<DebuggedModule^, 1>(m_moduleList->Count);
		
		m_moduleList->CopyTo(modules, 0);

		return modules;
	}
}

void DebuggedProcess::Continue(DebuggedThread^ thread)
{
	ASSERT(Worker::MainThreadId != Worker::CurrentThreadId);
	ASSERT(m_lastStoppingEvent != Invalid);

	if (m_lastStoppingEvent == Breakpoint)
	{
		// If the last stopping event was a breakpoint, the debuggee must have the breakpoint cleared 
		// the original instruction executed, and then have the breakpoint re-established. 
		RecoverFromBreakpoint();
	}

	ContinueDebugEvent(true);
}	

void DebuggedProcess::Execute(DebuggedThread^ thread)
{
	ASSERT(Worker::MainThreadId != Worker::CurrentThreadId);
	ASSERT(m_lastStoppingEvent != Invalid);

	if (m_lastStoppingEvent == Breakpoint)
	{
		// If the last stopping event was a breakpoint, the debuggee must have the breakpoint cleared 
		// the original instruction executed, and then have the breakpoint re-established. 
		RecoverFromBreakpoint();
	}

	// Continue the debug event handling the last exception.
	ContinueDebugEvent(true);
}	

// Initiate an x86 stack walk on this thread.
void DebuggedProcess::DoStackWalk(DebuggedThread^ thread)
{
	CComPtr<IDiaStackWalker> pDiaStackWalker;
	HRESULT hr = ::CoCreateInstance(CLSID_DiaStackWalker, 
		                            NULL, 
									CLSCTX_INPROC_SERVER, 
									IID_IDiaStackWalker, 
									(LPVOID*)&pDiaStackWalker);

	// Make a native copy of the modules so they can be accessed from the purely native stack walk helper class.
	ModuleInfo* pModuleInfos = NULL;
	int cModuleList = 0;
	try
	{
		msclr::lock lock(m_moduleList);
		{
			cModuleList = m_moduleList->Count;
			ModuleInfo* pModuleInfos = new ModuleInfo[cModuleList];
			System::Collections::Generic::LinkedListNode<DebuggedModule^>^ node = m_moduleList->First;
			
			for (int i = 0; i < cModuleList; i++)
			{
				pModuleInfos[i].BaseAddress = node->Value->BaseAddress;
				pModuleInfos[i].Size = node->Value->Size;
				node = node->Next;
			}
		}

		DiaStackWalkHelper* pHelper = new DiaStackWalkHelper(m_pSymbolEngine, 
															(HANDLE)this->m_hProcess, 
															(HANDLE(thread->Handle)),
															pModuleInfos,
															cModuleList);
		pHelper->Initialize();
		CComQIPtr<IDiaStackWalkHelper> pStackWalkHelper = pHelper;

		CComPtr<IDiaEnumStackFrames> pDiaStackFramesEnum;
		pDiaStackWalker->getEnumFrames(pHelper, &pDiaStackFramesEnum);

		ULONG cActual;
		CComPtr<IDiaStackFrame> pStackFrame;
		thread->ClearStackFrames();

		pDiaStackFramesEnum->Reset();
		pDiaStackFramesEnum->Next(1, &pStackFrame, &cActual);
		while (cActual == 1)
		{		
			thread->AddStackFrame(gcnew X86ThreadContext(Worker::ContextFromFrame(pStackFrame)));
			pStackFrame.Release();
			pDiaStackFramesEnum->Next(1, &pStackFrame, &cActual);
		}
	}
	finally
	{
		if (pModuleInfos != NULL)
		{
			delete [] pModuleInfos;
		}
	}
}

bool DebuggedProcess::GetSourceInformation(unsigned int ip, String^% documentName, String^% functionName, unsigned int% dwLineNumber, unsigned int% numParameters, unsigned int% numLocals)
{	
	// If the debuggee is currently at a breakpoint, and the requested ip is the ip of that breakpoint, then back-up one instruction and do the search.
	if (LastDebugEventWasBreakpoint())
	{
		const EXCEPTION_DEBUG_INFO* exceptionDebugInfo = &(m_lastDebugEvent.u.Exception);
		
		if ((unsigned int)(exceptionDebugInfo->ExceptionRecord.ExceptionAddress) == (ip - 1))
		{
			ip--;
		}
	}

	// find the base of the containing module so the offset from the beginning of the symbol can be found
	DebuggedModule^ module = ResolveAddress(ip);
	if (module == nullptr)
	{
		assert(module != nullptr);
		return false;
	}

	CComBSTR bstrDocumentName;
	CComBSTR bstrFunctionName;
	DWORD dwIpRVA = ip - module->BaseAddress;

	CComBSTR bstrModuleName;
	bstrModuleName.Attach((BSTR)(System::Runtime::InteropServices::Marshal::StringToBSTR(module->Name).ToPointer()));

	DWORD dwLineNum;
	DWORD dwNumParameters;
	DWORD dwNumLocals;
	HRESULT hr = m_pSymbolEngine->FindSourceForAddr(bstrModuleName, module->BaseAddress, dwIpRVA, &bstrDocumentName, &bstrFunctionName, &dwLineNum, &dwNumParameters, &dwNumLocals); 

	if (FAILED(hr) || hr == S_FALSE)
	{
		return false;
	}

	documentName = gcnew String(bstrDocumentName);
	functionName = gcnew String(bstrFunctionName);
	dwLineNumber = dwLineNum;
	numParameters = dwNumParameters;
	numLocals = dwNumLocals;

	return true;
}

void DebuggedProcess::GetFunctionArgumentsByIP(unsigned int ip, unsigned int bp, array<VariableInformation^>^ arguments)
{
	GetFunctionVariablesByIP(ip, bp, DataIsParam, arguments);
}

void DebuggedProcess::GetFunctionLocalsByIP(unsigned int ip, unsigned int bp, array<VariableInformation^>^ locals)
{
	GetFunctionVariablesByIP(ip, bp, DataIsLocal, locals);
}

void DebuggedProcess::GetFunctionVariablesByIP(unsigned int ip, unsigned int bp, DWORD dwDataKind, array<VariableInformation^>^ variables)
{
	DebuggedModule^ module = ResolveAddress(ip);
    DWORD dwIpRVA = ip - module->BaseAddress;

	for (int i = 0; i < variables->Length; i++)
	{
		CComBSTR bstrVarName;	
		CComBSTR bstrVarType;		
		bool fBuiltInType;
		DWORD dwOffset;
		DWORD dwIndirectionLevel = 0;
		m_pSymbolEngine->GetVarForAddr(module->BaseAddress, 
										dwIpRVA, 
										dwDataKind, 
										i, 
										&bstrVarName, 
										&bstrVarType, 
										&fBuiltInType, 
										&dwOffset, 
										&dwIndirectionLevel);

		variables[i] = VariableInformation::Create(this, bp, bstrVarName, bstrVarType, fBuiltInType, dwOffset, dwIndirectionLevel);
	}
}

array<unsigned int>^ DebuggedProcess::GetAddressesForSourceLocation(String^ moduleName, String^ documentName, DWORD dwStartLine, DWORD dwStartCol)
{
	HRESULT hr = S_OK;
	Collections::Generic::LinkedList<DebuggedModule^>^ modulesToSearch;
	CComBSTR bstrDocumentName;
	bstrDocumentName.Attach((BSTR)(System::Runtime::InteropServices::Marshal::StringToBSTR(documentName).ToPointer()));
	DebuggedModuleNode^ currNode = nullptr;

	msclr::lock lock(m_moduleList);

	if (moduleName != nullptr && m_moduleList->First != nullptr)
	{
		currNode = m_moduleList->First;
		// Find the module that matches the requested module name.
		
		while (currNode != nullptr)
		{
			if (String::Compare(currNode->Value->Name, moduleName, false) == 0)
			{
				modulesToSearch = gcnew Collections::Generic::LinkedList<DebuggedModule^>();
				modulesToSearch->AddLast(currNode->Value);
				break;
			}
			currNode = currNode->Next;
		}

		Debug::Assert(modulesToSearch != nullptr);
	}
	else
	{
		// Search all the modules.
		modulesToSearch = m_moduleList;
	}

	Collections::Generic::List<unsigned int>^ addresses = gcnew Collections::Generic::List<unsigned int>();
	currNode = m_moduleList->First;
	while (currNode != nullptr)
	{

		DebuggedModule^ mod = currNode->Value;

		DWORD dwAddress = 0;

		if (mod->SymbolsLoaded)
		{
			// The sample engine assumes each location will only bind to one location in the debuggee.
			hr = m_pSymbolEngine->GetAddressForSourceLocation(mod->BaseAddress, 
															  bstrDocumentName, 
															  dwStartLine, 
															  dwStartCol, 
															  &dwAddress);

			if (FAILED(hr))
			{
				ThrowHR(hr);
			}

			if (hr == S_OK)
			{
				addresses->Add(dwAddress);	
			}
		}	

		currNode = currNode->Next;
	}

	return addresses->ToArray();
}

// when continuing from a breakpoint, the original instruction must be restored.
// The processor is then single-stepped over the original instruction and finally,
// the breakpoint instruction is written again. The debug engine does not support
// embedded breakpoints (breakpoints the user embedded). If it did, special treatment
// would be needed.
void DebuggedProcess::RecoverFromBreakpoint()
{
	ASSERT(m_lastDebugEvent.dwDebugEventCode != 0); // should be stopped

	const EXCEPTION_DEBUG_INFO* exceptionDebugInfo = &(m_lastDebugEvent.u.Exception);
	System::Diagnostics::Debug::Assert(IsBreakpointException(exceptionDebugInfo->ExceptionRecord.ExceptionCode));

	// Back the instruction pointer up one byte regardless of whether or not the breakpoint is found. 
	// this is to account for the case where the breakpoint may have been deleted.
	RewindInstructionPointer(m_lastDebugEvent.dwThreadId, 1);

	// Find the bpdata at this location.
	DWORD dwBreakpointAddress = (DWORD)(exceptionDebugInfo->ExceptionRecord.ExceptionAddress);
	BreakpointData^ bpData = FindBreakpointAtAddress(dwBreakpointAddress);
	if (bpData != nullptr)
	{
		// Restore the original instruction in the debuggee
		array<byte>^ data = gcnew array<byte>(1);
		data[0] = bpData->OriginalData;
		WriteMemory(dwBreakpointAddress, data);

		// Enable single-stepping on the processor. When the debuggee is continued, it will execute one instruction,
		// and then fire the single step event to the debugger.
		EnableSingleStep(m_lastDebugEvent.dwThreadId);

		// Keep track of which breakpoint is being stepped over.
		m_singleStepBreakpoint = bpData;

		// Set that the debugger is currently expecting a single-step exception.
		m_fExpectingBreakpointSingleStep = true;
	}
}

DWORD DebuggedProcess::GetImageSizeFromPEHeader(HANDLE hProcess, LPVOID lpDllBase)
{
	IMAGE_DOS_HEADER dosHeader = {0};
	SIZE_T cActual;
	if (!::ReadProcessMemory(hProcess, lpDllBase, &dosHeader, sizeof (dosHeader), &cActual)) 
	{
		assert(!"Failed to read IMAGE_DOS_HEADER from loaded module");
		return 0;
	}

	IMAGE_NT_HEADERS ntHeaders = {0};
	if (!::ReadProcessMemory(hProcess, (LPVOID)((DWORD)(dosHeader.e_lfanew) + (DWORD)(lpDllBase)), &ntHeaders, sizeof (ntHeaders), &cActual)) 
	{
		assert(!"Failed to read IMAGE_NT_HEADERS from loaded module");
		return 0;
	}

	return ntHeaders.OptionalHeader.SizeOfImage;
}	

// Enable the single step flag on the context for this thread
void DebuggedProcess::EnableSingleStep(DWORD dwThreadId)
{
	msclr::lock lock(m_threadIdMap);
	DebuggedThread^ thread = m_threadIdMap[dwThreadId];

	ASSERT(thread != nullptr);

	CONTEXT context = {0};
	context.ContextFlags = CONTEXT_CONTROL;
	::Win32BoolCall(::GetThreadContext((HANDLE)thread->Handle, &context));

	// Set the trap flag
	const DWORD trapFlagBitMask = 0x100;
	context.EFlags |= trapFlagBitMask;

	::Win32BoolCall(::SetThreadContext((HANDLE)thread->Handle, &context));
}

// Rewind a thread's instruction pointer a certain number of bytes.
void DebuggedProcess::RewindInstructionPointer(DWORD dwThreadId, DWORD dwNumBytes)
{
	msclr::lock lock(m_threadIdMap);
	DebuggedThread^ thread = m_threadIdMap[dwThreadId];

	ASSERT(thread != nullptr);

	CONTEXT context = {0};
	context.ContextFlags = CONTEXT_CONTROL;
	::Win32BoolCall(::GetThreadContext((HANDLE)thread->Handle, &context));

	// back the instruction pointer back the number of requested bytes
	context.Eip = context.Eip - dwNumBytes; 

	::Win32BoolCall(::SetThreadContext((HANDLE)thread->Handle, &context));
}


// Convert an dia IDiaStackFrame to a CONTEXT structure for x86
CONTEXT Worker::ContextFromFrame(IDiaStackFrame* pStackFrame)
{
	assert(pStackFrame != NULL);
	CONTEXT context;
	
	ULONGLONG val;
	pStackFrame->get_registerValue(CV_REG_EAX, &val);
	context.Eax = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_ECX, &val);
	context.Ecx = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_EDX, &val);
	context.Edx = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_EBX, &val);
	context.Ebx = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_ESP, &val);
	context.Esp = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_EBP, &val);
	context.Ebp = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_EIP, &val);
	context.Eip = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_EDX, &val);
	context.Edx = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_ESI, &val);
	context.Esi = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_EDI, &val);	
	context.Edi = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_EFLAGS, &val);	
	context.EFlags = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_CS, &val);	
	context.SegCs = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_FS, &val);	
	context.SegFs = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_ES, &val);	
	context.SegEs = (DWORD)val;

	pStackFrame->get_registerValue(CV_REG_DS, &val);	
	context.SegDs = (DWORD)val;

	return context; // Shallow copy is fine
}

