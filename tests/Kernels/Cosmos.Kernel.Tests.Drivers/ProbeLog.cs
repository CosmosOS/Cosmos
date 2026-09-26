// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// The names of the suite's registrations whose Probe ran, in the order the
/// kit called them, so the ranking cells can check which candidate was
/// offered a device before which. A USB driver also records the interface
/// it was offered, since one cell's USB devices present several. A USB
/// device plugged back in may come back at the path it had, so the hot-plug
/// cells read only what was recorded since they plugged it.
/// </summary>
internal static class ProbeLog
{
    // Created on first use rather than by a static initializer, which would
    // make a class constructor run from inside the driver pass. The two
    // lists grow together: entry i was a Probe of s_entries[i] on
    // s_paths[i], null for a PCI driver's.
    private static List<string>? s_entries;
    private static List<string?>? s_paths;

    /// <summary>Probes recorded so far; <see cref="DescribeSince"/> starts from such a count.</summary>
    public static int Count => s_entries?.Count ?? 0;

    /// <summary>Appends <paramref name="name"/>; called first thing in each PCI test driver's Probe.</summary>
    public static void Record(string name) => Record(name, null);

    /// <summary>
    /// Appends <paramref name="name"/> offered <paramref name="path"/>;
    /// called first thing in each USB test driver's Probe.
    /// </summary>
    public static void Record(string name, string? path)
    {
        (s_entries ??= []).Add(name);
        (s_paths ??= []).Add(path);
    }

    /// <summary>True when a Probe recorded <paramref name="name"/>, on any device.</summary>
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
    public static string Describe() => Join(null, 0);

    /// <summary>
    /// The names recorded for <paramref name="path"/>, in order, joined with
    /// commas, or an empty string when no driver was offered it.
    /// </summary>
    public static string Describe(string path) => Join(path, 0);

    /// <summary>
    /// As <see cref="Describe(string)"/>, over the probes recorded after the
    /// first <paramref name="start"/> only.
    /// </summary>
    public static string DescribeSince(int start, string path) => Join(path, start);

    /// <summary>
    /// Joins the names recorded for <paramref name="path"/>, or every name
    /// when it is null, from entry <paramref name="start"/> on.
    /// </summary>
    private static string Join(string? path, int start)
    {
        List<string>? entries = s_entries;
        List<string?>? paths = s_paths;
        if (entries is null || paths is null)
        {
            return string.Empty;
        }

        string joined = string.Empty;
        for (int i = start; i < entries.Count; i++)
        {
            if (path is not null && paths[i] != path)
            {
                continue;
            }

            joined = joined.Length == 0 ? entries[i] : $"{joined},{entries[i]}";
        }

        return joined;
    }
}
