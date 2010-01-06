
#include "stdafx.h"
#include "symbolengine.h"
#include "diastackwalkhelper.h"

// Helper class used to implement callbacks from dia into the engine during a stackwalk.
// Stackwalking is an implementation detail of the sample and this is not intended to be
// a complete stack walk sample. See the documentation IDiaStackWalkHelper on MSDN for 
// more information.

DiaStackWalkHelper::DiaStackWalkHelper(SymbolEngine* pSymbolEngine, 
									   HANDLE hProcess, 
									   HANDLE hThread,
									   ModuleInfo* pModuleInfos, 
									   int cModuleInfos
									   ) : m_refCount(0), m_fInitialized(false)
{
	m_pSymbolEngine = pSymbolEngine;
	m_hProcess = hProcess;
	m_hThread = hThread;
	m_pModuleInfos = pModuleInfos;
	m_cModuleInfos = cModuleInfos;
}

void DiaStackWalkHelper::Initialize()
{
	if (m_fInitialized)
	{
		assert(!m_fInitialized);
	}

	m_context.ContextFlags = CONTEXT_FULL;
	::GetThreadContext(m_hThread, &m_context);

	for (int i = 0; i < cRegisters; i++)
	{
		m_rgRegisters[i] = 0;
	}

	// Seed the registers collection
	m_rgRegisters[CV_REG_EAX] = m_context.Eax;
	m_rgRegisters[CV_REG_ECX] = m_context.Ecx;
	m_rgRegisters[CV_REG_EDX] = m_context.Edx;
	m_rgRegisters[CV_REG_EBX] = m_context.Ebx;
	m_rgRegisters[CV_REG_ESP] = m_context.Esp;
	m_rgRegisters[CV_REG_EBP] = m_context.Ebp;
	m_rgRegisters[CV_REG_EIP] = m_context.Eip;
	m_rgRegisters[CV_REG_ESI] = m_context.Esi;
	m_rgRegisters[CV_REG_EDI] = m_context.Edi;
	m_rgRegisters[CV_REG_EFLAGS] = m_context.EFlags;
	m_rgRegisters[CV_REG_CS] = m_context.SegCs;
	m_rgRegisters[CV_REG_FS] = m_context.SegFs;
	m_rgRegisters[CV_REG_ES] = m_context.SegEs;
	m_rgRegisters[CV_REG_DS] = m_context.SegDs;

	m_fInitialized = true;
}

// Dia gets and sets register values with calls to get_registerValue and put_registerValue
STDMETHODIMP DiaStackWalkHelper::get_registerValue(DWORD index, ULONGLONG *pRetVal)
{
	if (!pRetVal)
	{
		return E_INVALIDARG;
	}	
	assert(m_fInitialized);

	switch (index)
	{
	case CV_REG_EAX:
	case CV_REG_ECX:	
	case CV_REG_EDX:	
	case CV_REG_EBX:
	case CV_REG_ESP:
	case CV_REG_EBP:
	case CV_REG_EIP:
	case CV_REG_ESI:
	case CV_REG_EDI:
	case CV_REG_EFLAGS:
	case CV_REG_CS:
	case CV_REG_FS:
	case CV_REG_ES:
	case CV_REG_DS:
		
		*pRetVal = m_rgRegisters[index];
		return NOERROR;
	default:
		assert(!"Unknown register being asked for.");
		return E_FAIL;
	}
}

STDMETHODIMP DiaStackWalkHelper::put_registerValue(DWORD index, ULONGLONG NewVal)
{
	assert(m_fInitialized);

	switch (index)
	{
	case CV_REG_EAX:
	case CV_REG_ECX:	
	case CV_REG_EDX:	
	case CV_REG_EBX:
	case CV_REG_ESP:
	case CV_REG_EBP:
	case CV_REG_EIP:
	case CV_REG_ESI:
	case CV_REG_EDI:
	case CV_REG_EFLAGS:
	case CV_REG_CS:
	case CV_REG_FS:
	case CV_REG_ES:
	case CV_REG_DS:
		m_rgRegisters[index] = NewVal;
		return NOERROR;
	default:
		//assert(!"Unknown register being set");
		return E_FAIL;
	}
}

