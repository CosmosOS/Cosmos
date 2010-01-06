#pragma once

BEGIN_NAMESPACE

interface class ISampleEngineCallback;
ref class DebuggedProcess;
ref class ProcessLaunchInfo;

public ref class Worker abstract sealed
{
public:
	static void Initialize();
	static DebuggedProcess^ AttachToProcess(ISampleEngineCallback^ callback, int processId);
	static DebuggedProcess^ LaunchProcess(ISampleEngineCallback^ callback, ProcessLaunchInfo ^processLaunchInfo);

	static property DWORD MainThreadId
	{
		DWORD get() { return s_mainThreadId; }
	}

	static property DWORD CurrentThreadId
	{
		DWORD get() { return GetCurrentThreadId(); }
	}

	static CONTEXT ContextFromFrame(IDiaStackFrame* pStackFrame);

private:
	static DWORD s_mainThreadId;
};

END_NAMESPACE