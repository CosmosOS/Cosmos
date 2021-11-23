using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.VS.PropertyPages.Designer;

namespace Cosmos.VS.ProjectSystem.ProjectSystem.VS.PropertyPages
{
    [Export(typeof(ILinkActionHandler))]
    [ExportMetadata("CommandName", "AddFilesISO")]
    internal sealed class MyCommandActionHandler : ILinkActionHandler
    {
        public Task HandleAsync(UnconfiguredProject project, IReadOnlyDictionary<string, string> editorMetadata)
        {
            var projectFolder = Path.GetDirectoryName(project.FullPath);
            var isoFolder = Path.Combine(projectFolder, "isoFiles");
            if (!Directory.Exists(isoFolder))
            {
                Directory.CreateDirectory(isoFolder);
            }

            Process.Start(isoFolder);

            return Task.CompletedTask;
        }
    }
}
