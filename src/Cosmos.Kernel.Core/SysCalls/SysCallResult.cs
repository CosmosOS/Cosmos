// This code is licensed under MIT license (see LICENSE for details)

using System.Runtime.CompilerServices;

namespace Cosmos.Kernel.Core.SysCalls;

/// <summary>
/// Result of a syscall. On success <see cref="Error"/> is <see cref="SysCallError.None"/>
/// and <see cref="Value"/> carries the call's return value. On failure
/// <see cref="Error"/> is set and <see cref="Value"/> is unspecified (handlers
/// should set it to 0 for determinism). The native entry stub packs this
/// into a single long return register so callers receive a C-style
/// <c>&gt;= 0</c> success / <c>&lt; 0</c> <c>-errno</c> value.
/// </summary>
internal readonly struct SysCallResult
{
    /// <summary>
    /// Construct a success result carrying <paramref name="value"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SysCallResult(long value)
    {
        Error = SysCallError.None;
        Value = value;
    }

    /// <summary>
    /// Construct an explicit result. User code should prefer
    /// <see cref="Success(long)"/> / <see cref="Failure(SysCallError)"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SysCallResult(SysCallError error, long value = 0)
    {
        Error = error;
        Value = value;
    }

    /// <summary>ERRNO-style failure code, or <see cref="SysCallError.None"/> on success.</summary>
    public SysCallError Error { get; }

    /// <summary>Success return value (unspecified on failure).</summary>
    public long Value { get; }

    /// <summary>True when <see cref="Error"/> is <see cref="SysCallError.None"/>.</summary>
    public bool IsSuccess => Error == SysCallError.None;

    /// <summary>Build a success result carrying <paramref name="value"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SysCallResult Success(long value = 0) => new SysCallResult(value);

    /// <summary>Build a failure result with <paramref name="error"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SysCallResult Failure(SysCallError error) => new SysCallResult(error, 0);

    /// <summary>
    /// Pack into the single long the native bridge returns to userspace:
    /// success returns <see cref="Value"/>; failure returns
    /// <c>-(long)Error</c>. Syscall numbers start at 0 and errno values are
    /// small positive integers, so the two ranges never collide.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long Pack()
    {
        return Error == SysCallError.None ? Value : -(long)Error;
    }
}
