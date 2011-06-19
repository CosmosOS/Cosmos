// Guids.cs
// MUST match guids.h
using System;

namespace Cosmos.Cosmos_VS_Debug2
{
    static class GuidList
    {
        public const string guidCosmos_VS_Debug2PkgString = "13b49ba3-e84c-4c50-b480-bfb1454f0ce2";
        public const string guidCosmos_VS_Debug2CmdSetString = "61a8038a-381e-41ab-8585-5b5d622c573c";
        public const string guidToolWindowPersistanceString = "6c945918-8ed1-4f8b-a839-893433b8285e";

        public static readonly Guid guidCosmos_VS_Debug2CmdSet = new Guid(guidCosmos_VS_Debug2CmdSetString);
    };
}