using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectionToEcmaCil
{
    public class ILStreamPositionReader
    {
        private static readonly byte[] InstructionLengthsLow;
        private static readonly byte[] InstructionLengthsHigh;

        static ILStreamPositionReader()
        {
            #region init InstructionLengthsLow
            InstructionLengthsLow = new byte[256]{
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
                1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                4, 8, 4, 8, 0, 0, 0, 4, 4, 4, 0, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 4, 4, 4, 4, 4, 4, 4, 4,
                4, 4, 4, 4, 4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4,
                4, 4, 4, 4, 4, 4, 0, 0, 0, 4, 0, 4, 4, 4, 4, 4,
                4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 4, 0, 4,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 4, 4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 4, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 1, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            #endregion
            #region init InstructionLengthsHigh
            InstructionLengthsHigh = new byte[256]{
                0, 0, 0, 0, 0, 0, 4, 4, 0, 2, 2, 2, 2, 2, 2, 0,
                0, 0, 1, 0, 0, 4, 4, 0, 0, 0, 0, 0, 4, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            #endregion
        }

        /// <summary>
        /// .Key contains stream index, .Value contains instruction (logical) index
        /// </summary>
        public static IEnumerable<KeyValuePair<int, int>> GetIndexes(byte[] stream)
        {
            int xCurrentIndex = 0;
            int xCurrentPosition = 0;
            yield return new KeyValuePair<int, int>(0, 0);
            
            while (xCurrentPosition < stream.Length)
            {
                var xOp = stream[xCurrentPosition];
                if (xOp == 0xFE)
                {
                    xCurrentPosition++;
                    xOp = stream[xCurrentPosition];
                    xCurrentPosition += InstructionLengthsHigh[xOp];
                }
                else
                {
                    xCurrentPosition += InstructionLengthsLow[xOp];
                }
                xCurrentPosition++;
                xCurrentIndex++;
                yield return new KeyValuePair<int, int>(xCurrentPosition, xCurrentIndex);
            }
        }
    }
}