
#include "stdafx.h"
#include "projinclude.h"
#include "DebuggedModule.h"
#include "SymbolEngine.h"
#include "dialoadcallback.h"


SymbolEngine::SymbolEngine() : m_pDiaDataSource(NULL)
{

}

void SymbolEngine::Initialize()
{
	::CoCreateInstance(CLSID_DiaSource, NULL, CLSCTX_INPROC_SERVER, IID_IDiaDataSource, (LPVOID*)&m_pDiaDataSource);
}

void SymbolEngine::Close()
{	
	if (m_pDiaDataSource != NULL)
	{
		m_pDiaDataSource.Release();
	}

	if (m_pDiaSession != NULL)
	{
		m_pDiaSession.Release();
	}
}

// Ask DIA to load the pdb for this module. No optional search path or callback is passed.
// This means it will only search the path provided in the module itself (i.e. the tools->options->symbol paths 
// option is not honored
bool SymbolEngine::LoadSymbolsForModule(BSTR bstrModuleName, BSTR* pbstrSymbolPath)
{
	HRESULT hr = NOERROR;

	bool fSymbolsLoaded = false;

	// The sample engine only supports loading symbols for one module.
	if (m_pDiaSession != NULL)
	{	
		assert(m_pDiaSession == NULL);
		return false;
	}

	DiaLoadCallback* pCallback = new DiaLoadCallback();
	pCallback->AddRef();
	try
	{
		m_pDiaDataSource->loadDataForExe(bstrModuleName, L"", (IUnknown*)pCallback);
		pCallback->GetLastSymbolPath(pbstrSymbolPath);

		if (SysStringLen(*pbstrSymbolPath) > 0)
		{
			fSymbolsLoaded = true;
		}
	}
	finally
	{
		if (pCallback != NULL)
		{
			pCallback->Release();
			pCallback = NULL;
		}
	}

	if (fSymbolsLoaded && m_pDiaSession == NULL)
	{
		m_pDiaDataSource->openSession(&m_pDiaSession);
	}

	return fSymbolsLoaded;
}

// Given a virual address in the debuggee address space, return the function symbol
// That matches.
HRESULT SymbolEngine::SymbolForVA(ULONGLONG va, IDiaSymbol** ppSymbol)
{
	return m_pDiaSession->findSymbolByVA(va, SymTagFunction, ppSymbol);
}

HRESULT SymbolEngine::GetEnumFrameData(IDiaEnumFrameData** ppEnumFrameData)
{
	HRESULT hr = NOERROR;
	CComPtr<IDiaEnumTables> pEnumTables;
	CComPtr<IDiaTable> pTable;
	REFIID iid = __uuidof(IDiaEnumFrameData);
	ULONG celt = 0;

	hr = m_pDiaSession->getEnumTables(&pEnumTables);

	if (SUCCEEDED(hr))
	{
		while ((hr = pEnumTables->Next(1, &pTable, &celt)) == S_OK && celt == 1)
		{
			HRESULT hr = pTable->QueryInterface(iid, (void**)ppEnumFrameData);
			pTable.Release();
			if (hr == S_OK)
			{
				break;
			}
		}
	}

    return hr;
}

