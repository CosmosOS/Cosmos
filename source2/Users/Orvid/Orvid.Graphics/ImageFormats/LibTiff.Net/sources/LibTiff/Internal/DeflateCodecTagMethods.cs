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

using System.Diagnostics;

using ComponentAce.Compression.Libs.zlib;

namespace BitMiracle.LibTiff.Classic.Internal
{
    class DeflateCodecTagMethods : TiffTagMethods
    {
        public override bool SetField(Tiff tif, TiffTag tag, FieldValue[] ap)
        {
            DeflateCodec sp = tif.m_currentCodec as DeflateCodec;
            Debug.Assert(sp != null);

            const string module = "ZIPVSetField";

            switch (tag)
            {
                case TiffTag.ZIPQUALITY:
                    sp.m_zipquality = ap[0].ToInt();
                    if ((sp.m_state & DeflateCodec.ZSTATE_INIT_ENCODE) != 0)
                    {
                        if (sp.m_stream.deflateParams(sp.m_zipquality, zlibConst.Z_DEFAULT_STRATEGY) != zlibConst.Z_OK)
                        {
                            Tiff.ErrorExt(tif, tif.m_clientdata, module, 
                                "{0}: zlib error: {0}", tif.m_name, sp.m_stream.msg);
                            return false;
                        }
                    }

                    return true;
            }

            return base.SetField(tif, tag, ap);
        }

        public override FieldValue[] GetField(Tiff tif, TiffTag tag)
        {
            DeflateCodec sp = tif.m_currentCodec as DeflateCodec;
            Debug.Assert(sp != null);

            switch (tag)
            {
                case TiffTag.ZIPQUALITY:
                    FieldValue[] result = new FieldValue[1];
                    result[0].Set(sp.m_zipquality);
                    return result;
            }

            return base.GetField(tif, tag);
        }
    }
}
