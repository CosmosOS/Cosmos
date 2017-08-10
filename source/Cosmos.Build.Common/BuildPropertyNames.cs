using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmos.Build.Common
{
    public static class BuildPropertyNames
    {
        public const string StackCorruptionDetectionEnabledString = "StackCorruptionDetectionEnabled";
        public const string StackCorruptionDetectionLevelString = "StackCorruptionDetectionLevel";
        public const string ProfileString = "Profile";
        public const string NameString = "Name";
        public const string DescriptionString = "Description";
        public const string DeploymentString = "Deployment";
        public const string LaunchString = "Launch";
        public const string ShowLaunchConsoleString = "ShowLaunchConsole";
        public const string DebugEnabledString = "DebugEnabled";
        public const string DebugModeString = "DebugMode";
        public const string IgnoreDebugStubAttributeString = "IgnoreDebugStubAttribute";
        public const string CosmosDebugPortString = "CosmosDebugPort";
        public const string VisualStudioDebugPortString = "VisualStudioDebugPort";
        public const string PxeInterfaceString = "PxeInterface";
        public const string SlavePortString = "SlavePort";
        public const string VMwareEditionString = "VMwareEdition";
        public const string OutputPathString = "OutputPath";
        public const string FrameworkString = "Framework";
        public const string UseInternalAssemblerString = "UseInternalAssembler";
        public const string TraceAssembliesString = "TraceAssemblies";
        public const string EnableGDBString = "EnableGDB";
        public const string StartCosmosGDBString = "StartCosmosGDB";
        public const string EnableBochsDebugString = "EnableBochsDebug";
        public const string StartBochsDebugGui = "StartBochsDebugGui";
        public const string BinFormatString = "BinFormat";
    }
}
