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

/*
 * Directory Tag Get & Set Routines.
 * (and also some miscellaneous stuff)
 */

using System.IO;

using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic
{
#if EXPOSE_LIBTIFF
    public
#endif
    partial class Tiff
    {
        /* is tag value normal or pseudo */
        internal static bool isPseudoTag(TiffTag t)
        {
            return ((int)t > 0xffff);
        }

        private bool isFillOrder(FillOrder o)
        {
            TiffFlags order = (TiffFlags)o;
            return ((m_flags & order) == order);
        }

        private static int BITn(int n)
        {
            return (1 << (n & 0x1f));
        }

        /*
        * Return true / false according to whether or not
        * it is permissible to set the tag's value.
        * Note that we allow ImageLength to be changed
        * so that we can append and extend to images.
        * Any other tag may not be altered once writing
        * has commenced, unless its value has no effect
        * on the format of the data that is written.
        */
        private bool okToChangeTag(TiffTag tag)
        {
            TiffFieldInfo fip = FindFieldInfo(tag, TiffType.ANY);
            if (fip == null)
            {
                // unknown tag
                ErrorExt(this, m_clientdata, "SetField", "{0}: Unknown {1}tag {2}",
                    m_name, isPseudoTag(tag) ? "pseudo-" : "", tag);
                return false;
            }

            if (tag != TiffTag.IMAGELENGTH &&
                (m_flags & TiffFlags.BEENWRITING) == TiffFlags.BEENWRITING &&
                !fip.OkToChange)
            {
                // Consult info table to see if tag can be changed after we've
                // started writing. We only allow changes to those tags that
                // don't / shouldn't affect the compression and / or format of
                // the data.
                ErrorExt(this, m_clientdata, "SetField", "{0}: Cannot modify tag \"{1}\" while writing",
                    m_name, fip.Name);
                return false;
            }

            return true;
        }

        /*
        * Setup a default directory structure.
        */
        private void setupDefaultDirectory()
        {
            int tiffFieldInfoCount;
            TiffFieldInfo[] tiffFieldInfo = getFieldInfo(out tiffFieldInfoCount);
            setupFieldInfo(tiffFieldInfo, tiffFieldInfoCount);

            m_dir = new TiffDirectory();
            m_postDecodeMethod = PostDecodeMethodType.pdmNone;
            m_foundfield = null;
            
            m_tagmethods = m_defaultTagMethods;

            /*
             *  Give client code a chance to install their own
             *  tag extensions & methods, prior to compression overloads.
             */
            if (m_extender != null)
                m_extender(this);
            
            SetField(TiffTag.COMPRESSION, Compression.NONE);
            
            /*
             * NB: The directory is marked dirty as a result of setting
             * up the default compression scheme.  However, this really
             * isn't correct -- we want DIRTYDIRECT to be set only
             * if the user does something.  We could just do the setup
             * by hand, but it seems better to use the normal mechanism
             * (i.e. SetField).
             */
            m_flags &= ~TiffFlags.DIRTYDIRECT;

            /*
             * we clear the ISTILED flag when setting up a new directory.
             * Should we also be clearing stuff like INSUBIFD?
             */
            m_flags &= ~TiffFlags.ISTILED;

            /*
             * Clear other directory-specific fields.
             */
            m_tilesize = -1;
            m_scanlinesize = -1;
        }

        private bool advanceDirectory(ref uint nextdir, out long off)
        {
            off = 0;

            const string module = "advanceDirectory";
            short dircount;
            
            if (!seekOK(nextdir) || !readShortOK(out dircount))
            {
                ErrorExt(this, m_clientdata, module, "{0}: Error fetching directory count", m_name);
                return false;
            }

            if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                SwabShort(ref dircount);

            off = seekFile(dircount * TiffDirEntry.SizeInBytes, SeekOrigin.Current);

            if (!readUIntOK(out nextdir))
            {
                ErrorExt(this, m_clientdata, module, "{0}: Error fetching directory link", m_name);
                return false;
            }

            if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                SwabUInt(ref nextdir);
            
            return true;
        }

        internal static void setString(out string cpp, string cp)
        {
            cpp = cp;
        }

        internal static void setShortArray(out short[] wpp, short[] wp, int n)
        {
            wpp = new short[n];
            for (int i = 0; i < n; i++)
                wpp[i] = wp[i];
        }

        internal static void setLongArray(out int[] lpp, int[] lp, int n)
        {
            lpp = new int[n];
            for (int i = 0; i < n; i++)
                lpp[i] = lp[i];
        }

        internal static void setFloatArray(out float[] fpp, float[] fp, int n)
        {
            fpp = new float[n];
            for (int i = 0; i < n; i++)
                fpp[i] = fp[i];
        }

        internal bool fieldSet(int field)
        {
            return ((m_dir.td_fieldsset[field / 32] & BITn(field)) != 0);
        }

        internal void setFieldBit(int field)
        {
            m_dir.td_fieldsset[field / 32] |= BITn(field);
        }

        internal void clearFieldBit(int field)
        {
            m_dir.td_fieldsset[field / 32] &= ~BITn(field);
        }
    }
}
