using System.Text.Unicode;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.Utilities;
using Cosmos.Kernel.System.Diagnostics;

namespace Cosmos.Kernel.Plugs.System;

[Plug(typeof(AppContext))]
public static class AppContextPlug
{
    // Native import lives in Cosmos.Kernel.Core/Bridge/Import/KnobsNative.cs.
    private static SimpleDictionary<string, object?>? s_dataStore;
    private static SimpleDictionary<string, bool>? s_switches;

    [PlugMember]
    public static void EnsureInitialized()
    {
        if (s_dataStore is not null)
        {
            return;
        }

        unsafe
        {
            uint count = KnobsNative.GetKnobValues(out byte** knobKeys, out byte** knobValues);

            s_dataStore = new(capacity: (int)count);
            s_switches = new();

            for (int i = 0; i < count; i++)
            {
                byte* ptrKey = knobKeys[i];
                byte* ptrVal = knobValues[i];

                string key = Utf8Decode(new(ptrKey, Strlen(ptrKey)));
                string value = Utf8Decode(new(ptrVal, Strlen(ptrVal)));

                Log.WriteString(key);
                Log.WriteString(" = ");
                Log.WriteString(value + "\n");

                s_dataStore[key] = value;

                if (bool.TryParse(value, out bool result))
                {
                    s_switches.Add(key, result);
                }
            }
        }
    }



    [PlugMember]
    public static bool TryGetSwitch(string switchName, out bool isEnabled)
    {
        EnsureInitialized();

        ArgumentException.ThrowIfNullOrEmpty(switchName);

        if (s_switches is not null)
        {
            if (s_switches.TryGetValue(switchName, out isEnabled))
            {
                return true;
            }
        }

        object? data = GetData(switchName);

        if (GetData(switchName) is string value && bool.TryParse(value, out isEnabled))
        {
            return true;
        }

        isEnabled = false;
        return false;
    }

    [PlugMember]
    public static object? GetData(string name)
    {
        EnsureInitialized();

        s_dataStore!.TryGetValue(name, out object? data);

        return data;
    }
    [PlugMember]
    public static void SetData(string switchName, object? data)
    {
        EnsureInitialized();

        s_dataStore![switchName] = data;
    }

    [PlugMember]
    public static void SetSwitch(string switchName, bool isEnabled)
    {
        EnsureInitialized();

        s_dataStore![switchName] = isEnabled;
    }


    internal unsafe static int Strlen(byte* str, int max = int.MaxValue)
    {
        int length = 0;
        while (str[length] != 0x00 && length < max)
        {
            length++;
        }
        return length;
    }

    internal static unsafe string Utf8Decode(ReadOnlySpan<byte> bytes)
    {
        Span<char> buffer = stackalloc char[bytes.Length];

        global::System.Buffers.OperationStatus status = Utf8.ToUtf16(bytes, buffer, out int bytesRead, out int charsWritten);

        if (status == global::System.Buffers.OperationStatus.Done)
        {
            return new string(buffer.Slice(0, charsWritten));
        }

        // Fallback for non-ASCII or errors
        return string.Empty;
    }
}