// Given a module name, module load address, and RVA for the instruction pointer, return the document that contains this address, the name of the function that contains the address, 
// the offset for the address and the number of locals and arguments for the function.
HRESULT SymbolEngine::FindSourceForAddr(BSTR bstrModuleName, DWORD dwModuleBase, DWORD dwRvaIp, BSTR* pbstrDocumentName, BSTR* pbstrFunctionName, DWORD* pdwOffset, DWORD* pdwArgs, DWORD* pdwLocals)
{
	HRESULT hr = NOERROR;
	if (m_pDiaSession == NULL)
	{
		assert(!L"Should have a dia session");
		return E_UNEXPECTED;
	}

	// Set the base address of the module so Dia can resole relative addresses.
	hr = m_pDiaSession->put_loadAddress(dwModuleBase);
	assert(SUCCEEDED(hr));

	// Find the function symbol this address. If no function symbol contains this address,
	// NULL is returned in pDiaFunction.
	CComPtr<IDiaSymbol> pDiaFunction;
	hr = m_pDiaSession->findSymbolByRVA(dwRvaIp, SymTagFunction, &pDiaFunction);

	// If a function was found.
	if (pDiaFunction != NULL)
	{		
		// Save some info about the function
		DWORD dwAddrOffset;
		DWORD dwSection;
		ULONGLONG ullLen;
		pDiaFunction->get_addressOffset(&dwAddrOffset);
		pDiaFunction->get_addressSection(&dwSection);
		pDiaFunction->get_length(&ullLen);
		pDiaFunction->get_name(pbstrFunctionName);

		// Enumerate the locals and arguments
		CComPtr<IDiaEnumSymbols> pEnumFunctionData;
		pDiaFunction->findChildren(SymTagData, NULL, nsNone, &pEnumFunctionData);
		LONG cData;
		pEnumFunctionData->get_Count(&cData);

		*pdwArgs = 0;
		*pdwLocals = 0;
		for (int i = 0; i < cData; i++)
		{
			CComPtr<IDiaSymbol> pDiaChild;
			ULONG cACtual2;
			pEnumFunctionData->Next(1, &pDiaChild, &cACtual2);
			
			DWORD dwDataKind;
			pDiaChild->get_dataKind(&dwDataKind);
			
			// This currently only looks at regular locals and parameters. See Dia's documentation for the other data kinds.
			if (dwDataKind == DataIsParam)
			{
				(*pdwArgs)++;
			}
			else if (dwDataKind == DataIsLocal)
			{
				(*pdwLocals)++;
			}
		}	
		
		// Grab the source line information for the function.
		CComPtr<IDiaEnumLineNumbers> pEnumLines;
		m_pDiaSession->findLinesByAddr(dwSection, dwAddrOffset, ullLen, &pEnumLines);

		LONG cLines = 0;
		pEnumLines->get_Count(&cLines);

		// Search for the closest line to the instruction pointer
		CComPtr<IDiaLineNumber> pClosestLineNumber;
		DWORD dwClosestLineNumberRVA = 0;

		for (int i = 0; i < cLines; i++)
		{
			CComPtr<IDiaLineNumber> pLineNumber;
			ULONG cActual;
			hr = pEnumLines->Next(1, &pLineNumber, &cActual);

			if (SUCCEEDED(hr) && cActual == 1)
			{
				DWORD dwLineRVA;
				pLineNumber->get_relativeVirtualAddress(&dwLineRVA);

				DWORD dwLineNumber;
				pLineNumber->get_lineNumber(&dwLineNumber);
				
				if (dwLineRVA <= dwRvaIp && dwLineRVA > dwClosestLineNumberRVA)
				{
					pClosestLineNumber = pLineNumber;	
					dwClosestLineNumberRVA = dwLineRVA;
				}
			}
		}

		// If a line was found, save off the source information.
		if (pClosestLineNumber != NULL)
		{
			CComPtr<IDiaSourceFile> pSourceFile;
			pClosestLineNumber->get_sourceFile(&pSourceFile);
			pSourceFile->get_fileName(pbstrDocumentName);
			pClosestLineNumber->get_lineNumber(pdwOffset);
		}
	}	

	return hr;
}

// Find the virtual address in the debuggee of a specific location in a source file. 
// This function assumes each module only contains one copy of each location.
// That assumption would not be valid in a real debugger.
HRESULT SymbolEngine::GetAddressForSourceLocation(DWORD dwModuleBase, 
													BSTR bstrDocumentName, 
													DWORD dwStartLine, 
													DWORD dwStartCol,
													DWORD* pdwAddress)
{
	HRESULT hr = NOERROR;
	bool fFound = FALSE;

	if (pdwAddress == NULL)
	{
		assert(!L"pdwAddress should not be null");
		return E_POINTER;
	}

	if (m_pDiaSession == NULL)
	{
		assert(!L"Should have a dia session");
		return E_UNEXPECTED;
	}

	m_pDiaSession->put_loadAddress(dwModuleBase);

	// Find all files whose path matches the requested file path
	CComPtr<IDiaEnumSourceFiles> pEnumSourceFiles;
	hr = m_pDiaSession->findFile(NULL, bstrDocumentName, nsFNameExt, &pEnumSourceFiles);

	if (SUCCEEDED(hr))
	{
		assert(pEnumSourceFiles != NULL);
		
		// For each source file
		CComPtr<IDiaSourceFile> pSourceFile;
		ULONG ulFetched = 0;
		while( pEnumSourceFiles->Next(1, &pSourceFile, &ulFetched) == S_OK)
		{
			assert(ulFetched == 1);

			// Enumerate the complilands that contain this source file.
			CComPtr<IDiaEnumSymbols> pEnumCompilands;
			pSourceFile->get_compilands(&pEnumCompilands);
			CComPtr<IDiaSymbol> pCompiland;
			while ( pEnumCompilands->Next ( 1, &pCompiland, &ulFetched ) == S_OK )
			{
				assert(ulFetched == 1);
			
				// Find the  line numbers
				CComPtr<IDiaEnumLineNumbers> pEnumLineNumbers;
				hr = m_pDiaSession->findLinesByLinenum(pCompiland, pSourceFile, dwStartLine, dwStartCol, &pEnumLineNumbers);

				if (SUCCEEDED(hr))
				{
					CComPtr<IDiaLineNumber> pLineNumber;
					while ( pEnumLineNumbers->Next ( 1, &pLineNumber, &ulFetched ) == S_OK )
					{
						assert(ulFetched == 1);
						
						ULONGLONG ullLineNumberVA = 0;
						hr = pLineNumber->get_virtualAddress(&ullLineNumberVA);

						assert(SUCCEEDED(hr));
						*pdwAddress = (DWORD)ullLineNumberVA;

						fFound = true;
						break;
					}
				}
				pCompiland.Release();
			}

			pSourceFile.Release();
		}
	}

	if (SUCCEEDED(hr))
	{
		if (!fFound)
		{
			hr = S_FALSE;
		}
	}

	return hr;
}

