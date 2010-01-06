#pragma once

#include "WorkerThreadObject.h"
#include "WorkerUtil.h"

BEGIN_NAMESPACE

struct CMappedPtr
{
	LPVOID p;

	CMappedPtr() 
	{ 
		this->p = NULL; 
	}
	~CMappedPtr() 
	{
		if (this->p != NULL)
		{
			VERIFY( UnmapViewOfFile(this->p) );
		}
	}
};

ref class ModuleResolver : public WorkerThreadObject, public IDisposable
{
private:
	value struct CacheLine
	{
		String^ DevicePath;
		String^ FilePath;
		DWORD LastUsed;
	};

	array<CacheLine>^ m_cache;
	initonly LPWSTR m_pDeviceNameBuffer;
	DWORD m_lastUsed;
	bool m_fElementZeroReserved;

	static const DWORD CCH_MAX_DEVICE_NAME = 512;

public:
	ModuleResolver()
	{
		m_cache = gcnew array<CacheLine,1>(8);
		m_pDeviceNameBuffer = new wchar_t[CCH_MAX_DEVICE_NAME];
	}
	~ModuleResolver()
	{
		m_cache = nullptr;
		this->!ModuleResolver();
	}
	void Close()
	{
		this->~ModuleResolver();
	}

private:
	!ModuleResolver()
	{
		delete[] m_pDeviceNameBuffer;
	}

public:
	String^ ResolveMappedFile(HANDLE hProcess, DWORD_PTR baseAddress, HANDLE hFile)
	{
		// Need to obtain the module name
		ASSERT(GetCurrentThreadId() == this->ThreadId);

		String^ deviceName = GetMappedFileDeviceName(hProcess, baseAddress);

		if (deviceName != nullptr)
		{
			String^ filePath = ResolveDeviceName(deviceName, hFile);

			return filePath;
		}

		return "";
	}

	void InitializeCache(String^ filePath)
	{
		ASSERT(GetCurrentThreadId() == this->ThreadId);

		String^ devicePath = GetFileDeviceName(filePath);	
		
		if (StaticResolveDeviceName(devicePath) != nullptr)
		{
			return; // if we can resolve this device path usingthe static map, no point in caching
		}

		int commonPathSuffix = FindCommonPathSuffixLength(filePath, devicePath);

		m_cache[0].DevicePath = devicePath->Substring(0, devicePath->Length - commonPathSuffix);
		m_cache[0].FilePath = filePath->Substring(0, filePath->Length - commonPathSuffix);
		m_fElementZeroReserved = true; // Indicate that we should never replace element 0 in our cache
	}

	static bool VerifyFilePath(String^ filePath, HANDLE hFile)
	{
		HRESULT hr;

		if (hFile == NULL || hFile == INVALID_HANDLE_VALUE)
			return true; // If we weren't provided with an hFile, we have nothing to validate against

		BY_HANDLE_FILE_INFORMATION handleInfo = {0};
		Win32BoolCall( GetFileInformationByHandle(hFile, &handleInfo) );

		CHandle hFileNameFile;
		hr = GetFileHandle(filePath, &hFileNameFile.m_h);
		if (hr != S_OK)
		{
			if (hr == HRESULT_FROM_WIN32(ERROR_ACCESS_DENIED))
			{
				return true; // no way to validate the result.
			}
			if (hr == HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND) ||
				hr == HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND) ||
				hr == HRESULT_FROM_WIN32(ERROR_INVALID_NAME))
			{
				return false; // we wound up with the wrong name
			}
			ThrowHR(hr); // unexpected failure
		}

		BY_HANDLE_FILE_INFORMATION fileNameInfo = {0};
		Win32BoolCall( GetFileInformationByHandle(hFileNameFile, &fileNameInfo) );

		bool fEqual = ( 
			handleInfo.dwVolumeSerialNumber == fileNameInfo.dwVolumeSerialNumber &&
			handleInfo.nFileIndexHigh == fileNameInfo.nFileIndexHigh &&
			handleInfo.nFileIndexLow == fileNameInfo.nFileIndexLow
		);

		return fEqual;
	}

