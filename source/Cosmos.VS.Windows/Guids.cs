using System;

namespace Cosmos.VS.Windows
{
    static class Guids
    {
        public const string PackageGuidString = "2084f89a-967c-4f7d-a3f8-217c3fa8ee66";
        public const string CosmosMenuGuidString = "5c177460-f057-4ac1-8b7f-d1685c915e3d";
        public const string AsmToolbarGuidString = "1a98fb64-8fc8-4d24-95e4-e507700ba23c";
        public const string IDEToolbarGuidString = "425c500b-ab24-44b0-812a-74eca127e6a1";
        public const string ImagesGuidString = "aa9d88d9-acd2-4f2c-ab40-92887bdce774";

        public const string AsmToolbarCmdSetString = "a875ea24-689a-4eab-b9c2-dac3eacb9501";
        public const string CosmosMenuCmdSetString = "3247c3e4-34b8-4db0-8748-ad62495a5222";
        public const string IDEToolbarCmdSetString = "3d4b3f35-36e7-4cea-8acc-d9601e45c0b5";

        public static readonly Guid CosmosMenuCmdSetGuid = new Guid(CosmosMenuCmdSetString);
        public static readonly Guid AsmToolbarCmdSetGuid = new Guid(AsmToolbarCmdSetString);
        public static readonly Guid IDEToolbarCmdSetGuid = new Guid(IDEToolbarCmdSetString);
    }
}
