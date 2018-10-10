using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

using Avalonia.Controls;

using Cosmos.Build.Common;
using Cosmos.TestRunner.Core;
using Cosmos.TestRunner.Full;

namespace Cosmos.TestRunner.UI.ViewModels
{
    internal class SettingsDialogViewModel : IEngineConfiguration, INotifyPropertyChanged
    {
        private static IEngineConfiguration defaultEngineConfiguration = new DefaultEngineConfiguration();
        private static IEnumerable<Type> stableKernelTypes = TestKernelSets.GetStableKernelTypes();

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsDialogViewModel(Window aWindow)
        {
            KernelTypesToRun = new ObservableCollection<Type>(stableKernelTypes);
            RunTests = new RunTestsCommand(aWindow, this);
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

        public ICommand RunTests { get; set; }

        private void SetProperty<T>(ref T aProperty, T aValue, [CallerMemberName]string aPropertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(aProperty, aValue))
            {
                aProperty = aValue;
                OnPropertyChanged(aPropertyName);
            }
        }

        private void OnPropertyChanged(string aPropertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(aPropertyName));

        internal class RunTestsCommand : ICommand
        {
            private Window mWindow;
            private SettingsDialogViewModel mViewModel;

            public RunTestsCommand(Window aWindow, SettingsDialogViewModel aViewModel)
            {
                mWindow = aWindow;
                mViewModel = aViewModel;

                mViewModel.KernelTypesToRun.CollectionChanged += KernelTypesToRun_CollectionChanged;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => mViewModel.KernelTypesToRun.Any();

            public void Execute(object parameter) => mWindow.Close(mViewModel);

            private void KernelTypesToRun_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) =>
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
