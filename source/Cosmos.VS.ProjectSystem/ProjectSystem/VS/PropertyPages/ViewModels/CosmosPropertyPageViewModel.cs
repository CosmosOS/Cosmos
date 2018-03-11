using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.ProjectSystem;

using VSPropertyPages;

namespace Cosmos.VS.ProjectSystem.VS.PropertyPages.ViewModels
{
    internal class CosmosPropertyPageViewModel : PropertyPageViewModel
    {
        public CosmosPropertyPageViewModel(
            IPropertyManager propertyManager,
            IProjectThreadingService projectThreadingService)
            : base(propertyManager, projectThreadingService)
        {
        }

        public bool DebugEnabled
        {
            get => String.Equals(GetProperty(nameof(DebugEnabled)), "True", StringComparison.OrdinalIgnoreCase);
            set => SetProperty(nameof(DebugEnabled), value.ToString(), nameof(DebugEnabled));
        }

        public Dictionary<string, string> DebugModeItems { get; } = new Dictionary<string, string>()
        {
            ["IL"] = "IL",
            ["Source"] = "Source"
        };

        public string DebugMode
        {
            get => GetProperty(nameof(DebugMode));
            set => SetProperty(nameof(DebugMode), value);
        }

        public Dictionary<string, string> TraceModeItems { get; } = new Dictionary<string, string>()
        {
            ["None"] = "None",
            ["User"] = "User",
            ["Cosmos"] = "Cosmos",
            ["All"] = "All"
        };

        public string TraceMode
        {
            get => GetProperty(nameof(TraceMode));
            set => SetProperty(nameof(TraceMode), value);
        }

        public bool StackCorruptionDetectionEnabled
        {
            get => String.Equals(GetProperty(nameof(StackCorruptionDetectionEnabled)), "True", StringComparison.OrdinalIgnoreCase);
            set => SetProperty(nameof(StackCorruptionDetectionEnabled), value.ToString(), nameof(StackCorruptionDetectionEnabled));
        }

        public Dictionary<string, string> StackCorruptionDetectionLevelItems { get; } = new Dictionary<string, string>()
        {
            ["AllInstructions"] = "All Instructions",
            ["MethodFooters"] = "Method Footers"
        };

        public string StackCorruptionDetectionLevel
        {
            get => GetProperty(nameof(StackCorruptionDetectionLevel));
            set => SetProperty(nameof(StackCorruptionDetectionLevel), value);
        }

        public string CosmosDebugPort
        {
            get => GetProperty(nameof(CosmosDebugPort));
            set => SetProperty(nameof(CosmosDebugPort), value);
        }

        public string VisualStudioDebugPort
        {
            get => GetProperty(nameof(VisualStudioDebugPort));
            set => SetProperty(nameof(VisualStudioDebugPort), value);
        }

        public bool IgnoreDebugStubAttribute
        {
            get => String.Equals(GetProperty(nameof(IgnoreDebugStubAttribute)), "True", StringComparison.OrdinalIgnoreCase);
            set => SetProperty(nameof(IgnoreDebugStubAttribute), value.ToString(), nameof(IgnoreDebugStubAttribute));
        }
    }
}
