using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using Avalonia.Controls;

using ReactiveUI;

using Cosmos.Build.Common;
using Cosmos.TestRunner.Core;
using Cosmos.TestRunner.Full;

namespace Cosmos.TestRunner.UI.ViewModels
{
    internal class SettingsDialogViewModel : ReactiveObject, IEngineConfiguration
    {
        private static IEngineConfiguration defaultEngineConfiguration = new DefaultEngineConfiguration();
        private static IEnumerable<Type> stableKernelTypes = TestKernelSets.GetStableKernelTypes();

        public SettingsDialogViewModel(Window aWindow)
        {
            KernelTypesToRun = new ObservableCollection<Type>(stableKernelTypes);

            RunTestsCommand = ReactiveCommand.Create(
                () => RunTests(aWindow),
                this.WhenAny(v => v.KernelTypesToRun, c => c.Value.Any()));
        }

        #region Engine Configuration

        private int mAllowedSecondsInKernel = defaultEngineConfiguration.AllowedSecondsInKernel;
        public int AllowedSecondsInKernel
        {
            get => mAllowedSecondsInKernel;
            set => SetProperty(ref mAllowedSecondsInKernel, value);
        }

        public IEnumerable<RunTargetEnum> RunTargets
        {
            get
            {
                yield return RunTarget;
            }
            set => RunTarget = value.Single();
        }

        private bool mRunWithGDB = defaultEngineConfiguration.RunWithGDB;
        public bool RunWithGDB
        {
            get => mRunWithGDB;
            set => SetProperty(ref mRunWithGDB, value);
        }

        private bool mStartBochsDebugGUI = defaultEngineConfiguration.StartBochsDebugGUI;
        public bool StartBochsDebugGUI
        {
            get => mStartBochsDebugGUI;
            set => SetProperty(ref mStartBochsDebugGUI, value);
        }

        private bool mDebugIL2CPU = defaultEngineConfiguration.DebugIL2CPU;
        public bool DebugIL2CPU
        {
            get => mDebugIL2CPU;
            set => SetProperty(ref mDebugIL2CPU, value);
        }

        private string mKernelPkg = defaultEngineConfiguration.KernelPkg;
        public string KernelPkg
        {
            get => mKernelPkg;
            set => SetProperty(ref mKernelPkg, value);
        }

        private TraceAssemblies mTraceAssembliesLevel = defaultEngineConfiguration.TraceAssembliesLevel;
        public TraceAssemblies TraceAssembliesLevel
        {
            get => mTraceAssembliesLevel;
            set => SetProperty(ref mTraceAssembliesLevel, value);
        }

        private bool mEnableStackCorruptionChecks = defaultEngineConfiguration.EnableStackCorruptionChecks;
        public bool EnableStackCorruptionChecks
        {
            get => mEnableStackCorruptionChecks;
            set => SetProperty(ref mEnableStackCorruptionChecks, value);
        }

        private StackCorruptionDetectionLevel mStackCorruptionDetectionLevel = defaultEngineConfiguration.StackCorruptionDetectionLevel;
        public StackCorruptionDetectionLevel StackCorruptionDetectionLevel
        {
            get => mStackCorruptionDetectionLevel;
            set => SetProperty(ref mStackCorruptionDetectionLevel, value);
        }

        private DebugMode mDebugMode = defaultEngineConfiguration.DebugMode;
        public DebugMode DebugMode
        {
            get => mDebugMode;
            set => SetProperty(ref mDebugMode, value);
        }

        public IEnumerable<string> KernelAssembliesToRun
        {
            get
            {
                foreach (var xKernelType in KernelTypesToRun)
                {
                    yield return xKernelType.Assembly.Location;
                }
            }
        }

        #endregion

        private RunTargetEnum mRunTarget = defaultEngineConfiguration.RunTargets.FirstOrDefault();
        public RunTargetEnum RunTarget
        {
            get => mRunTarget;
            set => SetProperty(ref mRunTarget, value);
        }

        public ObservableCollection<Type> KernelTypesToRun { get; }

        #region Items

        public IEnumerable<Type> TestKernels { get; } = stableKernelTypes;
        public IEnumerable RunTargetItems { get; } = Enum.GetValues(typeof(RunTargetEnum));
        public IEnumerable TraceAssembliesLevelItems { get; } = Enum.GetValues(typeof(TraceAssemblies));
        public IEnumerable StackCorruptionDetectionLevelItems { get; } = Enum.GetValues(typeof(StackCorruptionDetectionLevel));

        #endregion

        public ICommand RunTestsCommand { get; set; }

        private void RunTests(Window aWindow) => aWindow.Close(this);

        private void SetProperty<T>(ref T aProperty, T aValue, [CallerMemberName]string aPropertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(aProperty, aValue))
            {
                aProperty = aValue;
                this.RaisePropertyChanged(aPropertyName);
            }
        }
    }
}
