using System;

namespace Cosmos.VS.DebugEngine
{
    static class Guids
    {
        public const string guidPackageString = "a82b45e9-2a89-43bd-925d-c7f0edd212aa";
        public const string guidDebugEngineString = "dc8503ab-7ee6-456c-a209-66c690d9f6f4";
        public const string guidDebugEngineCmdSetString = "94ebfc49-ec0f-4bd3-b3ff-d3aadb8dac9f";

        public static readonly Guid DebugEngineGuid = new Guid(guidDebugEngineString);
        public static readonly Guid DebugEngineCmdSetGuid = new Guid(guidDebugEngineCmdSetString);
    }
}
