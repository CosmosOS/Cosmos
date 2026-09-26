// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// The names of the suite's registrations whose Probe ran, in the order the
/// driver pass called them, so the ranking cells can check which candidate
/// was offered a function before which.
/// </summary>
internal static class ProbeLog
{
    // Created on first use rather than by a static initializer, which would
    // make a class constructor run from inside the driver pass.
    private static List<string>? s_entries;

    /// <summary>Appends <paramref name="name"/>; called first thing in each test driver's Probe.</summary>
    public static void Record(string name) => (s_entries ??= []).Add(name);

    /// <summary>True when a Probe recorded <paramref name="name"/>.</summary>
    public static bool Recorded(string name)
    {
        List<string>? entries = s_entries;
        if (entries is null)
        {
            return false;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i] == name)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The recorded names joined with commas, or an empty string when no Probe ran.</summary>
    public static string Describe()
    {
        List<string>? entries = s_entries;
        if (entries is null)
        {
            return string.Empty;
        }

        string joined = string.Empty;
        for (int i = 0; i < entries.Count; i++)
        {
            joined = i == 0 ? entries[i] : $"{joined},{entries[i]}";
        }

        return joined;
    }
}
