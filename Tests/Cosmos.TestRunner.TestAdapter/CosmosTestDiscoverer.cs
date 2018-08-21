using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Cosmos.TestRunner.TestAdapter
{
    [FileExtension(".dll")]
    [DefaultExecutorUri(CosmosTestExecutor.ExecutorUri)]
    public sealed class CosmosTestDiscoverer : ITestDiscoverer
    {
        private static readonly Uri ExecutorUri = new Uri(CosmosTestExecutor.ExecutorUri);

        public void DiscoverTests(
            IEnumerable<string> sources,
            IDiscoveryContext discoveryContext,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink)
        {
            var testCases = DiscoverTests(sources, discoveryContext, logger);

            foreach (var testCase in testCases)
            {
                discoverySink.SendTestCase(testCase);
            }
        }

        internal IEnumerable<TestCase> DiscoverTests(
            IEnumerable<string> sources,
            IDiscoveryContext discoveryContext,
            IMessageLogger logger)
        {
            var testCases = new List<TestCase>();

            foreach (var source in sources)
            {
                try
                {
                    using (var stream = File.OpenRead(source))
                    {
                        using (var peReader = new PEReader(stream))
                        {
                            if (!peReader.HasMetadata)
                            {
                                continue;
                            }

                            var metadataReader = peReader.GetMetadataReader();

                            if (!metadataReader.IsAssembly)
                            {
                                continue;
                            }

                            // todo: check exported types?

                            foreach (var typeHandle in metadataReader.TypeDefinitions)
                            {
                                var type = metadataReader.GetTypeDefinition(typeHandle);

                                if ((type.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public)
                                {
                                    var baseTypeHandle = type.BaseType;

                                    // has to be type reference, as it's not generic
                                    // and it's not declared in the same module
                                    if (baseTypeHandle.Kind == HandleKind.TypeReference)
                                    {
                                        var baseType = metadataReader.GetTypeReference((TypeReferenceHandle)baseTypeHandle);

                                        var baseTypeNamespace = metadataReader.GetString(baseType.Namespace);
                                        var baseTypeName = metadataReader.GetString(baseType.Name);

                                        if (String.Equals(baseTypeNamespace, "Cosmos.System", StringComparison.Ordinal)
                                            && String.Equals(baseTypeName, "Kernel", StringComparison.Ordinal)
                                            && baseType.ResolutionScope.Kind == HandleKind.AssemblyReference)
                                        {
                                            var baseTypeAssemblyReference = metadataReader.GetAssemblyReference(
                                                (AssemblyReferenceHandle)baseType.ResolutionScope);

                                            var baseTypeAssemblyName = metadataReader.GetString(baseTypeAssemblyReference.Name);

                                            if (String.Equals(baseTypeAssemblyName, "Cosmos.System2", StringComparison.Ordinal))
                                            {
                                                var typeNamespace = metadataReader.GetString(type.Namespace);
                                                var typeName = metadataReader.GetString(type.Name);

                                                var assemblyDefinition = metadataReader.GetAssemblyDefinition();
                                                var assemblyName = metadataReader.GetString(assemblyDefinition.Name);

                                                var fullyQualifiedName = $"{typeName}, {assemblyName}";

                                                if (!String.IsNullOrEmpty(typeNamespace))
                                                {
                                                    fullyQualifiedName = $"{typeNamespace}.{fullyQualifiedName}";
                                                }

                                                var testCase = new TestCase(fullyQualifiedName, ExecutorUri, source);
                                                testCases.Add(testCase);

                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return testCases;
        }
    }
}
