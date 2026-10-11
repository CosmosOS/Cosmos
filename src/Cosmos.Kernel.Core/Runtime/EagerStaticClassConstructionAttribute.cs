// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace System.Runtime.CompilerServices;

/// <summary>
/// Runs the marked type's static constructor at startup instead of on first
/// access. ILC matches the attribute by its full name only, so this copy
/// stands in for CoreLib's internal one; the arch HALs reach it through
/// Core's <c>InternalsVisibleTo</c> grants to mark their <c>EagerCtor</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
internal class EagerStaticClassConstructionAttribute : Attribute
{
}
