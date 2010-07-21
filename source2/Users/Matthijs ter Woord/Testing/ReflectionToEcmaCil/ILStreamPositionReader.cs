using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectionToEcmaCil
{
    public class ILStreamPositionReader
    {
        /// <summary>
        /// .Key contains stream index, .Value contains instruction (logical) index
        /// </summary>
        public static IEnumerable<KeyValuePair<int, int>> GetIndexes(byte[] stream)
        {
            int xCurrentPosition = 0;
            yield return new KeyValuePair<int, int>(0, 0);
            while (xCurrentPosition < stream.Length)
            {
                
            }
        }
    }
}