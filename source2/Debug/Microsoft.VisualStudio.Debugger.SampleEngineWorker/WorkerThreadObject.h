#pragma once

BEGIN_NAMESPACE

ref class WorkerThreadObject
{
public:
#ifdef _DEBUG
	const DWORD ThreadId;
#endif

	WorkerThreadObject()
#ifdef _DEBUG
		: ThreadId(GetCurrentThreadId())
#endif
	{
	}
};

END_NAMESPACE