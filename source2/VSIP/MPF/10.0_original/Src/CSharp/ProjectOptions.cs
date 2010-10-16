/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

namespace Microsoft.VisualStudio.Project
{
	public class ProjectOptions : System.CodeDom.Compiler.CompilerParameters
	{
		public string Config { get; set; }

		public ModuleKindFlags ModuleKind { get; set; }

		public bool EmitManifest { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public StringCollection DefinedPreprocessorSymbols { get; set; }

		public string XmlDocFileName { get; set; }

		public string RecursiveWildcard { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public StringCollection ReferencedModules { get; set; }

		public string Win32Icon { get; set; }

		public bool PdbOnly { get; set; }

		public bool Optimize { get; set; }

		public bool IncrementalCompile { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public int[] SuppressedWarnings { get; set; }

		public bool CheckedArithmetic { get; set; }

		public bool AllowUnsafeCode { get; set; }

		public bool DisplayCommandLineHelp { get; set; }

		public bool SuppressLogo { get; set; }

		public long BaseAddress { get; set; }

		public string BugReportFileName { get; set; }

		/// <devdoc>must be an int if not null</devdoc>
		public object CodePage { get; set; }

		public bool EncodeOutputInUtf8 { get; set; }

		public bool FullyQualifyPaths { get; set; }

		public int FileAlignment { get; set; }

		public bool NoStandardLibrary { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public StringCollection AdditionalSearchPaths { get; set; }

		public bool HeuristicReferenceResolution { get; set; }

		public string RootNamespace { get; set; }

		public bool CompileAndExecute { get; set; }

		/// <devdoc>must be an int if not null.</devdoc>
		public object UserLocaleId { get; set; }

		public FrameworkName TargetFrameworkMoniker { get; set; }

		public ProjectOptions()
		{
			EmitManifest = true;
			ModuleKind = ModuleKindFlags.ConsoleApplication;
		}

		public virtual string GetOptionHelp()
		{
			return null;
		}
	}
}
