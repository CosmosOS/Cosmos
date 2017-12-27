using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.ProjectSystem
{
    internal class MigrateCosmosProjectFactory : IVsProjectFactory, IVsProjectUpgradeViaFactory, IVsProjectUpgradeViaFactory4
    {
        private const string CSharpProjectTypeGuid = "9A19103F-16F7-4668-BE54-9A1E7A4F7556";
        private const string FSharpProjectTypeGuid = "6EC3EE1D-3C4E-46DD-8F32-0CC8E7565705";
        private const string VisualBasicProjectTypeGuid = "778DAE3C-4631-46EA-AA77-85C1314464D9";

        private static readonly List<string> CommonProjectProperties = new List<string>()
        {
            "TargetFramework",
            "OutputType",
            "AppDesignerFolder",
            "ProjectGuid",
            "RootNamespace",
            "AssemblyName"
        };

        private static readonly List<string> PlugsProjects = new List<string>()
        {
            "Cosmos.Core.Plugs.Asm",
            "Cosmos.Core.Plugs",
            "Cosmos.Core_Asm",
            "Cosmos.Core_Plugs",
            "Cosmos.Debug.Kernel.Plugs.Asm",
            "Cosmos.System.Plugs",
            "Cosmos.System_Plugs",
            "Cosmos.System2.Plugs",
            "Cosmos.System2_Plugs"
        };

        #region IVsProjectFactory

        public int CanCreateProject(string pszFilename, uint grfCreateFlags, out int pfCanCreate)
        {
            pfCanCreate = !String.IsNullOrEmpty(pszFilename) && !PackageUtilities.IsFileNameInvalid(pszFilename) ? 1 : 0;
            return VSConstants.S_OK;
        }

        public int CreateProject(string pszFilename, string pszLocation, string pszName, uint grfCreateFlags,
            ref Guid iidProject, out IntPtr ppvProject, out int pfCanceled)
        {
            ppvProject = IntPtr.Zero;
            pfCanceled = 0;

            return VSConstants.S_OK;
        }

        public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp) => VSConstants.S_OK;

        public int Close() => VSConstants.S_OK;

        #endregion

        #region IVsProjectUpgradeViaFactory

        public int UpgradeProject(string bstrFileName, uint fUpgradeFlag, string bstrCopyLocation,
            out string pbstrUpgradedFullyQualifiedFileName, IVsUpgradeLogger pLogger, out int pUpgradeRequired,
            out Guid pguidNewProjectFactory)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectDir = new DirectoryInfo(Path.GetDirectoryName(bstrFileName));
            var projectName = Path.GetFileNameWithoutExtension(bstrFileName);

            string codeProjectPath;

            UpgradeProject_CheckOnly(bstrFileName, pLogger, out pUpgradeRequired, out pguidNewProjectFactory, out var flags);

            using (var projectStream = File.OpenRead(bstrFileName))
            {
                var document = XDocument.Load(projectStream);
                var itemGroups = document.Root.Descendants().Where(e => e.Name == "ItemGroup");
                var projectReferences = itemGroups.Descendants().Where(e => e.Name == "ProjectReference");

                if (!projectReferences.Any())
                {
                    pLogger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_ERROR, projectName, bstrFileName, "Invalid project!");

                    pbstrUpgradedFullyQualifiedFileName = bstrFileName;
                    return VSConstants.VS_E_PROJECTMIGRATIONFAILED;
                }
                else if (projectReferences.Count() == 1)
                {
                    var codeProject = projectReferences.Single().Attributes().Where(a => a.Name == "Include").Single().Value;
                    codeProjectPath = Path.IsPathRooted(codeProject) ? codeProject
                        : Path.GetFullPath(Path.Combine(projectDir.FullName, codeProject));
                }
                else
                {
                    var projectReferencesWithoutPlugsProjects = projectReferences.Where(
                        p => !PlugsProjects.Contains(
                            Path.GetFileNameWithoutExtension(
                                p.Attributes().Where(
                                    a => a.Name == "Include").Single().Value), StringComparer.OrdinalIgnoreCase));

                    if (projectReferencesWithoutPlugsProjects.Count() > 1)
                    {
                        pLogger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_WARNING, projectName, bstrFileName,
                            "Selecting project reference based on Cosmos project name!");

                        var codeProjectName = projectName.EndsWith("Boot") ? projectName.TrimSuffix("Boot") : projectName;
                        var codeProjectPathWithoutExtension = Path.Combine(projectDir.FullName, codeProjectName);

                        var possibleCodeProjects = projectReferences.Where(
                            p => Path.GetFileNameWithoutExtension(
                                p.Attributes().Where(
                                    a => a.Name == "Include").Single().Value) == codeProjectName);

                        if (possibleCodeProjects.Count() != 1)
                        {
                            pLogger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_ERROR, projectName, bstrFileName,
                                "Cannot find the code project!");

                            pbstrUpgradedFullyQualifiedFileName = bstrFileName;
                            return VSConstants.VS_E_PROJECTMIGRATIONFAILED;
                        }

                        var codeProject =
                            possibleCodeProjects.Single().Attributes().Where(a => a.Name == "Include").Single().Value;
                        codeProjectPath = Path.IsPathRooted(codeProject) ? codeProject
                            : Path.GetFullPath(Path.Combine(projectDir.FullName, codeProject));
                    }
                    else
                    {
                        var codeProject = projectReferencesWithoutPlugsProjects
                            .Single().Attributes().Where(a => a.Name == "Include").Single().Value;
                        codeProjectPath = Path.IsPathRooted(codeProject) ? codeProject
                            : Path.GetFullPath(Path.Combine(projectDir.FullName, codeProject));
                    }
                }
            }

            File.Copy(bstrFileName, Path.Combine(bstrCopyLocation, Path.GetFileName(bstrFileName)));
            File.Copy(codeProjectPath, Path.Combine(bstrCopyLocation, Path.GetFileName(codeProjectPath)));

            MigrateProject(bstrFileName, codeProjectPath);

            pbstrUpgradedFullyQualifiedFileName = null;
            return VSConstants.S_OK;
        }

        public int UpgradeProject_CheckOnly(string bstrFileName, IVsUpgradeLogger pLogger, out int pUpgradeRequired,
            out Guid pguidNewProjectFactory, out uint pUpgradeProjectCapabilityFlags)
        {
            var isCosmos = String.Equals(Path.GetExtension(bstrFileName), ".Cosmos", StringComparison.OrdinalIgnoreCase);

            pUpgradeRequired = isCosmos ? (int)__VSPPROJECTUPGRADEVIAFACTORYREPAIRFLAGS.VSPUVF_PROJECT_ONEWAYUPGRADE
                : (int)__VSPPROJECTUPGRADEVIAFACTORYREPAIRFLAGS.VSPUVF_PROJECT_NOREPAIR;

            if (isCosmos)
            {
                pguidNewProjectFactory = Guid.Empty;
                pUpgradeProjectCapabilityFlags = (uint)(__VSPPROJECTUPGRADEVIAFACTORYFLAGS.PUVFF_BACKUPSUPPORTED | __VSPPROJECTUPGRADEVIAFACTORYFLAGS.PUVFF_COPYBACKUP);
            }
            else
            {
                pguidNewProjectFactory = GetType().GUID;
                pUpgradeProjectCapabilityFlags = 0;
            }

            return VSConstants.S_OK;
        }

        public int GetSccInfo(string bstrProjectFileName, out string pbstrSccProjectName, out string pbstrSccAuxPath,
            out string pbstrSccLocalPath, out string pbstrProvider) => throw new NotImplementedException();

        #endregion

        #region IVsProjectUpgradeViaFactory4

        public void UpgradeProject_CheckOnly(string pszFileName, IVsUpgradeLogger pLogger, out uint pUpgradeRequired,
            out Guid pguidNewProjectFactory, out uint pUpgradeProjectCapabilityFlags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            UpgradeProject_CheckOnly(pszFileName, pLogger, out int upgradeRequired, out pguidNewProjectFactory,
                out pUpgradeProjectCapabilityFlags);

            pUpgradeRequired = (uint)upgradeRequired;
        }

        #endregion

        private void MigrateProject(string cosmosProject, string codeProject)
        {
            var cosmosProjectProperties = XDocument.Parse(File.ReadAllText(cosmosProject)).Root.Descendants()
                .Where(d => d.Name == "PropertyGroup").Select(p => p.Descendants()
                .Where(e => !CommonProjectProperties.Contains(e.Name.LocalName, StringComparer.OrdinalIgnoreCase)));

            var codeProjectStream = File.OpenRead(codeProject);

            var document = XDocument.Load(codeProjectStream);
            var itemGroups = document.Root.Descendants().Where(e => e.Name == "ItemGroup");
            var references = itemGroups.Descendants().Where(e => e.Name == "Reference");
            var projectReferences = itemGroups.Descendants().Where(e => e.Name == "ProjectReference");
            var packageReferences = itemGroups.Descendants().Where(e => e.Name == "PackageReference");

            if (packageReferences.Any(p => p.Attributes().Any(a => a.Name == "Include" && a.Value == "Cosmos.Build")))
            {
                return;
            }

            var cosmosBuildPackageReference = new XElement("PackageReference");
            cosmosBuildPackageReference.Add(new XAttribute("Include", "Cosmos.Build"));
            cosmosBuildPackageReference.Add(new XAttribute("Version", "*"));

            packageReferences.Append(cosmosBuildPackageReference);
            packageReferences = packageReferences.OrderBy(p => p.Attributes().Where(a => a.Name == "Include").First().Value);

            codeProjectStream.Dispose();

            File.WriteAllText(codeProject, GetProjectTemplate());

            var codeProjectDocument = XDocument.Parse(File.ReadAllText(codeProject));

            codeProjectDocument.Root.Descendants().Where(d => d.Name == "PropertyGroup").LastOrDefault()
                .Add(cosmosProjectProperties);

            if (references.Count() > 0)
            {
                codeProjectDocument.Root.Add(new XElement("ItemGroup", references));
            }

            if (projectReferences.Count() > 0)
            {
                codeProjectDocument.Root.Add(new XElement("ItemGroup", projectReferences));
            }

            if (packageReferences.Count() > 0)
            {
                codeProjectDocument.Root.Add(new XElement("ItemGroup", packageReferences));
            }

            using (var xmlWriter = XmlWriter.Create(codeProject,
                new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = "    ",
                    OmitXmlDeclaration = true
                }))
            {
                codeProjectDocument.Save(xmlWriter);
            }

            File.Delete(cosmosProject);
        }

        private string GetProjectTemplate()
        {
            using (var projectTemplateStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Cosmos.VS.ProjectSystem.ProjectTemplate.xml"))
            {
                using (var projectTemplateReader = new StreamReader(projectTemplateStream))
                {
                    return projectTemplateReader.ReadToEnd();
                }
            }
        }
    }
}
