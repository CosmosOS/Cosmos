// Guids.cs
// MUST match guids.h
using System;

namespace Cosmos.Cosmos_VS_Debug
{
    static class GuidList
    {
        public const string guidCosmos_VS_DebugPkgString = "14fb27b8-e3c9-4ef1-ba65-2f1c2275e53a";
        public const string guidCosmos_VS_DebugCmdSetString = "f476f392-95e1-4fda-ab97-14c833a197ee";
        public const string guidToolWindowPersistanceString = "3b704837-dbee-454a-ba83-ffe9b040e870";
        public const string guidCosmos_VS_DebugEditorFactoryString = "7bd2fcc5-6790-4cd6-afe6-84068d59fe49";

        public static readonly Guid guidCosmos_VS_DebugCmdSet = new Guid(guidCosmos_VS_DebugCmdSetString);
        public static readonly Guid guidCosmos_VS_DebugEditorFactory = new Guid(guidCosmos_VS_DebugEditorFactoryString);
    };
}