// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using SeamCpuSet = Cosmos.Kernel.Core.Scheduler.CpuSet;

namespace Cosmos.Kernel.System.Threading;

/// <summary>
/// A set of CPUs, by index: the CPUs a thread may run on
/// (<see cref="ThreadAffinity"/>, <see cref="KernelCpus.DefaultAffinity"/>) and the
/// CPUs the kernel reports (<see cref="KernelCpus.Online"/>). Holds
/// <see cref="Capacity"/> CPUs in four words; a value type that allocates nothing.
/// The ring's projection of the scheduler seam's
/// <c>Cosmos.Kernel.Core.Scheduler.CpuSet</c>, with the same members, as
/// <see cref="Diagnostics.KernelThreadState"/> projects the seam's thread state: a
/// kernel names this one without acknowledging the seam.
/// </summary>
public readonly struct CpuSet : IEquatable<CpuSet>
{
    /// <summary>CPUs a set can name: indices 0 to 255, the per-CPU table's bound.</summary>
    public const int Capacity = SeamCpuSet.Capacity;

    private readonly SeamCpuSet _seam;

    /// <summary>The set naming no CPU. The default value of the type.</summary>
    public static CpuSet Empty => default;

    /// <summary>The set naming every CPU the table can hold.</summary>
    public static CpuSet All => new(SeamCpuSet.All);

    /// <summary>True when the set names no CPU.</summary>
    public bool IsEmpty => _seam.IsEmpty;

    /// <summary>How many CPUs the set names.</summary>
    public int Count => _seam.Count;

    /// <summary>The lowest CPU index the set names.</summary>
    /// <exception cref="InvalidOperationException">The set is empty.</exception>
    public uint First => _seam.First;

    internal CpuSet(SeamCpuSet seam)
    {
        _seam = seam;
    }

    /// <summary>The set naming one CPU.</summary>
    /// <param name="cpu">The CPU's index.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cpu"/> is at or past <see cref="Capacity"/>.</exception>
    public static CpuSet Single(uint cpu)
    {
        return new CpuSet(SeamCpuSet.Single(cpu));
    }

    /// <summary>Whether the set names <paramref name="cpu"/>.</summary>
    /// <param name="cpu">The CPU's index.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cpu"/> is at or past <see cref="Capacity"/>.</exception>
    public bool Contains(uint cpu)
    {
        return _seam.Contains(cpu);
    }

    /// <summary>The set with <paramref name="cpu"/> added.</summary>
    /// <param name="cpu">The CPU's index.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cpu"/> is at or past <see cref="Capacity"/>.</exception>
    public CpuSet With(uint cpu)
    {
        return new CpuSet(_seam.With(cpu));
    }

    /// <summary>The set with <paramref name="cpu"/> removed.</summary>
    /// <param name="cpu">The CPU's index.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cpu"/> is at or past <see cref="Capacity"/>.</exception>
    public CpuSet Without(uint cpu)
    {
        return new CpuSet(_seam.Without(cpu));
    }

    /// <summary>The CPUs both sets name.</summary>
    /// <param name="other">The other set.</param>
    public CpuSet Intersect(CpuSet other)
    {
        return new CpuSet(_seam.Intersect(other._seam));
    }

    /// <summary>The seam's value, what the scheduler holds and is handed.</summary>
    internal SeamCpuSet ToSeam()
    {
        return _seam;
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
        return _seam.Equals(other._seam);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is CpuSet other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _seam.GetHashCode();
    }
}
