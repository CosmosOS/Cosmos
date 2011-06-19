// Guids.cs
// MUST match guids.h
using System;

namespace Cosmos.Cosmos_VS_Windows
{
    static class GuidList
    {
        public const string guidCosmos_VS_WindowsPkgString = "a82b45e9-2a89-43bd-925d-c7f0edd212aa";
        public const string guidCosmos_VS_WindowsCmdSetString = "3d4b3f35-36e7-4cea-8acc-d9601e45c0b9";
        public const string guidToolWindowPersistanceString = "f019fb29-c2c2-4d27-9abf-739533c939be";

        public static readonly Guid guidCosmos_VS_WindowsCmdSet = new Guid(guidCosmos_VS_WindowsCmdSetString);
    };
}