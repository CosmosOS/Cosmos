#pragma once

BEGIN_NAMESPACE

public ref class WorkerContainerObject
{
private:
	Object^ m_client;

public:
	property Object^ Client
	{
		Object^ get() 
		{ 
			return m_client; 
		}
		void set(Object^ client)
		{
			Object^ oldClient = System::Threading::Interlocked::CompareExchange(m_client, client, nullptr);
			if (oldClient != nullptr)
			{
				throw gcnew InvalidOperationException();
			}
		}
	}
};

END_NAMESPACE