#ifndef DEVICE_NAME_UNIT_TEST
private:
#endif

	String^ ResolveDeviceName(String^ deviceName, HANDLE hFile)
	{
		ASSERT(GetCurrentThreadId() == this->ThreadId);

		String^ filePath = StaticResolveDeviceName(deviceName);
		if (filePath != nullptr)
			return filePath;

		filePath = CacheResolveDeviceName(deviceName, hFile);
		if (filePath != nullptr)
			return filePath;

		filePath = DriveLetterResolveDeviceName(deviceName, hFile);
		if (filePath != nullptr)
			return filePath;

		// If all else fails, just return the device name with a '* ' in front of it
		filePath = String::Concat("* ", deviceName);
		return filePath;
	}

	static String^ StaticResolveDeviceName(String^ deviceName)
	{
		String^ const DfsPrefix = "\\Device\\WinDfs\\";
		if (deviceName->StartsWith(DfsPrefix, StringComparison::OrdinalIgnoreCase))
		{
			// This is a DFS share. Skip the next token: this will either 
			// be 'Root' for a UNC name, or the drive letter for a mapped drive.
			int oldPrefixLength = deviceName->IndexOf('\\', DfsPrefix->Length);
			if (oldPrefixLength < 0)
			{
				ASSERT(!"Badly formed DFS name");
				return nullptr;
			}

			return StringPrefixReplace(oldPrefixLength, "\\", deviceName);
		}

		String^ const LanManPrefix = "\\Device\\LanmanRedirector\\";
		if (deviceName->StartsWith(LanManPrefix, StringComparison::OrdinalIgnoreCase))
		{
			return StringPrefixReplace(LanManPrefix, "\\\\", deviceName);
		}

		String^ const TsClientPrefix = "\\Device\\RdpDr\\TSCLIENT\\";
		if (deviceName->StartsWith(TsClientPrefix, StringComparison::OrdinalIgnoreCase))
		{       
			C_ASSERT(_countof("\\Device\\RdpDr")-1 == 13);

			return deviceName->Substring(13);
		}
		
		return nullptr;
	}

	String^ CacheResolveDeviceName(String^ deviceName, HANDLE hFile)
	{
		for (int cLine = 0; cLine < m_cache->Length; cLine++)
		{
			if (m_cache[cLine].DevicePath == nullptr)
				break;

			if (deviceName->StartsWith(m_cache[cLine].DevicePath))
			{
				String^ filePath = StringPrefixReplace(m_cache[cLine].DevicePath, m_cache[cLine].FilePath, deviceName);

				if (VerifyFilePath(filePath, hFile))
				{
					if (m_cache[cLine].LastUsed != m_lastUsed)
					{
						m_lastUsed++;
						m_cache[cLine].LastUsed = m_lastUsed;
					}

					return filePath;
				}
			}
		}

		return nullptr;
	}

	String^ DriveLetterResolveDeviceName(String^ deviceName, HANDLE hFile)
	{		
		ASSERT(GetCurrentThreadId() == this->ThreadId);

		// Get a bit mask that tells us what logical drives there are
		const DWORD logicalDrives = GetLogicalDrives();
		
		// There are 26 letters in the roman alphabet, so there are 26 drives
		C_ASSERT('Z'-'A'+1 == 26);

		for (BYTE cDrive = 0; cDrive < 26; cDrive++)
		{
			if ((logicalDrives & (1 << cDrive)) == 0)
			{
				continue; // this drive letter is not available
			}

			// Create a string for the drive letter
			const TCHAR DriveLetter[3] = { 'A' + cDrive, ':', 0 };

			// This returns all the devices mapped to this drive letter. I don't understand how there can
			// be more than one, but whatever.
			if (QueryDosDevice(DriveLetter, m_pDeviceNameBuffer, CCH_MAX_DEVICE_NAME) > 0)
			{
				// szDevicePathBuffer is a double-null terminated string.
				for (LPCTSTR szDriveDevicePath = m_pDeviceNameBuffer; szDriveDevicePath[0] != 0; szDriveDevicePath = wcschr(szDriveDevicePath, 0)+1)
				{
					int lenDriveDevicePath = (int)_tcslen(szDriveDevicePath);

					if (lenDriveDevicePath < 2)
					{
						ASSERT(!"Unexpected format for device name");
						continue;
					}

					if (szDriveDevicePath[lenDriveDevicePath-1] == '\\')
					{
						lenDriveDevicePath--;
					}

					// deviceName length needs to be greator for the extra '\\' check
					if (lenDriveDevicePath >= deviceName->Length)
					{
						continue;
					}

					int comparison;
					{
						pin_ptr<const wchar_t> pDeviceName = PtrToStringChars(deviceName);

						comparison = memcmp(pDeviceName, szDriveDevicePath, lenDriveDevicePath*sizeof(wchar_t));
					}

					if (comparison == 0 && deviceName[lenDriveDevicePath] == '\\')
					{
						int lengthRequired = 2 + deviceName->Length - lenDriveDevicePath;

						Text::StringBuilder^ sb = gcnew Text::StringBuilder(lengthRequired, lengthRequired);
						sb->Append(DriveLetter[0]);
						sb->Append(DriveLetter[1]);
						sb->Append(deviceName, lenDriveDevicePath, deviceName->Length - lenDriveDevicePath);

						String^ filePath = sb->ToString();

						if (VerifyFilePath(filePath, hFile))
						{
							AddToCache(deviceName->Substring(0, lenDriveDevicePath+1), filePath->Substring(0, 3));

							return filePath;
						}
					}
				}
			}
		}

		return nullptr;
	}

	void AddToCache(String^ devicePath, String^ filePath)
	{
		int cacheIndex = GetCacheVictiumIndex();

		m_lastUsed++;
		m_cache[cacheIndex].DevicePath = devicePath;
		m_cache[cacheIndex].FilePath = filePath;
		m_cache[cacheIndex].LastUsed = m_lastUsed;
	}

	String^ GetFileDeviceName(String^ filePath)
	{
		ASSERT(GetCurrentThreadId() == this->ThreadId);

		HRESULT hr;

		CHandle hFile;
		hr = GetFileHandle(filePath, &hFile.m_h);
		if (hr != S_OK)
		{
			ThrowHR(hr);
		}

		CHandle hMapping;
		hMapping.Attach( Win32HandleCall( CreateFileMapping(
			hFile,             // file to map
			NULL,              // security attributes
			PAGE_READONLY | SEC_IMAGE,  // map the file as an image file (dll/exe)
			0,                 // high file size. zero = map the full file
			0,                 // low file size. zero = map the full file
			NULL               // file (not shared memory)
			) ) );


		CMappedPtr spView;
		spView.p = MapViewOfFileEx(
			hMapping,          // handle to the mapping
			FILE_MAP_COPY,     // copy-on-write
			0,                 // start offset (high)
			0,                 // start offset (low)
			4096,              // number of bytes to map
			NULL               // base address
			);

		Win32BoolCall( spView.p != NULL );

		String^ deviceName = GetMappedFileDeviceName(GetCurrentProcess(), (DWORD_PTR)spView.p);

		return deviceName;
	}

