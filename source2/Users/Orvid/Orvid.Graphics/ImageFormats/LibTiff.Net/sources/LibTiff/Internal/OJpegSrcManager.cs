using System;
using System.Collections.Generic;
using System.Text;
using BitMiracle.LibJpeg;

namespace BitMiracle.LibTiff.Classic.Internal
{
    class OJpegSrcManager : Jpeg_Source
    {
        protected OJpegCodec m_sp;

        public OJpegSrcManager(OJpegCodec sp)
        {
            initInternalBuffer(null, 0);
            m_sp = sp;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void init_source()
        {
        }

        /// <summary>
        /// Fills input buffer
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if operation succeed; otherwise, <c>false</c>
        /// </returns>
        public override bool fill_input_buffer()
        {
            Tiff tif = m_sp.GetTiff();
            byte[] mem = null;
            uint len = 0;
            if (!m_sp.OJPEGWriteStream(out mem, out len))
                Tiff.ErrorExt(tif, tif.m_clientdata, "LibJpeg", "Premature end of JPEG data");

            initInternalBuffer(mem, (int)len);
            return true;
        }

        /// <summary>
        /// Skip data - used to skip over a potentially large amount of
        /// uninteresting data (such as an APPn marker).
        /// </summary>
        /// <param name="num_bytes">The number of bytes to skip.</param>
        /// <remarks>Writers of suspendable-input applications must note that skip_input_data
        /// is not granted the right to give a suspension return.  If the skip extends
        /// beyond the data currently in the buffer, the buffer can be marked empty so
        /// that the next read will cause a fill_input_buffer call that can suspend.
        /// Arranging for additional bytes to be discarded before reloading the input
        /// buffer is the application writer's problem.</remarks>
        public override void skip_input_data(int num_bytes)
        {
            Tiff tif = m_sp.GetTiff();
            Tiff.ErrorExt(tif, tif.m_clientdata, "LibJpeg", "Unexpected error");
        }

        /// <summary>
        /// This is the default resync_to_restart method for data source
        /// managers to use if they don't have any better approach.
        /// </summary>
        /// <param name="cinfo">An instance of <see cref="JpegDecompressor"/></param>
        /// <param name="desired">The desired</param>
        /// <returns><c>false</c> if suspension is required.</returns>
        /// <remarks>That method assumes that no backtracking is possible.
        /// Some data source managers may be able to back up, or may have
        /// additional knowledge about the data which permits a more
        /// intelligent recovery strategy; such managers would
        /// presumably supply their own resync method.<br/><br/>
        /// read_restart_marker calls resync_to_restart if it finds a marker other than
        /// the restart marker it was expecting.  (This code is *not* used unless
        /// a nonzero restart interval has been declared.)  cinfo.unread_marker is
        /// the marker code actually found (might be anything, except 0 or FF).
        /// The desired restart marker number (0..7) is passed as a parameter.<br/><br/>
        /// This routine is supposed to apply whatever error recovery strategy seems
        /// appropriate in order to position the input stream to the next data segment.
        /// Note that cinfo.unread_marker is treated as a marker appearing before
        /// the current data-source input point; usually it should be reset to zero
        /// before returning.<br/><br/>
        /// This implementation is substantially constrained by wanting to treat the
        /// input as a data stream; this means we can't back up.  Therefore, we have
        /// only the following actions to work with:<br/>
        /// 1. Simply discard the marker and let the entropy decoder resume at next
        /// byte of file.<br/>
        /// 2. Read forward until we find another marker, discarding intervening
        /// data.  (In theory we could look ahead within the current bufferload,
        /// without having to discard data if we don't find the desired marker.
        /// This idea is not implemented here, in part because it makes behavior
        /// dependent on buffer size and chance buffer-boundary positions.)<br/>
        /// 3. Leave the marker unread (by failing to zero cinfo.unread_marker).
        /// This will cause the entropy decoder to process an empty data segment,
        /// inserting dummy zeroes, and then we will reprocess the marker.<br/>
        /// #2 is appropriate if we think the desired marker lies ahead, while #3 is
        /// appropriate if the found marker is a future restart marker (indicating
        /// that we have missed the desired restart marker, probably because it got
        /// corrupted).<br/>
        /// We apply #2 or #3 if the found marker is a restart marker no more than
        /// two counts behind or ahead of the expected one.  We also apply #2 if the
        /// found marker is not a legal JPEG marker code (it's certainly bogus data).
        /// If the found marker is a restart marker more than 2 counts away, we do #1
        /// (too much risk that the marker is erroneous; with luck we will be able to
        /// resync at some future point).<br/>
        /// For any valid non-restart JPEG marker, we apply #3.  This keeps us from
        /// overrunning the end of a scan.  An implementation limited to single-scan
        /// files might find it better to apply #2 for markers other than EOI, since
        /// any other marker would have to be bogus data in that case.</remarks>
        public override bool resync_to_restart(JpegDecompressor cinfo, int desired)
        {
            Tiff tif = m_sp.GetTiff();
            Tiff.ErrorExt(tif, tif.m_clientdata, "LibJpeg", "Unexpected error");
            return false;
        }

        /// <summary>
        /// Terminate source - called by jpeg_finish_decompress
        /// after all data has been read.  Often a no-op.
        /// </summary>
        /// <remarks>NB: <b>not</b> called by jpeg_abort or jpeg_destroy; surrounding
        /// application must deal with any cleanup that should happen even
        /// for error exit.</remarks>
        public override void term_source()
        {
        }
    }
}
