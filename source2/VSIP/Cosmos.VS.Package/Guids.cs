using System;

namespace Cosmos.VS.Package {
    static class Guids {
        public const string guidProjectPkgString = "4cae44ed-88b9-4b7c-822b-b040f18fcee3";
        public const string guidProjectCmdSetString = "c119efe0-fb56-44a1-b9f0-31247afb207f";
        public const string guidProjectFactoryString = "471EC4BB-E47E-4229-A789-D1F5F83B52D4";
        public const string guidPropPageEnvironmentString = "08C09552-8DD6-4b59-B901-51EBD5D564A6";

        // These GUIDs are also used in the .iss, as are the prop page names. If you change names
        // or GUIDs you need to update the .iss as well.
        public const string BuildPage = "d33a2d29-c4fd-4e12-a510-4c01a14d63e1";
		public const string DebugPage = "39801176-289f-405f-9425-2931a2c03912";
		public const string VMOptionsPropertyPage = "3b72bb68-7363-45a2-9eba-55c8d5f36e36";

        public static readonly Guid guidProjectCmdSet = new Guid(guidProjectCmdSetString);
        public static readonly Guid guidProjectFactory = new Guid(guidProjectFactoryString);
    };
}