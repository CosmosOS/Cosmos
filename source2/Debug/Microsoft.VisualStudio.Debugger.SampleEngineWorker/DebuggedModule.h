#pragma once

#include "WorkerContainerObject.h"

BEGIN_NAMESPACE

ref class DebuggedModule;

typedef System::Collections::Generic::LinkedListNode<DebuggedModule^> DebuggedModuleNode;

// DebuggedModule represents a Module in the debuggee process. A module is a loaded instance
// of an executible image.
public ref class DebuggedModule sealed : public WorkerContainerObject
{
private:
	DWORD m_loadOrder;
	DebuggedModuleNode^ m_node;

public:
	initonly DWORD_PTR BaseAddress; // The base address where the module loaded
    initonly DWORD Size;			// The szie of the module in bytes.
	initonly String^ Name;          // The module's name (kernel32.dll)
	bool SymbolsLoaded;             // true if symbols are loaded for this module. This will only be true for the exe in the sample.
	String^ SymbolPath;             // Full-path to the module

public:
    DWORD GetLoadOrder()
    {
        return m_loadOrder;
    }
	
internal:
	DebuggedModule(DWORD_PTR baseAddress, DWORD size, String^ name, DWORD loadOrder)
	{
		this->BaseAddress = baseAddress;
		this->Size = size;
		this->Name = name;

		m_loadOrder = loadOrder;
		SymbolsLoaded = false;
	}

	void DecrementLoadOrder()
	{
		// THREADING: Called from poll thread

		// When a module loaded before this module unloads, we must decrement its load order.
		ASSERT(m_loadOrder > 1);
		m_loadOrder--;
	}

	property DebuggedModuleNode^ Node
	{
		DebuggedModuleNode^ get()
		{
			// THREADING: Called from poll thread
			
			return m_node;
		}
		void set(DebuggedModuleNode^ value)
		{
			// THREADING: Called from poll thread

			// This function is used to init and clean the node
			ASSERT((m_node == nullptr && value != nullptr) || 
				   (m_node != nullptr && value == nullptr));

			m_node = value;
		}
	}
};

END_NAMESPACE
