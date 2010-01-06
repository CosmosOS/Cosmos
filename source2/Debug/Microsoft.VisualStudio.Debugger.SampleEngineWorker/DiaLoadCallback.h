
#include "stdafx.h"
#include "dia2.h"

// This is a native com object to be passed to Dia as a load callback. This is to workaround 
// limitations in the dia interop assembly which has no managed definition of IDiaLoadCallback.
// Dia uses this to inform the debugger about status while it is searching for symbols
// and to allow the debugger to extend the default search behavior. Please see the Dia documentation
// on MSDN for more information.
class DiaLoadCallback : IDiaLoadCallback
{
private:
	long m_refCount;
	CComBSTR m_bstrLastSymbolPath;

public:
	// Non-interface
	DiaLoadCallback() : m_refCount(0), m_bstrLastSymbolPath(NULL)
	{

	}

	HRESULT GetLastSymbolPath(BSTR* pBstr)
	{
		if (pBstr == NULL)
		{
			return E_INVALIDARG;
		}

		m_bstrLastSymbolPath.CopyTo(pBstr);

		return S_OK;
	}
	
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
	// IDiaLoadCallback
	virtual HRESULT STDMETHODCALLTYPE NotifyDebugDir( 
            /* [in] */ BOOL fExecutable,
            /* [in] */ DWORD cbData,
            /* [size_is][in] */ BYTE *pbData)
	{
		return S_OK;
	}
        
    virtual HRESULT STDMETHODCALLTYPE NotifyOpenDBG( 
        /* [in] */ LPCOLESTR dbgPath,
        /* [in] */ HRESULT resultCode)
	{
		return S_OK;
	}
    
    virtual HRESULT STDMETHODCALLTYPE NotifyOpenPDB( 
        /* [in] */ LPCOLESTR pdbPath,
        /* [in] */ HRESULT resultCode)
	{
		if (m_bstrLastSymbolPath.Length() > 0)
		{
			m_bstrLastSymbolPath.Empty();
		}
	
		return m_bstrLastSymbolPath.Append(pdbPath);
	}
    
    virtual HRESULT STDMETHODCALLTYPE RestrictRegistryAccess( void)
	{ 
		return S_FALSE;
	}
    
    virtual HRESULT STDMETHODCALLTYPE RestrictSymbolServerAccess( void)
	{
		return S_FALSE;
	}
};