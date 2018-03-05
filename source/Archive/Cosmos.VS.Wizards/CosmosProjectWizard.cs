using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace Cosmos.VS.Wizards
{
    public class CosmosProjectWizard : IWizard
    {
        private const string BochsConfigurationFileName = "Cosmos.bxrc";

        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
        }

        private static string GetTemplate(string templateName)
        {
            using (var xStream = typeof(CosmosProjectWizard).Assembly.GetManifestResourceStream(templateName))
            {
                if (xStream == null)
                {
                    MessageBox.Show("Could not find template manifest stream : " + templateName);
                    return null;
                }
                using (var xReader = new StreamReader(xStream))
                {
                    return xReader.ReadToEnd();
                }
            }
        }

        private static string GetBochsConfigurationFileTemplate()
        {
            return GetTemplate(BochsConfigurationFileName);
        }

        private static string GetProjectFileTemplate()
        {
            return GetTemplate("Cosmos.VS.Wizards.CosmosProject.Cosmos");
        }

        public void ProjectFinishedGenerating(Project project)
        {
            // add Cosmos template to solution
            // read embedded template file
            var xInputString = GetProjectFileTemplate();
            if (xInputString == null)
            {
                return;
            }

            // set project extension for reference
            string extension = Path.GetExtension(project.UniqueName);

            xInputString = xInputString.Replace("$KernelProjectGuid$", mGuidKernel.ToString("b"));
            xInputString = xInputString.Replace("$CosmosProjectGuid$", mGuidCosmosProj.ToString("b"));
            xInputString = xInputString.Replace("$KernelProjectName$", project.Name);
            xInputString = xInputString.Replace("$CosmosProjectName$", project.Name + "Boot");
            xInputString = xInputString.Replace("$ProjectTypeExtension$", extension);
            var xFilename = Path.GetDirectoryName(project.FullName);
            xFilename = Path.Combine(xFilename, project.Name + "Boot");
            xFilename += ".Cosmos";
            File.WriteAllText(xFilename, xInputString);

            Project xCosmosProject;
            try
            {
                xCosmosProject = project.DTE.Solution.AddFromFile(xFilename, false);
            }
            catch (COMException)
            {
                return;
            }

            // set Cosmos Boot as startup project
            project.DTE.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();
            EnvDTE.UIHierarchy hierarchy = project.DTE.ActiveWindow.Object as EnvDTE.UIHierarchy;
            string fullPath = FindProject(hierarchy.UIHierarchyItems, xCosmosProject);
            if (fullPath.Length > 0)
            {
                hierarchy.GetItem(fullPath).Select(EnvDTE.vsUISelectionType.vsUISelectionTypeSelect);
                project.DTE.ExecuteCommand("Project.SetasStartUpProject");
            }

            // set all projects in all configurations are supposed to build
            foreach (SolutionConfiguration item in project.DTE.Solution.SolutionBuild.SolutionConfigurations)
            {
                for (int i = 1; i <= item.SolutionContexts.Count; i++)
                {
                    SolutionContext context = item.SolutionContexts.Item(i);
                    if (context.ProjectName.EndsWith(project.UniqueName))
                        context.ShouldBuild = true;
                    else if (context.ProjectName.EndsWith(xCosmosProject.UniqueName))
                        context.ShouldBuild = true;
                }
            }
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        public string FindProject(UIHierarchyItems h, Project p)
        {
            foreach (UIHierarchyItem xUiHierarchyItem in h)
            {
                if (xUiHierarchyItem.Name == p.Name)
                {
                    dynamic solutionNode = xUiHierarchyItem.Object;
                    dynamic project = p.Object;

                    if (solutionNode.Project.ProjectIDGuid == project.ProjectIDGuid)
                    {
                        return p.Name;
                    }
                }

                var xPartOfName = FindProject(xUiHierarchyItem.UIHierarchyItems, p);
                if (xPartOfName.Length > 0)
                {
                    if (xUiHierarchyItem.Object is SolutionClass solution)
                    {
                        return xUiHierarchyItem.Name + "\\" + xPartOfName;
                    }
                    return xUiHierarchyItem.Name + "\\" + xPartOfName;
                }
            }
            return string.Empty;
        }

        private Guid mGuidKernel;
        private Guid mGuidCosmosProj;

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            mGuidKernel = Guid.NewGuid();
            mGuidCosmosProj = Guid.NewGuid();
            replacementsDictionary.Add("$KernelProjectGuid$", mGuidKernel.ToString("B"));
            replacementsDictionary.Add("$CosmosProjectGuid$", mGuidCosmosProj.ToString("B"));
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}