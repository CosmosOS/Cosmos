using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

using Cosmos.TestRunner.Core;

namespace Cosmos.TestRunner.TestAdapter
{
    [ExtensionUri(ExecutorUri)]
    public sealed class CosmosTestExecutor : ITestExecutor
    {
        public const string ExecutorUri = "executor://CosmosTestExecutor";

        private CancellationTokenSource _cancellationTokenSource;

        public void RunTests(
            IEnumerable<TestCase> tests,
            IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            foreach (var test in tests)
            {
                var configuration = new EngineConfiguration(new string[] { test.Source }, runContext);
                var testEngine = new Engine(configuration);

                var outputHandler = new TestAdapterOutputHandler(frameworkHandle);
                testEngine.SetOutputHandler(outputHandler);

                var testResult = new TestResult(test);

                frameworkHandle.RecordStart(test);

                var kernelTestResult = testEngine.Execute(_cancellationTokenSource.Token).KernelTestResults[0];

                testResult.Outcome = kernelTestResult.Result ? TestOutcome.Passed : TestOutcome.Failed;

                var messages = new Collection<TestResultMessage>();

                foreach (var message in outputHandler.Messages)
                {
                    messages.Add(new TestResultMessage(String.Empty, message));
                }
                
                frameworkHandle.RecordEnd(test, testResult.Outcome);
                frameworkHandle.RecordResult(testResult);
            }
        }

        public void RunTests(
            IEnumerable<string> sources,
            IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            var discoverer = new CosmosTestDiscoverer();
            var tests = discoverer.DiscoverTests(sources, runContext, null);

            RunTests(tests, runContext, frameworkHandle);
        }

        public void Cancel() => _cancellationTokenSource.Cancel();
    }
}