// Given a module base address, rva for the instruction pointer, a data kind and an index into the function's variables,
// return information about the variables name, type, including if it is built in and its indirection level. Used
// to construct a list of the parameters and locals for a function.
// 
// For the sample engine, this enumerates the variables for each call. A real debugger may choose to cache this information
// to avoid looking at the symbols multiple times.
HRESULT SymbolEngine::GetVarForAddr(DWORD dwModuleBase, DWORD dwRvaIp, DWORD dwKind, DWORD varNum, BSTR* pbstrVarName, BSTR* pbstrVarType, bool* pfBuiltInType, DWORD* pdwOffset, DWORD* pdwIndirectionLevel)
{
	HRESULT hr = NOERROR;

	assert(pbstrVarName != NULL);
	assert(pbstrVarType != NULL);
	assert(pfBuiltInType != NULL);
	assert(pdwIndirectionLevel != NULL);

	if (m_pDiaSession == NULL)
	{
		assert(!L"Should have a dia session");
		return E_UNEXPECTED;
	}

	// Initialize the indirection level to 0 (no indirection). If the type is a pointer,
	// this will be incremented recursively.
	*pdwIndirectionLevel = 0;

	hr = m_pDiaSession->put_loadAddress(dwModuleBase);

	assert(SUCCEEDED(hr));

	CComPtr<IDiaSymbol> pDiaFunction;
	hr = m_pDiaSession->findSymbolByVA(dwModuleBase + dwRvaIp, SymTagFunction, &pDiaFunction);

	if (pDiaFunction != NULL)
	{
		// Get the function's data children
		CComPtr<IDiaEnumSymbols> pEnumFunctionData;
		hr = pDiaFunction->findChildren(SymTagData, NULL, nsNone, &pEnumFunctionData);

		if (SUCCEEDED(hr))
		{
			LONG cData;
			hr = pEnumFunctionData->get_Count(&cData);

			assert(varNum < (ULONG)cData);

			DWORD dwCurrVar = 0;
 			for (int i = 0; i <= cData; i++)
			{
				CComPtr<IDiaSymbol> pDiaChild;
				ULONG cActual;
				hr = pEnumFunctionData->Next(1, &pDiaChild, &cActual);

				DWORD dwDataKind;
				pDiaChild->get_dataKind(&dwDataKind);
				
				// Check if the is the kind requested by the caller. DataKind describes if this is a local, parameter, static local etc...
				if (dwDataKind == dwKind)
				{			
					// Check the index of the variable
					if (dwCurrVar == varNum)
					{
						pDiaChild->get_name(pbstrVarName);
						pDiaChild->get_offset((LONG*)pdwOffset);

						CComPtr<IDiaSymbol> pVarType;
						pDiaChild->get_type(&pVarType);

						// Get the description of this variables type.
						GetTypeNameDiaType(pVarType, pbstrVarType, pfBuiltInType, pdwIndirectionLevel);

						// Stop Looping.
						break;
					}
					dwCurrVar++;
				}
			}	
		}
	}		

	return hr;
}

// Given a variable symbol (for a local or a parameter, return the type name, if the type is built into the language (i.e. is scaler)
// and the indirection level of the type (is it a pointer and how deep is the indirection.
// For the indirection level, it is assumed *pdwIndirectionLevel is initialized to 0 by the top-level caller since this function
// is recursive.
//
// This function has several limitations. For instance, it doesn't support arrays, but does handle typedefs, udts, and enums
void SymbolEngine::GetTypeNameDiaType(IDiaSymbol* pDiaSymbol, BSTR* pbstrTypeName, bool* pfBuiltInType, DWORD* pdwIndirectionLevel)
{
	assert(pDiaSymbol != NULL);
	assert(pbstrTypeName != NULL);
	assert(pfBuiltInType != NULL);
	assert(pdwIndirectionLevel != NULL);

	DWORD dwSymTag;
	pDiaSymbol->get_symTag(&dwSymTag);

	switch (dwSymTag)
	{
	case SymTagPointerType:
		{
			CComPtr<IDiaSymbol> pBaseType;
			pDiaSymbol->get_type(&pBaseType);
			CComBSTR bstrBaseTypeName;
			(*pdwIndirectionLevel)++;
			GetTypeNameDiaType(pBaseType, &bstrBaseTypeName, pfBuiltInType, pdwIndirectionLevel);		
			*pbstrTypeName = bstrBaseTypeName.Detach();
			break;
		}
	case SymTagBaseType:
		{
			DWORD dwBaseType;
			pDiaSymbol->get_baseType(&dwBaseType);

			CComBSTR bstrTypeName(rgBaseType[dwBaseType]);
			*pbstrTypeName = bstrTypeName.Detach();
			*pfBuiltInType = true;
			break;
		}
	case SymTagUDT:
	case SymTagEnum:
	case SymTagTypedef:
		pDiaSymbol->get_name(pbstrTypeName);
		*pfBuiltInType = false;
		break;
	default:
		assert(!L"Unknown sym tag");
	}
}


