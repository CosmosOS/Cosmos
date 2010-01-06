#pragma once

#include "AddressDictionary.h"
#include "BreakpointData.h"
#include "SymbolEngine.h"
#include "VariableInformation.h"

BEGIN_NAMESPACE

// Forward-refs
interface class ISampleEngineCallback;
ref class DebuggedModule;
ref class DebuggedThread;
ref class ModuleResolver;
ref class X86ThreadContext;

// If the engine is launching or attaching.
enum DEBUG_METHOD
{
	Launch, 
	Attach
};

// The type of the last stopping event.
enum STOPPING_EVENT_KIND
{
	Invalid,
	StartDebugging,
	Breakpoint,
	Exception,
	StepComplete,
	AyncBreakComplete,
	LoadComplete
};


[Flags]
public enum class ResumeEventPumpFlags
{
	ResumeForStepOrExecute = 0x1,
	ResumeWithExceptionHandled = 0x2
};

// Constants for exception types that are interesting to the sample engine.
// BreakpointExceptionCode is when the debuggee executes and int3. 
// SingleStepExceptionCode is sent when the processor is in trace mode and has completed
// a single-step
const DWORD BreakpointExceptionCode = 0x80000003;
const DWORD SingleStepExceptionCode = 0x80000004;

// DebuggeedProcess represents a process being debugged to the back-end portion of the debug engine sample.
public ref class DebuggedProcess sealed
{
private:
	// immutable fields
	const DEBUG_METHOD m_debugMethod;
	const HANDLE m_hProcess;
	const DWORD m_dwPollThreadId;
	initonly ISampleEngineCallback^ m_callback;
	initonly ModuleResolver^ m_resolver;
	SymbolEngine* m_pSymbolEngine;

	DebuggedThread^ m_entrypointThread;
	DebuggedModule^ m_entrypointModule;

	// LOCKING ORDER:
	// m_threadIdMap must be taken before m_breakpointMap
	// other locks are unordered

	// Only updated on the main thread
	int m_suspendCount;
	
	// Module & Thread and kept in a map and a list. The map is used for indexing,
	// the list is used to return these things in their load order. These can be read on
	// the poll thread without a lock, but should be locked for poll thread updates &
	// non-poll thread reads. Except for cleanup, all updates occur on the poll thread.
	initonly AddressDictionary<DebuggedModule^>^ m_moduleAddressMap;
	initonly Collections::Generic::LinkedList<DebuggedModule^>^ m_moduleList;

	initonly Collections::Generic::Dictionary<DWORD, DebuggedThread^>^ m_threadIdMap;
	initonly Collections::Generic::LinkedList<DebuggedThread^>^ m_threadList;

	// This map can be updated on any thread at any time. It needs to be locked to
	// read or write.
	initonly Collections::Generic::Dictionary<DWORD_PTR, BreakpointData^>^ m_breakpointMap;

	// These fields are only updated on the poll thread
	DEBUG_EVENT& m_lastDebugEvent;
	STOPPING_EVENT_KIND m_lastStoppingEvent;

	// True if the debug loop is currently pumping.
	bool m_fIsPumpingDebugEvents;

	// True if the sample engine has seen the entrypoint breakpoint.
	bool m_fSeenEntrypointBreakpoint;

	// True if the sample engine is expecting an asymbreak event.
	bool m_fExpectingAsyncBreak;

	bool m_fExpectingBreakpointSingleStep;
	BreakpointData^ m_singleStepBreakpoint;

public:
	// The pid of the process
	initonly int Id;

	// The name of the process
	initonly String^ Name;

	// The start address of the process (normally in the CRT)
	initonly DWORD_PTR StartAddress;

	// Resume pumping debug events
	void ResumeEventPump();

	// Async-Break
	void Break();
	void Suspend();
	void Resume();
	void ResumeFromLaunch();
	X86ThreadContext^ GetThreadContext(IntPtr hThread);
	DebuggedModule^ ResolveAddress(DWORD_PTR address);

	void SetBreakpoint(DWORD_PTR address, Object^ client);
	void RemoveBreakpoint(DWORD_PTR address, Object^ client);

	array<byte>^ ReadMemory(DWORD_PTR base, DWORD size);
	unsigned int ReadMemoryUInt(DWORD_PTR base);
	void WriteMemory(DWORD_PTR base, array<byte>^ data);
	void Detach();
	void Terminate();
	void Close();
	void Continue(DebuggedThread^ thread);
	void Execute(DebuggedThread^ thread);
	array<DebuggedThread^>^ GetThreads();
	array<DebuggedModule^>^ GetModules();

	// Initiate an x86 stack walk on this thread.
	void DoStackWalk(DebuggedThread^ thread);

	void WaitForAndDispatchDebugEvent(ResumeEventPumpFlags flags);

	property DWORD PollThreadId
	{
		DWORD get() { return m_dwPollThreadId; }
	}

	property bool IsStopped
	{
		bool get()
		{
			return m_lastDebugEvent.dwDebugEventCode != 0 ||
				m_suspendCount > 0;
		}
	}

	property bool IsPumpingDebugEvents
	{
		bool get()
		{
			return m_fIsPumpingDebugEvents;
		}
	}

public:
	// Symbol handler methods which allow the upper layers to obtain symbol information.
	bool GetSourceInformation(unsigned int ip, String^% documentName, String^% functionName, unsigned int% dwLine, unsigned int% numParameters, unsigned int% numLocals);
	void GetFunctionArgumentsByIP(unsigned int ip, unsigned int bp, array<VariableInformation^>^ arguments);
	void GetFunctionLocalsByIP(unsigned int ip, unsigned int bp, array<VariableInformation^>^ locals);

	array<unsigned int>^ GetAddressesForSourceLocation(String^ moduleName, String^ documentName, DWORD dwStartLine, DWORD dwStartCol);

internal:
	DebuggedProcess(DEBUG_METHOD method, ISampleEngineCallback^ callback, HANDLE hProcess, int processId, String^ name);

private:
	~DebuggedProcess();
	!DebuggedProcess();
	bool WaitForDebugEvent();
	bool WaitForDebugEvent(DWORD dwTimeout);
	void ContinueDebugEvent(bool fExceptionHandled);
	bool DispatchDebugEvent();
	DebuggedModule^ CreateModule(const LOAD_DLL_DEBUG_INFO& debugEvent);
	DebuggedThread^ CreateThread(DWORD threadId, HANDLE hThread, DWORD_PTR startAddress);

	bool HandleAsyncBreakException(const EXCEPTION_DEBUG_INFO* exceptionDebugInfo);
	bool HandleBreakpointException(const EXCEPTION_DEBUG_INFO* exceptionDebugInfo);
	bool HandleBreakpointSingleStepException(DWORD dwThreadId, const EXCEPTION_DEBUG_INFO* exceptionDebugInfo);

	void GetFunctionVariablesByIP(unsigned int ip, unsigned int bp, DWORD dwDataKind, array<VariableInformation^>^ variables);

	DWORD GetImageSizeFromPEHeader(HANDLE hProcess, LPVOID lpDllBase);	

	bool IsExpectingAsyncBreak()
	{
		return m_fExpectingAsyncBreak;
	}

	bool IsBreakpointException(DWORD dwExceptionCode)
	{
		bool xResult = (dwExceptionCode == BreakpointExceptionCode);
		printf("");
		return xResult;
	}

	bool IsSingleStepException(DWORD dwExceptionCode)
	{
		return (dwExceptionCode == SingleStepExceptionCode);
	}

	bool LastDebugEventWasBreakpoint()
	{
		if (m_lastStoppingEvent == Breakpoint && m_lastDebugEvent.dwDebugEventCode == EXCEPTION_DEBUG_EVENT)
		{
			const EXCEPTION_DEBUG_INFO* exceptionDebugInfo = &(m_lastDebugEvent.u.Exception);

			return IsBreakpointException(exceptionDebugInfo->ExceptionRecord.ExceptionCode);
		}

		return false;
	}

	BreakpointData^ FindBreakpointAtAddress(DWORD_PTR address);
	void RecoverFromBreakpoint();

	void EnableSingleStep(DWORD dwThreadId);

	void RewindInstructionPointer(DWORD dwThreadId, DWORD dwNumBytes);
};

END_NAMESPACE
