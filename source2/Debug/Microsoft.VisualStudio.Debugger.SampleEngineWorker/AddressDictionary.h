#pragma once

BEGIN_NAMESPACE

value struct AddressRange
{
	DWORD_PTR Start;
	DWORD Size;
};

template <class T>
ref class AddressDictionary
{
	typedef System::Collections::Generic::SortedDictionary<AddressRange, typename T> BaseDictionary;

	ref class AddressRangeComparer : System::Collections::Generic::IComparer<AddressRange>
	{
	public:
		static const DWORD BASE_ADDRESS_SEARCH_RESERVED_SIZE = 0; // We use AddressRange::Size==0 for base address searches

		static bool InRange(AddressRange range, DWORD_PTR value)
		{
			if (range.Start <= value)
			{
				if (range.Start + range.Size > value)
				{
					return true;
				}
			}

			return false;
		}

	// IComparer<AddressRange>
	public:
		virtual int Compare(AddressRange range, AddressRange existingValue)
		{
			ASSERT(existingValue.Size != BASE_ADDRESS_SEARCH_RESERVED_SIZE);

			// check if the caller is suppling a base address or an address range
			if (range.Size != BASE_ADDRESS_SEARCH_RESERVED_SIZE)
			{
				// address range
				if (InRange(existingValue, range.Start))
				{
					// Make sure that range doesn't cross out of existingValue
					if (!InRange(existingValue, range.Start + range.Size - 1))
					{
						throw gcnew System::ArgumentOutOfRangeException("range.Size");
					}

					return 0; // range is within existingValue
				}
			}
			else
			{
				// base address
				if (existingValue.Start == range.Start)
				{
					return 0; // The two basse addresses are the same
				}
			}

			ASSERT(existingValue.Start != range.Start);

			// note: these ranges are unsigned and potentially larger than 32-bit, so can't just return the value of a subtraction
			if (range.Start < existingValue.Start)
			{
				return -1; // range is less than existingValue
			}
			else
			{
				return 1; // range is greator than existingValue
			}
		}
	};

	BaseDictionary ^m_dictionary;

public:
	AddressDictionary()
	{
		AddressRangeComparer^ comparer = gcnew AddressRangeComparer();
		m_dictionary = gcnew BaseDictionary(comparer);
	}

	void Add(DWORD_PTR baseAddress, DWORD size, T value)
	{
		if (value == nullptr)
		{
			throw gcnew System::ArgumentNullException("value");
		}

		AddressRange key;
		key.Start = baseAddress;
		key.Size = size;
		ValidateRange(key);

		m_dictionary->Add(key, value);
	}

	void Remove(DWORD_PTR baseAddress)
	{
		AddressRange key;
		key.Start = baseAddress;
		key.Size = AddressRangeComparer::BASE_ADDRESS_SEARCH_RESERVED_SIZE;

		m_dictionary->Remove(key);
	}

	T operator[](DWORD_PTR baseAddress)
	{
		AddressRange key;
		key.Start = baseAddress;
		key.Size = AddressRangeComparer::BASE_ADDRESS_SEARCH_RESERVED_SIZE;

		return m_dictionary[key];
	}

	void Clear()
	{
		m_dictionary->Clear();
	}

	T FindAddress(AddressRange key)
	{
		ValidateRange(key);

		T value;
		if (!m_dictionary->TryGetValue(key, value))
		{
			return nullptr;
		}

		return value;
	}
	
	T FindAddress(DWORD_PTR address, DWORD size)
	{
		AddressRange key;
		key.Start = address;
		key.Size = size;

		return FindAddress(key);
	}

private:
	static void ValidateRange(AddressRange range)
	{
		if (range.Size == 0 || range.Start + range.Size - 1 < range.Start)
		{
			throw gcnew System::ArgumentOutOfRangeException("range.Size");
		}
	}
};

END_NAMESPACE