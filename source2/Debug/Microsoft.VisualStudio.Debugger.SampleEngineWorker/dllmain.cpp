// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"
#include "resource.h"

#pragma managed(off)

// Use the ATL Registrar to register the engine. 
class CSampleEngineModule : public CAtlDllModuleT< CSampleEngineModule >
{

};

CSampleEngineModule _SampleEngineModule;
HMODULE _hModThis;


// DllRegisterServer - Adds entries to the system registry
STDAPI DllRegisterServer(void)
{
    // Get this binaries full-path
    WCHAR wszThisFile[MAX_PATH + 1];
    GetModuleFileName(_hModThis, wszThisFile, MAX_PATH + 1);

	// Cut off the FileName. WORKERPATH should point to Microsoft.VisualStudio.Debugger.SampleEngine
	WCHAR wszPath[MAX_PATH + 1];
	WCHAR* wszFileName;
	GetFullPathName(wszThisFile, MAX_PATH + 1, wszPath, &wszFileName);
	*wszFileName = L'\0';

    // Register the sample engine in the Visual Studio registry hive. See SampleEngine.rgs for what is added.
     _ATL_REGMAP_ENTRY rgMap[] =
    {
        {L"WORKERPATH",                   wszPath},
        {NULL, NULL}
    };

    HRESULT hr = _SampleEngineModule.UpdateRegistryFromResourceS(IDR_SAMPLEENGINE, true, rgMap);
	return hr;
}


// DllUnregisterServer - Removes entries from the system registry
STDAPI DllUnregisterServer(void)
{
    // Get this binaries full-path
    WCHAR wszThisFile[MAX_PATH + 1];
    GetModuleFileName(_hModThis, wszThisFile, MAX_PATH + 1);

	// Cut off the FileName. WORKERPATH should point to Microsoft.VisualStudio.Debugger.SampleEngine
	WCHAR wszPath[MAX_PATH + 1];
	WCHAR* wszFileName;
	GetFullPathName(wszThisFile, MAX_PATH + 1, wszPath, &wszFileName);
	*wszFileName = L'\0';

    // Register the sample engine in the Visual Studio registry hive. See SampleEngine.rgs for what is added.
     _ATL_REGMAP_ENTRY rgMap[] =
    {
        {L"WORKERPATH",                   wszPath},
        {NULL, NULL}
    };

    HRESULT hr = _SampleEngineModule.UpdateRegistryFromResourceS(IDR_SAMPLEENGINE, false, rgMap);
	return hr;
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
    _hModThis = hModule;
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

