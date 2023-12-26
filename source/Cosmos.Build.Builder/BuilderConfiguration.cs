using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cosmos.Build.Builder
{
    internal class BuilderConfiguration : IBuilderConfiguration
    {
        public bool NoVsLaunch { get; set; }
        public bool UserKit { get; set; }
        public bool BuildExtensions { get; set; }
    }
}
