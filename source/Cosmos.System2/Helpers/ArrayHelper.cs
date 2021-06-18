using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Helpers
{
    /// <summary>
    /// Array Helper class (contains useful functions about array manipulation).
    /// </summary>
    public class ArrayHelper
    {
        /// <summary>
        /// Contatenate two byte arrays. https://stackoverflow.com/a/45531730
        /// </summary>
        /// <param name="first">First byte array.</param>
        /// <param name="second">Byte array to concatenate.</param>
        public static byte[] Concat(byte[] first, byte[] second)
        {
            byte[] output;
            int alen = 0;

            if (first == null)
            {
                output = new byte[second.Length];
            }
            else
            {
                output = new byte[first.Length + second.Length];
                alen = first.Length;
                for (int i = 0; i < first.Length; i++)
                {
                    output[i] = first[i];
                }
            }
            for (int j = 0; j < second.Length; j++)
            {
                output[alen + j] = second[j];
            }
            return output;
        }

        /// <summary>
        /// Split byte array into chunks of a specified size.
        /// </summary>
        /// <param name="buffer">Byte array to split.</param>
        /// <param name="chunksize">Chunk size.</param>
        public static byte[][] ArraySplit(byte[] buffer, int chunksize = 1000)
        {
            var size = buffer.Length;
            var chunkCount = (buffer.Length + chunksize - 1) / chunksize;
            var bufferArray = new byte[chunkCount][];
            int index = 0;

            for (var i = 0; i < chunkCount; i++)
            {
                bufferArray[i] = new byte[Math.Min(chunksize, buffer.Length - i * chunksize)];
            }
            for (var i = 0; i < chunkCount; i++)
            {
                for (var j = 0; j < bufferArray[i].Length; j++)
                {
                    bufferArray[i][j] = buffer[index];
                    index++;
                }
            }
            return bufferArray;
        }
    }
}
