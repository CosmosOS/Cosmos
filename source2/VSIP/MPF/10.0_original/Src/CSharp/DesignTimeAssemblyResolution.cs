using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Build.Utilities;
using Microsoft.Build.Execution;
using System.IO;
using System.Globalization;

namespace Microsoft.VisualStudio.Project
{
	public class DesignTimeAssemblyResolution
	{
		private const string OriginalItemSpec = "originalItemSpec";
	
		private const string FoundAssemblyVersion = "Version";
		
		private const string HighestVersionInRedistList = "HighestVersionInRedist";
		
		private const string OutOfRangeDependencies = "OutOfRangeDependencies";

		private RarInputs rarInputs;

		public bool EnableLogging { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetFrameworkPaths")]
		public virtual void Initialize(ProjectNode projectNode)
		{
			if (projectNode == null)
			{
				throw new ArgumentNullException("projectNode");
			}

			if (projectNode.CallMSBuild("GetFrameworkPaths") != MSBuildResult.Successful)
			{
				throw new InvalidOperationException("Build of GetFrameworkPaths failed.");
			}

			this.rarInputs = new RarInputs(projectNode.CurrentConfig);
		}

		public virtual VsResolvedAssemblyPath[] Resolve(IEnumerable<string> assemblies)
		{
			if (assemblies == null)
			{
				throw new ArgumentNullException("assemblies");
			}

			// Resolve references WITHOUT invoking MSBuild to avoid re-entrancy problems.
			const bool projectDtar = true;
			var rar = new Microsoft.Build.Tasks.ResolveAssemblyReference();
			var engine = new MockEngine(EnableLogging);
			rar.BuildEngine = engine;

			// first set common properties/items then if projectDtar then set additional projectDtar properties
			ITaskItem[] assemblyItems = assemblies.Select(assembly => new TaskItem(assembly)).ToArray();
			rar.Assemblies = assemblyItems;
			rar.SearchPaths = rarInputs.PdtarSearchPaths;

			rar.TargetFrameworkDirectories = rarInputs.TargetFrameworkDirectories;
			rar.AllowedAssemblyExtensions = rarInputs.AllowedAssemblyExtensions;
			rar.TargetProcessorArchitecture = rarInputs.TargetProcessorArchitecture;
			rar.TargetFrameworkVersion = rarInputs.TargetFrameworkVersion;
			rar.TargetFrameworkMoniker = rarInputs.TargetFrameworkMoniker;
			rar.TargetFrameworkMonikerDisplayName = rarInputs.TargetFrameworkMonikerDisplayName;
			rar.TargetedRuntimeVersion = rarInputs.TargetedRuntimeVersion;
			rar.FullFrameworkFolders = rarInputs.FullFrameworkFolders;
			rar.LatestTargetFrameworkDirectories = rarInputs.LatestTargetFrameworkDirectories;
			rar.FullTargetFrameworkSubsetNames = rarInputs.FullTargetFrameworkSubsetNames;
			rar.FullFrameworkAssemblyTables = rarInputs.FullFrameworkAssemblyTables;
			rar.IgnoreDefaultInstalledAssemblySubsetTables = rarInputs.IgnoreDefaultInstalledAssemblySubsetTables;
			rar.ProfileName = rarInputs.ProfileName;

			rar.Silent = !this.EnableLogging;
			rar.FindDependencies = true;
			rar.AutoUnify = false;
			rar.FindSatellites = false;
			rar.FindSerializationAssemblies = false;
			rar.FindRelatedFiles = false;

			// This set needs to be kept in sync with the set of project instance data that
			// is populated into RarInputs
			if (projectDtar)
			{
				// set project dtar specific properties 
				rar.CandidateAssemblyFiles = rarInputs.CandidateAssemblyFiles;
				rar.StateFile = rarInputs.StateFile;
				rar.InstalledAssemblySubsetTables = rarInputs.InstalledAssemblySubsetTables;
				rar.TargetFrameworkSubsets = rarInputs.TargetFrameworkSubsets;
			}

			IEnumerable<VsResolvedAssemblyPath> results;

			try
			{
				rar.Execute();
				results = FilterResults(rar.ResolvedFiles).Select(pair => new VsResolvedAssemblyPath
				{
					bstrOrigAssemblySpec = pair.Key,
					bstrResolvedAssemblyPath = pair.Value,
				});
			}
			catch (Exception ex)
			{
				if (ErrorHandler.IsCriticalException(ex))
				{
					throw;
				}

				engine.RecordRARExecutionException(ex);
				results = Enumerable.Empty<VsResolvedAssemblyPath>();
			}
			finally
			{
				if (this.EnableLogging)
				{
					WriteLogFile(engine, projectDtar, assemblies);
				}
			}

			return results.ToArray();
		}

		private static IEnumerable<KeyValuePair<string, string>> FilterResults(IEnumerable<ITaskItem> resolvedFiles)
		{
			foreach (ITaskItem resolvedFile in resolvedFiles)
			{
				bool bAddResolvedAssemblyToResultList = true;

				// excludeVersionWarningsFromResult
				string foundAssemblyVersion = resolvedFile.GetMetadata(FoundAssemblyVersion);
				string highestVersionInRedist = resolvedFile.GetMetadata(HighestVersionInRedistList);

				Version asmVersion = null;
				bool parsedAsmVersion = Version.TryParse(foundAssemblyVersion, out asmVersion);

				Version redistVersion = null;
				bool parsedRedistVersion = Version.TryParse(highestVersionInRedist, out redistVersion);

				if ((parsedAsmVersion && parsedRedistVersion) && asmVersion > redistVersion)
				{
					// if the version of the assembly is greater than the highest version - for that assembly - found in 
					// the chained(possibly) redist lists; then the assembly does not belong to the target framework 
					bAddResolvedAssemblyToResultList = false;
				}

				// check outOfRangeDependencies
				string outOfRangeDependencies = resolvedFile.GetMetadata(OutOfRangeDependencies);
				if (!String.IsNullOrEmpty(outOfRangeDependencies))
				{
					// This metadata is a semi-colon delimited list of dependent assembly names which target
					// a higher framework. If this metadata is NOT EMPTY then
					// the current assembly does have dependencies which are greater than the current target framework

					// so let's exclude this assembly
					bAddResolvedAssemblyToResultList = false;
				}

				if (bAddResolvedAssemblyToResultList)
				{
					yield return new KeyValuePair<string, string>(resolvedFile.GetMetadata(OriginalItemSpec), resolvedFile.ItemSpec);
				}
			}
		}

		private static void WriteLogFile(MockEngine engine, bool projectDtar, IEnumerable<string> assemblies)
		{
			string logFilePrefix = projectDtar ? "P" : "G";

			string logFilePath = Path.Combine(Path.GetTempPath(), logFilePrefix + @"Dtar" + (Guid.NewGuid()).ToString("N", CultureInfo.InvariantCulture) + ".log");

			StringBuilder inputs = new StringBuilder();

			Array.ForEach<string>(assemblies.ToArray(), assembly => { inputs.Append(assembly); inputs.Append(";"); inputs.Append("\n"); });

			string logAssemblies = "Inputs: \n" + inputs.ToString() + "\n\n";

			string finalLog = logAssemblies + engine.Log;

			string[] finalLogLines = finalLog.Split(new char[] { '\n' });

			File.WriteAllLines(logFilePath, finalLogLines);
		}
		
		/// <summary>
		/// Engine required by RAR, primarily for collecting logs
		/// </summary>
		private class MockEngine : IBuildEngine
		{
			private int messages = 0;
			private int warnings = 0;
			private int errors = 0;
			private StringBuilder log = new StringBuilder();
			private readonly bool enableLog = false;

			internal MockEngine(bool enableLog)
			{
				this.enableLog = enableLog;
			}

			public void RecordRARExecutionException(Exception ex)
			{
				if (!enableLog) return;

				log.Append(String.Format(CultureInfo.InvariantCulture, "{0}", ex.ToString()));
			}

			public void LogErrorEvent(BuildErrorEventArgs eventArgs)
			{
				if (eventArgs == null)
				{
					throw new ArgumentNullException("eventArgs");
				}

				if (!enableLog) return;

				if (eventArgs.File != null && eventArgs.File.Length > 0)
				{
					log.Append(String.Format(CultureInfo.InvariantCulture, "{0}({1},{2}): ", eventArgs.File, eventArgs.LineNumber, eventArgs.ColumnNumber));
				}

				log.Append("ERROR ");
				log.Append(eventArgs.Code);
				log.Append(": ");
				++errors;

				log.AppendLine(eventArgs.Message);
			}

			public void LogWarningEvent(BuildWarningEventArgs eventArgs)
			{
				if (eventArgs == null)
				{
					throw new ArgumentNullException("eventArgs");
				}

				if (!enableLog) return;

				if (eventArgs.File != null && eventArgs.File.Length > 0)
				{
					log.Append(String.Format(CultureInfo.InvariantCulture, "{0}({1},{2}): ", eventArgs.File, eventArgs.LineNumber, eventArgs.ColumnNumber));
				}

				log.Append("WARNING ");
				log.Append(eventArgs.Code);
				log.Append(": ");
				++warnings;

				log.AppendLine(eventArgs.Message);
			}

			public void LogCustomEvent(CustomBuildEventArgs eventArgs)
			{
				if (eventArgs == null)
				{
					throw new ArgumentNullException("eventArgs");
				}

				if (!enableLog) return;

				log.Append(eventArgs.Message);
				log.Append("\n");
			}

			public void LogMessageEvent(BuildMessageEventArgs eventArgs)
			{
				if (eventArgs == null)
				{
					throw new ArgumentNullException("eventArgs");
				}

				log.Append(eventArgs.Message);
				log.Append("\n");

				++messages;
			}

			public bool ContinueOnError
			{
				get { return false; }
			}

			public string ProjectFileOfTaskNode
			{
				get { return String.Empty; }
			}

			public int LineNumberOfTaskNode
			{
				get { return 0; }
			}

			public int ColumnNumberOfTaskNode
			{
				get { return 0; }
			}

			internal string Log
			{
				get { return log.ToString(); }
			}

			public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs)
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Accesssor for RAR related properties in the projectInstance.
		/// See ResolveAssemblyReferennce task msdn docs for member descriptions
		/// </summary>
		private class RarInputs
		{
			#region private fields

			// RAR related property/item names etc
			private const string TargetFrameworkDirectory = "TargetFrameworkDirectory";
			private const string RegistrySearchPathFormat = "Registry:{0},{1},{2}{3}";
			private const string FrameworkRegistryBase = "FrameworkRegistryBase";
			private const string TargetFrameworkVersionName = "TargetFrameworkVersion";
			private const string AssemblyFoldersSuffix = "AssemblyFoldersSuffix";
			private const string AssemblyFoldersExConditions = "AssemblyFoldersExConditions";
			private const string AllowedReferenceAssemblyFileExtensions = "AllowedReferenceAssemblyFileExtensions";
			private const string ProcessorArchitecture = "ProcessorArchitecture";
			private const string TargetFrameworkMonikerName = "TargetFrameworkMoniker";
			private const string TargetFrameworkMonikerDisplayNameName = "TargetFrameworkMonikerDisplayName";
			private const string TargetedRuntimeVersionName = "TargetedRuntimeVersion";
			private const string FullFrameworkReferenceAssemblyPaths = "_FullFrameworkReferenceAssemblyPaths";
			private const string TargetFrameworkProfile = "TargetFrameworkProfile";

			private const string ProjectDesignTimeAssemblyResolutionSearchPaths = "ProjectDesignTimeAssemblyResolutionSearchPaths";
			private const string Content = "Content";
			private const string None = "None";
			private const string RARResolvedReferencePath = "ReferencePath";
			private const string IntermediateOutputPath = "IntermediateOutputPath";
			private const string InstalledAssemblySubsetTablesName = "InstalledAssemblySubsetTables";
			private const string IgnoreInstalledAssemblySubsetTables = "IgnoreInstalledAssemblySubsetTables";
			private const string ReferenceInstalledAssemblySubsets = "_ReferenceInstalledAssemblySubsets";
			private const string FullReferenceAssemblyNames = "FullReferenceAssemblyNames";
			private const string LatestTargetFrameworkDirectoriesName = "LatestTargetFrameworkDirectories";
			private const string FullFrameworkAssemblyTablesName = "FullFrameworkAssemblyTables";
			private const string MSBuildProjectDirectory = "MSBuildProjectDirectory";

			#endregion //private fields

			public string[] TargetFrameworkDirectories { get; private set; }
			public string[] AllowedAssemblyExtensions { get; private set; }
			public string TargetProcessorArchitecture { get; private set; }
			public string TargetFrameworkVersion { get; private set; }
			public string TargetFrameworkMoniker { get; private set; }
			public string TargetFrameworkMonikerDisplayName { get; private set; }
			public string TargetedRuntimeVersion { get; private set; }
			public string[] FullFrameworkFolders { get; private set; }
			public string ProfileName { get; private set; }
			public string[] PdtarSearchPaths { get; private set; }
			public string[] CandidateAssemblyFiles { get; private set; }
			public string StateFile { get; private set; }
			public ITaskItem[] InstalledAssemblySubsetTables { get; private set; }
			public bool IgnoreDefaultInstalledAssemblySubsetTables { get; private set; }
			public string[] TargetFrameworkSubsets { get; private set; }
			public string[] FullTargetFrameworkSubsetNames { get; private set; }
			public ITaskItem[] FullFrameworkAssemblyTables { get; private set; }
			public string[] LatestTargetFrameworkDirectories { get; private set; }

			#region constructors
			public RarInputs(ProjectInstance projectInstance)
			{
				// Run through all of the entries we want to extract from the project instance before we discard it to save memory
				TargetFrameworkDirectories = GetTargetFrameworkDirectories(projectInstance);
				AllowedAssemblyExtensions = GetAllowedAssemblyExtensions(projectInstance);
				TargetProcessorArchitecture = GetTargetProcessorArchitecture(projectInstance);
				TargetFrameworkVersion = GetTargetFrameworkVersion(projectInstance);
				TargetFrameworkMoniker = GetTargetFrameworkMoniker(projectInstance);
				TargetFrameworkMonikerDisplayName = GetTargetFrameworkMonikerDisplayName(projectInstance);
				TargetedRuntimeVersion = GetTargetedRuntimeVersion(projectInstance);
				FullFrameworkFolders = GetFullFrameworkFolders(projectInstance);
				LatestTargetFrameworkDirectories = GetLatestTargetFrameworkDirectories(projectInstance);
				FullTargetFrameworkSubsetNames = GetFullTargetFrameworkSubsetNames(projectInstance);
				FullFrameworkAssemblyTables = GetFullFrameworkAssemblyTables(projectInstance);
				IgnoreDefaultInstalledAssemblySubsetTables = GetIgnoreDefaultInstalledAssemblySubsetTables(projectInstance);
				ProfileName = GetProfileName(projectInstance);

				/*               
				 * rar.CandidateAssemblyFiles = rarInputs.CandidateAssemblyFiles;
				   rar.StateFile = rarInputs.StateFile;
				   rar.InstalledAssemblySubsetTables = rarInputs.InstalledAssemblySubsetTables;
				   rar.TargetFrameworkSubsets = rarInputs.TargetFrameworkSubsets;
				 */

				// This set needs to be kept in sync with the set of project instance data that
				// is passed into Rar
				PdtarSearchPaths = GetPdtarSearchPaths(projectInstance);

				CandidateAssemblyFiles = GetCandidateAssemblyFiles(projectInstance);
				StateFile = GetStateFile(projectInstance);
				InstalledAssemblySubsetTables = GetInstalledAssemblySubsetTables(projectInstance);
				TargetFrameworkSubsets = GetTargetFrameworkSubsets(projectInstance);
			}
			#endregion // constructors

			#region public properties

			#region common properties/items

			private string[] GetTargetFrameworkDirectories(ProjectInstance projectInstance)
			{
				if (TargetFrameworkDirectories == null)
				{
					string val = projectInstance.GetPropertyValue(TargetFrameworkDirectory).Trim();

					TargetFrameworkDirectories = val.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
						.Select(s => s.Trim())
						.Where(s => s.Length > 0)
						.ToArray();
				}

				return TargetFrameworkDirectories;
			}

			private static string[] GetAllowedAssemblyExtensions(ProjectInstance projectInstance)
			{
				string[] allowedAssemblyExtensions;

				string val = projectInstance.GetPropertyValue(AllowedReferenceAssemblyFileExtensions).Trim();

				allowedAssemblyExtensions = val.Split(';').Select(s => s.Trim()).ToArray();

				return allowedAssemblyExtensions;
			}

			private static string GetTargetProcessorArchitecture(ProjectInstance projectInstance)
			{
				string val = projectInstance.GetPropertyValue(ProcessorArchitecture).Trim();

				return val;
			}

			private static string GetTargetFrameworkVersion(ProjectInstance projectInstance)
			{
				string val = projectInstance.GetPropertyValue(TargetFrameworkVersionName).Trim();

				return val;
			}

			private static string GetTargetFrameworkMoniker(ProjectInstance projectInstance)
			{
				string val = projectInstance.GetPropertyValue(TargetFrameworkMonikerName).Trim();

				return val;
			}

			private static string GetTargetFrameworkMonikerDisplayName(ProjectInstance projectInstance)
			{
				string val = projectInstance.GetPropertyValue(TargetFrameworkMonikerDisplayNameName).Trim();

				return val;
			}

			private static string GetTargetedRuntimeVersion(ProjectInstance projectInstance)
			{
				string val = projectInstance.GetPropertyValue(TargetedRuntimeVersionName).Trim();

				return val;
			}

			private static string[] GetFullFrameworkFolders(ProjectInstance projectInstance)
			{
				string val = projectInstance.GetPropertyValue(FullFrameworkReferenceAssemblyPaths).Trim();

				string[] _fullFrameworkFolders = val.Split(';').Select(s => s.Trim()).ToArray();

				return _fullFrameworkFolders;
			}

			private static string[] GetLatestTargetFrameworkDirectories(ProjectInstance projectInstance)
			{
				IEnumerable<ITaskItem> taskItems = projectInstance.GetItems(LatestTargetFrameworkDirectoriesName);

				string[] latestTargetFrameworkDirectory = (taskItems.Select((Func<ITaskItem, string>)((item) => { return item.ItemSpec.Trim(); }))).ToArray();

				return latestTargetFrameworkDirectory;
			}

			private static string GetProfileName(ProjectInstance projectInstance)
			{
				string val = projectInstance.GetPropertyValue(TargetFrameworkProfile).Trim();

				return val;
			}
			#endregion //common properties/items

			#region project dtar specific properties/items

			private static string[] GetPdtarSearchPaths(ProjectInstance projectInstance)
			{
				string val = projectInstance.GetPropertyValue(ProjectDesignTimeAssemblyResolutionSearchPaths).Trim();

				string[] _pdtarSearchPaths = val.Split(';').Select(s => s.Trim()).ToArray();

				return _pdtarSearchPaths;
			}

			private static string[] GetCandidateAssemblyFiles(ProjectInstance projectInstance)
			{
				var candidateAssemblyFilesList = new List<ProjectItemInstance>();

				candidateAssemblyFilesList.AddRange(projectInstance.GetItems(Content));
				candidateAssemblyFilesList.AddRange(projectInstance.GetItems(None));
				candidateAssemblyFilesList.AddRange(projectInstance.GetItems(RARResolvedReferencePath));

				string[] candidateAssemblyFiles = candidateAssemblyFilesList.Select((Func<ProjectItemInstance, string>)((item) => { return item.GetMetadataValue("FullPath").Trim(); })).ToArray();

				return candidateAssemblyFiles;
			}

			private static string GetStateFile(ProjectInstance projectInstance)
			{
				string intermediatePath = projectInstance.GetPropertyValue(IntermediateOutputPath).Trim();

				intermediatePath = GetFullPathInProjectContext(projectInstance, intermediatePath);

				string stateFile = Path.Combine(intermediatePath, "DesignTimeResolveAssemblyReferences.cache");

				return stateFile;
			}

			private static ITaskItem[] GetInstalledAssemblySubsetTables(ProjectInstance projectInstance)
			{
				return projectInstance.GetItems(InstalledAssemblySubsetTablesName).ToArray();
			}

			private static bool GetIgnoreDefaultInstalledAssemblySubsetTables(ProjectInstance projectInstance)
			{
				bool ignoreDefaultInstalledAssemblySubsetTables = false;

				string val = projectInstance.GetPropertyValue(IgnoreInstalledAssemblySubsetTables).Trim();

				if (!String.IsNullOrEmpty(val))
				{
					if (val == Boolean.TrueString || val == Boolean.FalseString)
					{
						ignoreDefaultInstalledAssemblySubsetTables = Convert.ToBoolean(val, CultureInfo.InvariantCulture);
					}
				}

				return ignoreDefaultInstalledAssemblySubsetTables;
			}

			private static string[] GetTargetFrameworkSubsets(ProjectInstance projectInstance)
			{
				IEnumerable<ITaskItem> taskItems = projectInstance.GetItems(ReferenceInstalledAssemblySubsets);

				string[] targetFrameworkSubsets = (taskItems.Select((Func<ITaskItem, string>)((item) => { return item.ItemSpec.Trim(); }))).ToArray();

				return targetFrameworkSubsets;
			}

			private static string[] GetFullTargetFrameworkSubsetNames(ProjectInstance projectInstance)
			{
				string val = projectInstance.GetPropertyValue(FullReferenceAssemblyNames).Trim();

				string[] fullTargetFrameworkSubsetNames = val.Split(';').Select(s => s.Trim()).ToArray();

				return fullTargetFrameworkSubsetNames;
			}

			private static ITaskItem[] GetFullFrameworkAssemblyTables(ProjectInstance projectInstance)
			{
				return projectInstance.GetItems(FullFrameworkAssemblyTablesName).ToArray();
			}

			#endregion //project dtar specific properties/items

			#endregion // public properties

			#region private methods
			static string GetFullPathInProjectContext(ProjectInstance projectInstance, string path)
			{
				string fullPath = path;

				if (!Path.IsPathRooted(path))
				{
					string projectDir = projectInstance.GetPropertyValue(MSBuildProjectDirectory).Trim();

					fullPath = Path.Combine(projectDir, path);

					fullPath = Path.GetFullPath(fullPath);
				}

				return fullPath;
			}
			#endregion // private methods
		}
	}
}
