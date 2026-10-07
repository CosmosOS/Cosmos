// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Mono.Cecil;

namespace Cosmos.Patcher.Resolution;

/// <summary>
/// Resolves the assemblies a patched assembly references from the directories
/// the build keeps them in. Mono.Cecil's default resolver searches the current
/// directory and the host runtime only. That finds a BCL enum but never a Cosmos
/// one, and writing a <c>const</c> field or a default parameter typed as an enum
/// needs the enum's underlying type: a constant of an enum declared in another
/// Cosmos assembly therefore failed to write back with an unresolved assembly.
/// The directories added here are searched before Cecil's own fallbacks.
/// </summary>
public sealed class PatcherAssemblyResolver : DefaultAssemblyResolver
{
    private readonly HashSet<string> _directories = new(StringComparer.Ordinal);

    /// <summary>The directories added so far, each once, in the order they were added.</summary>
    public IReadOnlyCollection<string> Directories => _directories;

    /// <summary>
    /// Adds the directory holding <paramref name="assemblyPath"/>. Nothing is
    /// added for an empty path, a path with no directory, or a directory that
    /// does not exist.
    /// </summary>
    /// <param name="assemblyPath">Path of an assembly whose neighbours should be found.</param>
    public void AddAssemblyDirectory(string? assemblyPath)
    {
        if (string.IsNullOrWhiteSpace(assemblyPath))
        {
            return;
        }

        string? directory = Path.GetDirectoryName(Path.GetFullPath(assemblyPath));
        if (directory is not null)
        {
            AddDirectory(directory);
        }
    }

    /// <summary>
    /// Adds <paramref name="directory"/> to the search list, once. A directory
    /// that does not exist is skipped: the build hands over every candidate
    /// location, and some hold no assembly on a given machine.
    /// </summary>
    /// <param name="directory">The directory to search.</param>
    public void AddDirectory(string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        string full = Path.GetFullPath(directory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (full.Length == 0 || !Directory.Exists(full) || !_directories.Add(full))
        {
            return;
        }

        AddSearchDirectory(full);
    }

    /// <summary>
    /// Adds every directory of a <c>;</c> or <c>,</c> separated list, as the
    /// <c>--search</c> command line option is spelled.
    /// </summary>
    /// <param name="separatedList">The list; null or empty adds nothing.</param>
    public void AddDirectories(string? separatedList)
    {
        if (string.IsNullOrWhiteSpace(separatedList))
        {
            return;
        }

        char[] separators = [';', ',', Path.PathSeparator];
        foreach (string directory in separatedList.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            AddDirectory(directory);
        }
    }
}
