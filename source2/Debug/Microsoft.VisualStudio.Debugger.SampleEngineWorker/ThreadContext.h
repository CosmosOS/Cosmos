#pragma once

BEGIN_NAMESPACE

public ref class X86ThreadContext sealed
{
internal:
	X86ThreadContext(const CONTEXT& threadContext) :
		eax(threadContext.Eax),
		ebx(threadContext.Ebx),
		ecx(threadContext.Ecx),
		edx(threadContext.Edx),
		esi(threadContext.Esi),
		edi(threadContext.Edi),
		eip(threadContext.Eip),
		esp(threadContext.Esp),
		ebp(threadContext.Ebp),
		EFlags(threadContext.EFlags)
	{
	}

public:
	DWORD eax;
	DWORD ebx;
	DWORD ecx;
	DWORD edx;
	DWORD esi;
	DWORD edi;
	DWORD eip;
	DWORD esp;
	DWORD ebp;
	DWORD EFlags;
};

END_NAMESPACE
