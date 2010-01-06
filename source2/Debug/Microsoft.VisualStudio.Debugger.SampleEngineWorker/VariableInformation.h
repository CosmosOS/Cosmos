#pragma once

#include "AddressDictionary.h"
#include "BreakpointData.h"
#include "SymbolEngine.h"

BEGIN_NAMESPACE

ref class DebuggedProcess;

public ref class VariableInformation sealed
{
public:
	String^ m_name;
	String^ m_typeName;
	String^ m_value;
	bool m_fUserDefinedType;
	DWORD m_dwBaseType;
	DWORD m_dwIndirectionLevel;
	bool m_fFrameRelative;
	unsigned int m_address;

	VariableInformation^ child;

public:
	static VariableInformation^ Create(DebuggedProcess^ debuggedProcess, 
										unsigned int bp, 
										BSTR bstrVarName, 
										BSTR bstrVarType, 
										bool fBuiltInType, 
										DWORD dwOffset, 
										DWORD dwIndirectionLevel);

private:
	VariableInformation()
	{

	}
};

END_NAMESPACE