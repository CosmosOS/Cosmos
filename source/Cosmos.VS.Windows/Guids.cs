// Guids.cs
// MUST match guids.h - There is no such file???? I think its referencing old VS2005 style, but vsct is used now instead.
using System;

namespace Cosmos.VS.Windows
{
    static class Guids
    {
        public const string PackageGuidString = "A82B45E9-2A89-43BD-925D-C7F0EDD212AA";
        public const string CosmosMenuGuidString = "5C177460-F057-4AC1-8B7F-D1685C915E3D";
        public const string AsmToolbarGuidString = "1A98FB64-8FC8-4D24-95E4-E507700BA23C";
        public const string IDEToolbarGuidString = "425C500B-AB24-44B0-812A-74ECA127E6A1";
        public const string ImagesGuidString = "AA9D88D9-ACD2-4F2C-AB40-92887BDCE774";

        public const string AsmToolbarCmdSetString = "A875EA24-689A-4EAB-B9C2-DAC3EACB9501";
        public const string CosmosMenuCmdSetString = "3247C3E4-34B8-4DB0-8748-AD62495A5222";
        public const string IDEToolbarCmdSetString = "3D4B3F35-36E7-4CEA-8ACC-D9601E45C0B5";

        public static readonly Guid CosmosMenuCmdSetGuid = new Guid(CosmosMenuCmdSetString);
        public static readonly Guid AsmToolbarCmdSetGuid = new Guid(AsmToolbarCmdSetString);
        public static readonly Guid IDEToolbarCmdSetGuid = new Guid(IDEToolbarCmdSetString);
    }
}