// Dia is asking the sample engine to read memory from the debuggee's address space during the 
// stack walk. This is necessary for finding return address and frame information.
STDMETHODIMP DiaStackWalkHelper::readMemory( 
				MemoryTypeEnum type,
				ULONGLONG va,
				DWORD cbData,
				DWORD *pcbData,
				BYTE *pbData)
{
	// Dia will ask us for a lot more memory than we can actually provide. 
	// So, read as much as possible in bytes and return that.
	// A real debugger would want to cache these memory reads until the next continue.
	bool fContinue = true;
	BYTE* pCurrByte = pbData;
	ULONGLONG currVa = va;
	DWORD totalRead = 0;

	while (totalRead < cbData)
	{
		DWORD cActual;
		if (!::ReadProcessMemory(m_hProcess, (LPVOID)currVa, pCurrByte, 1, &cActual))
		{
			// We've read as far as we can.
			fContinue = false;
			break;
		}
		totalRead += cActual;
		currVa++;
		pCurrByte++;
	}

	if (totalRead == 0)
	{
		// If we failed to read anything, return an error;
		return E_FAIL;
	}

	*pcbData = totalRead + 1; 
	
	return S_OK;
}

// Dia calls this to ask the engine to find the return address.
// This sample does not extend the default behavior.
STDMETHODIMP DiaStackWalkHelper::searchForReturnAddress( 
    IDiaFrameData *frame,
    ULONGLONG *returnAddress)
{
	// Use the default search
	return E_NOTIMPL;
}

// Dia calls this to ask the engine to find the return address.
// This sample does not extend the default behavior.
STDMETHODIMP DiaStackWalkHelper::searchForReturnAddressStart( 
    IDiaFrameData *frame,
    ULONGLONG startAddress,
    ULONGLONG *returnAddress)
{
	// Use the default search
	return E_NOTIMPL;
}

// The stack walker is asking for the IDiaFrameData
// for a virtual address in the debuggee. This information
// is found in the symbol files and is used when the address
// cannot easily be found because of reasons such as optimization
STDMETHODIMP DiaStackWalkHelper::frameForVA( 
    ULONGLONG va,
    IDiaFrameData **ppFrame)
{
	HRESULT hr = NOERROR;

	CComPtr<IDiaEnumFrameData> pDiaEnumFrameData;
	hr = m_pSymbolEngine->GetEnumFrameData(&pDiaEnumFrameData);

	if (SUCCEEDED(hr))
	{
		hr = pDiaEnumFrameData->frameByVA(va, ppFrame);
	}

	return hr;
}

// The stack walker is asking for the IDiaSymbol
// for a virtual address in the debuggee. 
STDMETHODIMP DiaStackWalkHelper::symbolForVA( 
    ULONGLONG va,
    IDiaSymbol **ppSymbol)
{
	return m_pSymbolEngine->SymbolForVA(va, ppSymbol);
}

// pdata is used for 64 bit platforms. This sample does not support debugging 64 bit applications.
STDMETHODIMP DiaStackWalkHelper::pdataForVA( 
    ULONGLONG va,
    DWORD cbData,
    DWORD *pcbData,
    BYTE *pbData)
{
	return E_NOTIMPL;
}

// The stack walker is asking for the base address for the module at a particular address.
STDMETHODIMP DiaStackWalkHelper::imageForVA( 
    ULONGLONG vaContext,
    ULONGLONG *pvaImageStart)
{
	bool fFound = false;
	for (int i = 0; i < m_cModuleInfos; i++)
	{
		if ((vaContext >= m_pModuleInfos[i].BaseAddress) && 
			(vaContext < m_pModuleInfos[i].BaseAddress + m_pModuleInfos[i].Size))
		{
			*pvaImageStart = m_pModuleInfos[i].BaseAddress;
			fFound = true;
			break;
		}
	}

	if (!fFound)
	{
		return S_FALSE;
	}
	return S_OK;
}

