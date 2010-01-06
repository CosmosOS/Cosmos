#pragma once

#include "WorkerContainerObject.h"
#include "DiaFrameHolder.h"

BEGIN_NAMESPACE

ref class DebuggedThread;

typedef System::Collections::Generic::LinkedListNode<DebuggedThread^> DebuggedThreadNode;

public ref class DebuggedThread : public WorkerContainerObject
{
private:
	DebuggedThreadNode^ m_node;
	System::Collections::Generic::List<X86ThreadContext^>^ m_stackFrameContexts;

public:
	initonly IntPtr Handle;
	initonly int Id;
	initonly DWORD_PTR StartAddress;
	
	void Close()
	{
		this->~DebuggedThread();
	}

	property System::Collections::Generic::List<X86ThreadContext^>^ StackFrames
	{
		System::Collections::Generic::List<X86ThreadContext^>^ get()
		{		
			return m_stackFrameContexts;
		}
	}

internal:
	DebuggedThread(HANDLE hThread, DWORD dwThreadId, DWORD_PTR startAddress)
	{
		m_stackFrameContexts = gcnew System::Collections::Generic::List<X86ThreadContext^>();
		this->Handle = (IntPtr)hThread;
		this->Id = (int)dwThreadId;
		this->StartAddress = startAddress;
	}

	property DebuggedThreadNode^ Node
	{
		DebuggedThreadNode^ get()
		{
			// THREADING: Called from poll thread
			
			return m_node;
		}
		void set(DebuggedThreadNode^ value)
		{
			// THREADING: Called from poll thread

			// This function is used to init and clean the node
			ASSERT((m_node == nullptr && value != nullptr) || 
				   (m_node != nullptr && value == nullptr));

			m_node = value;
		}
	}

	void ClearStackFrames()
	{
		m_stackFrameContexts->Clear();
	}

	void AddStackFrame(X86ThreadContext^ frame)
	{	
		m_stackFrameContexts->Add(frame);
	}	

private:
	~DebuggedThread()
	{
		m_node = nullptr;
		this->!DebuggedThread();
	}

	!DebuggedThread()
	{
		VERIFY( CloseHandle((HANDLE)Handle) );
	}
};

END_NAMESPACE
