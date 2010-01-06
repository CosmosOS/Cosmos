#pragma once

BEGIN_NAMESPACE

public ref class ComponentException : public Exception
{
public:
	ComponentException(HRESULT hr) : Exception()
	{
		this->HResult = hr;
	}

	property HRESULT HResult
	{
		HRESULT get()
		{
			return Exception::HResult;
		}
	}
};

END_NAMESPACE
