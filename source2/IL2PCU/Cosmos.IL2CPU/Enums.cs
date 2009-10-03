using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU
{
    public enum LogSeverityEnum : byte
    {
        Warning = 0, Error = 1, Informational = 2, Performance = 3
    }
    public enum TraceAssemblies { All, Cosmos, User };
    public enum DebugMode { None, IL, Source, MLUsingGDB }
}