using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil
{
    public static class DataIds
    {
        // leave this #if, it will help narrow down any using code which has not been #if-ed
#if DEBUG
        /// <summary>
        ///     This const is used only in debug builds, for being able to make useful dumps of metadata.
        /// </summary>
        public const int DebugMetaId = 1;
#endif
    }
}