// Guids.cs
// MUST match guids.h - There is no such file???? I think its referencing old VS2005 style, but vsct is used now instead.

using System;

namespace Cosmos.VS.DebugEngine
{
    static class Guids
    {
        public const string guidPackageString = "A82B45E9-2A89-43BD-925D-C7F0EDD212AA";
        public const string guidDebugEngineString = "DC8503AB-7EE6-456C-A209-66C690D9F6F4";
        public const string guidDebugEngineCmdSetString = "94EBFC49-EC0F-4BD3-B3FF-D3AADB8DAC9F";

        public static readonly Guid DebugEngineGuid = new Guid(guidDebugEngineString);
        public static readonly Guid DebugEngineCmdSetGuid = new Guid(guidDebugEngineCmdSetString);
    }
}