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
            using (var xStream = xAsm.GetManifestResourceStream(typeof(GenerateCosmosProjectWizard), "CosmosProject.Cosmos"))
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
            //project.DTE.Solution
            var xInputString = GetTemplateString();
            if (xInputString == null)
            {
                return;
            }

            xInputString = xInputString.Replace("$KernelGuid$", mGuidKernel.ToString("b"));
            xInputString = xInputString.Replace("$CosmosProjGuid$", mGuidCosmosProj.ToString("b"));
            xInputString = xInputString.Replace("$KernelName$", project.Name);
            xInputString = xInputString.Replace("$CosmosProjectName$", project.Name + "Boot");
            var xFilename = Path.GetDirectoryName(project.FullName);
            xFilename = Path.Combine(xFilename, project.Name + "Boot");
            xFilename += ".Cosmos";
            File.WriteAllText(xFilename, xInputString);
            project.DTE.Solution.AddFromFile(xFilename, false);
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
