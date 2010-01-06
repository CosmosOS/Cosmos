
#pragma once

BEGIN_NAMESPACE

// Struct for holding the base address and size for a module.
struct ModuleInfo
{
	DWORD_PTR BaseAddress;
    DWORD Size;
};

// Helper class used to implement callbacks from dia into the engine during a stackwalk.
// Stackwalking is an implementation detail of the sample and this is not intended to be
// a complete stack walk sample. See the documentation IDiaStackWalkHelper on MSDN for 
// more information.
class DiaStackWalkHelper : public IDiaStackWalkHelper 
{
private:
	long m_refCount;

	SymbolEngine* m_pSymbolEngine;
	HANDLE m_hProcess;
	HANDLE m_hThread;
	CONTEXT m_context;
	bool m_fInitialized;
	ModuleInfo* m_pModuleInfos;
	int m_cModuleInfos;

	static const int cRegisters = 1024;
	ULONGLONG m_rgRegisters[cRegisters]; 

public:
	DiaStackWalkHelper(SymbolEngine* pSymbolEngine, HANDLE hProcess, HANDLE hThread, ModuleInfo* pModuleInfos, int cModuleInfos);
	void Initialize();

public:
	// IUnknown
	virtual ULONG STDMETHODCALLTYPE AddRef()
	{
		InterlockedIncrement(&m_refCount);
		return m_refCount;
	}

	virtual ULONG STDMETHODCALLTYPE Release()
	{
		InterlockedDecrement(&m_refCount);
		if (m_refCount == 0)
		{
			delete this;
			return 0;
		}
		return m_refCount;
	}

	virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppvObject)
	{
		if (!ppvObject)
		{
			return E_INVALIDARG;
		}

		if (riid == __uuidof(IDiaLoadCallback))
		{
			*ppvObject = (IDiaLoadCallback*)this;
			AddRef();
			return S_OK;
		}
		else if (riid == __uuidof(IUnknown))
		{
			*ppvObject = (IUnknown*)this;
			AddRef();
			return S_OK;
		}

		return E_NOINTERFACE;
	}
public:

	STDMETHOD(get_registerValue)(DWORD index, ULONGLONG *pRetVal);
    STDMETHOD(put_registerValue)(DWORD index, ULONGLONG NewVal);
    STDMETHOD(readMemory)( 
					MemoryTypeEnum type,
					ULONGLONG va,
					DWORD cbData,
					DWORD *pcbData,
					BYTE *pbData);
    STDMETHOD(searchForReturnAddress)( 
        IDiaFrameData *frame,
        ULONGLONG *returnAddress);
    
    STDMETHOD(searchForReturnAddressStart)( 
        IDiaFrameData *frame,
        ULONGLONG startAddress,
        ULONGLONG *returnAddress);
    
    STDMETHOD(frameForVA)( 
        ULONGLONG va,
        IDiaFrameData **ppFrame);
    
    STDMETHOD(symbolForVA)( 
        ULONGLONG va,
        IDiaSymbol **ppSymbol);
    
    STDMETHOD(pdataForVA)( 
        ULONGLONG va,
        DWORD cbData,
        DWORD *pcbData,
        BYTE *pbData);
    
    STDMETHOD(imageForVA)( 
        ULONGLONG vaContext,
        ULONGLONG *pvaImageStart);
};
END_NAMESPACE