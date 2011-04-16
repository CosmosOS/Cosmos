using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TemplateWizard;
using System.Windows.Forms;
using System.IO;
using EnvDTE;

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
			#region add Cosmos template to solution
			// read embedded template file
            var xInputString = GetTemplateString();
            if (xInputString == null)
            {
                return;
            }

            // set project extension for reference
            string extension = project.UniqueName.Split('.')[1];

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
			#endregion

			#region make .Cosmos project dependent on library project.
			var xEnu = project.DTE.Solution.SolutionBuild.BuildDependencies.GetEnumerator();
			dynamic xCosmosBootProjectObj = xCosmosProject.Object; // VSProjectNode
			var xCosmosBootGuid = xCosmosBootProjectObj.ProjectIDGuid;
			while (xEnu.MoveNext())
			{
				EnvDTE.BuildDependency bd = (EnvDTE.BuildDependency)xEnu.Current;

				dynamic xDependencyGUID = bd.Project.Object;
				if (xDependencyGUID.ProjectIDGuid == xCosmosBootGuid)
				{
					bd.AddProject(project.UniqueName);
					break;
				}
			}
			#endregion

			#region set Cosmos Boot as startup project
			project.DTE.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();
			EnvDTE.UIHierarchy hierarchy = project.DTE.ActiveWindow.Object as EnvDTE.UIHierarchy;
			string fullPath = FindProject(hierarchy.UIHierarchyItems, xCosmosProject);
			if (fullPath.Length > 0)
			{
				hierarchy.GetItem(fullPath).Select(EnvDTE.vsUISelectionType.vsUISelectionTypeSelect);
				project.DTE.ExecuteCommand("Project.SetasStartUpProject");
			}
			#endregion

			#region set all projects in all configurations are supposed to build
			foreach (SolutionConfiguration item in project.DTE.Solution.SolutionBuild.SolutionConfigurations)
			{
				for (int i = 1; i <= item.SolutionContexts.Count; i++)
				{
					item.SolutionContexts.Item(i).ShouldBuild = true;
				}
			}
			#endregion
		}

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

		public string FindProject(EnvDTE.UIHierarchyItems h, Project p)
		{
			var xEnumerator = h.GetEnumerator();

			while (xEnumerator.MoveNext())
			{
				UIHierarchyItem xHierarchyItem = (UIHierarchyItem)xEnumerator.Current;
				if (xHierarchyItem.Name == p.Name)
				{
					dynamic node = xHierarchyItem.Object;
					dynamic nodeArg = p.Object; // Cosmos.VS.Package.VSProjectNode

					try
					{
						if (node.Object.Project.ProjectIDGuid == nodeArg.ProjectIDGuid)
							return p.Name;
					}
					catch
					{
					}

					try
					{
						if (node.Project.ProjectIDGuid == nodeArg.ProjectIDGuid)
							return p.Name;
					}
					catch
					{
					}
				}
				var xPartOfName = FindProject(xHierarchyItem.UIHierarchyItems, p);
				if (xPartOfName.Length > 0)
				{
					SolutionClass solution = xHierarchyItem.Object as SolutionClass;
					if (solution != null)
						return xHierarchyItem.Name + "\\" + xPartOfName;
					return xHierarchyItem.Name + "\\" + xPartOfName;
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
            replacementsDictionary.Add("$KernelGuid$", mGuidKernel.ToString("B"));
            replacementsDictionary.Add("$CosmosProjGuid$", mGuidCosmosProj.ToString("B"));
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}