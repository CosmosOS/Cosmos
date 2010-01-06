#pragma once

BEGIN_NAMESPACE

typedef Collections::Generic::LinkedList<Object^> ObjectList;

private ref class BreakpointData sealed
{
public:
	initonly BYTE OriginalData;
	initonly ObjectList^ Clients;
	initonly DWORD Address;

	BreakpointData(DWORD dwAddress, BYTE originalData, Object^ client)
	{
		Address = dwAddress;
		OriginalData = originalData;
		Clients = gcnew ObjectList();
		Clients->AddLast(client);
	}
};

END_NAMESPACE