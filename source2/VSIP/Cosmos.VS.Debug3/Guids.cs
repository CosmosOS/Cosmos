// Guids.cs
// MUST match guids.h
using System;

namespace Cosmos.Cosmos_VS_Debug3
{
    static class GuidList
    {
        public const string guidCosmos_VS_Debug3PkgString = "23d20bdd-63ad-4d95-b70c-5fa6f70bf6a8";
        public const string guidCosmos_VS_Debug3CmdSetString = "6a04202d-5e9a-4a9e-a62f-b9ae4ab8204f";
        public const string guidToolWindowPersistanceString = "74fb200c-1662-4ac5-ad11-891c8b59751a";

        public static readonly Guid guidCosmos_VS_Debug3CmdSet = new Guid(guidCosmos_VS_Debug3CmdSetString);
    };
}