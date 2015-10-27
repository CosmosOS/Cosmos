/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
//#define CCI_TRACING
using Microsoft.VisualStudio.Shell.Interop;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// An object that deals with user interaction via a GUI in the form a hierarchy: a parent node with zero or more child nodes, each of which
	/// can itself be a hierarchy.  
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), CLSCompliant(false), ComVisible(true)]
	public abstract class HierarchyNode :
		IVsUIHierarchy,
		IVsPersistHierarchyItem2,
		Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget,
		IVsHierarchyDropDataSource2,
		IVsHierarchyDropDataSource,
		IVsHierarchyDropDataTarget,
		IVsHierarchyDeleteHandler,
		IDisposable
	//, IVsBuildStatusCallback 
	{
		#region nested types
		/// <summary>
		/// DropEffect as defined in oleidl.h
		/// </summary>
		internal enum DropEffect
		{
			None,
			Copy = 1,
			Move = 2,
			Link = 4
		};
		#endregion

		#region Events
		internal event EventHandler<HierarchyNodeEventArgs> OnChildAdded
		{
			add { onChildAdded += value; }
			remove { onChildAdded -= value; }
		}
		internal event EventHandler<HierarchyNodeEventArgs> OnChildRemoved
		{
			add { onChildRemoved += value; }
			remove { onChildRemoved -= value; }
		}
		#endregion

		#region static/const fields
		public static readonly Guid SolutionExplorer = new Guid(EnvDTE.Constants.vsWindowKindSolutionExplorer);
		public const int NoImage = -1;
#if DEBUG
		internal static int LastTracedProperty;
#endif
		#endregion

		#region fields
		private EventSinkCollection hierarchyEventSinks = new EventSinkCollection();
		private ProjectNode projectMgr;
		private ProjectElement itemNode;
		private HierarchyNode parentNode;
		private HierarchyNode nextSibling;
		private HierarchyNode firstChild;
		private HierarchyNode lastChild;
		private bool isExpanded;
		private uint hierarchyId;
		private uint docCookie;
		private bool hasDesigner;
		private string virtualNodeName = String.Empty;	// Only used by virtual nodes
		private IVsHierarchy parentHierarchy;
		private int parentHierarchyItemId;
		private NodeProperties nodeProperties;
		private OleServiceProvider oleServiceProvider = new OleServiceProvider();
		private bool excludeNodeFromScc;
		private EventHandler<HierarchyNodeEventArgs> onChildAdded;
		private EventHandler<HierarchyNodeEventArgs> onChildRemoved;
		private bool hasParentNodeNameRelation;
		private List<HierarchyNode> itemsDraggedOrCutOrCopied;
		private bool sourceDraggedOrCutOrCopied;

		/// <summary>
		/// Has the object been disposed.
		/// </summary>
		/// <devremark>We will not specify a property for isDisposed, rather it is expected that the a private flag is defined
		/// on all subclasses. We do not want get in a situation where the base class's dipose is not called because a child sets the flag through the property.</devremark>
		private bool isDisposed;
		#endregion

		#region abstract properties
		/// <summary>
		/// The URL of the node.
		/// </summary>
		/// <value></value>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public abstract string Url
		{
			get;
		}

		/// <summary>
		/// The Caption of the node.
		/// </summary>
		/// <value></value>
		public abstract string Caption
		{
			get;
		}

		/// <summary>
		/// The item type guid associated to a node.
		/// </summary>
		/// <value></value>
		public abstract Guid ItemTypeGuid
		{
			get;
		}
		#endregion

		#region virtual properties
		/// <summary>
		/// Defines a string that is used to separate the name relation from the extension
		/// </summary>
		public virtual string NameRelationSeparator
		{
			get
			{
				return ".";
			}
		}


		public virtual int MenuCommandId
		{
			get { return VsMenus.IDM_VS_CTXT_NOCOMMANDS; }
		}


		/// <summary>
		/// Return an imageindex
		/// </summary>
		/// <returns></returns>
		public virtual int ImageIndex
		{
			get { return NoImage; }
		}

		/// <summary>
		/// Return an state icon index
		/// </summary>
		/// <returns></returns>
		/// <summary>
		/// Sets the state icon for a file.
		/// </summary>
		public virtual VsStateIcon StateIconIndex
		{
			get
			{
				if(!this.ExcludeNodeFromScc)
				{
					IVsSccManager2 sccManager = this.ProjectMgr.Site.GetService(typeof(SVsSccManager)) as IVsSccManager2;

					if(sccManager != null)
					{
						VsStateIcon[] statIcons = new VsStateIcon[1] { VsStateIcon.STATEICON_NOSTATEICON };
						uint[] sccStatus = new uint[1] { 0 };
						// Get the glyph from the scc manager. Note that it will fail in command line
						// scenarios.
						if(ErrorHandler.Succeeded(sccManager.GetSccGlyph(1, new string[] { this.GetMkDocument() }, statIcons, sccStatus)))
						{
							return statIcons[0];
						}
					}
				}

				return VsStateIcon.STATEICON_NOSTATEICON;
			}
		}

		/// <summary>
		/// Defines whether a node can execute a command if in selection.
		/// </summary>
		public virtual bool CanExecuteCommand
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Used to determine the sort order of different node types
		/// in the solution explorer window.
		/// Nodes with the same priorities are sorted based on their captions.
		/// </summary>
		public virtual int SortPriority
		{
			get { return DefaultSortOrderNode.HierarchyNode; }
		}

		/// <summary>
		/// Defines the properties attached to this node.
		/// </summary>
		public virtual NodeProperties NodeProperties
		{
			get
			{
				if(null == nodeProperties)
				{
					nodeProperties = CreatePropertiesObject();
				}
				return this.nodeProperties;
			}

		}

		/// <summary>
		/// Returns an object that is a special view over this object; this is the value
		/// returned by the Object property of the automation objects.
		/// </summary>
		internal virtual object Object
		{
			get { return this; }
		}
		#endregion

		#region properties

		public OleServiceProvider OleServiceProvider
		{
			get
			{
				return this.oleServiceProvider;
			}
		}

		[System.ComponentModel.BrowsableAttribute(false)]
		public ProjectNode ProjectMgr
		{
			get
			{
				return this.projectMgr;
			}
			set
			{
				this.projectMgr = value;
			}
		}


		[System.ComponentModel.BrowsableAttribute(false)]
		public HierarchyNode NextSibling
		{
			get
			{
				return this.nextSibling;
			}
			set
			{
				this.nextSibling = value;
			}
		}


		[System.ComponentModel.BrowsableAttribute(false)]
		public HierarchyNode FirstChild
		{
			get
			{
				return this.firstChild;
			}
			set
			{
				this.firstChild = value;
			}
		}

		[System.ComponentModel.BrowsableAttribute(false)]
		public HierarchyNode LastChild
		{
			get
			{
				return this.lastChild;
			}
			set
			{
				this.lastChild = value;
			}
		}


		[System.ComponentModel.BrowsableAttribute(false)]
		public HierarchyNode Parent
		{
			get
			{
				return this.parentNode;
			}
			set
			{
				this.parentNode = value;
			}
		}


		[System.ComponentModel.BrowsableAttribute(false)]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
		public uint ID
		{
			get
			{
				return this.hierarchyId;
			}
			internal set
			{
				this.hierarchyId = value;
			}
		}


		[System.ComponentModel.BrowsableAttribute(false)]
		public ProjectElement ItemNode
		{
			get
			{
				return itemNode;
			}
			set
			{
				itemNode = value;
			}
		}


		[System.ComponentModel.BrowsableAttribute(false)]
		public bool HasDesigner
		{
			get
			{
				return this.hasDesigner;
			}
			set { this.hasDesigner = value; }
		}


		[System.ComponentModel.BrowsableAttribute(false)]
		public bool IsExpanded
		{
			get
			{
				return this.isExpanded;
			}
			set { this.isExpanded = value; }
		}

		public string VirtualNodeName
		{
			get
			{
				return this.virtualNodeName;
			}
			set
			{
				this.virtualNodeName = value;
			}
		}


		[System.ComponentModel.BrowsableAttribute(false)]
		public HierarchyNode PreviousSibling
		{
			get
			{
				if(this.parentNode == null) return null;
				HierarchyNode prev = null;
				for(HierarchyNode child = this.parentNode.firstChild; child != null; child = child.nextSibling)
				{
					if(child == this)
						break;
					prev = child;
				}
				return prev;
			}
		}

		public uint DocCookie
		{
			get
			{
				return this.docCookie;
			}
			set
			{
				this.docCookie = value;
			}
		}

		/// <summary>
		/// Specifies if a Node is under source control.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scc")]
		public bool ExcludeNodeFromScc
		{
			get
			{
				return this.excludeNodeFromScc;
			}
			set
			{
				this.excludeNodeFromScc = value;
			}
		}

		/// <summary>
		/// Defines if a node a name relation to its parent node
		/// 
		/// </summary>
		public bool HasParentNodeNameRelation
		{
			get
			{
				return this.hasParentNodeNameRelation;
			}
			set
			{
				this.hasParentNodeNameRelation = value;
			}
		}

		protected bool SourceDraggedOrCutOrCopied
		{
			get
			{
				return this.sourceDraggedOrCutOrCopied;
			}
			set
			{
				this.sourceDraggedOrCutOrCopied = value;
			}
		}

		protected IList<HierarchyNode> ItemsDraggedOrCutOrCopied
		{
			get
			{
				return this.itemsDraggedOrCutOrCopied;
			}
		}
		#endregion

		#region ctors

		protected HierarchyNode()
		{
			this.IsExpanded = true;
		}

		protected HierarchyNode(ProjectNode root, ProjectElement element)
		{
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

			this.projectMgr = root;
			this.itemNode = element;
			this.hierarchyId = this.projectMgr.ItemIdMap.Add(this);
			this.oleServiceProvider.AddService(typeof(IVsHierarchy), root, false);
		}

		/// <summary>
		/// Overloaded ctor. 
		/// </summary>
		/// <param name="root"></param>
		protected HierarchyNode(ProjectNode root)
		{
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

			this.projectMgr = root;
			this.itemNode = new ProjectElement(this.projectMgr, null, true);
			this.hierarchyId = this.projectMgr.ItemIdMap.Add(this);
			this.oleServiceProvider.AddService(typeof(IVsHierarchy), root, false);
		}
		#endregion

		#region static methods
		/// <summary>
		/// Get the outer IVsHierarchy implementation.
		/// This is used for scenario where a flavor may be modifying the behavior
		/// </summary>
		internal static IVsHierarchy GetOuterHierarchy(HierarchyNode node)
		{
			IVsHierarchy hierarchy = null;
			// The hierarchy of a node is its project node hierarchy
			IntPtr projectUnknown = Marshal.GetIUnknownForObject(node.projectMgr);
			try
			{
				hierarchy = (IVsHierarchy)Marshal.GetTypedObjectForIUnknown(projectUnknown, typeof(IVsHierarchy));
			}
			finally
			{
				if(projectUnknown != IntPtr.Zero)
				{
					Marshal.Release(projectUnknown);
				}
			}
			return hierarchy;
		}
		#endregion

		#region virtual methods
		/// <summary>
		/// Creates an object derived from NodeProperties that will be used to expose properties
		/// spacific for this object to the property browser.
		/// </summary>
		/// <returns></returns>
		protected virtual NodeProperties CreatePropertiesObject()
		{
			return null;
		}

		/// <summary>
		/// Return an iconhandle
		/// </summary>
		/// <param name="open"></param>
		/// <returns></returns>
		public virtual object GetIconHandle(bool open)
		{
			return null;
		}

		/// <summary>
		/// AddChild - add a node, sorted in the right location.
		/// </summary>
		/// <param name="node">The node to add.</param>
		public virtual void AddChild(HierarchyNode node)
		{
			if(node == null)
			{
				throw new ArgumentNullException("node");
			}

			// make sure the node is in the map.
			Object nodeWithSameID = this.projectMgr.ItemIdMap[node.hierarchyId];
			if(!Object.ReferenceEquals(node, nodeWithSameID as HierarchyNode))
			{
				if(nodeWithSameID == null && node.ID <= this.ProjectMgr.ItemIdMap.Count)
				{ // reuse our hierarchy id if possible.
					this.projectMgr.ItemIdMap.SetAt(node.hierarchyId, this);
				}
				else
				{
					throw new InvalidOperationException();
				}
			}

			HierarchyNode previous = null;
			for(HierarchyNode n = this.firstChild; n != null; n = n.nextSibling)
			{
				if(this.ProjectMgr.CompareNodes(node, n) > 0) break;
				previous = n;
			}
			// insert "node" after "previous".
			if(previous != null)
			{
				node.nextSibling = previous.nextSibling;
				previous.nextSibling = node;
				if(previous == this.lastChild)
				{
					this.lastChild = node;
				}
			}
			else
			{
				if(this.lastChild == null)
				{
					this.lastChild = node;
				}
				node.nextSibling = this.firstChild;
				this.firstChild = node;
			}
			node.parentNode = this;
			this.OnItemAdded(this, node);
		}

		/// <summary>
		/// Removes a node from the hierarchy.
		/// </summary>
		/// <param name="node">The node to remove.</param>
		public virtual void RemoveChild(HierarchyNode node)
		{
			if(node == null)
			{
				throw new ArgumentNullException("node");
			}

			this.projectMgr.ItemIdMap.Remove(node);

			HierarchyNode last = null;
			for(HierarchyNode n = this.firstChild; n != null; n = n.nextSibling)
			{
				if(n == node)
				{
					if(last != null)
					{
						last.nextSibling = n.nextSibling;
					}
					if(n == this.lastChild)
					{
						if(last == this.lastChild)
						{
							this.lastChild = null;
						}
						else
						{
							this.lastChild = last;
						}
					}
					if(n == this.firstChild)
					{
						this.firstChild = n.nextSibling;
					}
					return;
				}
				last = n;
			}
			throw new InvalidOperationException("Node not found");
		}

		/// <summary>
		/// Returns an automation object representing this node
		/// </summary>
		/// <returns>The automation object</returns>
		public virtual object GetAutomationObject()
		{
			return new Automation.OAProjectItem<HierarchyNode>(this.projectMgr.GetAutomationObject() as Automation.OAProject, this);
		}

		/// <summary>
		/// Returns a property object based on a property id 
		/// </summary>
		/// <param name="propId">the property id of the property requested</param>
		/// <returns>the property object requested</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public virtual object GetProperty(int propId)
		{
			object result = null;
			switch((__VSHPROPID)propId)
			{
				case __VSHPROPID.VSHPROPID_Expandable:
					result = (this.firstChild != null);
					break;

				case __VSHPROPID.VSHPROPID_Caption:
					result = this.Caption;
					break;

				case __VSHPROPID.VSHPROPID_Name:
					result = this.Caption;
					break;

				case __VSHPROPID.VSHPROPID_ExpandByDefault:
					result = false;
					break;

				case __VSHPROPID.VSHPROPID_IconImgList:
					result = this.ProjectMgr.ImageHandler.ImageList.Handle;
					break;

				case __VSHPROPID.VSHPROPID_OpenFolderIconIndex:
				case __VSHPROPID.VSHPROPID_IconIndex:
					int index = this.ImageIndex;
					if(index != NoImage)
					{
						result = index;
					}
					break;

				case __VSHPROPID.VSHPROPID_StateIconIndex:
					result = (int)this.StateIconIndex;
					break;

				case __VSHPROPID.VSHPROPID_IconHandle:
					result = GetIconHandle(false);
					break;

				case __VSHPROPID.VSHPROPID_OpenFolderIconHandle:
					result = GetIconHandle(true);
					break;

				case __VSHPROPID.VSHPROPID_NextVisibleSibling:
					goto case __VSHPROPID.VSHPROPID_NextSibling;

				case __VSHPROPID.VSHPROPID_NextSibling:
					result = (int)((this.nextSibling != null) ? this.nextSibling.hierarchyId : VSConstants.VSITEMID_NIL);
					break;

				case __VSHPROPID.VSHPROPID_FirstChild:
					goto case __VSHPROPID.VSHPROPID_FirstVisibleChild;

				case __VSHPROPID.VSHPROPID_FirstVisibleChild:
					result = (int)((this.firstChild != null) ? this.firstChild.hierarchyId : VSConstants.VSITEMID_NIL);
					break;

				case __VSHPROPID.VSHPROPID_Parent:
					if(null == this.parentNode)
					{
						unchecked { result = new IntPtr((int)VSConstants.VSITEMID_NIL); }
					}
					else
					{
						result = new IntPtr((int)this.parentNode.hierarchyId);  // see bug 176470
					}
					break;

				case __VSHPROPID.VSHPROPID_ParentHierarchyItemid:
					if(parentHierarchy != null)
					{
						result = (int)parentHierarchyItemId; // VS requires VT_I4 | VT_INT_PTR
					}
					break;

				case __VSHPROPID.VSHPROPID_ParentHierarchy:
					result = parentHierarchy;
					break;

				case __VSHPROPID.VSHPROPID_Root:
					result = Marshal.GetIUnknownForObject(this.projectMgr);
					break;

				case __VSHPROPID.VSHPROPID_Expanded:
					result = this.isExpanded;
					break;

				case __VSHPROPID.VSHPROPID_BrowseObject:
					result = this.NodeProperties;
					if(result != null) result = new DispatchWrapper(result);
					break;

				case __VSHPROPID.VSHPROPID_EditLabel:
					if(this.ProjectMgr != null && !this.ProjectMgr.IsClosed && !this.ProjectMgr.IsCurrentStateASuppressCommandsMode())
					{
						result = GetEditLabel();
					}
					break;

				case __VSHPROPID.VSHPROPID_SaveName:
					//SaveName is the name shown in the Save and the Save Changes dialog boxes.
					result = this.Caption;
					break;

				case __VSHPROPID.VSHPROPID_ItemDocCookie:
					if(this.docCookie != 0) return (IntPtr)this.docCookie; //cast to IntPtr as some callers expect VT_INT
					break;

				case __VSHPROPID.VSHPROPID_ExtObject:
					result = GetAutomationObject();
					break;
			}

			__VSHPROPID2 id2 = (__VSHPROPID2)propId;
			switch(id2)
			{
				case __VSHPROPID2.VSHPROPID_NoDefaultNestedHierSorting:
					return true; // We are doing the sorting ourselves through VSHPROPID_FirstChild and VSHPROPID_NextSibling
				case __VSHPROPID2.VSHPROPID_BrowseObjectCATID:
					{
						// If there is a browse object and it is a NodeProperties, then get it's CATID
						object browseObject = this.GetProperty((int)__VSHPROPID.VSHPROPID_BrowseObject);
						if(browseObject != null)
						{
							if(browseObject is DispatchWrapper)
								browseObject = ((DispatchWrapper)browseObject).WrappedObject;
							result = this.ProjectMgr.GetCATIDForType(browseObject.GetType()).ToString("B");
							if(String.CompareOrdinal(result as string, Guid.Empty.ToString("B")) == 0)
								result = null;
						}
						break;
					}
				case __VSHPROPID2.VSHPROPID_ExtObjectCATID:
					{
						// If there is a extensibility object and it is a NodeProperties, then get it's CATID
						object extObject = this.GetProperty((int)__VSHPROPID.VSHPROPID_ExtObject);
						if(extObject != null)
						{
							if(extObject is DispatchWrapper)
								extObject = ((DispatchWrapper)extObject).WrappedObject;
							result = this.ProjectMgr.GetCATIDForType(extObject.GetType()).ToString("B");
							if(String.CompareOrdinal(result as string, Guid.Empty.ToString("B")) == 0)
								result = null;
						}
						break;
					}
			}

			__VSHPROPID4 id4 = (__VSHPROPID4)propId;
			switch (id4)
			{
				case __VSHPROPID4.VSHPROPID_TargetFrameworkMoniker:
					result = this.ProjectMgr.TargetFrameworkMoniker.FullName;
					break;
			}

#if DEBUG
			if(propId != LastTracedProperty)
			{
				string trailer = (result == null) ? "null" : result.ToString();
				CCITracing.TraceCall(this.hierarchyId + "," + propId.ToString() + " = " + trailer);
				LastTracedProperty = propId; // some basic filtering here...
			}
#endif
			return result;
		}

		/// <summary>
		/// Sets the value of a property for a given property id
		/// </summary>
		/// <param name="propid">the property id of the property to be set</param>
		/// <param name="value">value of the property</param>
		/// <returns>S_OK if succeeded</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "propid")]
		public virtual int SetProperty(int propid, object value)
		{
			__VSHPROPID id = (__VSHPROPID)propid;

			CCITracing.TraceCall(this.hierarchyId + "," + id.ToString());
			switch(id)
			{
				case __VSHPROPID.VSHPROPID_Expanded:
					this.isExpanded = (bool)value;
					break;

				case __VSHPROPID.VSHPROPID_ParentHierarchy:
					parentHierarchy = (IVsHierarchy)value;
					break;

				case __VSHPROPID.VSHPROPID_ParentHierarchyItemid:
					parentHierarchyItemId = (int)value;
					break;

				case __VSHPROPID.VSHPROPID_EditLabel:
					return SetEditLabel((string)value);

				default:
					CCITracing.TraceCall(" unhandled");
					break;
			}

			__VSHPROPID4 id4 = (__VSHPROPID4)propid;
			switch (id4)
			{
				case __VSHPROPID4.VSHPROPID_TargetFrameworkMoniker:
					this.ProjectMgr.TargetFrameworkMoniker = new FrameworkName((string)value);
					break;
			}

			return VSConstants.S_OK;
		}

		/// <summary>
		/// Get a guid property
		/// </summary>
		/// <param name="propid">property id for the guid property requested</param>
		/// <param name="guid">the requested guid</param>
		/// <returns>S_OK if succeded</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "propid")]
		public virtual int GetGuidProperty(int propid, out Guid guid)
		{
			guid = Guid.Empty;
			if(propid == (int)__VSHPROPID.VSHPROPID_TypeGuid)
			{
				guid = this.ItemTypeGuid;
			}

			if(guid.CompareTo(Guid.Empty) == 0)
			{
				return VSConstants.DISP_E_MEMBERNOTFOUND;
			}

			return VSConstants.S_OK;
		}

		/// <summary>
		/// Set a guid property.
		/// </summary>
		/// <param name="propid">property id of the guid property to be set</param>
		/// <param name="guid">the guid to be set</param>
		/// <returns>E_NOTIMPL</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "propid")]
		public virtual int SetGuidProperty(int propid, ref Guid guid)
		{
			return VSConstants.E_NOTIMPL;
		}

		/// <summary>
		/// Called by the shell when a node has been renamed from the GUI
		/// </summary>
		/// <param name="label"></param>
		/// <returns>E_NOTIMPL</returns>
		public virtual int SetEditLabel(string label)
		{
			return VSConstants.E_NOTIMPL;
		}

		/// <summary>
		/// Called by the shell to get the node caption when the user tries to rename from the GUI
		/// </summary>
		/// <returns>the node cation</returns>
		public virtual string GetEditLabel()
		{
			return this.Caption;
		}

		/// <summary>
		/// This method is called by the interface method GetMkDocument to specify the item moniker.
		/// </summary>
		/// <returns>The moniker for this item</returns>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Mk")]
		public virtual string GetMkDocument()
		{
			return String.Empty;
		}

		/// <summary>
		/// Removes items from the hierarchy. Project overwrites this
		/// </summary>
		/// <param name="removeFromStorage"></param>
		public virtual void Remove(bool removeFromStorage)
		{
			string documentToRemove = this.GetMkDocument();

			// Ask Document tracker listeners if we can remove the item.
			string[] filesToBeDeleted = new string[1] { documentToRemove };
			VSQUERYREMOVEFILEFLAGS[] queryRemoveFlags = this.GetQueryRemoveFileFlags(filesToBeDeleted);
			if(!this.ProjectMgr.Tracker.CanRemoveItems(filesToBeDeleted, queryRemoveFlags))
			{
				return;
			}

			// Close the document if it has a manager.
			DocumentManager manager = this.GetDocumentManager();
			if(manager != null)
			{
				if(manager.Close(!removeFromStorage ? __FRAMECLOSE.FRAMECLOSE_PromptSave : __FRAMECLOSE.FRAMECLOSE_NoSave) == VSConstants.E_ABORT)
				{
					// User cancelled operation in message box.
					return;
				}
			}

			// Check out the project file.
			if(!this.ProjectMgr.QueryEditProjectFile(false))
			{
				throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
			}

			// Notify hierarchy event listeners that the file is going to be removed.
			OnItemDeleted();

			// Remove child if any before removing from the hierarchy
			for(HierarchyNode child = this.FirstChild; child != null; child = child.NextSibling)
			{
				child.Remove(removeFromStorage);
			}

			// the project node has no parentNode
			if(this.parentNode != null)
			{
				// Remove from the Hierarchy
				this.parentNode.RemoveChild(this);
			}

			// We save here the path to delete since this.Url might call the Include which will be deleted by the RemoveFromProjectFile call.
			string pathToDelete = this.GetMkDocument();
			this.itemNode.RemoveFromProjectFile();

			if(removeFromStorage)
			{
				this.DeleteFromStorage(pathToDelete);
			}

			// Close the document window if opened.
			CloseDocumentWindow(this);

			// Notify document tracker listeners that we have removed the item.
			VSREMOVEFILEFLAGS[] removeFlags = this.GetRemoveFileFlags(filesToBeDeleted);
			Debug.Assert(removeFlags != null, "At least an empty array should be returned for the GetRemoveFileFlags");
			this.ProjectMgr.Tracker.OnItemRemoved(documentToRemove, removeFlags[0]);

			// Notify hierarchy event listeners that we have removed the item
			if(null != this.parentNode.onChildRemoved)
			{
				HierarchyNodeEventArgs args = new HierarchyNodeEventArgs(this);
				parentNode.onChildRemoved(parentNode, args);
			}

			// Notify hierarchy event listeners that items have been invalidated
			OnInvalidateItems(this.parentNode);

			// Dispose the node now that is deleted.
			this.Dispose(true);
		}

		/// <summary>
		/// Returns the relational name which is defined as the first part of the caption until indexof NameRelationSeparator
		/// </summary>
		public virtual string GetRelationalName()
		{
			//Get the first part of the caption
			string[] partsOfParent = this.Caption.Split(new string[] { this.NameRelationSeparator }, StringSplitOptions.None);
			return partsOfParent[0];
		}

		/// <summary>
		/// Returns the 'extension' of the relational name
		/// e.g. form1.resx returns .resx, form1.designer.cs returns .designer.cs
		/// </summary>
		/// <returns>The extension</returns>
		public virtual string GetRelationNameExtension()
		{
			return this.Caption.Substring(this.Caption.IndexOf(this.NameRelationSeparator, StringComparison.Ordinal));
		}

		/// <summary>
		/// Close open document frame for a specific node.
		/// </summary> 
		protected void CloseDocumentWindow(HierarchyNode node)
		{
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

			// We walk the RDT looking for all running documents attached to this hierarchy and itemid. There
			// are cases where there may be two different editors (not views) open on the same document.
			IEnumRunningDocuments pEnumRdt;
			IVsRunningDocumentTable pRdt = this.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
			if(pRdt == null)
			{
				throw new InvalidOperationException();
			}
			if(ErrorHandler.Succeeded(pRdt.GetRunningDocumentsEnum(out pEnumRdt)))
			{
				uint[] cookie = new uint[1];
				uint fetched;
				uint saveOptions = (uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_NoSave;
				IVsHierarchy srpOurHier = node.projectMgr as IVsHierarchy;

				ErrorHandler.ThrowOnFailure(pEnumRdt.Reset());
				while(VSConstants.S_OK == pEnumRdt.Next(1, cookie, out fetched))
				{
					// Note we can pass NULL for all parameters we don't care about
					uint empty;
					string emptyStr;
					IntPtr ppunkDocData;
					IVsHierarchy srpHier;
					uint itemid = VSConstants.VSITEMID_NIL;

					ErrorHandler.ThrowOnFailure(pRdt.GetDocumentInfo(
										 cookie[0],
										 out empty,
										 out empty,
										 out empty,
										 out emptyStr,
										 out srpHier,
										 out itemid,
										 out ppunkDocData));

					// Is this one of our documents?
					if(Utilities.IsSameComObject(srpOurHier, srpHier) && itemid == node.ID)
					{
						IVsSolution soln = GetService(typeof(SVsSolution)) as IVsSolution;
						ErrorHandler.ThrowOnFailure(soln.CloseSolutionElement(saveOptions, srpOurHier, cookie[0]));
					}
					if(ppunkDocData != IntPtr.Zero)
						Marshal.Release(ppunkDocData);

				}
			}
		}

		/// <summary>
		/// Redraws the state icon if the node is not excluded from source control.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scc")]
		protected internal virtual void UpdateSccStateIcons()
		{
			if(!this.ExcludeNodeFromScc)
			{
				this.ReDraw(UIHierarchyElement.SccState);
			}
		}

		/// <summary>
		/// To be overwritten by descendants.
		/// </summary>
		protected internal virtual int SetEditLabel(string label, string relativePath)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Called by the drag and drop implementation to ask the node
		/// which is being dragged/droped over which nodes should
		/// process the operation.
		/// This allows for dragging to a node that cannot contain
		/// items to let its parent accept the drop
		/// </summary>
		/// <returns>HierarchyNode that accept the drop handling</returns>
		protected internal virtual HierarchyNode GetDragTargetHandlerNode()
		{
			return this;
		}

		/// <summary>
		/// Add a new Folder to the project hierarchy.
		/// </summary>
		/// <returns>S_OK if succeeded, otherwise an error</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		protected virtual int AddNewFolder()
		{
			// Check out the project file.
			if(!this.ProjectMgr.QueryEditProjectFile(false))
			{
				throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
			}

			try
			{
				// Generate a new folder name
				string newFolderName;
				ErrorHandler.ThrowOnFailure(this.projectMgr.GenerateUniqueItemName(this.hierarchyId, String.Empty, String.Empty, out newFolderName));

				// create the project part of it, the project file
				HierarchyNode child = this.ProjectMgr.CreateFolderNodes(Path.Combine(this.virtualNodeName, newFolderName));

				if (child is FolderNode)
				{
					((FolderNode)child).CreateDirectory();
				}

				// If we are in automation mode then skip the ui part which is about renaming the folder
				if (!Utilities.IsInAutomationFunction(this.projectMgr.Site))
				{
					IVsUIHierarchyWindow uiWindow = UIHierarchyUtilities.GetUIHierarchyWindow(this.projectMgr.Site, SolutionExplorer);
					// This happens in the context of adding a new folder.
					// Since we are already in solution explorer, it is extremely unlikely that we get a null return.
					// If we do, the newly created folder will not be selected, and we will not attempt the rename
					// command (since we are selecting the wrong item).                        
					if (uiWindow != null)
					{
						// we need to get into label edit mode now...
						// so first select the new guy...
						ErrorHandler.ThrowOnFailure(uiWindow.ExpandItem(this.projectMgr, child.hierarchyId, EXPANDFLAGS.EXPF_SelectItem));
						// them post the rename command to the shell. Folder verification and creation will
						// happen in the setlabel code...
						IVsUIShell shell = this.projectMgr.Site.GetService(typeof(SVsUIShell)) as IVsUIShell;

						Debug.Assert(shell != null, "Could not get the ui shell from the project");
						if (shell == null)
						{
							return VSConstants.E_FAIL;
						}

						object dummy = null;
						Guid cmdGroup = VsMenus.guidStandardCommandSet97;
						ErrorHandler.ThrowOnFailure(shell.PostExecCommand(ref cmdGroup, (uint)VsCommands.Rename, 0, ref dummy));
					}
				}
			}
			catch (COMException e)
			{
				Trace.WriteLine("Exception : " + e.Message);
				return e.ErrorCode;
			}

			return VSConstants.S_OK;
		}

		protected virtual int AddItemToHierarchy(HierarchyAddType addType)
		{
			CCITracing.TraceCall();
			IVsAddProjectItemDlg addItemDialog;

			string strFilter = String.Empty;
			int iDontShowAgain;
			uint uiFlags;
			IVsProject3 project = (IVsProject3)this.projectMgr;

			string strBrowseLocations = Path.GetDirectoryName(this.projectMgr.BaseURI.Uri.LocalPath);

			System.Guid projectGuid = this.projectMgr.ProjectGuid;

			addItemDialog = this.GetService(typeof(IVsAddProjectItemDlg)) as IVsAddProjectItemDlg;

			if(addType == HierarchyAddType.AddNewItem)
				uiFlags = (uint)(__VSADDITEMFLAGS.VSADDITEM_AddNewItems | __VSADDITEMFLAGS.VSADDITEM_SuggestTemplateName | __VSADDITEMFLAGS.VSADDITEM_AllowHiddenTreeView);
			else
				uiFlags = (uint)(__VSADDITEMFLAGS.VSADDITEM_AddExistingItems | __VSADDITEMFLAGS.VSADDITEM_AllowMultiSelect | __VSADDITEMFLAGS.VSADDITEM_AllowStickyFilter);

			ErrorHandler.ThrowOnFailure(addItemDialog.AddProjectItemDlg(this.hierarchyId, ref projectGuid, project, uiFlags, null, null, ref strBrowseLocations, ref strFilter, out iDontShowAgain)); /*&fDontShowAgain*/

			return VSConstants.S_OK;
		}

		/// <summary>
		/// Overwritten in subclasses
		/// </summary>
		protected virtual void DoDefaultAction()
		{
			CCITracing.TraceCall();
		}

		/// <summary>
		/// Handles the exclude from project command.
		/// </summary>
		/// <returns></returns>
		protected virtual int ExcludeFromProject()
		{
			Debug.Assert(this.ProjectMgr != null, "The project item " + this.ToString() + " has not been initialised correctly. It has a null ProjectMgr");
			this.Remove(false);
			return VSConstants.S_OK;
		}

		/// <summary>
		/// Handles the Show in Designer command.
		/// </summary>
		/// <returns></returns>
		protected virtual int ShowInDesigner(IList<HierarchyNode> selectedNodes)
		{
			return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
		}

		/// <summary>
		/// Prepares a selected node for clipboard. 
		/// It takes the the project reference string of this item and adds it to a stringbuilder. 
		/// </summary>
		/// <returns>A stringbuilder.</returns>
		/// <devremark>This method has to be public since seleceted nodes will call it.</devremark>
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ClipBoard")]
		protected internal virtual StringBuilder PrepareSelectedNodesForClipBoard()
		{
			Debug.Assert(this.ProjectMgr != null, " No project mananager available for this node " + ToString());
			Debug.Assert(this.ProjectMgr.ItemsDraggedOrCutOrCopied != null, " The itemsdragged list should have been initialized prior calling this method");
			StringBuilder sb = new StringBuilder();

			if(this.hierarchyId == VSConstants.VSITEMID_ROOT)
			{
				if(this.ProjectMgr.ItemsDraggedOrCutOrCopied != null)
				{
					this.ProjectMgr.ItemsDraggedOrCutOrCopied.Clear();// abort
				}
				return sb;
			}

			if(this.ProjectMgr.ItemsDraggedOrCutOrCopied != null)
			{
				this.ProjectMgr.ItemsDraggedOrCutOrCopied.Add(this);
			}

			string projref = String.Empty;
			IVsSolution solution = this.GetService(typeof(IVsSolution)) as IVsSolution;
			if(solution != null)
			{
				ErrorHandler.ThrowOnFailure(solution.GetProjrefOfItem(this.ProjectMgr, this.hierarchyId, out projref));
				if(String.IsNullOrEmpty(projref))
				{
					if(this.ProjectMgr.ItemsDraggedOrCutOrCopied != null)
					{
						this.ProjectMgr.ItemsDraggedOrCutOrCopied.Clear();// abort
					}
					return sb;
				}
			}

			// Append the projectref and a null terminator to the string builder
			sb.Append(projref);
			sb.Append('\0');

			return sb;
		}

		/// <summary>
		/// Returns the Cannonical Name
		/// </summary>
		/// <returns>Cannonical Name</returns>
		protected virtual string GetCanonicalName()
		{
			return this.GetMkDocument();
		}

		/// <summary>
		/// Factory method for the Document Manager object
		/// </summary>
		/// <returns>null object, since a hierarchy node does not know its kind of document</returns>
		/// <remarks>Must be overriden by derived node classes if a document manager is needed</remarks>
		protected internal virtual DocumentManager GetDocumentManager()
		{
			return null;
		}

		/// <summary>
		/// Displays the context menu.
		/// </summary>
		/// <param name="selectedNodes">list of selected nodes.</param>
		/// <param name="pointerToVariant">contains the location (x,y) at which to show the menu.</param>
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "pointer")]
		protected virtual int DisplayContextMenu(IList<HierarchyNode> selectedNodes, IntPtr pointerToVariant)
		{
			if(selectedNodes == null || selectedNodes.Count == 0 || pointerToVariant == IntPtr.Zero)
			{
				return NativeMethods.OLECMDERR_E_NOTSUPPORTED;
			}

			int idmxStoredMenu = 0;

			foreach(HierarchyNode node in selectedNodes)
			{
				// We check here whether we have a multiple selection of
				// nodes of differing type.
				if(idmxStoredMenu == 0)
				{
					// First time through or single node case
					idmxStoredMenu = node.MenuCommandId;
				}
				else if(idmxStoredMenu != node.MenuCommandId)
				{
					// We have different node types. Check if any of the nodes is
					// the project node and set the menu accordingly.
					if(node.MenuCommandId == VsMenus.IDM_VS_CTXT_PROJNODE)
					{
						idmxStoredMenu = VsMenus.IDM_VS_CTXT_XPROJ_PROJITEM;
					}
					else
					{
						idmxStoredMenu = VsMenus.IDM_VS_CTXT_XPROJ_MULTIITEM;
					}
				}
			}

			object variant = Marshal.GetObjectForNativeVariant(pointerToVariant);
			UInt32 pointsAsUint = (UInt32)variant;
			short x = (short)(pointsAsUint & 0x0000ffff);
			short y = (short)((pointsAsUint & 0xffff0000) / 0x10000);


			POINTS points = new POINTS();
			points.x = x;
			points.y = y;
			return ShowContextMenu(idmxStoredMenu, VsMenus.guidSHLMainMenu, points);
		}

		/// <summary>
		/// Shows the specified context menu at a specified location.
		/// </summary>
		/// <param name="menuId">The context menu ID.</param>
		/// <param name="groupGuid">The GUID of the menu group.</param>
		/// <param name="points">The location at which to show the menu.</param>
		protected virtual int ShowContextMenu(int menuId, Guid menuGroup, POINTS points)
		{
			IVsUIShell shell = this.projectMgr.Site.GetService(typeof(SVsUIShell)) as IVsUIShell;

			Debug.Assert(shell != null, "Could not get the ui shell from the project");
			if(shell == null)
			{
				return VSConstants.E_FAIL;
			}
			POINTS[] pnts = new POINTS[1];
			pnts[0].x = points.x;
			pnts[0].y = points.y;
			return shell.ShowContextMenu(0, ref menuGroup, menuId, pnts, (Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget)this);
		}

		#region initiation of command execution
		/// <summary>
		/// Handles command execution.
		/// </summary>
		/// <param name="cmdGroup">Unique identifier of the command group</param>
		/// <param name="cmd">The command to be executed.</param>
		/// <param name="nCmdexecopt">Values describe how the object should execute the command.</param>
		/// <param name="pvaIn">Pointer to a VARIANTARG structure containing input arguments. Can be NULL</param>
		/// <param name="pvaOut">VARIANTARG structure to receive command output. Can be NULL.</param>
		/// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cmdexecopt")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "n")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "pva")]
		protected virtual int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			if(this.projectMgr == null || this.projectMgr.IsClosed)
			{
				return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
			}

			if(cmdGroup == Guid.Empty)
			{
				return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
			}
			else if(cmdGroup == VsMenus.guidVsUIHierarchyWindowCmds)
			{
				switch(cmd)
				{
					case (uint)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_DoubleClick:
					case (uint)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_EnterKey:
						this.DoDefaultAction();
						return VSConstants.S_OK;
				}
				return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
			}
			else if(cmdGroup == VsMenus.guidStandardCommandSet97)
			{
				HierarchyNode nodeToAddTo = this.GetDragTargetHandlerNode();
				switch((VsCommands)cmd)
				{
					case VsCommands.AddNewItem:
						return nodeToAddTo.AddItemToHierarchy(HierarchyAddType.AddNewItem);

					case VsCommands.AddExistingItem:
						return nodeToAddTo.AddItemToHierarchy(HierarchyAddType.AddExistingItem);

					case VsCommands.NewFolder:
						return nodeToAddTo.AddNewFolder();

					case VsCommands.Paste:
						return this.ProjectMgr.PasteFromClipboard(this);
				}

			}
			else if(cmdGroup == VsMenus.guidStandardCommandSet2K)
			{
				switch((VsCommands2K)cmd)
				{
					case VsCommands2K.EXCLUDEFROMPROJECT:
						return this.ExcludeFromProject();
				}
			}

			return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
		}

		/// <summary>
		/// Executes a command that can only be executed once the whole selection is known.
		/// </summary>
		/// <param name="cmdGroup">Unique identifier of the command group</param>
		/// <param name="cmdId">The command to be executed.</param>
		/// <param name="cmdExecOpt">Values describe how the object should execute the command.</param>
		/// <param name="vaIn">Pointer to a VARIANTARG structure containing input arguments. Can be NULL</param>
		/// <param name="vaOut">VARIANTARG structure to receive command output. Can be NULL.</param>
		/// <param name="commandOrigin">The origin of the command. From IOleCommandTarget or hierarchy.</param>
		/// <param name="selectedNodes">The list of the selected nodes.</param>
		/// <param name="handled">An out parameter specifying that the command was handled.</param>
		/// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "vaIn")]
		protected virtual int ExecCommandThatDependsOnSelectedNodes(Guid cmdGroup, uint cmdId, uint cmdExecOpt, IntPtr vaIn, IntPtr vaOut, CommandOrigin commandOrigin, IList<HierarchyNode> selectedNodes, out bool handled)
		{
			handled = false;
			if(cmdGroup == VsMenus.guidVsUIHierarchyWindowCmds)
			{
				switch(cmdId)
				{
					case (uint)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_RightClick:
						// The UIHWCMDID_RightClick is what tells an IVsUIHierarchy in a UIHierarchyWindow 
						// to put up the context menu.  Since the mouse may have moved between the 
						// mouse down and the mouse up, GetCursorPos won't tell you the right place 
						// to put the context menu (especially if it came through the keyboard).  
						// So we pack the proper menu position into pvaIn by
						// memcpy'ing a POINTS struct into the VT_UI4 part of the pvaIn variant.  The
						// code to unpack it looks like this:
						//			ULONG ulPts = V_UI4(pvaIn);
						//			POINTS pts;
						//			memcpy((void*)&pts, &ulPts, sizeof(POINTS));
						// You then pass that POINTS into DisplayContextMenu.
						handled = true;
						return this.DisplayContextMenu(selectedNodes, vaIn);
					default:
						break;
				}
			}
			else if(cmdGroup == VsMenus.guidStandardCommandSet2K)
			{
				switch((VsCommands2K)cmdId)
				{
					case VsCommands2K.ViewInClassDiagram:
						handled = true;
						return this.ShowInDesigner(selectedNodes);
				}
			}

			return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
		}

		/// <summary>
		/// Executes command that are independent of a selection.
		/// </summary>
		/// <param name="cmdGroup">Unique identifier of the command group</param>
		/// <param name="cmdId">The command to be executed.</param>
		/// <param name="cmdExecOpt">Values describe how the object should execute the command.</param>
		/// <param name="vaIn">Pointer to a VARIANTARG structure containing input arguments. Can be NULL</param>
		/// <param name="vaOut">VARIANTARG structure to receive command output. Can be NULL.</param>
		/// <param name="commandOrigin">The origin of the command. From IOleCommandTarget or hierarchy.</param>
		/// <param name="handled">An out parameter specifying that the command was handled.</param>
		/// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "vaIn")]
		protected virtual int ExecCommandIndependentOfSelection(Guid cmdGroup, uint cmdId, uint cmdExecOpt, IntPtr vaIn, IntPtr vaOut, CommandOrigin commandOrigin, out bool handled)
		{
			handled = false;

			if(this.projectMgr == null || this.projectMgr.IsClosed)
			{
				return VSConstants.E_FAIL;
			}

			if(cmdGroup == VsMenus.guidStandardCommandSet97)
			{
				if(commandOrigin == CommandOrigin.OleCommandTarget)
				{
					switch((VsCommands)cmdId)
					{
						case VsCommands.Cut:
						case VsCommands.Copy:
						case VsCommands.Paste:
						case VsCommands.Rename:
							handled = true;
							return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
					}
				}

				switch((VsCommands)cmdId)
				{
					case VsCommands.Copy:
						handled = true;
						return this.ProjectMgr.CopyToClipboard();

					case VsCommands.Cut:
						handled = true;
						return this.ProjectMgr.CutToClipboard();

					case VsCommands.SolutionCfg:
						handled = true;
						return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;

					case VsCommands.SearchCombo:
						handled = true;
						return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;

				}
			}
			else if(cmdGroup == VsMenus.guidStandardCommandSet2K)
			{
				// There should only be the project node who handles these and should manifest in the same action regardles of selection.
				switch((VsCommands2K)cmdId)
				{
					case VsCommands2K.SHOWALLFILES:
						handled = true;
						return this.projectMgr.ShowAllFiles();
					case VsCommands2K.ADDREFERENCE:
						handled = true;
						return this.projectMgr.AddProjectReference();
					case VsCommands2K.ADDWEBREFERENCE:
						handled = true;
						return this.projectMgr.AddWebReference();
				}
			}

			return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
		}

		/// <summary>
		/// The main entry point for command excection. Gets called from the IVsUIHierarchy and IOleCommandTarget methods.
		/// </summary>
		/// <param name="cmdGroup">Unique identifier of the command group</param>
		/// <param name="cmdId">The command to be executed.</param>
		/// <param name="cmdExecOpt">Values describe how the object should execute the command.</param>
		/// <param name="vaIn">Pointer to a VARIANTARG structure containing input arguments. Can be NULL</param>
		/// <param name="vaOut">VARIANTARG structure to receive command output. Can be NULL.</param>
		/// <param name="commandOrigin">The origin of the command. From IOleCommandTarget or hierarchy.</param>
		/// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "vaIn")]
		protected virtual int InternalExecCommand(Guid cmdGroup, uint cmdId, uint cmdExecOpt, IntPtr vaIn, IntPtr vaOut, CommandOrigin commandOrigin)
		{
			CCITracing.TraceCall(cmdGroup.ToString() + "," + cmdId.ToString());
			if(this.projectMgr == null || this.projectMgr.IsClosed)
			{
				return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
			}

			if(cmdGroup == Guid.Empty)
			{
				return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
			}

			IList<HierarchyNode> selectedNodes = this.projectMgr.GetSelectedNodes();

			// Check if all nodes can execute a command. If there is at least one that cannot return not handled.
			foreach(HierarchyNode node in selectedNodes)
			{
				if(!node.CanExecuteCommand)
				{
					return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
				}
			}

			// Handle commands that are independent of a selection.
			bool handled = false;
			int returnValue = this.ExecCommandIndependentOfSelection(cmdGroup, cmdId, cmdExecOpt, vaIn, vaOut, commandOrigin, out handled);
			if(handled)
			{
				return returnValue;
			}


			// Now handle commands that need the selected nodes as input parameter.
			returnValue = this.ExecCommandThatDependsOnSelectedNodes(cmdGroup, cmdId, cmdExecOpt, vaIn, vaOut, commandOrigin, selectedNodes, out handled);
			if(handled)
			{
				return returnValue;
			}

			returnValue = (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;

			// Handle commands iteratively. The same action will be executed for all of the selected items.
			foreach(HierarchyNode node in selectedNodes)
			{
				try
				{
					returnValue = node.ExecCommandOnNode(cmdGroup, cmdId, cmdExecOpt, vaIn, vaOut);
				}
				catch(COMException e)
				{
					Trace.WriteLine("Exception : " + e.Message);
					returnValue = e.ErrorCode;
				}
				if(returnValue != VSConstants.S_OK)
				{
					break;
				}
			}

			if(returnValue == VSConstants.E_ABORT || returnValue == VSConstants.OLE_E_PROMPTSAVECANCELLED)
			{
				returnValue = VSConstants.S_OK;
			}

			return returnValue;
		}

		#endregion

		#region query command handling
		/// <summary>
		/// Handles menus originating from IOleCommandTarget.
		/// </summary>
		/// <param name="cmdGroup">Unique identifier of the command group</param>
		/// <param name="cmd">The command to be executed.</param>
		/// <param name="handled">Specifies whether the menu was handled.</param>
		/// <returns>A QueryStatusResult describing the status of the menu.</returns>
		protected virtual QueryStatusResult QueryStatusCommandFromOleCommandTarget(Guid cmdGroup, uint cmd, out bool handled)
		{
			handled = false;
			// NOTE: We only want to support Cut/Copy/Paste/Delete/Rename commands
			// if focus is in the project window. This means that we should only
			// support these commands if they are dispatched via IVsUIHierarchy
			// interface and not if they are dispatch through IOleCommandTarget
			// during the command routing to the active project/hierarchy.
			if(VsMenus.guidStandardCommandSet97 == cmdGroup)
			{

				switch((VsCommands)cmd)
				{
					case VsCommands.Copy:
					case VsCommands.Paste:
					case VsCommands.Cut:
					case VsCommands.Rename:
						handled = true;
						return QueryStatusResult.NOTSUPPORTED;
				}
			}
			// The reference menu and the web reference menu should always be shown.
			else if(cmdGroup == VsMenus.guidStandardCommandSet2K)
			{
				switch((VsCommands2K)cmd)
				{
					case VsCommands2K.ADDREFERENCE:
						handled = true;
						return QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
				}
			}
			return QueryStatusResult.NOTSUPPORTED;
		}

		/// <summary>
		/// Specifies which command does not support multiple selection and should be disabled if multi-selected.
		/// </summary>
		/// <param name="cmdGroup">Unique identifier of the command group</param>
		/// <param name="cmd">The command to be executed.</param>
		/// <param name="selectedNodes">The list of selected nodes.</param>
		/// <param name="handled">Specifies whether the menu was handled.</param>
		/// <returns>A QueryStatusResult describing the status of the menu.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
		protected virtual QueryStatusResult DisableCommandOnNodesThatDoNotSupportMultiSelection(Guid cmdGroup, uint cmd, IList<HierarchyNode> selectedNodes, out bool handled)
		{
			handled = false;
			QueryStatusResult queryResult = QueryStatusResult.NOTSUPPORTED;
			if(selectedNodes == null || selectedNodes.Count == 1)
			{
				return queryResult;
			}

			if(VsMenus.guidStandardCommandSet97 == cmdGroup)
			{
				switch((VsCommands)cmd)
				{
					case VsCommands.Cut:
					case VsCommands.Copy:
						// If the project node is selected then cut and copy is not supported.
						if(selectedNodes.Contains(this.projectMgr))
						{
							queryResult = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
							handled = true;
						}
						break;

					case VsCommands.Paste:
					case VsCommands.NewFolder:
						queryResult = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
						handled = true;
						break;
				}
			}
			else if(cmdGroup == VsMenus.guidStandardCommandSet2K)
			{
				switch((VsCommands2K)cmd)
				{
					case VsCommands2K.QUICKOBJECTSEARCH:
					case VsCommands2K.SETASSTARTPAGE:
					case VsCommands2K.ViewInClassDiagram:
						queryResult = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
						handled = true;
						break;
				}
			}

			return queryResult;
		}

		/// <summary>
		/// Handles command status on a node. Should be overridden by descendant nodes. If a command cannot be handled then the base should be called.
		/// </summary>
		/// <param name="cmdGroup">A unique identifier of the command group. The pguidCmdGroup parameter can be NULL to specify the standard group.</param>
		/// <param name="cmd">The command to query status for.</param>
		/// <param name="pCmdText">Pointer to an OLECMDTEXT structure in which to return the name and/or status information of a single command. Can be NULL to indicate that the caller does not require this information.</param>
		/// <param name="result">An out parameter specifying the QueryStatusResult of the command.</param>
		/// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "p")]
		protected virtual int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
		{
			if(cmdGroup == VsMenus.guidStandardCommandSet2K)
			{
				if((VsCommands2K)cmd == VsCommands2K.SHOWALLFILES)
				{
					result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
					return VSConstants.S_OK;
				}
			}

			return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
		}

		/// <summary>
		/// Disables commands when the project is in run/break mode.
		/// </summary>/
		/// <param name="commandGroup">Unique identifier of the command group</param>
		/// <param name="command">The command to be executed.</param>
		/// <returns>A QueryStatusResult describing the status of the menu.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InCurrent")]
		protected virtual bool DisableCmdInCurrentMode(Guid commandGroup, uint command)
		{
			if(this.ProjectMgr == null || this.ProjectMgr.IsClosed)
			{
				return false;
			}

			// Don't ask if it is not these two commandgroups.
			if(commandGroup == VsMenus.guidStandardCommandSet97 || commandGroup == VsMenus.guidStandardCommandSet2K)
			{
				if(this.ProjectMgr.IsCurrentStateASuppressCommandsMode())
				{
					if(commandGroup == VsMenus.guidStandardCommandSet97)
					{
						switch((VsCommands)command)
						{
							default:
								break;
							case VsCommands.AddExistingItem:
							case VsCommands.AddNewItem:
							case VsCommands.NewFolder:
							case VsCommands.Remove:
							case VsCommands.Cut:
							case VsCommands.Paste:
							case VsCommands.Copy:
							case VsCommands.EditLabel:
							case VsCommands.Rename:
							case VsCommands.UnloadProject:
								return true;
						}
					}
					else if(commandGroup == VsMenus.guidStandardCommandSet2K)
					{
						switch((VsCommands2K)command)
						{
							default:
								break;
							case VsCommands2K.EXCLUDEFROMPROJECT:
							case VsCommands2K.INCLUDEINPROJECT:
							case VsCommands2K.ADDWEBREFERENCECTX:
							case VsCommands2K.ADDWEBREFERENCE:
							case VsCommands2K.ADDREFERENCE:
							case VsCommands2K.SETASSTARTPAGE:
								return true;
						}
					}
				}
				// If we are not in a cut or copy mode then disable the paste command
				else if(!this.ProjectMgr.AllowPasteCommand())
				{
					if(commandGroup == VsMenus.guidStandardCommandSet97 && (VsCommands)command == VsCommands.Paste)
					{
						return true;
					}
				}
			}

			return false;
		}


		/// <summary>
		/// Queries the object for the command status on a list of selected nodes.
		/// </summary>
		/// <param name="cmdGroup">A unique identifier of the command group.</param>
		/// <param name="cCmds">The number of commands in the prgCmds array</param>
		/// <param name="prgCmds">A caller-allocated array of OLECMD structures that indicate the commands for which the caller requires status information. This method fills the cmdf member of each structure with values taken from the OLECMDF enumeration</param>
		/// <param name="pCmdText">Pointer to an OLECMDTEXT structure in which to return the name and/or status information of a single command. Can be NULL to indicate that the caller does not require this information. </param>
		/// <param name="commandOrigin">Specifies the origin of the command. Either it was called from the QueryStatusCommand on IVsUIHierarchy or from the IOleCommandTarget</param>
		/// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cmds")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "c")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "p")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "prg")]
		protected virtual int QueryStatusSelection(Guid cmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText, CommandOrigin commandOrigin)
		{
			if(this.projectMgr.IsClosed)
			{
				return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
			}

			if(cmdGroup == Guid.Empty)
			{
				return (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP;
			}

            if (prgCmds == null)
            {
                throw new ArgumentNullException("prgCmds");
            }

			uint cmd = prgCmds[0].cmdID;
			QueryStatusResult queryResult = QueryStatusResult.NOTSUPPORTED;

			// For now ask this node (that is the project node) to disable or enable a node.
			// This is an optimization. Why should we ask each node for its current state? They all are in the same state.
			// Also please note that we return QueryStatusResult.INVISIBLE instead of just QueryStatusResult.SUPPORTED.
			// The reason is that if the project has nested projects, then providing just QueryStatusResult.SUPPORTED is not enough.
			// What will happen is that the nested project will show grayed commands that belong to this project and does not belong to the nested project. (like special commands implemented by subclassed projects).
			// The reason is that a special command comes in that is not handled because we are in debug mode. Then VsCore asks the nested project can you handle it.
			// The nested project does not know about it, thus it shows it on the nested project as grayed.
			if(this.DisableCmdInCurrentMode(cmdGroup, cmd))
			{
				queryResult = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
			}
			else
			{
				bool handled = false;

				if(commandOrigin == CommandOrigin.OleCommandTarget)
				{
					queryResult = this.QueryStatusCommandFromOleCommandTarget(cmdGroup, cmd, out handled);
				}

				if(!handled)
				{
					IList<HierarchyNode> selectedNodes = this.projectMgr.GetSelectedNodes();

					// Want to disable in multiselect case.
					if(selectedNodes != null && selectedNodes.Count > 1)
					{
						queryResult = this.DisableCommandOnNodesThatDoNotSupportMultiSelection(cmdGroup, cmd, selectedNodes, out handled);
					}

					// Now go and do the job on the nodes.
					if(!handled)
					{
						queryResult = this.QueryStatusSelectionOnNodes(selectedNodes, cmdGroup, cmd, pCmdText);
					}

				}
			}

			// Process the results set in the QueryStatusResult
			if(queryResult != QueryStatusResult.NOTSUPPORTED)
			{
				// Set initial value
				prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

				if((queryResult & QueryStatusResult.ENABLED) != 0)
				{
					prgCmds[0].cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
				}

				if((queryResult & QueryStatusResult.INVISIBLE) != 0)
				{
					prgCmds[0].cmdf |= (uint)OLECMDF.OLECMDF_INVISIBLE;
				}

				if((queryResult & QueryStatusResult.LATCHED) != 0)
				{
					prgCmds[0].cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;
				}

				return VSConstants.S_OK;
			}

			return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
		}

		/// <summary>
		/// Queries the selected nodes for the command status. 
		/// A command is supported iff any nodes supports it.
		/// A command is enabled iff all nodes enable it.
		/// A command is invisible iff any node sets invisibility.
		/// A command is latched only if all are latched.
		/// </summary>
		/// <param name="selectedNodes">The list of selected nodes.</param>
		/// <param name="cmdGroup">A unique identifier of the command group.</param>
		/// <param name="cmd">The command id to query for.</param>
		/// <param name="pCmdText">Pointer to an OLECMDTEXT structure in which to return the name and/or status information of a single command. Can be NULL to indicate that the caller does not require this information. </param>
		/// <returns>Retuns the result of the query on the slected nodes.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "p")]
		protected virtual QueryStatusResult QueryStatusSelectionOnNodes(IList<HierarchyNode> selectedNodes, Guid cmdGroup, uint cmd, IntPtr pCmdText)
		{
			if(selectedNodes == null || selectedNodes.Count == 0)
			{
				return QueryStatusResult.NOTSUPPORTED;
			}

			int result = 0;
			bool supported = false;
			bool enabled = true;
			bool invisible = false;
			bool latched = true;
			QueryStatusResult tempQueryResult = QueryStatusResult.NOTSUPPORTED;

			foreach(HierarchyNode node in selectedNodes)
			{
				result = node.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref tempQueryResult);
				if(result < 0)
				{
					break;
				}

				// cmd is supported iff any node supports cmd
				// cmd is enabled iff all nodes enable cmd
				// cmd is invisible iff any node sets invisibility
				// cmd is latched only if all are latched.
				supported = supported || ((tempQueryResult & QueryStatusResult.SUPPORTED) != 0);
				enabled = enabled && ((tempQueryResult & QueryStatusResult.ENABLED) != 0);
				invisible = invisible || ((tempQueryResult & QueryStatusResult.INVISIBLE) != 0);
				latched = latched && ((tempQueryResult & QueryStatusResult.LATCHED) != 0);
			}

			QueryStatusResult queryResult = QueryStatusResult.NOTSUPPORTED;

			if(result >= 0 && supported)
			{
				queryResult = QueryStatusResult.SUPPORTED;

				if(enabled)
				{
					queryResult |= QueryStatusResult.ENABLED;
				}

				if(invisible)
				{
					queryResult |= QueryStatusResult.INVISIBLE;
				}

				if(latched)
				{
					queryResult |= QueryStatusResult.LATCHED;
				}
			}

			return queryResult;
		}

		#endregion
		protected virtual bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
		{
			return this.ProjectMgr.CanProjectDeleteItems;
		}

		/// <summary>
		/// Overwrite this method to tell that you support the default icon for this node.
		/// </summary>
		/// <returns></returns>
		protected virtual bool CanShowDefaultIcon()
		{
			return false;
		}

		/// <summary>
		/// Performs save as operation for an item after the save as dialog has been processed.
		/// </summary>
		/// <param name="docData">A pointer to the rdt</param>
		/// <param name="newName">The newName of the item</param>
		/// <returns></returns>
		protected virtual int AfterSaveItemAs(IntPtr docData, string newName)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// The method that does the cleanup.
		/// </summary>
		/// <param name="disposing">Is the Dispose called by some internal member, or it is called by from GC.</param>
		protected virtual void Dispose(bool disposing)
		{
			if(this.isDisposed)
			{
				return;
			}

			if(disposing)
			{
				// This will dispose any subclassed project node that implements IDisposable.
				if(this.oleServiceProvider != null)
				{
					// Dispose the ole service provider object.
					this.oleServiceProvider.Dispose();
				}
			}

			this.isDisposed = true;
		}

		/// <summary>
		/// Sets the VSADDFILEFLAGS that will be used to call the  IVsTrackProjectDocumentsEvents2 OnAddFiles
		/// </summary>
		/// <param name="files">The files to which an array of VSADDFILEFLAGS has to be specified.</param>
		/// <returns></returns>
		protected internal virtual VSADDFILEFLAGS[] GetAddFileFlags(string[] files)
		{
			if(files == null || files.Length == 0)
			{
				return new VSADDFILEFLAGS[1] { VSADDFILEFLAGS.VSADDFILEFLAGS_NoFlags };
			}

			VSADDFILEFLAGS[] addFileFlags = new VSADDFILEFLAGS[files.Length];

			for(int i = 0; i < files.Length; i++)
			{
				addFileFlags[i] = VSADDFILEFLAGS.VSADDFILEFLAGS_NoFlags;
			}

			return addFileFlags;
		}

		/// <summary>
		/// Sets the VSQUERYADDFILEFLAGS that will be used to call the  IVsTrackProjectDocumentsEvents2 OnQueryAddFiles
		/// </summary>
		/// <param name="files">The files to which an array of VSADDFILEFLAGS has to be specified.</param>
		/// <returns></returns>
		protected internal virtual VSQUERYADDFILEFLAGS[] GetQueryAddFileFlags(string[] files)
		{
			if(files == null || files.Length == 0)
			{
				return new VSQUERYADDFILEFLAGS[1] { VSQUERYADDFILEFLAGS.VSQUERYADDFILEFLAGS_NoFlags };
			}

			VSQUERYADDFILEFLAGS[] queryAddFileFlags = new VSQUERYADDFILEFLAGS[files.Length];

			for(int i = 0; i < files.Length; i++)
			{
				queryAddFileFlags[i] = VSQUERYADDFILEFLAGS.VSQUERYADDFILEFLAGS_NoFlags;
			}

			return queryAddFileFlags;
		}

		/// <summary>
		/// Sets the VSREMOVEFILEFLAGS that will be used to call the  IVsTrackProjectDocumentsEvents2 OnRemoveFiles
		/// </summary>
		/// <param name="files">The files to which an array of VSREMOVEFILEFLAGS has to be specified.</param>
		/// <returns></returns>
		protected internal virtual VSREMOVEFILEFLAGS[] GetRemoveFileFlags(string[] files)
		{
			if(files == null || files.Length == 0)
			{
				return new VSREMOVEFILEFLAGS[1] { VSREMOVEFILEFLAGS.VSREMOVEFILEFLAGS_NoFlags };
			}

			VSREMOVEFILEFLAGS[] removeFileFlags = new VSREMOVEFILEFLAGS[files.Length];

			for(int i = 0; i < files.Length; i++)
			{
				removeFileFlags[i] = VSREMOVEFILEFLAGS.VSREMOVEFILEFLAGS_NoFlags;
			}

			return removeFileFlags;
		}

		/// <summary>
		/// Sets the VSQUERYREMOVEFILEFLAGS that will be used to call the  IVsTrackProjectDocumentsEvents2 OnQueryRemoveFiles
		/// </summary>
		/// <param name="files">The files to which an array of VSQUERYREMOVEFILEFLAGS has to be specified.</param>
		/// <returns></returns>
		protected internal virtual VSQUERYREMOVEFILEFLAGS[] GetQueryRemoveFileFlags(string[] files)
		{
			if(files == null || files.Length == 0)
			{
				return new VSQUERYREMOVEFILEFLAGS[1] { VSQUERYREMOVEFILEFLAGS.VSQUERYREMOVEFILEFLAGS_NoFlags };
			}

			VSQUERYREMOVEFILEFLAGS[] queryRemoveFileFlags = new VSQUERYREMOVEFILEFLAGS[files.Length];

			for(int i = 0; i < files.Length; i++)
			{
				queryRemoveFileFlags[i] = VSQUERYREMOVEFILEFLAGS.VSQUERYREMOVEFILEFLAGS_NoFlags;
			}

			return queryRemoveFileFlags;
		}

		/// <summary>
		/// This method should be overridden to provide the list of files and associated flags for source control.
		/// </summary>
		/// <param name="files">The list of files to be placed under source control.</param>
		/// <param name="flags">The flags that are associated to the files.</param>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scc")]
		protected internal virtual void GetSccFiles(IList<string> files, IList<tagVsSccFilesFlags> flags)
		{
			if(this.ExcludeNodeFromScc)
			{
				return;
			}

			if(files == null)
			{
				throw new ArgumentNullException("files");
			}

			if(flags == null)
			{
				throw new ArgumentNullException("flags");
			}

			files.Add(this.GetMkDocument());

			tagVsSccFilesFlags flagsToAdd = (this.firstChild != null && (this.firstChild is DependentFileNode)) ? tagVsSccFilesFlags.SFF_HasSpecialFiles : tagVsSccFilesFlags.SFF_NoFlags;

			flags.Add(flagsToAdd);
		}

		/// <summary>
		/// This method should be overridden to provide the list of special files and associated flags for source control.
		/// </summary>
		/// <param name="sccFile">One of the file associated to the node.</param>
		/// <param name="files">The list of files to be placed under source control.</param>
		/// <param name="flags">The flags that are associated to the files.</param>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scc")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "scc")]
		protected internal virtual void GetSccSpecialFiles(string sccFile, IList<string> files, IList<tagVsSccFilesFlags> flags)
		{
			if(this.ExcludeNodeFromScc)
			{
				return;
			}

			if(files == null)
			{
				throw new ArgumentNullException("files");
			}

			if(flags == null)
			{
				throw new ArgumentNullException("flags");
			}
		}

		/// <summary>
		/// Delete the item corresponding to the specified path from storage.
		/// </summary>
		/// <param name="path">Url of the item to delete</param>
		internal protected virtual void DeleteFromStorage(string path)
		{
		}

		/// <summary>
		/// Determines whether a file change should be ignored or not.
		/// </summary>
		/// <param name="ignoreFlag">Flag indicating whether or not to ignore changes (true to ignore changes).</param>
		protected internal virtual void IgnoreItemFileChanges(bool ignoreFlag)
		{
		}

		/// <summary>
		/// Called to determine whether a project item is reloadable. 
		/// </summary>
		/// <returns>True if the project item is reloadable.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Reloadable")]
		protected internal virtual bool IsItemReloadable()
		{
			return true;
		}

		/// <summary>
		/// Reloads an item.
		/// </summary>
		/// <param name="reserved">Reserved parameter defined at the IVsPersistHierarchyItem2::ReloadItem parameter.</param>
		protected internal virtual void ReloadItem(uint reserved)
		{

		}

		/// <summary>
		/// Handle the Copy operation to the clipboard
		/// This method is typically overriden on the project node
		/// </summary>
		protected internal virtual int CopyToClipboard()
		{
			return VSConstants.E_NOTIMPL;
		}

		/// <summary>
		/// Handle the Cut operation to the clipboard
		/// This method is typically overriden on the project node
		/// </summary>
		protected internal virtual int CutToClipboard()
		{
			return VSConstants.E_NOTIMPL;
		}

		/// <summary>
		/// Handle the paste from Clipboard command.
		/// This method is typically overriden on the project node
		/// </summary>
		protected internal virtual int PasteFromClipboard(HierarchyNode targetNode)
		{
			return VSConstants.E_NOTIMPL;
		}

		/// <summary>
		/// Determines if the paste command should be allowed.
		/// This method is typically overriden on the project node
		/// </summary>
		protected internal virtual bool AllowPasteCommand()
		{
			return false; ;
		}

		/// <summary>
		/// Register/Unregister for Clipboard events for the UiHierarchyWindow (solution explorer)
		/// This method is typically overriden on the project node
		/// </summary>
		/// <param name="value">true for register, false for unregister</param>
		protected internal virtual void RegisterClipboardNotifications(bool value)
		{
			return;
		}
		#endregion

		#region public methods

		public void OnItemAdded(HierarchyNode parent, HierarchyNode child)
		{
            if (parent == null)
            { 
                throw new ArgumentNullException("parent");
            }

            if (child == null)
            {
                throw new ArgumentNullException("child");
            }

			if(null != parent.onChildAdded)
			{
				HierarchyNodeEventArgs args = new HierarchyNodeEventArgs(child);
				parent.onChildAdded(parent, args);
			}
			
			HierarchyNode foo;
			foo = this.projectMgr == null ? this : this.projectMgr;

			if(foo == this.projectMgr && (this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerHierarchyEvents) != 0)
			{
				return;
			}

			HierarchyNode prev = child.PreviousSibling;
			uint prevId = (prev != null) ? prev.hierarchyId : VSConstants.VSITEMID_NIL;
			foreach(IVsHierarchyEvents sink in foo.hierarchyEventSinks)
			{
				int result = sink.OnItemAdded(parent.hierarchyId, prevId, child.hierarchyId);
				if(ErrorHandler.Failed(result) && result != VSConstants.E_NOTIMPL)
				{
					ErrorHandler.ThrowOnFailure(result);
				}
			}
		}


		public virtual void OnItemDeleted()
		{
			HierarchyNode foo;
			foo = this.projectMgr == null ? this : this.projectMgr;

			if(foo == this.projectMgr && (this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerHierarchyEvents) != 0)
			{
				return;
			}

			if(foo.hierarchyEventSinks.Count > 0)
			{
				// Note that in some cases (deletion of project node for example), an Advise
				// may be removed while we are iterating over it. To get around this problem we
				// take a snapshot of the advise list and walk that.
				List<IVsHierarchyEvents> clonedSink = new List<IVsHierarchyEvents>();

				foreach(IVsHierarchyEvents anEvent in foo.hierarchyEventSinks)
				{
					clonedSink.Add(anEvent);
				}

				foreach(IVsHierarchyEvents clonedEvent in clonedSink)
				{
					int result = clonedEvent.OnItemDeleted(this.hierarchyId);
					if(ErrorHandler.Failed(result) && result != VSConstants.E_NOTIMPL)
					{
						ErrorHandler.ThrowOnFailure(result);
					}
				}
			}
		}

		public void OnItemsAppended(HierarchyNode parent)
		{
			if(parent == null)
			{
				throw new ArgumentNullException("parent");
			}

			HierarchyNode foo;
			foo = this.projectMgr == null ? this : this.projectMgr;

			if(foo == this.projectMgr && (this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerHierarchyEvents) != 0)
			{
				return;
			}

			foreach(IVsHierarchyEvents sink in foo.hierarchyEventSinks)
			{
				int result = sink.OnItemsAppended(parent.hierarchyId);

				if(ErrorHandler.Failed(result) && result != VSConstants.E_NOTIMPL)
				{
					ErrorHandler.ThrowOnFailure(result);
				}
			}
		}


		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "propid")]
		public void OnPropertyChanged(HierarchyNode node, int propid, uint flags)
		{
			if(node == null)
			{
				throw new ArgumentNullException("node");
			}
			HierarchyNode foo;
			foo = this.projectMgr == null ? this : this.projectMgr;
			if(foo == this.projectMgr && (this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerHierarchyEvents) != 0)
			{
				return;
			}

			foreach(IVsHierarchyEvents sink in foo.hierarchyEventSinks)
			{
				int result = sink.OnPropertyChanged(node.hierarchyId, propid, flags);

				if(ErrorHandler.Failed(result) && result != VSConstants.E_NOTIMPL)
				{
					ErrorHandler.ThrowOnFailure(result);
				}
			}
		}


		public void OnInvalidateItems(HierarchyNode parent)
		{
			if(parent == null)
			{
				throw new ArgumentNullException("parent");
			}
			HierarchyNode foo;
			foo = this.projectMgr == null ? this : this.projectMgr;
			if(foo == this.projectMgr && (this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerHierarchyEvents) != 0)
			{
				return;
			}

			foreach(IVsHierarchyEvents sink in foo.hierarchyEventSinks)
			{
				int result = sink.OnInvalidateItems(parent.hierarchyId);

				if(ErrorHandler.Failed(result) && result != VSConstants.E_NOTIMPL)
				{
					ErrorHandler.ThrowOnFailure(result);
				}
			}
		}

		/// <summary>
		/// Causes the hierarchy to be redrawn.
		/// </summary>
		/// <param name="element">Used by the hierarchy to decide which element to redraw</param>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Re")]
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ReDraw")]
		public virtual void ReDraw(UIHierarchyElement element)
		{

			foreach(IVsHierarchyEvents sink in this.projectMgr.hierarchyEventSinks)
			{
				int result;
				if((element & UIHierarchyElement.Icon) != 0)
				{
					result = sink.OnPropertyChanged(this.ID, (int)__VSHPROPID.VSHPROPID_IconIndex, 0);
					Debug.Assert(ErrorHandler.Succeeded(result), "Redraw failed for node " + this.GetMkDocument());
				}

				if((element & UIHierarchyElement.Caption) != 0)
				{
					result = sink.OnPropertyChanged(this.ID, (int)__VSHPROPID.VSHPROPID_Caption, 0);
					Debug.Assert(ErrorHandler.Succeeded(result), "Redraw failed for node " + this.GetMkDocument());
				}

				if((element & UIHierarchyElement.SccState) != 0)
				{
					result = sink.OnPropertyChanged(this.ID, (int)__VSHPROPID.VSHPROPID_StateIconIndex, 0);
					Debug.Assert(ErrorHandler.Succeeded(result), "Redraw failed for node " + this.GetMkDocument());
				}
			}

		}

		/// <summary>
		/// Finds a non virtual hierarchy item by its project element.
		/// </summary>
		/// <param name="node">The Project element to find</param>
		/// <returns>The node found</returns>
		public HierarchyNode FindChildByProjectElement(ProjectElement node)
		{
			if(node == null)
			{
				throw new ArgumentNullException("node");
			}

			for(HierarchyNode child = this.FirstChild; child != null; child = child.NextSibling)
			{
				if(!child.ItemNode.IsVirtual && child.ItemNode == node)
				{
					return child;
				}
			}
			return null;
		}

		public object GetService(Type type)
		{
			if(type == null)
			{
				throw new ArgumentNullException("type");
			}

			if(this.projectMgr.Site == null) return null;
			return this.projectMgr.Site.GetService(type);
		}


		#endregion

		#region IDisposable
		/// <summary>
		/// The IDispose interface Dispose method for disposing the object determinastically.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region IVsHierarchy methods

		public virtual int AdviseHierarchyEvents(IVsHierarchyEvents sink, out uint cookie)
		{
			cookie = this.hierarchyEventSinks.Add(sink) + 1;
			return VSConstants.S_OK;
		}


		public virtual int Close()
		{
			DocumentManager manager = this.GetDocumentManager();
			try
			{
				if(manager != null)
				{
					manager.Close(__FRAMECLOSE.FRAMECLOSE_PromptSave);
				}

			}
			catch { }
			finally
			{
				this.Dispose(true);
			}

			return VSConstants.S_OK;
		}


		public virtual int GetCanonicalName(uint itemId, out string name)
		{
			HierarchyNode n = this.projectMgr.NodeFromItemId(itemId);
			name = (n != null) ? n.GetCanonicalName() : null;
			return VSConstants.S_OK;
		}


		public virtual int GetGuidProperty(uint itemId, int propid, out Guid guid)
		{
			guid = Guid.Empty;
			HierarchyNode n = this.projectMgr.NodeFromItemId(itemId);
			if(n != null)
			{
				int hr = n.GetGuidProperty(propid, out guid);
				__VSHPROPID vspropId = (__VSHPROPID)propid;
				CCITracing.TraceCall(vspropId.ToString() + "=" + guid.ToString());
				return hr;
			}
			if(guid == Guid.Empty)
			{
				return VSConstants.DISP_E_MEMBERNOTFOUND;
			}
			return VSConstants.S_OK;
		}


		public virtual int GetProperty(uint itemId, int propId, out object propVal)
		{
			propVal = null;
			if(itemId != VSConstants.VSITEMID_ROOT && propId == (int)__VSHPROPID.VSHPROPID_IconImgList)
			{
				return VSConstants.DISP_E_MEMBERNOTFOUND;
			}


			HierarchyNode n = this.projectMgr.NodeFromItemId(itemId);
			if(n != null)
			{
				propVal = n.GetProperty(propId);
			}
			if(propVal == null)
			{
				return VSConstants.DISP_E_MEMBERNOTFOUND;
			}
			return VSConstants.S_OK;
		}


		public virtual int GetNestedHierarchy(uint itemId, ref Guid iidHierarchyNested, out IntPtr ppHierarchyNested, out uint pItemId)
		{
			ppHierarchyNested = IntPtr.Zero;
			pItemId = 0;
			// If itemid is not a nested hierarchy we must return E_FAIL.
			return VSConstants.E_FAIL;
		}


		public virtual int GetSite(out Microsoft.VisualStudio.OLE.Interop.IServiceProvider site)
		{
			site = this.projectMgr.Site.GetService(typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider)) as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
			return VSConstants.S_OK;
		}


		/// <summary>
		/// the canonicalName of an item is it's URL, or better phrased,
		/// the persistence data we put into @RelPath, which is a relative URL
		/// to the root project
		/// returning the itemID from this means scanning the list
		/// </summary>
		/// <param name="name"></param>
		/// <param name="itemId"></param>
		public virtual int ParseCanonicalName(string name, out uint itemId)
		{
			// we always start at the current node and go it's children down, so 
			//  if you want to scan the whole tree, better call 
			// the root
			uint notFoundValue = (uint)VSConstants.VSITEMID.Nil;
			itemId = notFoundValue;

			// The default implemenation will check for case insensitive comparision.
			if(String.Equals(name, this.Url, StringComparison.OrdinalIgnoreCase))
			{
				itemId = this.hierarchyId;
				return VSConstants.S_OK;
			}
			if(this.firstChild != null)
			{
				this.firstChild.ParseCanonicalName(name, out itemId);
			}
			if (itemId == notFoundValue && this.nextSibling != null)
			{
				this.nextSibling.ParseCanonicalName(name, out itemId);
			}

			return itemId == notFoundValue ? VSConstants.E_FAIL : VSConstants.S_OK;
		}


		public virtual int QueryClose(out int fCanClose)
		{
			fCanClose = 1;
			return VSConstants.S_OK;
		}


		public virtual int SetGuidProperty(uint itemId, int propid, ref Guid guid)
		{
			HierarchyNode n = this.projectMgr.NodeFromItemId(itemId);
			int rc = VSConstants.E_INVALIDARG;
			if(n != null)
			{
				rc = n.SetGuidProperty(propid, ref guid);
			}
			return rc;
		}


		public virtual int SetProperty(uint itemId, int propid, object value)
		{
			HierarchyNode n = this.projectMgr.NodeFromItemId(itemId);
			if(n != null)
			{
				return n.SetProperty(propid, value);
			}
			else
			{
				return VSConstants.DISP_E_MEMBERNOTFOUND;
			}
		}


		public virtual int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider site)
		{
			return VSConstants.E_NOTIMPL;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "cookie-1")]
		public virtual int UnadviseHierarchyEvents(uint cookie)
		{
			this.hierarchyEventSinks.RemoveAt(cookie - 1);
			return VSConstants.S_OK;
		}


		public int Unused0()
		{
			return VSConstants.E_NOTIMPL;
		}


		public int Unused1()
		{
			return VSConstants.E_NOTIMPL;
		}


		public int Unused2()
		{
			return VSConstants.E_NOTIMPL;
		}


		public int Unused3()
		{
			return VSConstants.E_NOTIMPL;
		}


		public int Unused4()
		{
			return VSConstants.E_NOTIMPL;
		}
		#endregion

		#region IVsUIHierarchy methods

		public virtual int ExecCommand(uint itemId, ref Guid guidCmdGroup, uint nCmdId, uint nCmdExecOpt, IntPtr pvain, IntPtr p)
		{
			return this.InternalExecCommand(guidCmdGroup, nCmdId, nCmdExecOpt, pvain, p, CommandOrigin.UiHierarchy);
		}

		public virtual int QueryStatusCommand(uint itemId, ref Guid guidCmdGroup, uint cCmds, OLECMD[] cmds, IntPtr pCmdText)
		{
			return this.QueryStatusSelection(guidCmdGroup, cCmds, cmds, pCmdText, CommandOrigin.UiHierarchy);
		}
		#endregion

		#region IVsPersistHierarchyItem2 methods

		/// <summary>
		/// Determines whether the hierarchy item changed. 
		/// </summary>
		/// <param name="itemId">Item identifier of the hierarchy item contained in VSITEMID.</param>
		/// <param name="docData">Pointer to the IUnknown interface of the hierarchy item.</param>
		/// <param name="isDirty">true if the hierarchy item changed.</param>
		/// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code. </returns>
		public virtual int IsItemDirty(uint itemId, IntPtr docData, out int isDirty)
		{
			IVsPersistDocData pd = (IVsPersistDocData)Marshal.GetObjectForIUnknown(docData);
			return ErrorHandler.ThrowOnFailure(pd.IsDocDataDirty(out isDirty));
		}

		/// <summary>
		/// Saves the hierarchy item to disk. 
		/// </summary>
		/// <param name="saveFlag">Flags whose values are taken from the VSSAVEFLAGS enumeration.</param>
		/// <param name="silentSaveAsName">New filename when doing silent save as</param>
		/// <param name="itemid">Item identifier of the hierarchy item saved from VSITEMID.</param>
		/// <param name="docData">Item identifier of the hierarchy item saved from VSITEMID.</param>
		/// <param name="cancelled">[out] true if the save action was canceled.</param>
		/// <returns>[out] true if the save action was canceled.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public virtual int SaveItem(VSSAVEFLAGS saveFlag, string silentSaveAsName, uint itemid, IntPtr docData, out int cancelled)
		{
			cancelled = 0;

			if(this.ProjectMgr == null || this.ProjectMgr.IsClosed)
			{
				return VSConstants.E_FAIL;
			}

			// Validate itemid 
			if(itemid == VSConstants.VSITEMID_ROOT || itemid == VSConstants.VSITEMID_SELECTION)
			{
				return VSConstants.E_INVALIDARG;
			}

			HierarchyNode node = this.ProjectMgr.NodeFromItemId(itemid);
			if(node == null)
			{
				return VSConstants.E_FAIL;
			}

			string existingFileMoniker = node.GetMkDocument();

			// We can only perform save if the document is open
			if(docData == IntPtr.Zero)
			{
				string errorMessage = string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.CanNotSaveFileNotOpeneInEditor, CultureInfo.CurrentUICulture), node.Url);
				throw new InvalidOperationException(errorMessage);
			}

			string docNew = String.Empty;
			int returnCode = VSConstants.S_OK;
			IPersistFileFormat ff = null;
			IVsPersistDocData dd = null;
			IVsUIShell shell = this.projectMgr.Site.GetService(typeof(SVsUIShell)) as IVsUIShell;

			if(shell == null)
			{
				return VSConstants.E_FAIL;
			}

			try
			{
				//Save docdata object. 
				//For the saveas action a dialog is show in order to enter new location of file.
				//In case of a save action and the file is readonly a dialog is also shown
				//with a couple of options, SaveAs, Overwrite or Cancel.
				ff = Marshal.GetObjectForIUnknown(docData) as IPersistFileFormat;
				if(ff == null)
				{
					return VSConstants.E_FAIL;
				}
				if(VSSAVEFLAGS.VSSAVE_SilentSave == saveFlag)
				{
					ErrorHandler.ThrowOnFailure(shell.SaveDocDataToFile(saveFlag, ff, silentSaveAsName, out docNew, out cancelled));
				}
				else
				{
					dd = Marshal.GetObjectForIUnknown(docData) as IVsPersistDocData;
					if(dd == null)
					{
						return VSConstants.E_FAIL;
					}
					ErrorHandler.ThrowOnFailure(dd.SaveDocData(saveFlag, out docNew, out cancelled));
				}

				// We can be unloaded after the SaveDocData() call if the save caused a designer to add a file and this caused
				// the project file to be reloaded (QEQS caused a newer version of the project file to be downloaded). So we check
				// here.
				if(this.ProjectMgr == null || this.ProjectMgr.IsClosed)
				{
					cancelled = 1;
					return (int)OleConstants.OLECMDERR_E_CANCELED;
				}
				else
				{
					// if a SaveAs occurred we need to update to the fact our item's name has changed.
					// this includes the following:
					//	  1. call RenameDocument on the RunningDocumentTable
					//	  2. update the full path name for the item in our hierarchy
					//	  3. a directory-based project may need to transfer the open editor to the
					//		 MiscFiles project if the new file is saved outside of the project directory.
					//		 This is accomplished by calling IVsExternalFilesManager::TransferDocument                    

					// we have three options for a saveas action to be performed
					// 1. the flag was set (the save as command was triggered)
					// 2. a silent save specifying a new document name
					// 3. a save command was triggered but was not possible because the file has a read only attrib. Therefore
					//    the user has chosen to do a save as in the dialog that showed up
					bool emptyOrSamePath = String.IsNullOrEmpty(docNew) || NativeMethods.IsSamePath(existingFileMoniker, docNew);
					bool saveAs = ((saveFlag == VSSAVEFLAGS.VSSAVE_SaveAs)) ||
						((saveFlag == VSSAVEFLAGS.VSSAVE_SilentSave) && !emptyOrSamePath) ||
						((saveFlag == VSSAVEFLAGS.VSSAVE_Save) && !emptyOrSamePath);

					if(saveAs)
					{
						returnCode = node.AfterSaveItemAs(docData, docNew);

						// If it has been cancelled recover the old name.
						if((returnCode == (int)OleConstants.OLECMDERR_E_CANCELED || returnCode == VSConstants.E_ABORT))
						{
							// Cleanup.
							this.DeleteFromStorage(docNew);
							if(this is ProjectNode && File.Exists(docNew))
							{
								File.Delete(docNew);
							}

							if(ff != null)
							{
								returnCode = shell.SaveDocDataToFile(VSSAVEFLAGS.VSSAVE_SilentSave, ff, existingFileMoniker, out docNew, out cancelled);
							}
						}
						else if(returnCode != VSConstants.S_OK)
						{
							ErrorHandler.ThrowOnFailure(returnCode);
						}
					}
				}
			}
			catch(COMException e)
			{
				Trace.WriteLine("Exception :" + e.Message);
				returnCode = e.ErrorCode;

				// Try to recover
				if(ff != null)
				{
					ErrorHandler.ThrowOnFailure(shell.SaveDocDataToFile(VSSAVEFLAGS.VSSAVE_SilentSave, ff, existingFileMoniker, out docNew, out cancelled));
				}
			}

			return returnCode;
		}

		/// <summary>
		/// Flag indicating that changes to a file can be ignored when item is saved or reloaded. 
		/// </summary>
		/// <param name="itemId">Specifies the item id from VSITEMID.</param>
		/// <param name="ignoreFlag">Flag indicating whether or not to ignore changes (1 to ignore, 0 to stop ignoring).</param>
		/// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
		public virtual int IgnoreItemFileChanges(uint itemId, int ignoreFlag)
		{
			#region precondition
			if(this.ProjectMgr == null || this.ProjectMgr.IsClosed)
			{
				return VSConstants.E_FAIL;
			}
			#endregion

			HierarchyNode n = this.ProjectMgr.NodeFromItemId(itemId);
			if(n != null)
			{
				n.IgnoreItemFileChanges(ignoreFlag == 0 ? false : true);
			}

			return VSConstants.S_OK;
		}

		/// <summary>
		/// Called to determine whether a project item is reloadable before calling ReloadItem. 
		/// </summary>
		/// <param name="itemId">Item identifier of an item in the hierarchy. Valid values are VSITEMID_NIL, VSITEMID_ROOT and VSITEMID_SELECTION.</param>
		/// <param name="isReloadable">A flag indicating that the project item is reloadable (1 for reloadable, 0 for non-reloadable).</param>
		/// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code. </returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Reloadable")]
		public virtual int IsItemReloadable(uint itemId, out int isReloadable)
		{
			isReloadable = 0;

			if(this.ProjectMgr == null || this.ProjectMgr.IsClosed)
			{
				return VSConstants.E_FAIL;
			}

			HierarchyNode n = this.ProjectMgr.NodeFromItemId(itemId);
			if(n != null)
			{
				isReloadable = (n.IsItemReloadable()) ? 1 : 0;
			}

			return VSConstants.S_OK;
		}

		/// <summary>
		/// Called to reload a project item. 
		/// </summary>
		/// <param name="itemId">Specifies itemid from VSITEMID.</param>
		/// <param name="reserved">Reserved.</param>
		/// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code. </returns>
		public virtual int ReloadItem(uint itemId, uint reserved)
		{
			#region precondition
			if(this.ProjectMgr == null || this.ProjectMgr.IsClosed)
			{
				return VSConstants.E_FAIL;
			}
			#endregion

			HierarchyNode n = this.ProjectMgr.NodeFromItemId(itemId);
			if(n != null)
			{
				n.ReloadItem(reserved);
			}

			return VSConstants.S_OK;
		}
		#endregion

		#region IOleCommandTarget methods
		/// <summary>
		/// CommandTarget.Exec is called for most major operations if they are NOT UI based. Otherwise IVSUInode::exec is called first
		/// </summary>
		public virtual int Exec(ref Guid guidCmdGroup, uint nCmdId, uint nCmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
		{
			return this.InternalExecCommand(guidCmdGroup, nCmdId, nCmdExecOpt, pvaIn, pvaOut, CommandOrigin.OleCommandTarget);
		}

		/// <summary>
		/// Queries the object for the command status
		/// </summary>
		/// <remarks>we only support one command at a time, i.e. the first member in the OLECMD array</remarks>
		public virtual int QueryStatus(ref Guid guidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
		{
			return this.QueryStatusSelection(guidCmdGroup, cCmds, prgCmds, pCmdText, CommandOrigin.OleCommandTarget);
		}
		#endregion

		#region IVsHierarchyDeleteHandler methods

		public virtual int DeleteItem(uint delItemOp, uint itemId)
		{
			if(itemId == VSConstants.VSITEMID_SELECTION)
			{
				return VSConstants.E_INVALIDARG;
			}

			HierarchyNode node = this.projectMgr.NodeFromItemId(itemId);
			if(node != null)
			{
				node.Remove((delItemOp & (uint)__VSDELETEITEMOPERATION.DELITEMOP_DeleteFromStorage) != 0);
				return VSConstants.S_OK;
			}

			return VSConstants.E_FAIL;
		}


		public virtual int QueryDeleteItem(uint delItemOp, uint itemId, out int candelete)
		{
			candelete = 0;
			if(itemId == VSConstants.VSITEMID_SELECTION)
			{
				return VSConstants.E_INVALIDARG;
			}

			if(this.ProjectMgr == null || this.ProjectMgr.IsClosed)
			{
				return VSConstants.E_FAIL;
			}

			// We ask the project what state it is. If he is a state that should not allow delete then we return.
			if(this.ProjectMgr.IsCurrentStateASuppressCommandsMode())
			{
				return VSConstants.S_OK;
			}

			HierarchyNode node = this.projectMgr.NodeFromItemId(itemId);

			if(node == null)
			{
				return VSConstants.E_FAIL;
			}

			// Ask the nodes if they can remove the item.
			bool canDeleteItem = node.CanDeleteItem((__VSDELETEITEMOPERATION)delItemOp);
			if(canDeleteItem)
			{
				candelete = 1;
			}

			return VSConstants.S_OK;
		}
		#endregion

		#region IVsHierarchyDropDataSource2 methods

		public virtual int GetDropInfo(out uint pdwOKEffects, out Microsoft.VisualStudio.OLE.Interop.IDataObject ppDataObject, out IDropSource ppDropSource)
		{
			pdwOKEffects = (uint)DropEffect.None;
			ppDataObject = null;
			ppDropSource = null;
			return VSConstants.E_NOTIMPL;
		}

		public virtual int OnDropNotify(int fDropped, uint dwEffects)
		{
			return VSConstants.E_NOTIMPL;
		}

		public virtual int OnBeforeDropNotify(Microsoft.VisualStudio.OLE.Interop.IDataObject pDataObject, uint dwEffect, out int fCancelDrop)
		{
			pDataObject = null;
			fCancelDrop = 0;
			return VSConstants.E_NOTIMPL;
		}
		#endregion

		#region IVsHierarchyDropDataTarget methods

		public virtual int DragEnter(Microsoft.VisualStudio.OLE.Interop.IDataObject pDataObject, uint grfKeyState, uint itemid, ref uint pdwEffect)
		{
			return VSConstants.E_NOTIMPL;
		}

		public virtual int DragLeave()
		{
			return VSConstants.E_NOTIMPL;
		}

		public virtual int DragOver(uint grfKeyState, uint itemid, ref uint pdwEffect)
		{
			return VSConstants.E_NOTIMPL;
		}

		public virtual int Drop(Microsoft.VisualStudio.OLE.Interop.IDataObject pDataObject, uint grfKeyState, uint itemid, ref uint pdwEffect)
		{
			return VSConstants.E_NOTIMPL;
		}
		#endregion

		#region helper methods
		internal HierarchyNode FindChild(string name)
		{
			if(String.IsNullOrEmpty(name))
			{
				return null;
			}

			HierarchyNode result;
			for(HierarchyNode child = this.firstChild; child != null; child = child.NextSibling)
			{
				if(!String.IsNullOrEmpty(child.VirtualNodeName) && String.Compare(child.VirtualNodeName, name, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return child;
				}
				// If it is a foldernode then it has a virtual name but we want to find folder nodes by the document moniker or url
				else if((String.IsNullOrEmpty(child.VirtualNodeName) || (child is FolderNode)) &&
						(NativeMethods.IsSamePath(child.GetMkDocument(), name) || NativeMethods.IsSamePath(child.Url, name)))
				{
					return child;
				}

				result = child.FindChild(name);
				if(result != null)
				{
					return result;
				}
			}
			return null;
		}

		/// <summary>
		/// Recursively find all nodes of type T
		/// </summary>
		/// <typeparam name="T">The type of hierachy node being serched for</typeparam>
		/// <param name="nodes">A list of nodes of type T</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal void FindNodesOfType<T>(List<T> nodes)
			where T : HierarchyNode
		{
			for(HierarchyNode n = this.FirstChild; n != null; n = n.NextSibling)
			{
				if(n is T)
				{
					T nodeAsT = (T)n;
					nodes.Add(nodeAsT);
				}

				n.FindNodesOfType<T>(nodes);
			}
		}

		/// <summary>
		/// Adds an item from a project refererence to target node.
		/// </summary>
		/// <param name="projectRef"></param>
		/// <param name="targetNode"></param>
		internal bool AddFileToNodeFromProjectReference(string projectRef, HierarchyNode targetNode)
		{
			if(String.IsNullOrEmpty(projectRef))
			{
				throw new ArgumentException(SR.GetString(SR.ParameterCannotBeNullOrEmpty, CultureInfo.CurrentUICulture), "projectRef");
			}

			if(targetNode == null)
			{
				throw new ArgumentNullException("targetNode");
			}

			IVsSolution solution = this.GetService(typeof(IVsSolution)) as IVsSolution;
			if(solution == null)
			{
				throw new InvalidOperationException();
			}

			uint itemidLoc;
			IVsHierarchy hierarchy;
			string str;
			VSUPDATEPROJREFREASON[] reason = new VSUPDATEPROJREFREASON[1];
			ErrorHandler.ThrowOnFailure(solution.GetItemOfProjref(projectRef, out hierarchy, out itemidLoc, out str, reason));
			if(hierarchy == null)
			{
				throw new InvalidOperationException();
			}

			// This will throw invalid cast exception if the hierrachy is not a project.
			IVsProject project = (IVsProject)hierarchy;

			string moniker;
			ErrorHandler.ThrowOnFailure(project.GetMkDocument(itemidLoc, out moniker));
			string[] files = new String[1] { moniker };
			VSADDRESULT[] vsaddresult = new VSADDRESULT[1];
			vsaddresult[0] = VSADDRESULT.ADDRESULT_Failure;
			int addResult = targetNode.ProjectMgr.AddItem(targetNode.ID, VSADDITEMOPERATION.VSADDITEMOP_OPENFILE, null, 0, files, IntPtr.Zero, vsaddresult);
			if(addResult != VSConstants.S_OK && addResult != VSConstants.S_FALSE && addResult != (int)OleConstants.OLECMDERR_E_CANCELED)
			{
				ErrorHandler.ThrowOnFailure(addResult);
				return false;
			}
			return (vsaddresult[0] == VSADDRESULT.ADDRESULT_Success);
		}

		internal void InstantiateItemsDraggedOrCutOrCopiedList()
		{
			this.itemsDraggedOrCutOrCopied = new List<HierarchyNode>();
		}
		#endregion
	}
}
