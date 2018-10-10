using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

using Cosmos.Build.Common;
using Cosmos.TestRunner.Core;

namespace Cosmos.TestRunner.TestAdapter
{
    internal sealed class EngineConfiguration : IEngineConfiguration
    {
        public int AllowedSecondsInKernel { get; }
        public IEnumerable<RunTargetEnum> RunTargets { get; }
        public bool RunWithGDB { get; }
        public bool StartBochsDebugGUI { get; }

        public bool DebugIL2CPU { get; }
        public string KernelPkg { get; }
        public TraceAssemblies TraceAssembliesLevel { get; }
        public bool EnableStackCorruptionChecks { get; }
        public StackCorruptionDetectionLevel StackCorruptionDetectionLevel { get; }

        public IEnumerable<string> KernelAssembliesToRun { get; }

        private readonly IRunContext _context;

        public EngineConfiguration(
            IEnumerable<string> testKernels,
            IRunContext context)
        {
            KernelAssembliesToRun = testKernels;

            _context = context;

            var settingsXml = _context.RunSettings.SettingsXml;

            if (string.IsNullOrEmpty(settingsXml))
            {
                settingsXml = "<RunSettings />";
            }

            var doc = XDocument.Parse(settingsXml);
            var runConfiguration = doc.Element("RunConfiguration");

            AllowedSecondsInKernel = GetIntValue(runConfiguration, nameof(AllowedSecondsInKernel), 1200);

            var runTargetsString = GetStringValue(runConfiguration, nameof(RunTargets), "Bochs");
            var runTargets = new List<RunTargetEnum>();

            foreach (var runTargetName in runTargetsString.Split(';'))
            {
                if (Enum.TryParse<RunTargetEnum>(runTargetName, out var runTarget))
                {
                    runTargets.Add(runTarget);
                }
            }

            RunTargets = runTargets;
            RunWithGDB = GetBoolValue(runConfiguration, nameof(RunWithGDB), false);
            StartBochsDebugGUI = GetBoolValue(runConfiguration, nameof(StartBochsDebugGUI), false);

            DebugIL2CPU = GetBoolValue(runConfiguration, nameof(DebugIL2CPU), false);
            KernelPkg = GetStringValue(runConfiguration, nameof(KernelPkg), String.Empty);
            TraceAssembliesLevel = GetEnumValue(runConfiguration, nameof(KernelPkg), TraceAssemblies.User);
            EnableStackCorruptionChecks = GetBoolValue(runConfiguration, nameof(EnableStackCorruptionChecks), true);
            StackCorruptionDetectionLevel = GetEnumValue(
                runConfiguration, nameof(KernelPkg), StackCorruptionDetectionLevel.AllInstructions);
        }

        private static bool GetBoolValue(XElement element, XName name, bool defaultValue)
        {
            var value = GetStringValue(element, name, null);

            if (Boolean.TryParse(value, out var result))
            {
                return result;
            }

            return defaultValue;
        }

        private static T GetEnumValue<T>(XElement element, XName name, T defaultValue) where T : struct, Enum
        {
            var value = GetStringValue(element, name, null);

            if (Enum.TryParse<T>(value, true, out var result))
            {
                return result;
            }

            return defaultValue;
        }

        private static int GetIntValue(XElement element, XName name, int defaultValue)
        {
            var value = GetStringValue(element, name, null);

            if (Int32.TryParse(value, out var result))
            {
                return result;
            }

            return defaultValue;
        }

        private static string GetStringValue(XElement element, XName name, string defaultValue)
        {
            var childElement = element?.Element(name);

            if (childElement != null)
            {
                return childElement.Value;
            }

            return defaultValue;
        }
    }
}
