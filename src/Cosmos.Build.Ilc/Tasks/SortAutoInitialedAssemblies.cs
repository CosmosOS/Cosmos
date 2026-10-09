using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Mono.Cecil;

namespace Cosmos.Build.Ilc.Tasks;

/// <summary>
/// Orders the assemblies whose library initializers the ILC startup code runs
/// before <c>Main</c>. The order has three stages, chosen by the <c>Stage</c>
/// metadata of each <see cref="AssemblyNames"/> item, and within a stage an
/// assembly runs after the assemblies it references.
/// </summary>
/// <remarks>
/// <para>
/// <c>Runtime</c> runs first: the initializer that brings up the heap, the
/// garbage collector and the managed modules. Nothing may allocate before it.
/// <c>Framework</c> runs next: the runtime's own initializers, which allocate
/// and which create the class constructor runner, the piece every later
/// initializer needs before it can read a lazily constructed static. Items
/// without a stage are the kernel libraries and run last.
/// </para>
/// <para>
/// The reference graph alone cannot give this order. The patcher copies plug
/// bodies into <c>System.Private.CoreLib</c>, so the patched CoreLib references
/// the kernel assemblies its plugs call into, and a plain dependency sort put
/// its initializer after them: the scheduler was installed and its timer armed
/// before the class constructor runner existed.
/// </para>
/// </remarks>
public class SortAutoInitialedAssemblies : Microsoft.Build.Utilities.Task
{
    private const string StageMetadata = "Stage";
    private const string RuntimeStage = "Runtime";
    private const string FrameworkStage = "Framework";
    private const string LibraryStage = "";

    private static readonly string[] s_stageOrder = [RuntimeStage, FrameworkStage, LibraryStage];

    /// <summary>
    /// The ILC reference assemblies. The reference edges are read from these files.
    /// </summary>
    [Required]
    public ITaskItem[] AssemblyPaths { get; set; } = [];

    /// <summary>
    /// The assemblies with a library initializer, each with an optional <c>Stage</c> metadata.
    /// </summary>
    [Required]
    public ITaskItem[] AssemblyNames { get; set; } = [];

    /// <summary>
    /// The same assemblies, in the order ILC must run their initializers.
    /// </summary>
    [Output]
    public ITaskItem[] SortedAssemblyNames { get; set; } = [];

    public override bool Execute()
    {
        Dictionary<string, List<ITaskItem>> stages = new(StringComparer.Ordinal);
        foreach (string stage in s_stageOrder)
        {
            stages[stage] = new List<ITaskItem>();
        }

        foreach (ITaskItem item in AssemblyNames)
        {
            string stage = item.GetMetadata(StageMetadata);
            if (!stages.TryGetValue(stage, out List<ITaskItem> members))
            {
                Log.LogError(
                    $"'{item.ItemSpec}' has the unknown library initializer stage '{stage}'. " +
                    $"Use '{RuntimeStage}', '{FrameworkStage}', or no stage for a kernel library.");
                return false;
            }

            members.Add(item);
        }

        try
        {
            Dictionary<string, HashSet<string>> references = ReadReferences();
            List<ITaskItem> ordered = new(AssemblyNames.Length);
            foreach (string stage in s_stageOrder)
            {
                List<ITaskItem>? sorted = SortByReferences(stages[stage], references);
                if (sorted is null)
                {
                    return false;
                }

                ordered.AddRange(sorted);
            }

            Log.LogMessage(
                MessageImportance.Normal,
                "Library initializer order: " + string.Join(", ", ordered.Select(item => item.ItemSpec)));
            SortedAssemblyNames = ordered.ToArray();
            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
            return false;
        }
    }

    /// <summary>
    /// The references of each initializer assembly that are themselves initializer
    /// assemblies, read from the ILC reference files. An assembly that is not among
    /// the references gets no entry and keeps its declared position.
    /// </summary>
    private Dictionary<string, HashSet<string>> ReadReferences()
    {
        HashSet<string> requested = new(AssemblyNames.Select(item => item.ItemSpec), StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> paths = new(StringComparer.OrdinalIgnoreCase);
        foreach (ITaskItem path in AssemblyPaths)
        {
            paths[Path.GetFileNameWithoutExtension(path.ItemSpec)] = path.ItemSpec;
        }

        Dictionary<string, HashSet<string>> references = new(StringComparer.OrdinalIgnoreCase);
        foreach (string name in requested)
        {
            if (!paths.TryGetValue(name, out string path))
            {
                Log.LogMessage(
                    MessageImportance.High,
                    $"'{name}' is not among the ILC references; its initializer keeps its declared position.");
                continue;
            }

            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(path);
            references[name] = new HashSet<string>(
                assembly.MainModule.AssemblyReferences.Select(reference => reference.Name).Where(requested.Contains),
                StringComparer.OrdinalIgnoreCase);
        }

        return references;
    }

    /// <summary>
    /// Kahn's algorithm over one stage, with the declared order as the tie-break so
    /// the result does not depend on directory enumeration order. Returns null on a
    /// reference cycle, which is logged as an error.
    /// </summary>
    private List<ITaskItem>? SortByReferences(List<ITaskItem> members, Dictionary<string, HashSet<string>> references)
    {
        HashSet<string> inStage = new(members.Select(member => member.ItemSpec), StringComparer.OrdinalIgnoreCase);
        HashSet<string> placed = new(StringComparer.OrdinalIgnoreCase);
        List<ITaskItem> pending = new(members);
        List<ITaskItem> result = new(members.Count);

        while (pending.Count > 0)
        {
            int index = pending.FindIndex(item => ReferencesArePlaced(item.ItemSpec));
            if (index < 0)
            {
                Log.LogError(
                    "The library initializer assemblies " +
                    string.Join(", ", pending.Select(item => item.ItemSpec)) +
                    " reference each other in a cycle.");
                return null;
            }

            ITaskItem next = pending[index];
            pending.RemoveAt(index);
            placed.Add(next.ItemSpec);
            result.Add(next);
        }

        return result;

        bool ReferencesArePlaced(string name)
        {
            return !references.TryGetValue(name, out HashSet<string> refs)
                || refs.All(reference => !inStage.Contains(reference) || placed.Contains(reference));
        }
    }
}
