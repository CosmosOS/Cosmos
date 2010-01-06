#include "stdafx.h"

#pragma managed(on)

#include "ProjInclude.h"
#include "VariableInformation.h"
#include "DebuggedProcess.h"

// Static factory method for constructing a VariableInformation instance
// VariableInformation represents a local or a parameter for a function in the sample engine.
VariableInformation^ VariableInformation::Create(DebuggedProcess^ debuggedProcess, 
												 unsigned int bp,
												 BSTR bstrVarName, 
												 BSTR bstrVarType, 
												 bool fBuiltInType, 
												 DWORD dwOffset, 
												 DWORD dwIndirectionLevel)
{
	VariableInformation^ variable = gcnew VariableInformation();
	variable->m_name = gcnew String(bstrVarName);
	variable->m_typeName = gcnew String(bstrVarType);
	variable->m_fFrameRelative = true;
	variable->m_address = dwOffset;
	variable->m_fUserDefinedType = !fBuiltInType;
	variable->m_dwIndirectionLevel = dwIndirectionLevel;

	// If type was a pointer, add the correct number of "*" to the end of the type name.
	if (dwIndirectionLevel > 0)
	{	
		for (DWORD k = 0; k < dwIndirectionLevel; k++)
		{
			variable->m_typeName = String::Concat(variable->m_typeName, gcnew String(L"*"));
		}	
	}

	if (fBuiltInType)
	{
		// Read the memory in the debuggee at this variables location. The top level function variable is always be frame relative.
		System::UInt32 val = debuggedProcess->ReadMemoryUInt(bp + dwOffset);
		variable->m_value = val.ToString();

		// If the function variable is a pointer (dwIndirectionLevel > 0), obtain the information for its children.
		VariableInformation^ currParent = variable;
		for (DWORD k = 0; k < dwIndirectionLevel; k++)
		{
			VariableInformation^ currChild = gcnew VariableInformation();
			
			currChild->m_name = String::Concat(gcnew String(L"*"), currParent->m_name);
			currChild->m_typeName = gcnew String(bstrVarType);
			currChild->m_fFrameRelative = false;
			currChild->m_address = val;
			currChild->m_fUserDefinedType = !fBuiltInType;
			currChild->m_dwIndirectionLevel = dwIndirectionLevel - k;

			// The child addresses are flat-virtual addresses
			val = debuggedProcess->ReadMemoryUInt(currChild->m_address);
			currChild->m_value = val.ToString();

			currParent->child = currChild;
			currParent = currChild;
		}
	}
	else
	{
		variable->m_value = L"{..}";		
	}
	
	return variable;
}


