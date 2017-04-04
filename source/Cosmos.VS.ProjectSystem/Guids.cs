using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.VS.ProjectSystem
{
    static class Guids
    {
        public const string guidCosmosProjectPkgString =
            "4cae44ed-88b9-4b7c-822b-b040f18fcee3";

        public const string guidCosmosProjectCmdSetString =
            "c119efe0-fb56-44a1-b9f0-31247afb207f";

        public const string guidCosmosProjectFactoryString =
            "471EC4BB-E47E-4229-A789-D1F5F83B52D4";

        public static readonly Guid guidCosmosProjectCmdSet =
            new Guid(guidCosmosProjectCmdSetString);

        public static readonly Guid guidCosmosProjectFactory =
            new Guid(guidCosmosProjectFactoryString);
    }
}
