// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;

namespace SampleDrivers;

/// <summary>
/// How many of the dynamic interrupt vectors, 0x40 to 0xEE, have a handler:
/// on x64 each MSI-X entry the kit binds takes one, and a torn-down attempt
/// must give it back. ARM64 binds LPIs instead, which leave the count alone.
/// </summary>
/// <remarks>
/// Core's handler table cannot be read from here: its element type,
/// <c>InterruptManager.IrqDelegate</c>, is internal, and an
/// <see cref="UnsafeAccessorAttribute"/> cannot return a reference to a
/// field of an inaccessible type. So each vector is asked through the
/// allocator instead. <c>AllocateVector</c> takes the first vector at or
/// after its cursor whose handler is null and stores the handler it is
/// given there; given null, it stores nothing, so it only answers which
/// vector that is. With the cursor set to a vector, the answer is that
/// vector exactly when it has no handler. The cursor is put back
/// afterwards, so the next real allocation starts where it would have.
/// Should the allocator or its cursor be renamed or change signature, the
/// accessors throw (see <see cref="KitInternals"/>), and should the
/// allocator stop accepting a null handler it throws too: the cells that
/// count vectors fail rather than pass.
/// </remarks>
internal static class InterruptVectors
{
    private const string InterruptManagerType = "Cosmos.Kernel.Core.CPU.InterruptManager, Cosmos.Kernel.Core";
    private const string IrqDelegateType = "Cosmos.Kernel.Core.CPU.InterruptManager+IrqDelegate, Cosmos.Kernel.Core";

    // InterruptManager's dynamic vector window.
    private const int FirstDynamicVector = 0x40;
    private const int LastDynamicVector = 0xEE;

    /// <summary>
    /// Counts the dynamic vectors that have a handler now. Thread context:
    /// it takes the allocator's lock, which a thread holds with interrupts on.
    /// </summary>
    /// <returns>The number of bound vectors in 0x40 to 0xEE.</returns>
    /// <exception cref="MissingFieldException">The allocator's cursor is gone.</exception>
    /// <exception cref="MissingMethodException">The allocator is gone or changed signature.</exception>
    /// <exception cref="InvalidOperationException">Every dynamic vector is bound, or the interrupt table is not set up.</exception>
    public static int CountBound()
    {
        ref int cursor = ref NextDynamicVector(null);
        int saved = cursor;
        int bound = 0;
        try
        {
            for (int vector = FirstDynamicVector; vector <= LastDynamicVector; vector++)
            {
                cursor = vector;
                if (AllocateVector(null, null) != vector)
                {
                    bound++;
                }
            }
        }
        finally
        {
            cursor = saved;
        }

        return bound;
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "s_nextDynamicVector")]
    private static extern ref int NextDynamicVector([UnsafeAccessorType(InterruptManagerType)] object? manager);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "AllocateVector")]
    private static extern byte AllocateVector([UnsafeAccessorType(InterruptManagerType)] object? manager,
        [UnsafeAccessorType(IrqDelegateType)] object? handler);
}