String^ GetMappedFileDeviceName(HANDLE hProcess, DWORD_PTR baseAddress)
	{
		ASSERT(GetCurrentThreadId() == this->ThreadId);

		DWORD nChars = ::GetMappedFileName(hProcess, (LPVOID)baseAddress, m_pDeviceNameBuffer, CCH_MAX_DEVICE_NAME);

		// NOTE: if the buffer is insufficient, GetMappedFileName will return CCH_MAX_DEVICE_NAME

		if (nChars > 0)
		{
			Win32BoolCall(nChars < CCH_MAX_DEVICE_NAME);

			return gcnew String(m_pDeviceNameBuffer, 0, nChars);
		}
		else
		{
			return nullptr;
		}
	}

	int GetCacheVictiumIndex()
	{
		ASSERT(GetCurrentThreadId() == this->ThreadId);

		int index = m_fElementZeroReserved ? 1 : 0;		
		DWORD oldest = m_lastUsed - m_cache[index].LastUsed;
		int oldestIndex = index;

		for (index++; index < m_cache->Length; index++)
		{
			const DWORD age = m_lastUsed - m_cache[index].LastUsed;
			if (age > oldest)
			{
				oldest = age;
				oldestIndex = index;
			}
		}

		return oldestIndex;
	}

	static HRESULT GetFileHandle(String^ fileName, HANDLE* phFile)
	{
		*phFile = NULL;

		pin_ptr<const wchar_t> pFileName = PtrToStringChars(fileName);

		HANDLE hFile = CreateFile(
			pFileName,
			GENERIC_READ,  // we need to read the attributes
			FILE_SHARE_DELETE | FILE_SHARE_READ | FILE_SHARE_WRITE, // allow all sharing
			NULL,          // security attributes
			OPEN_EXISTING, // open an existing file (don't create)
			FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OPEN_NO_RECALL, // flags and attribute
			NULL           // template file
			);

		if (hFile == INVALID_HANDLE_VALUE)
		{
			return HRLastError();
		}

		*phFile = hFile;
		return S_OK;
	}
};

END_NAMESPACE
