/* Copyright (C) 2008-2011, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * This software is based in part on the work of the Sam Leffler, Silicon 
 * Graphics, Inc. and contributors.
 *
 * Copyright (c) 1988-1997 Sam Leffler
 * Copyright (c) 1991-1997 Silicon Graphics, Inc.
 * For conditions of distribution and use, see the accompanying README file.
 */

using System.Collections;
using System.Diagnostics;

namespace BitMiracle.LibTiff.Classic.Internal
{
    internal class TagCompare : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            TiffFieldInfo ta = x as TiffFieldInfo;
            TiffFieldInfo tb = y as TiffFieldInfo;

            Debug.Assert(ta != null);
            Debug.Assert(tb != null);

            if (ta.Tag != tb.Tag)
                return ((int)ta.Tag - (int)tb.Tag);

            return (ta.Type == TiffType.ANY) ? 0 : ((int)tb.Type - (int)ta.Type);
        }
    }
}
