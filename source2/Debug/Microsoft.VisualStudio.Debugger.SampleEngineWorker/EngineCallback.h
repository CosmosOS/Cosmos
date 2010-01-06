#pragma once

BEGIN_NAMESPACE

ref class DebuggedThread;
ref class DebuggedModule;

public interface class ISampleEngineCallback
{
	void OnModuleLoad(DebuggedModule^ module);
	void OnModuleUnload(DebuggedModule^ module);
	void OnThreadStart(DebuggedThread^ thread);
	void OnThreadExit(DebuggedThread^ thread, DWORD exitCode);
	void OnProcessExit(DWORD exitCode);
	void OnOutputString(String^ outputString);
	void OnError(HRESULT hrErr);
	void OnBreakpoint(DebuggedThread^ thread, Collections::ObjectModel::ReadOnlyCollection<Object^>^ clients, DWORD_PTR address);
	void OnException(DebuggedThread^ thread, DWORD code);
	void OnStepComplete(DebuggedThread^ thread);
	void OnAsyncBreakComplete(DebuggedThread^ thread);
	void OnLoadComplete(DebuggedThread^ thread);
	void OnProgramDestroy(unsigned int exitCode);
	void OnSymbolSearch(DebuggedModule^ module, String^ status, DWORD dwStatsFlags);
	void OnBreakpointBound(Object^ objPendingBreakpoint, unsigned int address);
};


END_NAMESPACE