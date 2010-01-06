#pragma once

BEGIN_NAMESPACE

// Basic types
const wchar_t * const rgBaseType[] = {
    L"<NoType>",             // btNoType = 0,
    L"void",                 // btVoid = 1,
    L"char",                 // btChar = 2,
    L"wchar_t",              // btWChar = 3,
    L"signed char",          
    L"unsigned char",
    L"int",                  // btInt = 6,
    L"unsigned int",         // btUInt = 7,
    L"float",                // btFloat = 8,
    L"<BCD>",                // btBCD = 9,
    L"bool",                 // btBool = 10,
    L"short",
    L"unsigned short",
    L"long",                 // btLong = 13,
    L"unsigned long",        // btULong = 14,
    L"__int8",
    L"__int16",
    L"__int32",
    L"__int64",
    L"__int128",
    L"unsigned __int8",
    L"unsigned __int16",
    L"unsigned __int32",
    L"unsigned __int64",
    L"unsigned __int128",
    L"<currency>",           // btCurrency = 25,
    L"<date>",               // btDate = 26,
    L"VARIANT",              // btVariant = 27,
    L"<complex>",            // btComplex = 28,
    L"<bit>",                // btBit = 29,
    L"BSTR",                 // btBSTR = 30,
    L"HRESULT"              // btHresult = 31
};


// This class implements the various symbol searching events for the sample.
// The sample engine only supports loading one symbol file per debugging session.
// A real debugger will need to be capable of loading symbols for multiple modules and
// make queries over all of them. 
public class SymbolEngine
{
private:
	CComPtr<IDiaDataSource> m_pDiaDataSource; 
	CComPtr<IDiaSession> m_pDiaSession;
public:

	SymbolEngine();

	void Initialize();

	void Close();

	bool LoadSymbolsForModule(BSTR bstrModuleName, BSTR* pbstrSymbolPath);

	HRESULT SymbolForVA(ULONGLONG va, IDiaSymbol** ppSymbol);

	HRESULT GetEnumFrameData(IDiaEnumFrameData** ppEnumFrameData);

	HRESULT FindSourceForAddr(BSTR bstrModuleName, DWORD dwModuleBase, DWORD dwRvaIp, BSTR* pbstrDocumentName, BSTR* pbstrFunctionName, DWORD* pdwOffset, DWORD* pdwArgs, DWORD* pdwLocals);

	HRESULT GetVarForAddr(DWORD dwModuleBase, DWORD dwRvaIp, DWORD dwKind, DWORD argNum, BSTR* pbstrArgName, BSTR* pbstrArgType, bool* pfBuiltInType, DWORD* pOffset, DWORD* pdwIndirectionLevel);

	HRESULT GetAddressForSourceLocation(DWORD dwModuleBase, 
										BSTR bstrDocumentName, 
										DWORD dwStartLine, 
										DWORD dwStartCol,
										DWORD* pdwAddress);

private:
	HRESULT FindModuleSymbols(BSTR bstrModuleName, IDiaEnumSymbols** ppModuleSymbols);
	void GetTypeNameDiaType(IDiaSymbol* pDiaSymbol, BSTR* pbstrTypeName, bool* pfBuiltInType, DWORD* pdwIndirectionLevel);
};

END_NAMESPACE