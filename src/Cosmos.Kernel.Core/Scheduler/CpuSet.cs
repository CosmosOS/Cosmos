// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// A set of CPUs, by index: what <see cref="SchedulerThread.Affinity"/> holds and
/// what <see cref="IScheduler.SelectCpu"/> is handed as the CPUs a thread may go to.
/// Holds <see cref="Capacity"/> CPUs, as many as the per-CPU table has blocks, in four
/// words; a value type that allocates nothing, so a set is built and compared on any
/// path, a queue lock held or not. The ring projects it as
/// <c>Cosmos.Kernel.System.Threading.CpuSet</c>, with the same members, for a kernel
/// that does not acknowledge the seam.
/// </summary>
[Experimental(Experimentals.SchedulerSeamDiagId)]
public readonly struct CpuSet : IEquatable<CpuSet>
{
    /// <summary>CPUs a set can name: indices 0 to 255, the per-CPU table's bound.</summary>
    public const int Capacity = 256;

    private const int BitsPerWord = 64;

    private readonly ulong _w0;
    private readonly ulong _w1;
    private readonly ulong _w2;
    private readonly ulong _w3;

    /// <summary>True when the set names no CPU.</summary>
    public bool IsEmpty => (_w0 | _w1 | _w2 | _w3) == 0;

    /// <summary>How many CPUs the set names.</summary>
    public int Count
    {
        get
        {
            return BitOperations.PopCount(_w0) + BitOperations.PopCount(_w1)
                + BitOperations.PopCount(_w2) + BitOperations.PopCount(_w3);
        }
    }

    /// <summary>The lowest CPU index the set names.</summary>
    /// <exception cref="InvalidOperationException">The set is empty.</exception>
    public uint First
    {
        get
        {
            if (_w0 != 0)
            {
                return (uint)BitOperations.TrailingZeroCount(_w0);
            }

            if (_w1 != 0)
            {
                return BitsPerWord + (uint)BitOperations.TrailingZeroCount(_w1);
            }

            if (_w2 != 0)
            {
                return 2 * BitsPerWord + (uint)BitOperations.TrailingZeroCount(_w2);
            }

            if (_w3 != 0)
            {
                return 3 * BitsPerWord + (uint)BitOperations.TrailingZeroCount(_w3);
            }

            throw new InvalidOperationException("The CPU set is empty: it has no first CPU.");
        }
    }

    internal CpuSet(ulong w0, ulong w1, ulong w2, ulong w3)
    {
        _w0 = w0;
        _w1 = w1;
        _w2 = w2;
        _w3 = w3;
    }

    /// <summary>The set naming no CPU. The default value of the type.</summary>
    public static CpuSet Empty => default;

    /// <summary>The set naming every CPU the table can hold.</summary>
    public static CpuSet All => new(ulong.MaxValue, ulong.MaxValue, ulong.MaxValue, ulong.MaxValue);

    /// <summary>The set naming one CPU.</summary>
    /// <param name="cpu">The CPU's index.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cpu"/> is at or past <see cref="Capacity"/>.</exception>
    public static CpuSet Single(uint cpu)
    {
        return Empty.With(cpu);
    }

    /// <summary>Whether the set names <paramref name="cpu"/>.</summary>
    /// <param name="cpu">The CPU's index.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cpu"/> is at or past <see cref="Capacity"/>.</exception>
    public bool Contains(uint cpu)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(cpu, (uint)Capacity);
        return (Word((int)(cpu / BitsPerWord)) & (1UL << (int)(cpu % BitsPerWord))) != 0;
    }

    /// <summary>The set with <paramref name="cpu"/> added.</summary>
    /// <param name="cpu">The CPU's index.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cpu"/> is at or past <see cref="Capacity"/>.</exception>
    public CpuSet With(uint cpu)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(cpu, (uint)Capacity);
        ulong bit = 1UL << (int)(cpu % BitsPerWord);
        return (cpu / BitsPerWord) switch
        {
            0 => new CpuSet(_w0 | bit, _w1, _w2, _w3),
            1 => new CpuSet(_w0, _w1 | bit, _w2, _w3),
            2 => new CpuSet(_w0, _w1, _w2 | bit, _w3),
            _ => new CpuSet(_w0, _w1, _w2, _w3 | bit),
        };
    }

    /// <summary>The set with <paramref name="cpu"/> removed.</summary>
    /// <param name="cpu">The CPU's index.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cpu"/> is at or past <see cref="Capacity"/>.</exception>
    public CpuSet Without(uint cpu)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(cpu, (uint)Capacity);
        ulong mask = ~(1UL << (int)(cpu % BitsPerWord));
        return (cpu / BitsPerWord) switch
        {
            0 => new CpuSet(_w0 & mask, _w1, _w2, _w3),
            1 => new CpuSet(_w0, _w1 & mask, _w2, _w3),
            2 => new CpuSet(_w0, _w1, _w2 & mask, _w3),
            _ => new CpuSet(_w0, _w1, _w2, _w3 & mask),
        };
    }

    /// <summary>The CPUs both sets name.</summary>
    /// <param name="other">The other set.</param>
    public CpuSet Intersect(CpuSet other)
    {
        return new CpuSet(_w0 & other._w0, _w1 & other._w1, _w2 & other._w2, _w3 & other._w3);
    }

    /// <summary>One of the four words, 64 CPUs each, lowest indices first: what the ring's copy converts through.</summary>
    /// <param name="index">The word, 0 to 3.</param>
    internal ulong Word(int index)
    {
        return index switch
        {
            0 => _w0,
            1 => _w1,
            2 => _w2,
            _ => _w3,
        };
    }

    /// <summary>Whether two sets name the same CPUs.</summary>
    /// <param name="left">The first set.</param>
    /// <param name="right">The second set.</param>
    public static bool operator ==(CpuSet left, CpuSet right)
    {
        return left.Equals(right);
    }

    /// <summary>Whether two sets name different CPUs.</summary>
    /// <param name="left">The first set.</param>
    /// <param name="right">The second set.</param>
    public static bool operator !=(CpuSet left, CpuSet right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc/>
    public bool Equals(CpuSet other)
    {
        return _w0 == other._w0 && _w1 == other._w1 && _w2 == other._w2 && _w3 == other._w3;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is CpuSet other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        ulong mixed = _w0 ^ (_w1 * 31) ^ (_w2 * 961) ^ (_w3 * 29791);
        return (int)mixed ^ (int)(mixed >> 32);
    }
}
