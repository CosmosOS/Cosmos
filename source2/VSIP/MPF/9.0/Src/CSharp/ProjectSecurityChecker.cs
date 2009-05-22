/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Build.ComInteropWrapper;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// Does security validation of a project before loading the project
	/// </summary>
	public class ProjectSecurityChecker : IDisposable
	{
		#region constants
		/// <summary>
		/// The dangereous target property.
		/// </summary>
		internal const string DangerousTargetProperty = "LoadTimeSensitiveTargets";

		/// <summary>
		/// The dangereous properties property.
		/// </summary>
		internal const string DangerousPropertyProperty = "LoadTimeSensitiveProperties";

		/// <summary>
		/// The dangereous items property.
		/// </summary>
		internal const string DangerousItemsProperty = "LoadTimeSensitiveItems";

		/// <summary>
		/// The check item locations property.
		/// </summary>
		internal const string CheckItemLocationProperty = "LoadTimeCheckItemLocation";

		/// <summary>
		/// The dangereous list item separator.
		/// </summary>
		internal const string DangerousListSeparator = ";";

		/// <summary>
		/// The project directory property.
		/// </summary>
		internal const string ProjectDirectoryProperty = "MSBuildProjectDirectory";

		/// <summary>
		/// The default dangereous properties.
		/// </summary>
		internal const string DefaultDangerousProperties = "LoadTimeSensitiveTargets;LoadTimeSensitiveProperties;LoadTimeSensitiveItems;LoadTimeCheckItemLocation;";

		/// <summary>
		/// The default dangereous targets.
		/// </summary>
		internal const string DefaultDangerousTargets = "Compile;GetFrameworkPaths;AllProjectOutputGroups;AllProjectOutputGroupsDependencies;CopyRunEnvironmentFiles;ResolveComReferences;ResolveAssemblyReferences;ResolveNativeReferences;";

		/// <summary>
		/// The default dangereous items.
		/// </summary>
		internal const string DefaultDangerousItems = ";";

		/// <summary>
		/// Defined the safe imports subkey in the registry.
		/// </summary>
		internal const string SafeImportsSubkey = @"MSBuild\SafeImports";
		#endregion

		#region fields
		/// <summary>
		/// Defines an object that will be a mutex for this object for synchronizing thread calls.
		/// </summary>
		private static volatile object Mutex = new object();

		/// <summary>
		/// Flag determining if the object has been disposed.
		/// </summary>
		private bool isDisposed;

		/// <summary>
		/// The associated project shim for the project file
		/// </summary>
		private ProjectShim projectShim;

		/// <summary>
		/// The security check helper object used to call out to do necessary security checkings.
		/// </summary>
		private SecurityCheckHelper securityCheckHelper = new SecurityCheckHelper();

		/// <summary>
		/// The associated service provider.
		/// </summary>
		private IServiceProvider serviceProvider;

		#endregion

		#region properties
		/// <summary>
		/// The associated project shim for the project file
		/// </summary>
		/// <devremark>The project shim is made internal in order to be able to be passed to the user project.</devremark>
		internal protected ProjectShim ProjectShim
		{
			get { return this.projectShim; }
		}

		/// <summary>
		/// The security check helper that will be used to perform the necessary checkings.
		/// </summary>
		protected SecurityCheckHelper SecurityCheckHelper
		{
			get { return this.securityCheckHelper; }
		}

		/// <summary>
		/// The associated service provider.
		/// </summary>
		protected IServiceProvider ServiceProvider
		{
			get
			{
				return this.serviceProvider;
			}
		}
		#endregion

		#region ctors
		/// <summary>
		/// Overloaded Constructor 
		/// </summary>
		/// <param name="projectFilePath">path to the project file</param>
		/// <param name="serviceProvider">A service provider.</param>
		public ProjectSecurityChecker(IServiceProvider serviceProvider, string projectFilePath)
		{
			if(serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}

			if(String.IsNullOrEmpty(projectFilePath))
			{
				throw new ArgumentException(SR.GetString(SR.ParameterCannotBeNullOrEmpty, CultureInfo.CurrentUICulture), "projectFilePath");
			}

			this.serviceProvider = serviceProvider;

			// Instantiate a new project shim that we are going to use for security checkings.
			EngineShim engine = new EngineShim();
			this.projectShim = engine.CreateNewProject();
			this.projectShim.Load(projectFilePath);
		}
		#endregion

		#region IDisposable Members

		/// <summary>
		/// The IDispose interface Dispose method for disposing the object determinastically.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region virtual methods
		/// <summary>
		/// Check if the project is safe at load/design time
		/// </summary>
		/// <param name="securityErrorMessage">If the project is not safe contains an error message, describing the reason.</param>
		/// <returns>true if the project is safe, false otherwise</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#",
			Justification = "The error message needs to be an out parameter. We are following here the Try... method patterns.")]
		public virtual bool IsProjectSafeAtLoadTime(out string securityErrorMessage)
		{
			securityErrorMessage = String.Empty;

			StringBuilder securityMessageMaker = new StringBuilder();
			int counter = 0;
			string tempMessage;

			// STEP 1: Check direct imports.
			if(!this.IsProjectSafeWithImports(out tempMessage))
			{
				ProjectSecurityChecker.FormatMessage(securityMessageMaker, ++counter, tempMessage);
				securityErrorMessage = tempMessage;
			}

			// STEP 2: Check dangerous properties
			if(!this.IsProjectSafeWithProperties(out tempMessage))
			{
				ProjectSecurityChecker.FormatMessage(securityMessageMaker, ++counter, tempMessage);
				securityErrorMessage = tempMessage;
			}

			// STEP 3: Check dangerous targets
			if(!this.IsProjectSafeWithTargets(out tempMessage))
			{
				ProjectSecurityChecker.FormatMessage(securityMessageMaker, ++counter, tempMessage);
				securityErrorMessage = tempMessage;
			}

			// STEP 4: Check dangerous items
			if(!this.IsProjectSafeWithItems(out tempMessage))
			{
				ProjectSecurityChecker.FormatMessage(securityMessageMaker, ++counter, tempMessage);
				securityErrorMessage = tempMessage;
			}

			// STEP 5: Check UsingTask tasks
			if(!this.IsProjectSafeWithUsingTasks(out tempMessage))
			{
				ProjectSecurityChecker.FormatMessage(securityMessageMaker, ++counter, tempMessage);
				securityErrorMessage = tempMessage;
			}

			// STEP 6: Check for items defined within the LoadTimeCheckItemLocation, whether they are defined in safe locations
			if(!this.CheckItemsLocation(out tempMessage))
			{
				securityMessageMaker.AppendFormat(CultureInfo.CurrentCulture, "{0}: ", (++counter).ToString(CultureInfo.CurrentCulture));
				securityMessageMaker.AppendLine(tempMessage);
				securityErrorMessage = tempMessage;
			}

			if(counter > 1)
			{
				securityErrorMessage = securityMessageMaker.ToString();
			}

			return String.IsNullOrEmpty(securityErrorMessage);
		}

		/// <summary>
		/// Checks if the project is safe with imports. The project file is considered
		/// unsafe if it contains any imports not registered in the safe import regkey.
		/// </summary>
		/// <param name="securityErrorMessage">At return describes the reason why the projects is not considered safe.</param>
		/// <returns>true if the project is safe regarding imports.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#",
			Justification = "The error message needs to be an out parameter. We are following here the Try... method patterns.")]
		protected virtual bool IsProjectSafeWithImports(out string securityErrorMessage)
		{
			securityErrorMessage = String.Empty;

			// Now get the directly imports and do the comparision.
			string[] directImports = this.securityCheckHelper.GetDirectlyImportedProjects(this.projectShim);
			if(directImports != null && directImports.Length > 0)
			{
				IList<string> safeImportList = ProjectSecurityChecker.GetSafeImportList();

				for(int i = 0; i < directImports.Length; i++)
				{
					string fileToCheck = directImports[i];
					if(!ProjectSecurityChecker.IsSafeImport(safeImportList, fileToCheck))
					{
						using(RegistryKey root = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_Configuration))
						{
							securityErrorMessage = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.DetailsImport, CultureInfo.CurrentUICulture), Path.GetFileName(this.projectShim.FullFileName), fileToCheck, Path.Combine(root.Name, SafeImportsSubkey));
						}

						return false;
					}
				}
			}

			return true;
		}



		/// <summary>
		/// Checks if the project is safe regarding properties.
		/// </summary>
		/// <param name="securityErrorMessage">At return describes the reason why the projects is not considered safe.</param>       
		/// <returns>true if the project has only safe properties.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#",
			Justification = "The error message needs to be an out parameter. We are following here the Try... method patterns.")]
		protected virtual bool IsProjectSafeWithProperties(out string securityErrorMessage)
		{
			securityErrorMessage = String.Empty;

			// Now ask the security check heper for the safe properties.
			string reasonForFailure;
			bool isUserFile;
			bool isProjectSafe = this.securityCheckHelper.IsProjectSafe(ProjectSecurityChecker.DangerousPropertyProperty,
																		ProjectSecurityChecker.DefaultDangerousProperties,
																		this.projectShim,
																		null,
																		SecurityCheckPass.Properties,
																		out reasonForFailure,
																		out isUserFile);

			if(!isProjectSafe)
			{
				securityErrorMessage = this.GetMessageString(reasonForFailure, SR.DetailsProperty);
			}

			return isProjectSafe;
		}

		/// <summary>
		/// Checks if the project is safe regarding targets.
		/// </summary>
		/// <param name="securityErrorMessage">At return describes the reason why the projects is not considered safe.</param>       
		/// <returns>true if the project has only safe targets.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#",
			Justification = "The error message needs to be an out parameter. We are following here the Try... method patterns.")]
		protected virtual bool IsProjectSafeWithTargets(out string securityErrorMessage)
		{
			securityErrorMessage = String.Empty;

			// Now ask the security check heper for the safe targets.
			string reasonForFailure;
			bool isUserFile;
			bool isProjectSafe = this.securityCheckHelper.IsProjectSafe(ProjectSecurityChecker.DangerousTargetProperty,
																		ProjectSecurityChecker.DefaultDangerousTargets,
																		this.projectShim,
																		null,
																		SecurityCheckPass.Targets,
																		out reasonForFailure,
																		out isUserFile);

			if(!isProjectSafe)
			{
				securityErrorMessage = this.GetMessageString(reasonForFailure, SR.DetailsTarget);
			}

			return isProjectSafe;
		}

		/// <summary>
		/// Checks if the project is safe regarding items.
		/// </summary>
		/// <param name="securityErrorMessage">At return describes the reason why the projects is not considered safe.</param>       
		/// <returns>true if the project has only safe items.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#",
			Justification = "The error message needs to be an out parameter. We are following here the Try... method patterns.")]
		protected virtual bool IsProjectSafeWithItems(out string securityErrorMessage)
		{
			securityErrorMessage = String.Empty;

			// Now ask the security check heper for the safe items.
			string reasonForFailure;
			bool isUserFile;
			bool isProjectSafe = this.securityCheckHelper.IsProjectSafe(ProjectSecurityChecker.DangerousItemsProperty,
																		ProjectSecurityChecker.DefaultDangerousItems,
																		this.projectShim,
																		null,
																		SecurityCheckPass.Items,
																		out reasonForFailure,
																		out isUserFile);

			if(!isProjectSafe)
			{
				securityErrorMessage = this.GetMessageString(reasonForFailure, SR.DetailsItem);
			}

			return isProjectSafe;
		}

		/// <summary>
		/// Checks if the project is safe with using tasks.
		/// </summary>
		/// <param name="securityErrorMessage">At return describes the reason why the projects is not considered safe.</param>
		/// <returns>true if the project has no using tasks defined in the project file.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#",
			Justification = "The error message needs to be an out parameter. We are following here the Try... method patterns.")]
		protected virtual bool IsProjectSafeWithUsingTasks(out string securityErrorMessage)
		{
			securityErrorMessage = String.Empty;

			string[] usingTasks = this.securityCheckHelper.GetNonImportedUsingTasks(this.projectShim);

			if(usingTasks != null && usingTasks.Length > 0)
			{
				securityErrorMessage = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.DetailsUsingTask, CultureInfo.CurrentUICulture), Path.GetFileName(this.projectShim.FullFileName), usingTasks[0]);
				return false;
			}

			return true;
		}

		/// <summary>
		///  If the project contains the LoadTimeCheckItemsWithinProjectCone property, the method verifies that all the items listed in there are within the project cone.
		///  Also checks that the project is not in Program Files or Windows if the property was there.
		/// </summary>
		/// <param name="securityErrorMessage">At return describes the reason why the projects is not considered safe.</param>
		/// <returns>true if the project has no badly defined project items.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#",
			Justification = "The error message needs to be an out parameter. We are following here the Try... method patterns.")]
		protected virtual bool CheckItemsLocation(out string securityErrorMessage)
		{
			securityErrorMessage = String.Empty;

			// Get the <LoadTimeCheckItemLocation> property from the project
			string itemLocationProperty = this.projectShim.GetEvaluatedProperty(ProjectSecurityChecker.CheckItemLocationProperty);

			if(String.IsNullOrEmpty(itemLocationProperty))
			{
				return true;
			}

			// Takes a semicolon separated list of entries, splits them and puts them into a list with values trimmed.
			string[] items = itemLocationProperty.Split(ProjectSecurityChecker.DangerousListSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

			IList<string> itemsToCheck = new List<string>();
			foreach(string item in items)
			{
				itemsToCheck.Add(item.Trim());
			}

			// Now check the items for being defined in a safe location.
			string reasonForFailure;
			ItemSecurityChecker itemsSecurityChecker = new ItemSecurityChecker(this.serviceProvider, this.projectShim.FullFileName);
			if(!itemsSecurityChecker.CheckItemsSecurity(this.projectShim, itemsToCheck, out reasonForFailure))
			{
				securityErrorMessage = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.DetailsItemLocation, CultureInfo.CurrentUICulture), Path.GetFileName(this.projectShim.FullFileName), reasonForFailure);
				return false;
			}

			return true;
		}


		/// <summary>
		/// The method that does the cleanup.
		/// </summary>
		/// <param name="disposing">true if called from IDispose.Dispose; false if called from Finalizer.</param>
		protected virtual void Dispose(bool disposing)
		{
			// Everybody can go here.
			if(!this.isDisposed)
			{
				// Synchronize calls to the Dispose simultaniously.
				lock(Mutex)
				{
					if(disposing)
					{
						this.projectShim.ParentEngine.UnloadProject(this.projectShim);
					}

					this.isDisposed = true;
				}
			}
		}
		#endregion

		#region helper methods
		/// <summary>
		/// Gets a message string that has an associated format with a reason for failure.
		/// </summary>
		/// <param name="reasonForFailure"></param>
		/// <param name="resourceID"></param>
		/// <returns></returns>
		internal string GetMessageString(string reasonForFailure, string resourceID)
		{
			Debug.Assert(!String.IsNullOrEmpty(reasonForFailure), "The reason for failure should not be empty or null");
			Debug.Assert(!String.IsNullOrEmpty(resourceID), "The resource id string cannot be empty");

			return String.Format(CultureInfo.CurrentCulture, SR.GetString(resourceID, Path.GetFileName(this.projectShim.FullFileName), reasonForFailure));
		}

		/// <summary>
		/// Generates a format string that will be pushed to the More Detailed dialog.
		/// </summary>
		/// <param name="securityMessageMaker">The Stringbuilder object containing the formatted message.</param>
		/// <param name="counter">The 'issue' number.</param>
		/// <param name="securityErrorMessage">The message to format.</param>
		private static void FormatMessage(StringBuilder securityMessageMaker, int counter, string securityErrorMessage)
		{
			securityMessageMaker.AppendFormat(CultureInfo.CurrentCulture, "{0}: ", counter.ToString(CultureInfo.CurrentCulture));
			securityMessageMaker.AppendLine(securityErrorMessage);
			securityMessageMaker.Append(Environment.NewLine);
		}

		/// <summary>
		/// Returns a set of file info's describing the files in the SafeImports registry location.
		/// </summary>
		/// <returns>A set of FileInfo objects describing the files in the SafeImports location.</returns>
		private static IList<string> GetSafeImportList()
		{
			List<string> importsList = new List<string>();

			using(RegistryKey root = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_Configuration))
			{
				if(root != null)
				{
					using(RegistryKey key = root.OpenSubKey(SafeImportsSubkey))
					{
						if(key != null)
						{
							foreach(string value in key.GetValueNames())
							{
								string keyValue = key.GetValue(value, String.Empty, RegistryValueOptions.None) as string;
								// Make sure that the environment variables are expanded.
								keyValue = System.Environment.ExpandEnvironmentVariables(keyValue);
								Uri uri;
								if(!String.IsNullOrEmpty(keyValue) && Uri.TryCreate(keyValue, UriKind.Absolute, out uri) && uri.IsAbsoluteUri)
								{
									importsList.Add(keyValue);
								}
							}
						}
					}
				}
			}

			return importsList;
		}

		/// <summary>
		/// Checks if an import is a safe import.
		/// </summary>
		/// <param name="safeImportsList">A list of safe imports from teh registry.</param>
		/// <param name="fileToCheck">The file to check.</param>
		/// <returns>true if the file to check can be found in the safe import list</returns>
		private static bool IsSafeImport(IList<string> safeImportsList, string fileToCheck)
		{
			foreach(string safeImport in safeImportsList)
			{
				if(NativeMethods.IsSamePath(safeImport, fileToCheck))
				{
					return true;
				}
			}

			return false;
		}
		#endregion

		#region nested types
		/// <summary>
		/// Class for checking that the items defined in LoadTimeCheckItemLocation are being defined in safe locations.
		/// </summary>
		private class ItemSecurityChecker
		{
			#region fields
			/// <summary>
			/// The associated service provider.
			/// </summary>
			private IServiceProvider serviceProvider;

			/// <summary>
			/// The solutionFolder;
			/// </summary>
			private Uri solutionFolder;

			/// <summary>
			/// The project folder
			/// </summary>
			private Uri projectFolder;

			/// <summary>
			/// The set of special folders.
			/// </summary>
			private IList<Uri> specialFolders;
			#endregion

			#region ctors
			/// <summary>
			/// Overloaded Constructor 
			/// </summary>
			/// <param name="projectFilePath">path to the project file</param>
			/// <param name="serviceProvider">A service provider.</param>
			internal ItemSecurityChecker(IServiceProvider serviceProvider, string projectFullPath)
			{
				this.serviceProvider = serviceProvider;

				// Initialize the project and solution folders.
				this.SetProjectFolder(projectFullPath);
				this.SetSolutionFolder();

				// Set the special folders. Maybe this should be a static.
				this.specialFolders = ItemSecurityChecker.SetSpecialFolders();
			}
			#endregion

			#region methods
			/// <summary>
			/// Checks whether a set of project items described by the LoadTimeCheckItemLocation are in a safe location.
			/// </summary>
			/// <param name="projectShim">The project shim containing the items to be checked.</param>
			/// <param name="itemsToCheck">The list of items to check if they are in the project cone.</param>
			/// <param name="reasonForFailure">The reason for failure if any of the files fails</param>
			/// <returns>true if all project items are in the project cone. Otherwise false.</returns>
			internal bool CheckItemsSecurity(ProjectShim projectShim, IList<string> itemsToCheck, out string reasonForFailure)
			{
				reasonForFailure = String.Empty;

				// If nothing to check assume that everything is ok.
				if(itemsToCheck == null)
				{
					return true;
				}

				Debug.Assert(projectShim != null, "Cannot check the items if no project has been defined!");

				foreach(string itemName in itemsToCheck)
				{
					BuildItemGroupShim group = projectShim.GetEvaluatedItemsByNameIgnoringCondition(itemName);
					if(group != null)
					{
						IEnumerator enumerator = group.GetEnumerator();
						while(enumerator.MoveNext())
						{
							BuildItemShim item = enumerator.Current as BuildItemShim;

							string finalItem = item.FinalItemSpec;

							if(!String.IsNullOrEmpty(finalItem))
							{
								// Perform the actual check - start with normalizing the path.  Relative paths
								// should be treated as relative to the project file.
								string fullPath = this.GetFullPath(finalItem);

								// If the fullpath of the item is suspiciously short do not check it.
								if(fullPath.Length >= 3)
								{
									Uri uri = null;

									// If we cannot create a uri from the item path return with the error
									if(!Uri.TryCreate(fullPath, UriKind.Absolute, out uri))
									{
										reasonForFailure = fullPath;
										return false;
									}

									// Check if the item points to a network share
									if(uri.IsUnc)
									{
										reasonForFailure = fullPath;
										return false;
									}

									// Check if the item is located in a drive root directory
									if(uri.Segments.Length == 3 && uri.Segments[1] == ":" && uri.Segments[2][0] == Path.DirectorySeparatorChar)
									{
										reasonForFailure = fullPath;
										return false;
									}

									//Check if the item is not in a special folder.
									foreach(Uri specialFolder in this.specialFolders)
									{
										if(ItemSecurityChecker.IsItemInCone(uri, specialFolder))
										{
											reasonForFailure = fullPath;
											return false;
										}
									}
								}
								else
								{
									reasonForFailure = fullPath;
									return false;
								}
							}
						}
					}
				}

				return true;
			}


			/// <summary>
			/// Gets the list of special directories. This method should be optimized if called more then once.
			/// </summary>
			/// <returns>The list of special directories</returns>
			private static IList<Uri> SetSpecialFolders()
			{
				string[] specialFolderArray = new string[5]
                {
                   Environment.GetFolderPath(Environment.SpecialFolder.System),
                   Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                   Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                   ItemSecurityChecker.GetSpecialDirectoryFromNative(NativeMethods.ExtendedSpecialFolder.Windows),
                   ItemSecurityChecker.GetSpecialDirectoryFromNative(NativeMethods.ExtendedSpecialFolder.CommonStartup)
                };

				List<Uri> specialFolders = new List<Uri>(5);

				// Add trailing backslash to the folders.
				foreach(string specialFolder in specialFolderArray)
				{
					string tempFolder = specialFolder;
					if(!tempFolder.EndsWith("\\", StringComparison.Ordinal))
					{
						tempFolder += "\\";
					}

					specialFolders.Add(new Uri(tempFolder));
				}

				return specialFolders;
			}

			/// <summary>
			/// Some special folders are not supported by System.Environment.GetFolderPath. Get these special folders using p/invoke.
			/// </summary>
			/// <param name="specialFolder">The type of special folder to retrieve.</param>
			/// <returns>The folder path</returns>
			private static string GetSpecialDirectoryFromNative(NativeMethods.ExtendedSpecialFolder extendedSpecialFolder)
			{
				string specialFolder = null;
				IntPtr buffer = IntPtr.Zero;

				// Demand Unmanaged code permission. It should be normal to demand UnmanagedCodePermission from an assembly integrating into VS.
				new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
				try
				{
					buffer = Marshal.AllocHGlobal((NativeMethods.MAX_PATH + 1) * 2);
					IntPtr[] pathIdentifier = new IntPtr[1];

					if(ErrorHandler.Succeeded(UnsafeNativeMethods.SHGetSpecialFolderLocation(IntPtr.Zero, (int)extendedSpecialFolder, pathIdentifier)) && UnsafeNativeMethods.SHGetPathFromIDList(pathIdentifier[0], buffer))
					{
						specialFolder = Marshal.PtrToStringAuto(buffer);
					}
				}
				finally
				{
					if(buffer != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(buffer);
					}
				}


				return specialFolder;
			}


			/// <summary>
			/// Checks if the itemToCheck is in the cone of the baseUri.
			/// </summary>
			/// <param name="itemToCheck">The item to check</param>
			/// <param name="baseUri">The base to the item. This should define a folder.</param>
			/// <returns>true if the item to check is in the cone of the baseUri.</returns>
			private static bool IsItemInCone(Uri itemToCheck, Uri baseUri)
			{
				Debug.Assert(itemToCheck != null && baseUri != null, "Cannot check for items since the input is wrong");
				Debug.Assert(!NativeMethods.IsSamePath(Path.GetDirectoryName(baseUri.LocalPath), baseUri.LocalPath), "The " + baseUri.LocalPath + " is not a folder!");

				return (itemToCheck.IsFile && baseUri.IsFile &&
					String.Compare(itemToCheck.LocalPath, 0, baseUri.LocalPath, 0, baseUri.LocalPath.Length, StringComparison.OrdinalIgnoreCase) == 0);
			}

			/// <summary>
			/// Sets the solution folder.
			/// </summary>
			private void SetSolutionFolder()
			{
				if(this.solutionFolder != null)
				{
					return;
				}

				IVsSolution solution = this.serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
				Debug.Assert(solution != null, "Could not retrieve the solution service from the global service provider");

				string solutionDirectory, solutionFile, userOptionsFile;

				// We do not want to throw. If we cannot set the solution related constants we set them to empty string.
				ErrorHandler.ThrowOnFailure(solution.GetSolutionInfo(out solutionDirectory, out solutionFile, out userOptionsFile));

				if(String.IsNullOrEmpty(solutionDirectory))
				{
					return;
				}

				// Make sure the solution dir ends with a backslash
				if(solutionDirectory[solutionDirectory.Length - 1] != Path.DirectorySeparatorChar)
				{
					solutionDirectory += Path.DirectorySeparatorChar;
				}

				Uri.TryCreate(solutionDirectory, UriKind.Absolute, out this.solutionFolder);

				Debug.Assert(this.solutionFolder != null, "Could not create the Uri for the solution folder");
			}

			/// <summary>
			/// Sets the project folder.
			/// </summary>
			/// <param name="projectFullPath">The path to the project</param>
			private void SetProjectFolder(string projectFullPath)
			{
				if(this.projectFolder != null)
				{
					return;
				}

				string tempProjectFolder = Path.GetDirectoryName(projectFullPath);

				// Make sure the project dir ends with a backslash
				if(!tempProjectFolder.EndsWith("\\", StringComparison.Ordinal) && !tempProjectFolder.EndsWith("/", StringComparison.Ordinal))
				{
					tempProjectFolder += "\\";
				}

				Uri.TryCreate(tempProjectFolder, UriKind.Absolute, out this.projectFolder);

				Debug.Assert(this.projectFolder != null, "Could not create the Uri for the project folder");
			}

			/// <summary>
			/// Gets the fullpath of an item. 
			/// Relative pathes are treated as relative to the project file.
			/// </summary>
			/// <param name="item">The item.</param>
			/// <returns>The ful path of the item.</returns>
			private string GetFullPath(string item)
			{
				Url url;
				if(Path.IsPathRooted(item))
				{
					// Use absolute path
					url = new Microsoft.VisualStudio.Shell.Url(item);
				}
				else
				{
					// Path is relative, so make it relative to project path
					url = new Url(new Url(this.projectFolder.LocalPath), item);
				}

				return url.AbsoluteUrl;
			}
			#endregion
		}
		#endregion
	}
}
