#pragma once

BEGIN_NAMESPACE

#include <assert.h>

inline HRESULT HRFromWin32Error(LONG lError)
{
    HRESULT hr = HRESULT_FROM_WIN32(lError);
    if (SUCCEEDED(hr))
        hr = E_FAIL;
    return hr;
}

inline HRESULT HRLastError()
{
    return HRFromWin32Error(GetLastError());
}

inline HRESULT HRWin32BoolCall(BOOL rc)
{
    if (!rc)
        return HRLastError();
    return S_OK;
}

inline HRESULT HRWin32ErrorCall(LONG lError)
{
    if (lError != ERROR_SUCCESS)
        return HRFromWin32Error(lError);
    return S_OK;
}

inline void __declspec(noreturn) ThrowHR(HRESULT hr)
{
	throw gcnew ComponentException(hr);
}

inline void __declspec(noreturn) ThrowLastError()
{
	HRESULT hr = HRLastError();

	ThrowHR(hr);
}

inline void Win32BoolCall(BOOL fCondition)
{
	if (!fCondition)
	{
		ThrowLastError();
	}
}

inline void Win32ErrorCall(DWORD err)
{
	if (err != 0)
	{
		ThrowHR(HRESULT_FROM_WIN32(err));
	}
}

inline HANDLE Win32HandleCall(HANDLE h)
{
	if (h == NULL || h == INVALID_HANDLE_VALUE)
	{
		ThrowLastError();
	}

	return h;
}

inline int StringLength(String^ s)
{
	if (s == nullptr)
	{
		return 0;
	}

	return s->Length;
}

inline String^ GetProcessName(HANDLE hProcess)
{
	WCHAR szProcessNameBuff[MAX_PATH+1];
	DWORD nChars = GetModuleFileNameEx(
		hProcess, // process
		NULL,     // Module handle
		szProcessNameBuff,
		_countof(szProcessNameBuff)
		);

	Win32BoolCall(nChars > 0 && nChars <= _countof(szProcessNameBuff));

	return gcnew String(szProcessNameBuff);
}

inline LPWSTR wcsdup(String^ source)
{
	pin_ptr<const wchar_t> pSource = PtrToStringChars( source );
	int cch = (( source->Length+1) * 2);
	LPWSTR pDest = new wchar_t[cch];
	memcpy(pDest, pSource, cch * sizeof(wchar_t));
	
	return pDest;
}

inline String^ StringPrefixReplace(int oldPrefixLen, String^ newPrefix, String^ s)
{
	int lengthNeeded = s->Length + newPrefix->Length - oldPrefixLen;
	Text::StringBuilder^ sb = gcnew Text::StringBuilder(lengthNeeded, lengthNeeded);

	sb->Append(newPrefix);
	sb->Append(s, oldPrefixLen, s->Length-oldPrefixLen);

	return sb->ToString();
}

inline String^ StringPrefixReplace(String^ oldPrefix, String^ newPrefix, String^ s)
{
	return StringPrefixReplace(oldPrefix->Length, newPrefix, s);
}

// Note: return value does not include a trailing slash
inline int FindCommonPathSuffixLength(String^ path1, String^ path2)
{
	const int len1 = path1->Length;
	const int len2 = path2->Length;

	int commonSuffixLength = 0;

	// scan backwards until we find a character that is different
	for (; true; commonSuffixLength++)
	{
		int index1 = len1 - commonSuffixLength - 1;
		int index2 = len2 - commonSuffixLength - 1;

		if (index2 < 0 || index1 < 0)
			break;

		wchar_t char1 = path1[index1];
		wchar_t char2 = path2[index2];

		// first compare the characters exsactly (fast).
		if (char1 == char2)
			continue;

		// if they aren't exsactly the same, see if they are canonicialy the same
		char1 = (char1 == '/') ? '\\' : System::Char::ToUpperInvariant(char1);
		char2 = (char2 == '/') ? '\\' : System::Char::ToUpperInvariant(char2);

		if (char1 == char2)
			continue;

		break;
	}

	// scan forward until we find a slash
	for (; commonSuffixLength > 0; commonSuffixLength--)
	{
		wchar_t char1 = path1[len1 - commonSuffixLength];

		if (char1 == '\\' || char1 == '/')
		{
			commonSuffixLength--;
			break;
		}			
	}

	return commonSuffixLength;
}

END_NAMESPACE