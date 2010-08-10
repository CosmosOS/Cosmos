using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TemplateWizard;

namespace Cosmos.VS.Package.Templates.Wizards
{
    public class GenerateGuidsWizard: IWizard
    {
        public static Guid? Guid1 = null;
        public static Guid? Guid2 = null;

        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            Guid1 = Guid.NewGuid();
            Guid2 = Guid.NewGuid();
            replacementsDictionary.Add("$GeneratedGuid1$", GenerateGuidsWizard.Guid1.Value.ToString("B"));
            replacementsDictionary.Add("$GeneratedGuid2$", GenerateGuidsWizard.Guid2.Value.ToString("B"));

        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
