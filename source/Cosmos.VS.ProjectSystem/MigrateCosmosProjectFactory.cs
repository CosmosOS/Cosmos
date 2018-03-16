using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.ProjectSystem
{
    internal class MigrateCosmosProjectFactory : IVsProjectFactory, IVsProjectUpgradeViaFactory, IVsProjectUpgradeViaFactory4
    {
        private const string PropertyGroup = nameof(PropertyGroup);
        private const string ProjectGuid = nameof(ProjectGuid);
        private const string TargetFramework = nameof(TargetFramework);

        private const string ItemGroup = nameof(ItemGroup);
        private const string Include = nameof(Include);
        private const string Remove = nameof(Remove);
        private const string None = nameof(None);
        private const string PackageReference = nameof(PackageReference);
        private const string ProjectReference = nameof(ProjectReference);

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
                var itemGroups = document.Root.Descendants().Where(e => XNameEqualsString(e.Name, ItemGroup));
                var projectReferences = itemGroups.Descendants().Where(e => XNameEqualsString(e.Name, ProjectReference));

                if (!projectReferences.Any())
                {
                    pLogger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_ERROR, projectName, bstrFileName, "Invalid project!");

                    pbstrUpgradedFullyQualifiedFileName = bstrFileName;
                    return VSConstants.VS_E_PROJECTMIGRATIONFAILED;
                }
                else if (projectReferences.Count() == 1)
                {
                    var codeProject = projectReferences.Single().Attributes().Where(
                        a => XNameEqualsString(a.Name, Include)).Single().Value;
                    codeProjectPath = Path.IsPathRooted(codeProject) ? codeProject
                        : Path.GetFullPath(Path.Combine(projectDir.FullName, codeProject));
                }
                else
                {
                    var projectReferencesWithoutPlugsProjects = projectReferences.Where(
                        p => !PlugsProjects.Contains(
                            Path.GetFileNameWithoutExtension(
                                p.Attributes().Where(
                                    a => XNameEqualsString(a.Name, Include)).Single().Value), StringComparer.OrdinalIgnoreCase));

                    if (projectReferencesWithoutPlugsProjects.Count() > 1)
                    {
                        pLogger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_WARNING, projectName, bstrFileName,
                            "Selecting project reference based on Cosmos project name!");

                        var codeProjectName = projectName.EndsWith("Boot") ? projectName.TrimSuffix("Boot") : projectName;
                        var codeProjectPathWithoutExtension = Path.Combine(projectDir.FullName, codeProjectName);

                        var possibleCodeProjects = projectReferences.Where(
                            p => StringEquals(
                                Path.GetFileNameWithoutExtension(
                                    p.Attributes().Where(
                                        a => XNameEqualsString(a.Name, Include)).Single().Value), codeProjectName));

                        if (possibleCodeProjects.Count() != 1)
                        {
                            pLogger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_ERROR, projectName, bstrFileName,
                                "Cannot find the code project!");

                            pbstrUpgradedFullyQualifiedFileName = bstrFileName;
                            return VSConstants.VS_E_PROJECTMIGRATIONFAILED;
                        }

                        var codeProject =
                            possibleCodeProjects.Single().Attributes().Where(a => XNameEqualsString(a.Name, Include)).Single().Value;
                        codeProjectPath = Path.IsPathRooted(codeProject) ? codeProject
                            : Path.GetFullPath(Path.Combine(projectDir.FullName, codeProject));
                    }
                    else
                    {
                        var codeProject = projectReferencesWithoutPlugsProjects
                            .Single().Attributes().Where(a => XNameEqualsString(a.Name, Include)).Single().Value;
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
                .Where(d => XNameEqualsString(d.Name, PropertyGroup)).Select(p => p.Descendants()
                .Where(e => !CommonProjectProperties.Contains(e.Name.LocalName, StringComparer.OrdinalIgnoreCase)));

            XDocument document;

            using (var codeProjectStream = File.OpenRead(codeProject))
            {
                document = XDocument.Load(codeProjectStream);
            }

            var root = document.Root;

            root.Descendants().Where(
                e => XNameEqualsString(e.Name, PropertyGroup)).Last().AddAfterSelf(
                    new XElement(PropertyGroup, cosmosProjectProperties));

            var packageReferencesItemGroups = root.Descendants().Where(
                e => XNameEqualsString(e.Name, ItemGroup) && e.Descendants().Where(
                    i => XNameEqualsString(i.Name, PackageReference)).Any());

            if (packageReferencesItemGroups.Descendants().Any(
                i => XNameEqualsString(i.Name, PackageReference) && i.Attributes().Any(
                    a => XNameEqualsString(a.Name, Include) && XNameEqualsString(a.Value, "Cosmos.Build"))))
            {
                return;
            }

            var properties = root.Descendants().Where(
                e => XNameEqualsString(e.Name, PropertyGroup)).Descendants();

            foreach(var targetFrameworkProperty in properties.Where(
                p => XNameEqualsString(p.Name, TargetFramework)))
            {
                targetFrameworkProperty.Value = "netcoreapp2.0";
            }

            properties.Where(p => XNameEqualsString(p.Name, ProjectGuid)).Remove();

            var itemGroups = root.Descendants().Where(
                e => XNameEqualsString(e.Name, ItemGroup));

            itemGroups.Where(e =>
            {
                var items = e.Descendants();

                if (items.Count() != 1)
                {
                    return false;
                }

                var singleItem = items.Single();

                if (!XNameEqualsString(singleItem.Name, None))
                {
                    return false;
                }

                var removeAttribute = singleItem.Attributes().Where(a => XNameEqualsString(a.Name, Remove)).FirstOrDefault();

                if (removeAttribute == null)
                {
                    return false;
                }

                if (removeAttribute.Value.IndexOf(Path.GetFileName(cosmosProject), StringComparison.OrdinalIgnoreCase) != -1)
                {
                    return true;
                }

                return false;
            }).Remove();
            
            var count = packageReferencesItemGroups.Count();
            XElement itemGroup;

            if (count == 0)
            {
                itemGroup = new XElement(ItemGroup);
                root.Add(itemGroup);
            }
            else if (count == 1)
            {
                itemGroup = packageReferencesItemGroups.Single();
            }
            else
            {
                var packageReferencesOnlyItemGroups = packageReferencesItemGroups.Where(
                    e => e.Descendants().All(i => XNameEqualsString(i.Name, PackageReference)));

                count = packageReferencesOnlyItemGroups.Count();

                if (count == 1)
                {
                    itemGroup = packageReferencesOnlyItemGroups.Single();
                }
                else
                {
                    if (count == 0)
                    {
                        itemGroup = packageReferencesItemGroups.First();
                    }
                    else
                    {
                        var cosmosPackageReferencesItemGroups = packageReferencesOnlyItemGroups.Where(
                            e => e.Descendants().Any(
                                p => p.Attributes().First(
                                    a => XNameEqualsString(a.Name, Include)).Value.Contains("Cosmos")));

                        if (cosmosPackageReferencesItemGroups.Any())
                        {
                            itemGroup = cosmosPackageReferencesItemGroups.First();
                        }
                        else
                        {
                            itemGroup = packageReferencesItemGroups.First();
                        }
                    }
                }
            }

            var cosmosBuildPackageReference = new XElement("PackageReference");
            cosmosBuildPackageReference.Add(new XAttribute(Include, "Cosmos.Build"));
            cosmosBuildPackageReference.Add(new XAttribute("Version", "*"));
            cosmosBuildPackageReference.Add(new XAttribute("NoWarn", "NU1604"));

            itemGroup.AddFirst(cosmosBuildPackageReference);

            using (var xmlWriter = XmlWriter.Create(
                codeProject,
                new XmlWriterSettings()
                {
                    Indent = true,
                    OmitXmlDeclaration = true
                }))
            {
                document.Save(xmlWriter);
            }

            File.Delete(cosmosProject);
        }

        private static bool XNameEqualsString(XName xName, string name) => StringEquals(xName.LocalName, name);

        private static bool StringEquals(string a, string b) => String.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
