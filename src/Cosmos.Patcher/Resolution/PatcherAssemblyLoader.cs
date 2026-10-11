// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Cosmos.Patcher.Resolution;

/// <summary>
/// Reads the assemblies the patcher works on with a resolver that knows where
/// their references live, so that writing a patched assembly back can resolve
/// every type it has to look through (the underlying type of an enum constant,
/// for one) instead of failing on the first reference outside the host runtime.
/// </summary>
public static class PatcherAssemblyLoader
{
    /// <summary>
    /// Reads the assembly to patch. Symbols are read when a matching portable
    /// PDB sits next to the assembly, so the patched output keeps its sequence
    /// points; without one the assembly is read alone.
    /// </summary>
    /// <param name="path">Path of the assembly.</param>
    /// <param name="resolver">Resolver for the assemblies it references.</param>
    /// <param name="hasSymbols">True when the PDB was read and must be written back with the assembly.</param>
    /// <returns>The assembly, owned by the caller.</returns>
    public static AssemblyDefinition ReadTarget(string path, IAssemblyResolver resolver, out bool hasSymbols)
    {
        try
        {
            ReaderParameters withSymbols = new() { AssemblyResolver = resolver, ReadSymbols = true };
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(path, withSymbols);
            hasSymbols = true;
            return assembly;
        }
        catch (Exception exception) when (exception is SymbolsNotFoundException or SymbolsNotMatchingException or IOException or BadImageFormatException)
        {
            hasSymbols = false;
            return Read(path, resolver);
        }
    }

    /// <summary>Reads an assembly without symbols, resolving its references through <paramref name="resolver"/>.</summary>
    /// <param name="path">Path of the assembly.</param>
    /// <param name="resolver">Resolver for the assemblies it references.</param>
    /// <returns>The assembly, owned by the caller.</returns>
    public static AssemblyDefinition Read(string path, IAssemblyResolver resolver)
    {
        ReaderParameters parameters = new() { AssemblyResolver = resolver };
        return AssemblyDefinition.ReadAssembly(path, parameters);
    }
}
