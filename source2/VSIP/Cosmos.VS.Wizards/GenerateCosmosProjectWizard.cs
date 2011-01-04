using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TemplateWizard;
using System.Windows.Forms;
using System.IO;

namespace Cosmos.VS.Package.Templates
{
    public class GenerateCosmosProjectWizard: IWizard
    {
        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
        }

        private static string GetTemplateString()
        {
            var xAsm = typeof(GenerateCosmosProjectWizard).Assembly;
            using (var xStream = xAsm.GetManifestResourceStream(typeof(Cosmos.VS.Wizards.ResHelper), "CosmosProject.Cosmos"))
            {
                if (xStream == null)
                {
                    MessageBox.Show("Could not find template manifest stream!");
                    return null;
                }
                using (var xReader = new StreamReader(xStream))
                {
                    return xReader.ReadToEnd();
                }
            }
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
            // read embedded template file
            var xInputString = GetTemplateString();
            if (xInputString == null)
            {
                return;
            }

            // set project extension for reference
            string extension = null;
            switch (project.Kind)
            {
                // VB.NET
                case "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}":
                    extension = "vbproj";
                    break;
                // C#
                case "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}":
                    extension = "csproj";
                    break;
                default:
                    // unknown project type
                    extension = project.UniqueName.Split('.')[1];
                    break;
            }

            xInputString = xInputString.Replace("$KernelGuid$", mGuidKernel.ToString("b"));
            xInputString = xInputString.Replace("$CosmosProjGuid$", mGuidCosmosProj.ToString("b"));
            xInputString = xInputString.Replace("$KernelName$", project.Name);
            xInputString = xInputString.Replace("$CosmosProjectName$", project.Name + "Boot");
            xInputString = xInputString.Replace("$ProjectTypeExtension$", extension);
            var xFilename = Path.GetDirectoryName(project.FullName);
            xFilename = Path.Combine(xFilename, project.Name + "Boot");
            xFilename += ".Cosmos";
            File.WriteAllText(xFilename, xInputString);
            var xCosmosProject = project.DTE.Solution.AddFromFile(xFilename, false);

            // Make .Cosmos project dependent on library project.
            // not working for all people EnvDTE.BuildDependency bd = project.DTE.Solution.SolutionBuild.BuildDependencies.Item(project.Name + "Boot.Cosmos");
            var xEnu = project.DTE.Solution.SolutionBuild.BuildDependencies.GetEnumerator();
            while (xEnu.MoveNext())
            {
                EnvDTE.BuildDependency bd = (EnvDTE.BuildDependency)xEnu.Current;
                if (bd.Project.Name == project.Name + "Boot")
                {
                    bd.AddProject(project.UniqueName);
                    break;
                }
            }

            //// found this with macro functionality in VS2010
            // but crashes
            //project.DTE.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();
            //EnvDTE.UIHierarchy hierarchy = project.DTE.ActiveWindow.Object as EnvDTE.UIHierarchy;
            //hierarchy.GetItem(GetNames(xCosmosProject)).Select(EnvDTE.vsUISelectionType.vsUISelectionTypeSelect);
            //project.DTE.ExecuteCommand("Project.SetasStartUpProject");
            // because
            // project.DTE.Solution.SolutionBuild.StartupProjects = new object[] { project.Name + "Boot.Cosmos"}; 
            // didnt work correct

            // set building Cosmos project
            var xCurrent = project.DTE.Solution.SolutionBuild.ActiveConfiguration;
            if (xCurrent != null)
            {
                var eno = xCurrent.SolutionContexts.GetEnumerator();
                while (eno.MoveNext())
                {
                    EnvDTE.SolutionContext context = eno.Current as EnvDTE.SolutionContext;
                    if (context.ProjectName == xCosmosProject.UniqueName || context.ProjectName == project.UniqueName)
                    {
                        context.ShouldBuild = true;
                    }
                }
            }
        }

        private static string GetNames(EnvDTE.Project project)
        {
            var xCurProjectItem = project.ParentProjectItem;
            var xResult = String.Empty;
            do
            {
                if (String.IsNullOrEmpty(xResult))
                {
                    xResult = xCurProjectItem.Name;
                }
                else
                {
                    xResult = xCurProjectItem.Name + "\\" + xResult;
                }
                if (xCurProjectItem.ContainingProject != null)
                {
                    xCurProjectItem = xCurProjectItem.ContainingProject.ParentProjectItem;
                }
                else
                {
                    xCurProjectItem = null;
                }
            } while (xCurProjectItem != null);
            return xResult;
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        private Guid mGuidKernel;
        private Guid mGuidCosmosProj;

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            mGuidKernel = Guid.NewGuid();
            mGuidCosmosProj = Guid.NewGuid();
            replacementsDictionary.Add("$KernelGuid$", mGuidKernel.ToString("B"));
            replacementsDictionary.Add("$CosmosProjGuid$", mGuidCosmosProj.ToString("B"));

        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}