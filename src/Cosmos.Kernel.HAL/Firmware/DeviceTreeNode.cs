// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Firmware;

/// <summary>
/// A cursor over one node of a device tree: the offset of its begin-node
/// token and the cell counts it inherited, which its own <c>reg</c> and
/// <c>interrupts</c> use. Children and siblings are reached by walking
/// the structure block from here. Every member is thread context and
/// allocation-free, and returns false on a malformed tree (an offset past
/// the block, or an unknown token, which is reported through
/// <see cref="DeviceTree.MarkWalkStopped"/> and ends every later read).
/// </summary>
internal readonly struct DeviceTreeNode
{
    /// <summary>Builds the cursor. Thread context; allocation-free.</summary>
    /// <param name="tree">The tree.</param>
    /// <param name="offset">The offset of the node's begin-node token.</param>
    /// <param name="addressCells">The parent's address cells, which this node's reg uses.</param>
    /// <param name="sizeCells">The parent's size cells, which this node's reg uses.</param>
    /// <param name="parentInterruptCells">The interrupt cells the parent handed down.</param>
    /// <param name="interruptCells">The interrupt cells this node's interrupts span, 0 when unresolved.</param>
    internal DeviceTreeNode(DeviceTree tree, uint offset, uint addressCells, uint sizeCells, uint parentInterruptCells, uint interruptCells)
    {
        Tree = tree;
        Offset = offset;
        AddressCells = addressCells;
        SizeCells = sizeCells;
        ParentInterruptCells = parentInterruptCells;
        InterruptCells = interruptCells;
    }

    /// <summary>The tree, or null for a default instance. Any context.</summary>
    internal DeviceTree? Tree { get; }

    /// <summary>The offset of the node's begin-node token. Any context.</summary>
    internal uint Offset { get; }

    /// <summary>The parent's address cells: what this node's reg uses. Any context.</summary>
    internal uint AddressCells { get; }

    /// <summary>The parent's size cells: what this node's reg uses. Any context.</summary>
    internal uint SizeCells { get; }

    /// <summary>The interrupt cells the parent handed down: what a sibling without its own interrupt parent uses. Any context.</summary>
    internal uint ParentInterruptCells { get; }

    /// <summary>The cells one entry of this node's interrupts spans, through its own interrupt parent when it names one, else the parent's; what its children inherit; 0 when unresolved. Any context.</summary>
    internal uint InterruptCells { get; }

    /// <summary>Whether the cursor points into a tree. Any context.</summary>
    internal bool IsValid => Tree is not null;

    /// <summary>The first token after the node's padded name; the block's end when the name is unterminated.</summary>
    private uint BodyOffset => Tree!.SkipName(Offset + (uint)DeviceTreeFormat.TokenBytes);

    /// <summary>Whether the node's name equals <paramref name="name"/>. Thread context; allocation-free.</summary>
    /// <param name="name">The name to compare with.</param>
    internal bool NameEquals(string name)
    {
        if (Tree is null || Tree.WalkStopped)
        {
            return false;
        }

        return Tree.NameAtEquals(Offset + (uint)DeviceTreeFormat.TokenBytes, name);
    }

    /// <summary>
    /// Finds the node's own property <paramref name="name"/>: the
    /// properties precede the children, so the first begin-node,
    /// end-node or end token ends the search. Thread context;
    /// allocation-free.
    /// </summary>
    /// <param name="name">The property's name.</param>
    /// <param name="property">The property, or a default instance.</param>
    internal bool TryGetProperty(string name, out DeviceTreeProperty property)
    {
        property = default;
        if (Tree is null || Tree.WalkStopped)
        {
            return false;
        }

        uint offset = BodyOffset;
        while (true)
        {
            if (!Tree.TryReadToken(offset, out uint token))
            {
                return false;
            }

            if (token == DeviceTreeFormat.TokenProperty)
            {
                if (!TryReadPropertyHeader(offset, out uint length, out uint nameOffset))
                {
                    return false;
                }

                uint valueOffset = offset + (uint)DeviceTreeFormat.TokenBytes + (uint)DeviceTreeFormat.PropertyHeaderBytes;
                if (Tree.StringsNameEquals(nameOffset, name))
                {
                    property = new DeviceTreeProperty(Tree, valueOffset, length);
                    return true;
                }

                if (!TrySkipProperty(offset, length, out offset))
                {
                    return false;
                }
            }
            else if (token == DeviceTreeFormat.TokenNop)
            {
                offset += (uint)DeviceTreeFormat.TokenBytes;
            }
            else if (token == DeviceTreeFormat.TokenBeginNode || token == DeviceTreeFormat.TokenEndNode || token == DeviceTreeFormat.TokenEnd)
            {
                return false;
            }
            else
            {
                Tree.MarkWalkStopped(token, offset);
                return false;
            }
        }
    }

    /// <summary>Whether the node's compatible list names <paramref name="compatible"/>. Thread context; allocation-free.</summary>
    /// <param name="compatible">The compatible string.</param>
    internal bool IsCompatible(string compatible) =>
        TryGetProperty("compatible", out DeviceTreeProperty property) && property.ContainsString(compatible);

    /// <summary>Whether the node's status is "disabled"; an absent status is okay. Thread context; allocation-free.</summary>
    internal bool IsDisabled =>
        TryGetProperty("status", out DeviceTreeProperty property) && property.ValueEquals("disabled");

    /// <summary>This node's own #address-cells, the default when absent: what its children's reg uses. Thread context; allocation-free.</summary>
    internal uint ChildAddressCells => ReadCellCount("#address-cells", DeviceTreeFormat.DefaultAddressCells);

    /// <summary>This node's own #size-cells, the default when absent: what its children's reg uses. Thread context; allocation-free.</summary>
    internal uint ChildSizeCells => ReadCellCount("#size-cells", DeviceTreeFormat.DefaultSizeCells);

    /// <summary>Moves to the node's first child. Thread context; allocation-free.</summary>
    /// <param name="child">The child cursor, or a default instance.</param>
    internal bool TryGetFirstChild(out DeviceTreeNode child)
    {
        // The caller may pass this very cursor as the out argument, so the
        // copy is taken before the out parameter is written.
        DeviceTreeNode self = this;
        child = default;
        if (self.Tree is null || self.Tree.WalkStopped)
        {
            return false;
        }

        if (!self.TryFindNodeToken(self.BodyOffset, out uint childOffset))
        {
            return false;
        }

        child = new DeviceTreeNode(self.Tree, childOffset, self.ChildAddressCells, self.ChildSizeCells, self.InterruptCells, ResolveInterruptCells(self.Tree, childOffset, self.InterruptCells));
        return true;
    }

    /// <summary>Moves to the node's next sibling: past this node's properties and subtree, then on in the parent's token stream. Thread context; allocation-free.</summary>
    /// <param name="next">The sibling cursor, or a default instance.</param>
    internal bool TryGetNextSibling(out DeviceTreeNode next)
    {
        // The caller may pass this very cursor as the out argument, so the
        // copy is taken before the out parameter is written.
        DeviceTreeNode self = this;
        next = default;
        if (self.Tree is null || self.Tree.WalkStopped)
        {
            return false;
        }

        DeviceTree tree = self.Tree;
        uint offset = self.BodyOffset;
        int depth = 0;
        while (true)
        {
            if (!tree.TryReadToken(offset, out uint token))
            {
                return false;
            }

            if (token == DeviceTreeFormat.TokenBeginNode)
            {
                depth++;
                offset = tree.SkipName(offset + (uint)DeviceTreeFormat.TokenBytes);
            }
            else if (token == DeviceTreeFormat.TokenEndNode)
            {
                offset += (uint)DeviceTreeFormat.TokenBytes;
                if (depth == 0)
                {
                    break;
                }

                depth--;
            }
            else if (token == DeviceTreeFormat.TokenProperty)
            {
                if (!self.TryReadPropertyHeader(offset, out uint length, out _) || !TrySkipProperty(offset, length, out offset))
                {
                    return false;
                }
            }
            else if (token == DeviceTreeFormat.TokenNop)
            {
                offset += (uint)DeviceTreeFormat.TokenBytes;
            }
            else if (token == DeviceTreeFormat.TokenEnd)
            {
                return false;
            }
            else
            {
                tree.MarkWalkStopped(token, offset);
                return false;
            }
        }

        if (!self.TryFindNodeToken(offset, out uint siblingOffset))
        {
            return false;
        }

        next = new DeviceTreeNode(tree, siblingOffset, self.AddressCells, self.SizeCells, self.ParentInterruptCells, ResolveInterruptCells(tree, siblingOffset, self.ParentInterruptCells));
        return true;
    }

    /// <summary>Reads entry <paramref name="index"/> of reg with the inherited cell counts, each at most two cells, folded big-endian. Thread context; allocation-free.</summary>
    /// <param name="index">The entry's index.</param>
    /// <param name="address">The entry's address.</param>
    /// <param name="size">The entry's size.</param>
    internal bool TryReadReg(int index, out ulong address, out ulong size)
    {
        address = 0;
        size = 0;
        if (index < 0 || !TryGetProperty("reg", out DeviceTreeProperty reg))
        {
            return false;
        }

        if (AddressCells == 0 || AddressCells > 2 || SizeCells == 0 || SizeCells > 2)
        {
            return false;
        }

        uint entryCells = AddressCells + SizeCells;
        if (((ulong)index + 1) * entryCells * (uint)DeviceTreeFormat.CellBytes > reg.Length)
        {
            return false;
        }

        int cell = (int)(index * entryCells);
        return TryFoldCells(reg, ref cell, AddressCells, out address) && TryFoldCells(reg, ref cell, SizeCells, out size);
    }

    /// <summary>Reads entry <paramref name="index"/> of interrupts in the GIC's three-cell form. Thread context; allocation-free.</summary>
    /// <param name="index">The entry's index.</param>
    /// <param name="type">The interrupt type: 0 an SPI, 1 a PPI.</param>
    /// <param name="number">The interrupt number within its type.</param>
    /// <param name="flags">The trigger flags.</param>
    internal bool TryReadInterrupt(int index, out uint type, out uint number, out uint flags)
    {
        type = 0;
        number = 0;
        flags = 0;
        if (index < 0 || !TryGetProperty("interrupts", out DeviceTreeProperty interrupts))
        {
            return false;
        }

        if (InterruptCells != DeviceTreeFormat.GicInterruptCells)
        {
            return false;
        }

        if (((ulong)index + 1) * DeviceTreeFormat.GicInterruptCells * (uint)DeviceTreeFormat.CellBytes > interrupts.Length)
        {
            return false;
        }

        int first = index * (int)DeviceTreeFormat.GicInterruptCells;
        return interrupts.TryReadCell(first, out type)
            && interrupts.TryReadCell(first + 1, out number)
            && interrupts.TryReadCell(first + 2, out flags);
    }

    /// <summary>Reads the two cells of bus-range. Thread context; allocation-free.</summary>
    /// <param name="first">The first bus.</param>
    /// <param name="last">The last bus.</param>
    internal bool TryReadBusRange(out byte first, out byte last)
    {
        first = 0;
        last = 0;
        if (!TryGetProperty("bus-range", out DeviceTreeProperty range)
            || !range.TryReadCell(0, out uint firstCell)
            || !range.TryReadCell(1, out uint lastCell))
        {
            return false;
        }

        if (firstCell > byte.MaxValue || lastCell > byte.MaxValue || firstCell > lastCell)
        {
            return false;
        }

        first = (byte)firstCell;
        last = (byte)lastCell;
        return true;
    }

    /// <summary>The first cell of the node's own <paramref name="name"/>, or <paramref name="fallback"/> when absent.</summary>
    private uint ReadCellCount(string name, uint fallback)
    {
        if (TryGetProperty(name, out DeviceTreeProperty property) && property.TryReadCell(0, out uint value))
        {
            return value;
        }

        return fallback;
    }

    /// <summary>The length and name offset of the property record at <paramref name="offset"/>.</summary>
    private bool TryReadPropertyHeader(uint offset, out uint length, out uint nameOffset)
    {
        nameOffset = 0;
        return Tree!.TryReadCell(offset + (uint)DeviceTreeFormat.TokenBytes, out length)
            && Tree.TryReadCell(offset + (uint)DeviceTreeFormat.TokenBytes + (uint)DeviceTreeFormat.CellBytes, out nameOffset);
    }

    /// <summary>The offset after the property record at <paramref name="offset"/> whose value is <paramref name="length"/> bytes; false when it overflows.</summary>
    private static bool TrySkipProperty(uint offset, uint length, out uint next)
    {
        ulong after = (ulong)offset + (uint)DeviceTreeFormat.TokenBytes + (uint)DeviceTreeFormat.PropertyHeaderBytes + DeviceTreeFormat.AlignUp((ulong)length);
        if (after > uint.MaxValue)
        {
            next = offset;
            return false;
        }

        next = (uint)after;
        return true;
    }

    /// <summary>
    /// Skips properties and NOPs from <paramref name="offset"/> to the next
    /// begin-node token; false at an end-node or end token (no node), or
    /// at an unknown token (reported).
    /// </summary>
    private bool TryFindNodeToken(uint offset, out uint nodeOffset)
    {
        nodeOffset = 0;
        while (true)
        {
            if (!Tree!.TryReadToken(offset, out uint token))
            {
                return false;
            }

            if (token == DeviceTreeFormat.TokenBeginNode)
            {
                nodeOffset = offset;
                return true;
            }

            if (token == DeviceTreeFormat.TokenProperty)
            {
                if (!TryReadPropertyHeader(offset, out uint length, out _) || !TrySkipProperty(offset, length, out offset))
                {
                    return false;
                }
            }
            else if (token == DeviceTreeFormat.TokenNop)
            {
                offset += (uint)DeviceTreeFormat.TokenBytes;
            }
            else if (token == DeviceTreeFormat.TokenEndNode || token == DeviceTreeFormat.TokenEnd)
            {
                return false;
            }
            else
            {
                Tree.MarkWalkStopped(token, offset);
                return false;
            }
        }
    }

    /// <summary>
    /// The interrupt cells of the node at <paramref name="offset"/>: its own
    /// interrupt-parent resolved through the tree when it carries one (0
    /// when the resolution fails), else <paramref name="inherited"/>.
    /// </summary>
    private static uint ResolveInterruptCells(DeviceTree tree, uint offset, uint inherited)
    {
        DeviceTreeNode probe = new(tree, offset, 0, 0, 0, 0);
        if (!probe.TryGetProperty("interrupt-parent", out DeviceTreeProperty parent))
        {
            return inherited;
        }

        if (parent.TryReadCell(0, out uint phandle) && tree.TryResolveInterruptCells(phandle, out uint cells))
        {
            return cells;
        }

        return 0;
    }

    /// <summary>Folds <paramref name="count"/> cells from <paramref name="cell"/> big-endian, high cell first, and advances the index.</summary>
    private static bool TryFoldCells(DeviceTreeProperty property, ref int cell, uint count, out ulong value)
    {
        value = 0;
        for (uint i = 0; i < count; i++)
        {
            if (!property.TryReadCell(cell, out uint part))
            {
                return false;
            }

            value = (value << 32) | part;
            cell++;
        }

        return true;
    }
}
