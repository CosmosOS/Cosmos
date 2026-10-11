// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace System.Runtime.CompilerServices;

/// <summary>
/// Marker the compiler needs for <c>init</c> accessors, which the generator's
/// records use. netstandard2.0 does not ship it; this internal copy is the
/// usual polyfill and is never seen outside this assembly.
/// </summary>
internal static class IsExternalInit
{
}
