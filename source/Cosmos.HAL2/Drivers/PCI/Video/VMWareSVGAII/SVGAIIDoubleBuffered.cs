using Cosmos.Core;
using System;

namespace Cosmos.HAL.Drivers.PCI.Video
{
    /// <summary>
    /// VMWareSVGAII class.
    /// </summary>
    public class DoubleBufferedVMWareSVGAII : VMWareSVGAII
    {
        /// <summary>
        /// Copy rectangle.
        /// </summary>
        /// <param name="x">Source X coordinate.</param>
        /// <param name="y">Source Y coordinate.</param>
        /// <param name="newX">Destination X coordinate.</param>
        /// <param name="newY">Destination Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectange copy capability</exception>
        public override void Copy(uint x, uint y, uint newX, uint newY, uint width, uint height)
        {
            throw new Exception("Copy not supported by double buffered SVGAII driver.");
        }

        /// <summary>
        /// Define cursor.
        /// </summary>
        public override void DefineCursor()
        {
            throw new Exception("DefineCursor not supported by double buffered SVGAII driver.");
        }

        /// <summary>
        /// Set cursor.
        /// </summary>
        /// <param name="visible">Visible.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public override void SetCursor(bool visible, uint x, uint y)
        {
            throw new Exception("SetCursor not supported by double buffered SVGAII driver.");
        }

        /// <summary>
        /// Set pixel.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="color">Color.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public override void SetPixel(uint x, uint y, uint color)
        {
            if (x < width && y < height)
            {
                Video_Memory[frameBufferSize + ((y * width + x) * depth)] = color;
            }
        }

        /// <summary>
        /// Fill rectangle.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="color">Color.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectange copy capability</exception>
        public override void Fill(uint x, uint y, uint width, uint height, uint color)
        {
            for (uint h = 0; h < height; h++)
            {
                //Video_Memory.Fill(frameBufferSize + (h * width + x) * depth, width, color);
                for (uint w = 0; w < width; w++)
                {
                    SetPixel(w + x, y + h, color);
                }
            }
        }

        /// <summary>
        /// Update FIFO.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public void Update()
        {
            try
            {
                Video_Memory.MoveDown(frameBufferOffset, frameBufferSize, frameBufferSize);
            }
            catch (Exception)
            {
                Global.mDebugger.SendInternal("Faild To Update");
            }

            WriteToFifo((uint)FIFOCommand.Update);
            WriteToFifo(0);
            WriteToFifo(0);
            WriteToFifo(width);
            WriteToFifo(height);
            WaitForFifo();
        }
    }
}
