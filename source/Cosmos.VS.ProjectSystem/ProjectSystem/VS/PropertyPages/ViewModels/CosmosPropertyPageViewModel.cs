using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.ProjectSystem;

using VSPropertyPages;

namespace Cosmos.VS.ProjectSystem.VS.PropertyPages.ViewModels
{
    internal class CosmosPropertyPageViewModel : PropertyPageViewModel
    {
        #region Static Items

        private static readonly List<int> DebugComItemsList = new List<int>() { 1, 2, 3, 4 };

        private static readonly Dictionary<string, string> DebugModeItemsDictionary = new Dictionary<string, string>()
        {
            ["IL"] = "IL",
            ["Source"] = "Source"
        };

        private static readonly Dictionary<string, string> TraceModeItemsDictionary = new Dictionary<string, string>()
        {
            ["None"] = "None",
            ["User"] = "User",
            ["Cosmos"] = "Cosmos",
            ["All"] = "All"
        };

        private static readonly Dictionary<string, string> StackCorruptionDetectionLevelItemsDictionary = new Dictionary<string, string>()
        {
            ["AllInstructions"] = "All Instructions",
            ["MethodFooters"] = "Method Footers"
        };

        #endregion

        public bool DebugEnabled
        {
            get => String.Equals(GetProperty(nameof(DebugEnabled)), "True", StringComparison.OrdinalIgnoreCase);
            set => SetProperty(nameof(DebugEnabled), value.ToString(), nameof(DebugEnabled));
        }

        public List<int> DebugComItems => DebugComItemsList;

        public int DebugCom
        {
            get => Int32.Parse(GetProperty(nameof(DebugCom)));
            set => SetProperty(nameof(DebugCom), value.ToString());
        }

        public Dictionary<string, string> DebugModeItems => DebugModeItemsDictionary;

        public string DebugMode
        {
            get => GetProperty(nameof(DebugMode));
            set => SetProperty(nameof(DebugMode), value);
        }

        public Dictionary<string, string> TraceModeItems => TraceModeItemsDictionary;

        public string TraceMode
        {
            get => GetProperty(nameof(TraceMode));
            set => SetProperty(nameof(TraceMode), value);
        }

        public bool IgnoreDebugStubAttribute
        {
            get => String.Equals(GetProperty(nameof(IgnoreDebugStubAttribute)), "True", StringComparison.OrdinalIgnoreCase);
            set => SetProperty(nameof(IgnoreDebugStubAttribute), value.ToString(), nameof(IgnoreDebugStubAttribute));
        }

        public bool StackCorruptionDetectionEnabled
        {
            get => String.Equals(GetProperty(nameof(StackCorruptionDetectionEnabled)), "True", StringComparison.OrdinalIgnoreCase);
            set => SetProperty(nameof(StackCorruptionDetectionEnabled), value.ToString(), nameof(StackCorruptionDetectionEnabled));
        }

        public Dictionary<string, string> StackCorruptionDetectionLevelItems => StackCorruptionDetectionLevelItemsDictionary;

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

        public CosmosPropertyPageViewModel(
            IPropertyManager propertyManager,
            IProjectThreadingService projectThreadingService)
            : base(propertyManager, projectThreadingService)
        {
        }

        private void SetAndRaiseIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
    }
}
