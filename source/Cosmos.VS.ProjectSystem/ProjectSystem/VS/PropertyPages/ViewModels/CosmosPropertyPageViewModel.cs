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
    }
}
