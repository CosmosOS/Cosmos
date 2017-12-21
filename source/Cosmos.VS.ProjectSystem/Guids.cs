using System;

namespace Cosmos.VS.ProjectSystem
{
    static class Guids
    {
        public const string guidCosmosProjectPkgString = "4cae44ed-88b9-4b7c-822b-b040f18fcee3";
        public const string guidCosmosProjectCmdSetString = "c119efe0-fb56-44a1-b9f0-31247afb207f";
        public const string guidCosmosProjectFactoryString = "471ec4bb-e47e-4229-a789-d1f5f83b52d4";
        public const string guidPropPageEnvironmentString = "08c09552-8dd6-4b59-b901-51ebd5d564a6";
        public const string guidCosmosPropertyPageString = "d33a2d29-c4fd-4e12-a510-4c01a14d63e1";

        public static readonly Guid guidCosmosProjectCmdSet = new Guid(guidCosmosProjectCmdSetString);
        public static readonly Guid guidCosmosProjectFactory = new Guid(guidCosmosProjectFactoryString);
        public static readonly Guid guidCosmosPropertyPage = new Guid(guidCosmosPropertyPageString);
    }
}
