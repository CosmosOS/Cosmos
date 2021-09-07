using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace Cosmos.VS.ProjectSystem.VS
{
    [Export(ExportContractNames.ProjectTreeProviders.PhysicalViewRootGraft, typeof(IProjectTreeProvider))]
    [AppliesTo(ProjectCapability.Cosmos)]
    internal class PlugsProjectTreeProvider : ProjectTreeProviderBase
    {
        private static readonly ProjectImageMoniker ReferenceIcon = KnownMonikers.Reference.ToProjectSystemType();
        private static readonly ProjectTreeFlags PlugsProjectTreeFlags = ProjectTreeFlags.Create(
            ProjectTreeFlags.Common.BubbleUp | ProjectTreeFlags.Common.VirtualFolder);

        private readonly IActiveConfiguredProjectSubscriptionService _activeConfiguredProjectSubscriptionService;

        private IDisposable _itemsSubscriptionLink;

        [ImportingConstructor]
        protected PlugsProjectTreeProvider(
            IProjectThreadingService threadingService,
            UnconfiguredProject unconfiguredProject,
            IActiveConfiguredProjectSubscriptionService activeConfiguredProjectSubscriptionService)
            : base(threadingService, unconfiguredProject)
        {
            _activeConfiguredProjectSubscriptionService = activeConfiguredProjectSubscriptionService;
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            SubmitTreeUpdateAsync(
                (treeSnapshot, configuredProjectExports, cancellationToken) =>
                {
                    return Task.FromResult(new TreeUpdateResult(CreatePlugsFolder(), true));
                });

            var itemsBlock = _activeConfiguredProjectSubscriptionService.ProjectCatalogSource.SourceBlock;
            var targetBlock = new ActionBlock<IProjectVersionedValue<IProjectCatalogSnapshot>>(ItemsChangedAsync);

            _itemsSubscriptionLink = itemsBlock.LinkTo(
                targetBlock, new DataflowLinkOptions() { PropagateCompletion = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _itemsSubscriptionLink?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override ConfiguredProjectExports GetActiveConfiguredProjectExports(
            ConfiguredProject newActiveConfiguredProject)
        {
            return GetActiveConfiguredProjectExports<MyConfiguredProjectExports>(newActiveConfiguredProject);
        }

        private IProjectTree CreatePlugsFolder()
        {
            var caption = "Cosmos Plugs";
            var icon = KnownMonikers.Reference.ToProjectSystemType();
            var flags = PlugsProjectTreeFlags;

            return NewTree(caption, icon: ReferenceIcon, expandedIcon: ReferenceIcon, flags: PlugsProjectTreeFlags);
        }

        private Task ItemsChangedAsync(IProjectVersionedValue<IProjectCatalogSnapshot> snapshot)
        {
            SubmitTreeUpdateAsync(
                (treeSnapshot, configuredProjectExports, cancellationToken) =>
                {
                    var dependenciesNode = treeSnapshot.Value.Tree;

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        dependenciesNode = BuildTree(dependenciesNode, snapshot.Value, cancellationToken);
                    }
                    
                    return Task.FromResult(new TreeUpdateResult(dependenciesNode, false));
                });

            return Task.CompletedTask;
        }

        private IProjectTree BuildTree(
            IProjectTree oldTree,
            IProjectCatalogSnapshot snapshot,
            CancellationToken cancellationToken)
        {
            var tree = oldTree.ClearChildren();

            foreach (var reference in snapshot.Project.ProjectInstance.GetItems("PlugsReference"))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return oldTree;
                }

                tree = tree.Add(NewTree(reference.GetMetadataValue("Filename"),
                    icon: ReferenceIcon, flags: ProjectTreeFlags.ResolvedReference)).Parent;
            }

            return tree;
        }

        [Export]
        protected class MyConfiguredProjectExports : ConfiguredProjectExports
        {
            [ImportingConstructor]
            protected MyConfiguredProjectExports(ConfiguredProject configuredProject)
                : base(configuredProject)
            {
            }
        }
    }
}
