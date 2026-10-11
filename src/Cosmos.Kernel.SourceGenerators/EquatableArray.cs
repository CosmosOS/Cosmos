// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections;
using System.Collections.Immutable;

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// An immutable array compared element by element, so an incremental
/// pipeline step that produces one is cached when its content did not
/// change. <see cref="ImmutableArray{T}"/> compares by reference, which
/// would re-run every downstream step on every edit.
/// </summary>
/// <typeparam name="T">The element type; equatable by value.</typeparam>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
    where T : IEquatable<T>
{
    private readonly ImmutableArray<T> _items;

    /// <summary>Wraps <paramref name="items"/>; a default array reads as empty.</summary>
    /// <param name="items">The elements.</param>
    public EquatableArray(ImmutableArray<T> items)
    {
        _items = items.IsDefault ? ImmutableArray<T>.Empty : items;
    }

    /// <summary>Number of elements.</summary>
    public int Count => _items.IsDefault ? 0 : _items.Length;

    /// <inheritdoc/>
    public T this[int index] => _items[index];

    /// <inheritdoc/>
    public bool Equals(EquatableArray<T> other)
    {
        if (Count != other.Count)
        {
            return false;
        }

        for (int i = 0; i < Count; i++)
        {
            if (!this[i].Equals(other[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hash = 17;
        for (int i = 0; i < Count; i++)
        {
            hash = unchecked((hash * 31) + this[i].GetHashCode());
        }

        return hash;
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => (_items.IsDefault ? ImmutableArray<T>.Empty : _items).AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
