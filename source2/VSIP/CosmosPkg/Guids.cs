using System;

namespace Cosmos.VS.Package {
    static class Guids {
        public const string guidProjectPkgString = "4cae44ed-88b9-4b7c-822b-b040f18fcee3";
        public const string guidProjectCmdSetString = "c119efe0-fb56-44a1-b9f0-31247afb207f";
        public const string guidProjectFactoryString = "471EC4BB-E47E-4229-A789-D1F5F83B52D4";
        public const string guidPropPageEnvironmentString = "08C09552-8DD6-4b59-B901-51EBD5D564A6";

        public static readonly Guid guidProjectCmdSet =
            new Guid(guidProjectCmdSetString);

        public static readonly Guid guidProjectFactory =
            new Guid(guidProjectFactoryString);
    };
}