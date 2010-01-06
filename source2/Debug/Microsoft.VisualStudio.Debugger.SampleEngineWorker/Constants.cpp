#include "stdafx.h"

#pragma managed(on)

BEGIN_NAMESPACE

public ref class Constants
{
public:
	#undef S_OK
	static const int S_OK = 0;

	#undef S_FALSE
	static const int S_FALSE = 1;

	#define __E_NOTIMPL E_NOTIMPL
	#undef E_NOTIMPL
	static const int E_NOTIMPL = __E_NOTIMPL;

	#define __E_FAIL E_FAIL
	#undef E_FAIL
	static const int E_FAIL = __E_FAIL;

	static const int E_WIN32_INVALID_NAME = HRESULT_FROM_WIN32(ERROR_INVALID_NAME);

	static const int E_WIN32_ALREADY_INITIALIZED = HRESULT_FROM_WIN32(ERROR_ALREADY_INITIALIZED);

	#define __RPC_E_SERVERFAULT RPC_E_SERVERFAULT
	#undef RPC_E_SERVERFAULT
	static const int RPC_E_SERVERFAULT = __RPC_E_SERVERFAULT;
};

END_NAMESPACE