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

namespace BitMiracle.LibTiff.Classic.Internal
{
    /// <summary>
    /// Alternate source manager for reading from JPEGTables.
    /// We can share all the code except for the init routine.
    /// </summary>
    class JpegTablesSource : JpegStdSource
    {
        public JpegTablesSource(JpegCodec sp)
            : base(sp)
        {
        }

        public override void init_source()
        {
            initInternalBuffer(m_sp.m_jpegtables, m_sp.m_jpegtables_length);
        }
    }
}
