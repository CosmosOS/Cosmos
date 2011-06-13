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
using System.IO;

namespace BitMiracle.LibTiff.Classic.Internal
{
    class CodecWithPredictorTagMethods : TiffTagMethods
    {
        public override bool SetField(Tiff tif, TiffTag tag, FieldValue[] ap)
        {
            CodecWithPredictor sp = tif.m_currentCodec as CodecWithPredictor;
            Debug.Assert(sp != null);

            switch (tag)
            {
                case TiffTag.PREDICTOR:
                    sp.SetPredictorValue((Predictor)ap[0].ToByte());
                    tif.setFieldBit(CodecWithPredictor.FIELD_PREDICTOR);
                    tif.m_flags |= TiffFlags.DIRTYDIRECT;
                    return true;
            }

            TiffTagMethods childMethods = sp.GetChildTagMethods();
            if (childMethods != null)
                return childMethods.SetField(tif, tag, ap);

            return base.SetField(tif, tag, ap);
        }

        public override FieldValue[] GetField(Tiff tif, TiffTag tag)
        {
            CodecWithPredictor sp = tif.m_currentCodec as CodecWithPredictor;
            Debug.Assert(sp != null);

            switch (tag)
            {
                case TiffTag.PREDICTOR:
                    FieldValue[] result = new FieldValue[1];
                    result[0].Set(sp.GetPredictorValue());
                    return result;
            }

            TiffTagMethods childMethods = sp.GetChildTagMethods();
            if (childMethods != null)
                return childMethods.GetField(tif, tag);

            return base.GetField(tif, tag);
        }

        public override void PrintDir(Tiff tif, Stream fd, TiffPrintFlags flags)
        {
            CodecWithPredictor sp = tif.m_currentCodec as CodecWithPredictor;
            Debug.Assert(sp != null);

            if (tif.fieldSet(CodecWithPredictor.FIELD_PREDICTOR))
            {
                Tiff.fprintf(fd, "  Predictor: ");
                Predictor predictor = sp.GetPredictorValue();
                switch (predictor)
                {
                    case Predictor.NONE:
                        Tiff.fprintf(fd, "none ");
                        break;
                    case Predictor.HORIZONTAL:
                        Tiff.fprintf(fd, "horizontal differencing ");
                        break;
                    case Predictor.FLOATINGPOINT:
                        Tiff.fprintf(fd, "floating point predictor ");
                        break;
                }

                Tiff.fprintf(fd, "{0} (0x{1:x})\r\n", predictor, predictor);
            }

            TiffTagMethods childMethods = sp.GetChildTagMethods();
            if (childMethods != null)
                childMethods.PrintDir(tif, fd, flags);
            else
                base.PrintDir(tif, fd, flags);
        }
    }
}
