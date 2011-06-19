using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using tom;

using ISysServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using VSStd97CmdID = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;

namespace Cosmos.Cosmos_VS_Debug
{
    /// <summary>
    /// This control host the editor (an extended RichTextBox) and is responsible for
    /// handling the commands targeted to the editor as well as saving and loading
    /// the document. This control also implement the search and replace functionalities.
    /// </summary>

    ///////////////////////////////////////////////////////////////////////////////
    // Having an entry in the new file dialog.
    //
    // For our file type should appear under "General" in the new files dialog, we need the following:-
    //     - A .vsdir file in the same directory as NewFileItems.vsdir (generally under Common7\IDE\NewFileItems).
    //       In our case the file name is Editor.vsdir but we only require a file with .vsdir extension.
    //     - An empty test file in the same directory as NewFileItems.vsdir. In
    //       our case we chose MyExtFile.test. Note this file name appears in Editor.vsdir
    //       (see vsdir file format below)
    //     - Three text strings in our language specific resource. File Resources.resx :-
    //          - "Rich Text file" - this is shown next to our icon.
    //          - "A blank rich text file" - shown in the description window
    //             in the new file dialog.
    //          - "MyExtFile" - This is the base file name. New files will initially
    //             be named as MyExtFile1.test, MyExtFile2.test... etc.
    ///////////////////////////////////////////////////////////////////////////////
    // Editor.vsdir contents:-
    //    MyExtFile.test|{3085E1D6-A938-478e-BE49-3546C09A1AB1}|#106|80|#109|0|401|0|#107
    //
    // The fields in order are as follows:-
    //    - MyExtFile.test - our empty test file
    //    - {db16ff5e-400a-4cb7-9fde-cb3eab9d22d2} - our Editor package guid
    //    - #106 - the ID of "Rich Text file" in the resource
    //    - 80 - the display ordering priority
    //    - #109 - the ID of "A blank rich text file" in the resource
    //    - 0 - resource dll string (we don't use this)
    //    - 401 - the ID of our icon
    //    - 0 - various flags (we don't use this - se vsshell.idl)
    //    - #107 - the ID of "test"
    ///////////////////////////////////////////////////////////////////////////////

    //This is required for Find In files scenario to work properly. This provides a connection point 
    //to the event interface
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [ComSourceInterfaces(typeof(IVsTextViewEvents))]
    [ComVisible(true)]
    public sealed class EditorPane : Microsoft.VisualStudio.Shell.WindowPane,
                                IVsPersistDocData,  //to Enable persistence functionality for document data
                                IPersistFileFormat, //to enable the programmatic loading or saving of an object 
        //in a format specified by the user.
                                IVsFileChangeEvents,//to notify the client when file changes on disk
                                IVsDocDataFileChangeControl, //to Determine whether changes to files made outside 
        //of the editor should be ignored
                                IVsFileBackup,      //to support backup of files. Visual Studio File Recovery 
        //backs up all objects in the Running Document Table that 
        //support IVsFileBackup and have unsaved changes.
                                IVsStatusbarUser,   //support updating the status bar
                                IVsFindTarget,      //to implement find and replace capabilities within the editor
                                IVsTextImage,       //to support find and replace in a text image
                                IVsTextSpanSet,     //to support find and replace in a text image
                                IVsTextBuffer,      //needed for Find and Replace to work appropriately
                                IVsTextView,        //needed for Find and Replace to work appropriately
                                IVsCodeWindow,      //needed for Find and Replace to work appropriately
                                IVsTextLines,       //needed for Find and Replace to work appropriately
                                IExtensibleObject,  //so we can get the atuomation object
                                IEditor,  //the automation interface for Editor
                                IVsToolboxUser      //Sends notification about Toolbox items to the owner of these items
    {
        private const uint MyFormat = 0;
        private const string MyExtension = ".test";
        private static string[] fontSizeArray = { "8", "9", "10", "11", "12", "14", "16", "18",
                                                  "20", "22", "24", "26", "28", "36", "48", "72"};

        private class EditorProperties
        {
            private EditorPane editor;
            public EditorProperties(EditorPane Editor)
            {
                editor = Editor;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public string FileName
            {
                get { return editor.FileName; }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public bool DataChanged
            {
                get { return editor.DataChanged; }
            }
        }

        #region Fields
        private Cosmos_VS_DebugPackage myPackage;

        private string fileName = string.Empty;
        private bool isDirty;
        // Flag true when we are loading the file. It is used to avoid to change the isDirty flag
        // when the changes are related to the load operation.
        private bool loading;
        // This flag is true when we are asking the QueryEditQuerySave service if we can edit the
        // file. It is used to avoid to have more than one request queued.
        private bool gettingCheckoutStatus;
        private MyEditor editorControl;

        private Microsoft.VisualStudio.Shell.SelectionContainer selContainer;
        private ITrackSelection trackSel;
        private IVsFileChangeEx vsFileChangeEx;

        private Timer FileChangeTrigger = new Timer();

        private Timer FNFStatusbarTrigger = new Timer();

        private bool fileChangedTimerSet;
        private int ignoreFileChangeLevel;
        private bool backupObsolete = true;
        private uint vsFileChangeCookie;
        private string[] fontListArray;

        private object findState;
        private bool lockImage;
        private ArrayList textSpanArray = new ArrayList();
        private IVsTextImage spTextImage;

        private IExtensibleObjectSite extensibleObjectSite;

        #endregion

        #region "Window.Pane Overrides"
        /// <summary>
        /// Constructor that calls the Microsoft.VisualStudio.Shell.WindowPane constructor then
        /// our initialization functions.
        /// </summary>
        /// <param name="package">Our Package instance.</param>
        public EditorPane(Cosmos_VS_DebugPackage package)
            : base(null)
        {
            PrivateInit(package);
        }

        protected override void OnClose()
        {
            editorControl.StopRecorder();

            base.OnClose();
        }

        /// <summary>
        /// This is a required override from the Microsoft.VisualStudio.Shell.WindowPane class.
        /// It returns the extended rich text box that we host.
        /// </summary>
        public override IWin32Window Window
        {
            get
            {
                return this.editorControl;
            }
        }
        #endregion

        /// <summary>
        /// Initialization routine for the Editor. Loads the list of properties for the test document 
        /// which will show up in the properties window 
        /// </summary>
        /// <param name="package"></param>
        private void PrivateInit(Cosmos_VS_DebugPackage package)
        {
            myPackage = package;
            loading = false;
            gettingCheckoutStatus = false;
            trackSel = null;

            Control.CheckForIllegalCrossThreadCalls = false;
            // Create an ArrayList to store the objects that can be selected
            ArrayList listObjects = new ArrayList();

            // Create the object that will show the document's properties
            // on the properties window.
            EditorProperties prop = new EditorProperties(this);
            listObjects.Add(prop);

            // Create the SelectionContainer object.
            selContainer = new Microsoft.VisualStudio.Shell.SelectionContainer(true, false);
            selContainer.SelectableObjects = listObjects;
            selContainer.SelectedObjects = listObjects;

            // Create and initialize the editor

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorPane));
            this.editorControl = new MyEditor();

            resources.ApplyResources(this.editorControl, "editorControl", CultureInfo.CurrentUICulture);
            // Event handlers for macro recording.
            this.editorControl.RichTextBoxControl.TextChanged += new System.EventHandler(this.OnTextChange);
            this.editorControl.RichTextBoxControl.MouseDown += new MouseEventHandler(this.OnMouseClick);
            this.editorControl.RichTextBoxControl.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            this.editorControl.RichTextBoxControl.KeyDown += new KeyEventHandler(this.OnKeyDown);
            
            // Handle Focus event
            this.editorControl.RichTextBoxControl.GotFocus += new EventHandler(this.OnGotFocus);

            // Call the helper function that will do all of the command setup work
            setupCommands();
        }

        /// <summary>
        /// returns the name of the file currently loaded
        /// </summary>
        public string FileName
        {
            get { return fileName; }
        }

        /// <summary>
        /// returns whether the contents of file have changed since the last save
        /// </summary>
        public bool DataChanged
        {
            get { return isDirty; }
        }

        /// <summary>
        /// returns an instance of the ITrackSelection service object
        /// </summary>
        private ITrackSelection TrackSelection
        {
            get
            {
                if (trackSel == null)
                {
                    trackSel = (ITrackSelection)GetService(typeof(ITrackSelection));
                }
                return trackSel;
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // Dispose the timers
                    if (null != FileChangeTrigger)
                    {
                        FileChangeTrigger.Dispose();
                        FileChangeTrigger = null;
                    }
                    if (null != FNFStatusbarTrigger)
                    {
                        FNFStatusbarTrigger.Dispose();
                        FNFStatusbarTrigger = null;
                    }

                    SetFileChangeNotification(null, false);

                    if (editorControl != null)
                    {
                        editorControl.RichTextBoxControl.Dispose();
                        editorControl.Dispose();
                        editorControl = null;
                    }
                    if (FileChangeTrigger != null)
                    {
                        FileChangeTrigger.Dispose();
                        FileChangeTrigger = null;
                    }
                    if (extensibleObjectSite != null)
                    {
                        extensibleObjectSite.NotifyDelete(this);
                        extensibleObjectSite = null;
                    }
                    GC.SuppressFinalize(this);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Gets an instance of the RunningDocumentTable (RDT) service which manages the set of currently open 
        /// documents in the environment and then notifies the client that an open document has changed
        /// </summary>
        private void NotifyDocChanged()
        {
            // Make sure that we have a file name
            if (fileName.Length == 0)
                return;

            // Get a reference to the Running Document Table
            IVsRunningDocumentTable runningDocTable = (IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable));

            // Lock the document
            uint docCookie;
            IVsHierarchy hierarchy;
            uint itemID;
            IntPtr docData;
            int hr = runningDocTable.FindAndLockDocument(
                (uint)_VSRDTFLAGS.RDT_ReadLock,
                fileName,
                out hierarchy,
                out itemID,
                out docData,
                out docCookie
            );
            ErrorHandler.ThrowOnFailure(hr);

            // Send the notification
            hr = runningDocTable.NotifyDocumentChanged(docCookie, (uint)__VSRDTATTRIB.RDTA_DocDataReloaded);

            // Unlock the document.
            // Note that we have to unlock the document even if the previous call failed.
            ErrorHandler.ThrowOnFailure(runningDocTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, docCookie));

            // Check ff the call to NotifyDocChanged failed.
            ErrorHandler.ThrowOnFailure(hr);
        }

        /// <summary>
        /// This is an added command handler that will make it so the ITrackSelection.OnSelectChange
        /// function gets called whenever the cursor position is changed and also so the position 
        /// displayed on the status bar will update whenever the cursor position changes.
        /// </summary>
        /// <param name="sender"> Not used.</param>
        /// <param name="e"> Not used.</param>
        void OnSelectionChanged(object sender, EventArgs e)
        {
            // Call the function that will update the position displayed on the status bar.
            this.SetStatusBarPosition();

            // Now call the OnSelectChange function using our stored TrackSelection and
            // selContainer variables.
            ITrackSelection track = TrackSelection;
            if (null != track)
            {
                ErrorHandler.ThrowOnFailure(track.OnSelectChange((ISelectionContainer)selContainer));
            }
        }

        #region Command Handling Functions

        /// <summary>
        /// This helper function, which is called from the EditorPane's PrivateInit
        /// function, does all the work involving adding commands.
        /// </summary>
        private void setupCommands()
        {
            // Now get the IMenuCommandService; this object is the one
            // responsible for handling the collection of commands implemented by the package.

            IMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (null != mcs)
            {
                // Now create one object derived from MenuCommnad for each command defined in
                // the CTC file and add it to the command service.

                // For each command we have to define its id that is a unique Guid/integer pair, then
                // create the OleMenuCommand object for this command. The EventHandler object is the
                // function that will be called when the user will select the command. Then we add the 
                // OleMenuCommand to the menu service.  The addCommand helper function does all this for us.

                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.SelectAll,
                                new EventHandler(onSelectAll), null);
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Copy,
                                new EventHandler(onCopy), new EventHandler(onQueryCopy));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Cut,
                                new EventHandler(onCut), new EventHandler(onQueryCutOrDelete));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Paste,
                                new EventHandler(onPaste), new EventHandler(onQueryPaste));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Delete,
                                new EventHandler(onDelete), new EventHandler(onQueryCutOrDelete));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Undo,
                                new EventHandler(onUndo), new EventHandler(onQueryUndo));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Redo,
                                new EventHandler(onRedo), new EventHandler(onQueryRedo));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Bold,
                                new EventHandler(onBold), new EventHandler(onQueryBold));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Italic,
                                new EventHandler(onItalic), new EventHandler(onQueryItalic));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Underline,
                                new EventHandler(onUnderline), new EventHandler(onQueryUnderline));
                addCommand(mcs, GuidList.guidCosmos_VS_DebugCmdSet, (int)PkgCmdIDList.icmdStrike,
                                new EventHandler(onStrikethrough), new EventHandler(onQueryStrikethrough));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.JustifyCenter,
                                new EventHandler(onJustifyCenter), new EventHandler(onQueryJustifyCenter));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.JustifyLeft,
                                new EventHandler(onJustifyLeft), new EventHandler(onQueryJustifyLeft));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.JustifyRight,
                                new EventHandler(onJustifyRight), new EventHandler(onQueryJustifyRight));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.FontNameGetList,
                                new EventHandler(onFontNameGetList), null);
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.FontName,
                                new EventHandler(onFontName), null);
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.FontSizeGetList,
                                new EventHandler(onFontSizeGetList), null);
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.FontSize,
                                new EventHandler(onFontSize), null);
                addCommand(mcs, VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.BULLETEDLIST,
                                new EventHandler(onBulletedList), new EventHandler(onQueryBulletedList));
                // Support clipboard rings
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.PasteNextTBXCBItem,
                                new EventHandler(onPasteNextTBXCBItem), new EventHandler(onQueryPasteNextTBXCBItem));

                // These two commands enable Visual Studio's default undo/redo toolbar buttons.  When these
                // buttons are clicked it triggers a multi-level undo/redo (even when we are undoing/redoing
                // only one action.  Note that we are not implementing the multi-level undo/redo functionality,
                // we are just adding a handler for this command so these toolbar buttons are enabled (Note that
                // we are just reusing the undo/redo command handlers).  To implement multi-level functionality
                // we would need to properly handle these two commands as well as MultiLevelUndoList and
                // MultiLevelRedoList.
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.MultiLevelUndo,
                                new EventHandler(onUndo), new EventHandler(onQueryUndo));
                addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.MultiLevelRedo,
                                new EventHandler(onRedo), new EventHandler(onQueryRedo));
            }
        }

        /// <summary>
        /// Helper function used to add commands using IMenuCommandService
        /// </summary>
        /// <param name="mcs"> The IMenuCommandService interface.</param>
        /// <param name="menuGroup"> This guid represents the menu group of the command.</param>
        /// <param name="cmdID"> The command ID of the command.</param>
        /// <param name="commandEvent"> An EventHandler which will be called whenever the command is invoked.</param>
        /// <param name="queryEvent"> An EventHandler which will be called whenever we want to query the status of
        /// the command.  If null is passed in here then no EventHandler will be added.</param>
        private static void addCommand(IMenuCommandService mcs, Guid menuGroup, int cmdID,
                                       EventHandler commandEvent, EventHandler queryEvent)
        {
            // Create the OleMenuCommand from the menu group, command ID, and command event
            CommandID menuCommandID = new CommandID(menuGroup, cmdID);
            OleMenuCommand command = new OleMenuCommand(commandEvent, menuCommandID);

            // Add an event handler to BeforeQueryStatus if one was passed in
            if (null != queryEvent)
            {
                command.BeforeQueryStatus += queryEvent;
            }

            // Add the command using our IMenuCommandService instance
            mcs.AddCommand(command);
        }

        /// <summary>
        /// Handler for out SelectAll command.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onSelectAll(object sender, EventArgs e)
        {
            editorControl.RichTextBoxControl.SelectAll();
        }

        /// <summary>
        /// Handler for when we want to query the status of the copy command.  If there
        /// is any text selected then it will set the Enabled property to true.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryCopy(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Enabled = editorControl.RichTextBoxControl.SelectionLength > 0 ? true : false;
        }

        /// <summary>
        /// Handler for our Copy command.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onCopy(object sender, EventArgs e)
        {
            Copy();
            editorControl.RecordCommand("Copy");
        }

        /// <summary>
        /// Handler for when we want to query the status of the cut or delete
        /// commands.  If there is any selected text then it will set the 
        /// enabled property to true.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryCutOrDelete(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Enabled = editorControl.RichTextBoxControl.SelectionLength > 0 ? true : false;
        }

        /// <summary>
        /// Handler for our Cut command.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onCut(object sender, EventArgs e)
        {
            Cut();
            editorControl.RecordCommand("Cut");
        }

        /// <summary>
        /// Handler for our Delete command.
        /// </summary>
        private void onDelete(object sender, EventArgs e)
        {
            editorControl.RichTextBoxControl.SelectedText = "";
            editorControl.RecordCommand("Delete");
        }

        /// <summary>
        /// Handler for when we want to query the status of the paste command.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryPaste(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Enabled = editorControl.RichTextBoxControl.CanPaste(DataFormats.GetFormat(DataFormats.Text));
        }

        /// <summary>
        /// Handler for our Paste command.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onPaste(object sender, EventArgs e)
        {
            Paste();
            editorControl.RecordCommand("Paste");
        }

        /// <summary>
        /// Handler for when we want to query the status of the clipboard ring.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryPasteNextTBXCBItem(object sender, EventArgs e)
        {
            // Get the Toolbox Service from the package
            IVsToolboxClipboardCycler clipboardCycler = GetService(typeof(SVsToolbox)) as IVsToolboxClipboardCycler;

            int itemsAvailable;
            ErrorHandler.ThrowOnFailure(clipboardCycler.AreDataObjectsAvailable((IVsToolboxUser)this, out itemsAvailable));

            OleMenuCommand command = (OleMenuCommand)sender;
            command.Enabled = ((itemsAvailable > 0) ? true : false);
        }

        /// <summary>
        /// Handler for our Paste command.
        /// </summary>
        /// <param name="sender">  Not used.</param>
        /// <param name="e">  Not used.</param>
        private void onPasteNextTBXCBItem(object sender, EventArgs e)
        {
            // Get the Toolbox Service from the package
            IVsToolboxClipboardCycler clipboardCycler = GetService(typeof(SVsToolbox)) as IVsToolboxClipboardCycler;

            Microsoft.VisualStudio.OLE.Interop.IDataObject pDO;

            ErrorHandler.ThrowOnFailure(clipboardCycler.GetAndSelectNextDataObject((IVsToolboxUser)this, out pDO));

            ITextSelection textSelection = editorControl.TextDocument.Selection;

            // Get the current position of the start of the current selection. 
            // After the paste the positiono of the start of current selection
            // will be moved to the end of inserted text, so it needs to
            // move back to original position so that inserted text can be highlighted to 
            // allow cycling through our clipboard items.
            int originalStart;
            originalStart = textSelection.Start;

            // This will do the actual pasting of the object
            ItemPicked(pDO);

            // Now move the start position backwards to the original position.
            int currentStart;
            currentStart = textSelection.Start;
            textSelection.MoveStart((int)tom.tomConstants.tomCharacter, originalStart - currentStart);

            // Select the pasted text
            textSelection.Select();
        }

        /// <summary>
        /// Handler for when we want to query the status of the Undo command.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryUndo(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Enabled = editorControl.RichTextBoxControl.CanUndo;
        }

        /// <summary>
        /// Handler for our Undo command.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onUndo(object sender, EventArgs e)
        {
            editorControl.RichTextBoxControl.Undo();
        }

        /// <summary>
        /// Handler for when we want to query the status of the Redo command.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryRedo(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Enabled = editorControl.RichTextBoxControl.CanRedo;
        }

        /// <summary>
        /// Handler for our Redo command.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onRedo(object sender, EventArgs e)
        {
            editorControl.RichTextBoxControl.Redo();
        }

        /// <summary>
        /// Handler for when we want to query the status of the Bold command.  It will
        /// always be enabled, but we want to check if the current text is bold or not
        /// so we can set the Checked property which will change how the button looks
        /// in the toolbar and the context menu.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryBold(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Checked = editorControl.RichTextBoxControl.SelectionFont.Bold;
        }

        /// <summary>
        /// Handler for our Bold command.  Toggles the bold state of the selected text.
        /// Or if there is no selected text then it toggles the bold state for 
        /// newly entered text.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onBold(object sender, EventArgs e)
        {
            setFontStyle(FontStyle.Bold, editorControl.RichTextBoxControl.SelectionFont.Bold);
        }

        /// <summary>
        /// Handler for when we want to query the status of the Italic command.  It will
        /// always be enabled, but we want to check if the current text is Italic or not
        /// so we can set the Checked property which will change how the button looks
        /// in the toolbar and the context menu.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryItalic(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Checked = editorControl.RichTextBoxControl.SelectionFont.Italic;
        }

        /// <summary>
        /// Handler for our Italic command.  Toggles the italic state of the selected text.
        /// Or if there is no selected text then it toggles the italic state for 
        /// newly entered text.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onItalic(object sender, EventArgs e)
        {
            setFontStyle(FontStyle.Italic, editorControl.RichTextBoxControl.SelectionFont.Italic);
        }

        /// <summary>
        /// Handler for when we want to query the status of the Underline command.  It will
        /// always be enabled, but we want to check if the current text is underlined or not
        /// so we can set the Checked property which will change how the button looks
        /// in the toolbar and the context menu.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryUnderline(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Checked = editorControl.RichTextBoxControl.SelectionFont.Underline;
        }

        /// <summary>
        /// Handler for our Underline command.  Toggles the underline state of the selected
        /// text.  Or if there is no selected text then it toggles the underline state for 
        /// newly entered text.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onUnderline(object sender, EventArgs e)
        {
            setFontStyle(FontStyle.Underline, editorControl.RichTextBoxControl.SelectionFont.Underline);
        }

        /// <summary>
        /// Handler for when we want to query the status of the Strikethrough command.  It will
        /// always be enabled, but we want to check if the current text has Strikethrough or not
        /// so we can set the Checked property which will change how the button looks
        /// in the toolbar and the context menu.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryStrikethrough(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Checked = editorControl.RichTextBoxControl.SelectionFont.Strikeout;
        }

        /// <summary>
        /// Handler for our Strikethrough command.  Toggles the strikethrough state of 
        /// the selected text.  Or if there is no selected text then it toggles the 
        /// strikethrough state for newly entered text.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onStrikethrough(object sender, EventArgs e)
        {
            setFontStyle(FontStyle.Strikeout, editorControl.RichTextBoxControl.SelectionFont.Strikeout);
        }

        /// <summary>
        /// This helper function is called when we need to toggle the states bold,
        /// underline, italic or strikeout.
        /// </summary>
        /// <param name="fontStyleToSet"> Which FontStyle to toggle (bold, italic, underline or strikeout).</param>
        /// <param name="currentStateOn"> The current state of the font style.  If this is true then we
        /// will turn the font style off and if it is false we will turn it on.</param>
        private void setFontStyle(FontStyle fontStyleToSet, bool currentStateOn)
        {
            // Figure out what the new FontStyle should be based on the current one
            FontStyle fs = currentStateOn ? editorControl.RichTextBoxControl.SelectionFont.Style & (~fontStyleToSet) :
                                            editorControl.RichTextBoxControl.SelectionFont.Style | fontStyleToSet;

            // Create the new Font based on the current one and fs then set it
            Font f = new Font(editorControl.RichTextBoxControl.SelectionFont, fs);
            editorControl.RichTextBoxControl.SelectionFont = f;

            if (f != null)
                f.Dispose();
        }

        /// <summary>
        /// Handler for when we want to query the status of the justify center command.  It will
        /// always be enabled, but we want to check if the current text is center-justified or not
        /// so we can set the Checked property which will change how the button looks in the toolbar.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryJustifyCenter(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Checked = (editorControl.RichTextBoxControl.SelectionAlignment == HorizontalAlignment.Center);
        }

        /// <summary>
        /// Handler for our Justify Center command.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onJustifyCenter(object sender, EventArgs e)
        {
            editorControl.RichTextBoxControl.SelectionAlignment = HorizontalAlignment.Center;
        }

        /// <summary>
        /// Handler for when we want to query the status of the justify left command.  It will
        /// always be enabled, but we want to check if the current text is left-justified or not
        /// so we can set the Checked property which will change how the button looks in the toolbar.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryJustifyLeft(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Checked = (editorControl.RichTextBoxControl.SelectionAlignment == HorizontalAlignment.Left);
        }

        /// <summary>
        /// Handler for our Justify Left command.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onJustifyLeft(object sender, EventArgs e)
        {
            editorControl.RichTextBoxControl.SelectionAlignment = HorizontalAlignment.Left;
        }

        /// <summary>
        /// Handler for when we want to query the status of the justify right command.  It will
        /// always be enabled, but we want to check if the current text is right-justified or not
        /// so we can set the Checked property which will change how the button looks in the toolbar.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryJustifyRight(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Checked = (editorControl.RichTextBoxControl.SelectionAlignment == HorizontalAlignment.Right);
        }

        /// <summary>
        /// Handler for our Justify Right command.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onJustifyRight(object sender, EventArgs e)
        {
            editorControl.RichTextBoxControl.SelectionAlignment = HorizontalAlignment.Right;
        }

        /// <summary>
        /// Helper function that fills the fontList array (of strings) with
        /// all the available fonts.
        /// </summary>
        private void fillFontList()
        {
            FontFamily[] fontFamilies;

            System.Drawing.Text.InstalledFontCollection installedFontCollection = new System.Drawing.Text.InstalledFontCollection();

            // Get the array of FontFamily objects.
            fontFamilies = installedFontCollection.Families;

            // Create the font list array and fill it with the list of available fonts.
            fontListArray = new string[fontFamilies.Length];
            for (int i = 0; i < fontFamilies.Length; ++i)
            {
                fontListArray[i] = fontFamilies[i].Name;
            }
        }

        /// <summary>
        /// This function is called when the drop down that lists the possible
        /// fonts is clicked.  It is responsible for populating the list of fonts
        /// with strings.  The fillFontList function is responsible for getting the
        /// list of possible fonts and will be called from here the first time
        /// this function is called.  Note that we use the EventArgs parameter to
        /// pass back the list after casting it to an OleMenuCmdEventArgs object.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  We will cast this to an OleMenuCommandEventArgs
        /// object and then use it to pass back the array of strings.</param>
        private void onFontNameGetList(object sender, EventArgs e)
        {
            // If this is the first time we are calling this function then
            // we need to set up the fontListArray
            if (this.fontListArray == null)
            {
                fillFontList();
            }

            // Cast the EventArgs to an OleMenuCmdEventArgs object
            OleMenuCmdEventArgs args = (OleMenuCmdEventArgs)e;

            // Set the out value of the OleMenuCmdEventArgs to our font list array
            Marshal.GetNativeVariantForObject(fontListArray, args.OutValue);
        }

        /// <summary>
        /// This function will be called for two seperate reasons.  It will be called constantly
        /// to figure out what string needs to be displayed in the font name combo box.  In this
        /// case we need to cast the EventArgs to OleMenuCmdEventArgs and set the OutValue to
        /// the name of the currently used font.  It will also be called when the user selects a new
        /// font.  In this case we need to cast EventArgs to OleMenuCmdEventArgs so that we can get the
        /// name of the new font from InValue and set it for our hosted text editor.
        /// </summary>
        /// <param name="sender"> This can be cast to an OleMenuCommand.</param>
        /// <param name="e"> We will cast this to an OleMenuCommandEventArgs and use it in
        /// two ways.  If we are setting a new font we will get its name by casting the
        /// InValue to a string.  Otherwise we will just set the OutValue to the name
        /// of the current font.</param>
        private void onFontName(object sender, EventArgs e)
        {
            OleMenuCmdEventArgs args = (OleMenuCmdEventArgs)e;

            // If args.InValue is null then we just need to set the OutValue
            // to the current font.  If it is not null then that means that we
            // need to cast it to a string and set it as the font.
            if (null == args.InValue)
            {
                string currentFont = editorControl.RichTextBoxControl.SelectionFont.FontFamily.Name;
                Marshal.GetNativeVariantForObject(currentFont, args.OutValue);
            }
            else
            {
                string fontName = (string)args.InValue;
                Font f = new Font(fontName, editorControl.RichTextBoxControl.SelectionFont.Size, editorControl.RichTextBoxControl.SelectionFont.Style);
                editorControl.RichTextBoxControl.SelectionFont = f;

                if (f != null)
                    f.Dispose();
            }
        }

        /// <summary>
        /// This function is called when the drop down that lists the possible
        /// font sizes is clicked.  It is responsible for populating the list
        /// with strings.  The static string array fontSizeArray is filled with the most
        /// commonly used font sizes, although the user can enter any number they want. 
        /// Note that we use the EventArgs parameter to pass back the list after
        /// casting it to an OleMenuCmdEventArgs object.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  We will cast this to an OleMenuCommandEventArgs
        /// object and then use it to pass back the array of strings.</param>
        private void onFontSizeGetList(object sender, EventArgs e)
        {
            // Cast the EventArgs to an OleMenuCmdEventArgs object
            OleMenuCmdEventArgs args = (OleMenuCmdEventArgs)e;

            // Set the out value of the OleMenuCmdEventArgs to our font size array
            Marshal.GetNativeVariantForObject(fontSizeArray, args.OutValue);
        }

        /// <summary>
        /// This function will be called for two seperate reasons.  It will be called constantly
        /// to figure out what string needs to be displayed in the font size combo box.  In this
        /// case we need to cast the EventArgs to OleMenuCmdEventArgs and set the OutValue to
        /// the current font size.  It will also be called when the user changes the font size.
        /// In this case we need to cast EventArgs to OleMenuCmdEventArgs so that we can get the
        /// new font size and set it for our hosted text editor.
        /// </summary>
        /// <param name="sender"> This can be cast to an OleMenuCommand.</param>
        /// <param name="e"> We will cast this to an OleMenuCommandEventArgs and use it in
        /// two ways.  If we are setting a new font sizse we will get its name by casting the
        /// InValue to a string.  Otherwise we will just set the OutValue to the current 
        /// font size.</param>
        private void onFontSize(object sender, EventArgs e)
        {
            OleMenuCmdEventArgs args = (OleMenuCmdEventArgs)e;

            // If args.InValue is null then we just need to set the OutValue
            // to the current font size.  If it is not null then that means that we
            // need to cast it to a string and set it as the new font size.
            if (null == args.InValue)
            {
                string currentSize = Convert.ToString(Convert.ToInt32(editorControl.RichTextBoxControl.SelectionFont.Size), CultureInfo.InvariantCulture);
                Marshal.GetNativeVariantForObject(currentSize, args.OutValue);
            }
            else
            {
                string fontSize = (string)args.InValue;
                Font f = new Font(editorControl.RichTextBoxControl.SelectionFont.FontFamily, Convert.ToSingle(fontSize, CultureInfo.InvariantCulture), editorControl.RichTextBoxControl.SelectionFont.Style);
                editorControl.RichTextBoxControl.SelectionFont = f;

                if (f != null)
                    f.Dispose();
            }
        }

        /// <summary>
        /// Handler for when we want to query the status of the justify right command.  It will
        /// always be enabled, but we want to check if this is active in the current text so
        /// we can change the look of the command in the toolbar and context menu.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryBulletedList(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Checked = editorControl.RichTextBoxControl.SelectionBullet;
        }

        /// <summary>
        /// Handler for our Bulleted List command.  This simply toggles the state
        /// of the SelectionBullet property.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onBulletedList(object sender, EventArgs e)
        {
            editorControl.RichTextBoxControl.SelectionBullet = !editorControl.RichTextBoxControl.SelectionBullet;
        }

        /// <summary>
        /// This is an extra command handler that we will use to intercept right
        /// mouse click events so that we can call our function to display the
        /// context menu.
        /// </summary>
        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point mouseDownLocation = new Point(e.X, e.Y);

                // Convert the point to screen coordinates and pass it into
                // our DisplayContextMenuAt function
                Point screenCoordinates = this.editorControl.RichTextBoxControl.PointToScreen(mouseDownLocation);
                DisplayContextMenuAt(screenCoordinates);
            }
        }

        /// <summary>
        /// Function that we use to display our context menu.  This function
        /// makes use of the IMenuCommandService's ShowContextMenu function.
        /// </summary>
        /// <param name="point"> The point that we want to display the context menu at.
        /// Note that this must be in screen coordinates.</param>
        private void DisplayContextMenuAt(Point point)
        {
            // Pass in the GUID:ID pair for the context menu.
            CommandID contextMenuID = new CommandID(GuidList.guidCosmos_VS_DebugCmdSet, PkgCmdIDList.IDMX_RTF);

            // Get the OleMenuCommandService from the package
            IMenuCommandService menuService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;

            if (null != menuService)
            {
                // Note: point must be in screen coordinates
                menuService.ShowContextMenu(contextMenuID, point.X, point.Y);
            }
        }

        #endregion

        #region IEditor Implementation

        // Note that all functions implemented here call functions from the rich
        // edit control's text object model.

        /// <summary>
        /// This property gets/sets the default tab width.
        /// </summary>
        public float DefaultTabStop
        {
            get { return editorControl.TextDocument.DefaultTabStop; }
            set { editorControl.TextDocument.DefaultTabStop = value; }
        }

        /// <summary>
        /// This property gets our editor's current ITextRange interface.  ITextRange is part
        /// of the rich edit control's text object model.
        /// </summary>
        public ITextRange Range
        {
            get { return editorControl.TextRange; }
        }

        /// <summary>
        /// This property gets our editor's current ITextSelection interface.  ITextSelection
        /// is part of the rich edit control's text object model.
        /// </summary>
        public ITextSelection Selection
        {
            get { return editorControl.TextSelection; }
        }

        /// <summary>
        /// This property gets/sets the selection properties that contain certain information
        /// about our editor's current selection.
        /// </summary>
        public int SelectionProperties
        {
            get { return editorControl.TextSelection.Flags; }
            set { editorControl.TextSelection.Flags = value; }
        }

        /// <summary>
        /// This function finds a string and returns the length of the matched string.
        /// Note that this function does not move the cursor to the string that it finds.
        /// </summary>
        /// <param name="textToFind"> The string that we want to look for.</param>
        /// <returns> The length of the matched string.</returns>
        public int FindText(string textToFind)
        {
            return editorControl.TextRange.FindText(textToFind, (int)tom.tomConstants.tomForward, 0);
        }

        /// <summary>
        /// This function has the same effect as typing the passed in string into the editor.
        /// Our implementation will just call TypeText since for now we want them both to do
        /// the same thing.
        /// </summary>
        /// <param name="textToSet"> The string to set/</param>
        /// <returns> HResult that indicates success/failure.</returns>
        public int SetText(string textToSet)
        {
            // Just delegate to TypeText
            return TypeText(textToSet);
        }

        /// <summary>
        /// This function has the same effect as typing the passed in string into the editor.
        /// </summary>
        /// <param name="textToType"> The string to type.</param>
        /// <returns> HResult that indicates success/failure.</returns>
        public int TypeText(string textToType)
        {
            editorControl.TextSelection.TypeText(textToType);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This function performs the cut operation in the editor.
        /// </summary>
        /// <returns> HResult that indicates success/failure.</returns>
        public int Cut()
        {
            object o = null;
            editorControl.TextSelection.Cut(out o);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This function performs the copy operation in the editor.
        /// </summary>
        /// <returns> HResult that indicates success/failure.</returns>
        public int Copy()
        {
            object o = null;
            editorControl.TextSelection.Copy(out o);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This function performs the paste operation in the editor.
        /// </summary>
        /// <returns> HResult that indicates success/failure.</returns>
        public int Paste()
        {
            object o = null;
            editorControl.TextSelection.Paste(ref o, 0);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This function performs a delete in the editor.
        /// </summary>
        /// <param name="unit"> The type of units that we are going to delete.  The two valid options
        /// for this are TOMWord and TOMCharacter, which are defined in the TOMConstants enumeration.</param>
        /// <param name="count"> The number of units that we are going to delete.  Passing in a negative number
        /// will be similar to pressing backspace and passing in a positive number will be similar to
        /// pressing delete.</param>
        /// <returns> HResult that indicates success/failure.</returns>
        public int Delete(long unit, long count)
        {
            editorControl.TextSelection.Delete((int)unit, (int)count);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This function will move up by the specified number of lines/paragraphs in the editor.
        /// </summary>
        /// <param name="unit"> The type of unit to move up by.  The two valid options for this are
        /// TOMLine and TOMParagraph, which are defined in the TOMConstants enumeration.</param>
        /// <param name="count"> The number of units to move.</param>
        /// <param name="extend"> This should be set to TOMExtend if we want to select as we move
        /// or TOMMove if we don't.  These values are defined in the TOMConstants enumeration.</param>
        /// <returns> The number of units that the cursor moved up.</returns>
        public int MoveUp(int unit, int count, int extend)
        {
            return editorControl.TextSelection.MoveUp(unit, count, extend);
        }

        /// <summary>
        /// This function will move down by the specified number of lines/paragraphs in the editor.
        /// </summary>
        /// <param name="unit"> The type of unit to move down by.  The two valid options for this are
        /// TOMLine and TOMParagraph, which are defined in the TOMConstants enumeration.</param>
        /// <param name="count"> The number of units to move.</param>
        /// <param name="extend"> This should be set to TOMExtend if we want to select as we move
        /// or TOMMove if we don't.  These values are defined in the TOMConstants enumeration.</param>
        /// <returns> The number of units that the cursor moved down.</returns>
        public int MoveDown(int unit, int count, int extend)
        {
            return editorControl.TextSelection.MoveDown(unit, count, extend);
        }

        /// <summary>
        /// This function will move to the left by the specified number of characters/words in the editor.
        /// </summary>
        /// <param name="unit"> The type of unit to move left by.  The two valid options for this are
        /// TOMWord and TOMCharacter, which are defined in the TOMConstants enumeration.</param>
        /// <param name="count"> The number of units to move.</param>
        /// <param name="extend"> This should be set to TOMExtend if we want to select as we move
        /// or TOMMove if we don't.  These values are defined in the TOMConstants enumeration.</param>
        /// <returns> The number of units that the cursor moved to the left.</returns>
        public int MoveLeft(int unit, int count, int extend)
        {
            return editorControl.TextSelection.MoveLeft(unit, count, extend);
        }

        /// <summary>
        /// This function will move to the right by the specified number of characters/words in the editor.
        /// </summary>
        /// <param name="unit"> The type of unit to move right by.  The two valid options for this are
        /// TOMWord and TOMCharacter, which are defined in the TOMConstants enumeration.</param>
        /// <param name="count"> The number of units to move.</param>
        /// <param name="extend"> This should be set to TOMExtend if we want to select as we move
        /// or TOMMove if we don't.  These values are defined in the TOMConstants enumeration.</param>
        /// <returns> The number of units that the cursor moved to the right.</returns>
        public int MoveRight(int unit, int count, int extend)
        {
            return editorControl.TextSelection.MoveRight(unit, count, extend);
        }

        /// <summary>
        /// This function will either move the cursor to either the end of the current line or the end of the document.
        /// </summary>
        /// <param name="unit"> If this value is equal to TOMLine it will move the cursor to the end of the line.  If
        /// it is set to TOMStory then it will move to the end of the document.  These values are defined in the
        /// TOMConstants enumeration.</param>
        /// <param name="extend"> This should be set to TOMExtend if we want to select as we move
        /// or TOMMove if we don't.  These values are defined in the TOMConstants enumeration.</param>
        /// <returns> The number of characters that the operation moved the cursor by.  This value
        /// should always be positive since we are moving "forward" in the text buffer.</returns>
        public int EndKey(int unit, int extend)
        {
            return editorControl.TextSelection.EndKey(unit, extend);
        }

        /// <summary>
        /// This function will either move the cursor to either the beggining of the current line or
        /// the beggining of the document.
        /// </summary>
        /// <param name="unit"> If this value is equal to TOMLine it will move the cursor to the beggining of the line.
        /// If it is set to TOMStory then it will move to the beggining of the document.  These values are defined in the
        /// TOMConstants enumeration.</param>
        /// <param name="extend"> This should be set to TOMExtend if we want to select as we move
        /// or TOMMove if we don't.  These values are defined in the TOMConstants enumeration.</param>
        /// <returns> The number of characters that the operation moved the cursor by.  This value
        /// should always be negative since we are moving "backward" in the text buffer.</returns>
        public int HomeKey(int unit, int extend)
        {
            return editorControl.TextSelection.HomeKey(unit, extend);
        }

        #endregion

        #region IExtensibleObject Implementation

        /// <summary>
        /// This function is used for Macro playback.  Whenever a macro gets played this funtion will be
        /// called and then the IEditor functions will be called on the object that ppDisp is set to.
        /// Since EditorPane implements IEditor we will just set it to "this".
        /// </summary>
        /// <param name="Name"> Passing in either null, empty string or "Document" will work.  Anything
        /// else will result in ppDisp being set to null.</param>
        /// <param name="pParent"> An object of type IExtensibleObjectSite.  We will keep a reference to this
        /// so that in the Dispose method we can call the NotifyDelete function.</param>
        /// <param name="ppDisp"> The object that this is set to will act as the automation object for macro
        /// playback.  In our case since IEditor is the automation interface and EditorPane
        /// implements it we will just be setting this parameter to "this".</param>
        void IExtensibleObject.GetAutomationObject(string Name, IExtensibleObjectSite pParent, out Object ppDisp)
        {
            // null or empty string just means the default object, but if a specific string
            // is specified, then make sure it's the correct one, but don't enforce case
            if (!string.IsNullOrEmpty(Name) && !Name.Equals("Document", StringComparison.CurrentCultureIgnoreCase))
            {
                ppDisp = null;
                return;
            }

            // Set the out value to this
            ppDisp = (IEditor)this;

            // Store the IExtensibleObjectSite object, it will be used in the Dispose method
            extensibleObjectSite = pParent;
        }

        #endregion

        int Microsoft.VisualStudio.OLE.Interop.IPersist.GetClassID(out Guid pClassID)
        {
            ErrorHandler.ThrowOnFailure(((Microsoft.VisualStudio.OLE.Interop.IPersist)this).GetClassID(out pClassID));
            return VSConstants.S_OK;
        }

        #region IPersistFileFormat Members

        /// <summary>
        /// Notifies the object that it has concluded the Save transaction
        /// </summary>
        /// <param name="pszFilename">Pointer to the file name</param>
        /// <returns>S_OK if the funtion succeeds</returns>
        int IPersistFileFormat.SaveCompleted(string pszFilename)
        {
            // TODO:  Add Editor.SaveCompleted implementation
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Returns the path to the object's current working file 
        /// </summary>
        /// <param name="ppszFilename">Pointer to the file name</param>
        /// <param name="pnFormatIndex">Value that indicates the current format of the file as a zero based index
        /// into the list of formats. Since we support only a single format, we need to return zero. 
        /// Subsequently, we will return a single element in the format list through a call to GetFormatList.</param>
        /// <returns></returns>
        int IPersistFileFormat.GetCurFile(out string ppszFilename, out uint pnFormatIndex)
        {
            // We only support 1 format so return its index
            pnFormatIndex = MyFormat;
            ppszFilename = fileName;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Initialization for the object 
        /// </summary>
        /// <param name="nFormatIndex">Zero based index into the list of formats that indicates the current format 
        /// of the file</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.InitNew(uint nFormatIndex)
        {
            if (nFormatIndex != MyFormat)
            {
                return VSConstants.E_INVALIDARG;
            }
            // until someone change the file, we can consider it not dirty as
            // the user would be annoyed if we prompt him to save an empty file
            isDirty = false;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Returns the class identifier of the editor type
        /// </summary>
        /// <param name="pClassID">pointer to the class identifier</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.GetClassID(out Guid pClassID)
        {
            ErrorHandler.ThrowOnFailure(((Microsoft.VisualStudio.OLE.Interop.IPersist)this).GetClassID(out pClassID));
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Provides the caller with the information necessary to open the standard common "Save As" dialog box. 
        /// This returns an enumeration of supported formats, from which the caller selects the appropriate format. 
        /// Each string for the format is terminated with a newline (\n) character. 
        /// The last string in the buffer must be terminated with the newline character as well. 
        /// The first string in each pair is a display string that describes the filter, such as "Text Only 
        /// (*.txt)". The second string specifies the filter pattern, such as "*.txt". To specify multiple filter 
        /// patterns for a single display string, use a semicolon to separate the patterns: "*.htm;*.html;*.asp". 
        /// A pattern string can be a combination of valid file name characters and the asterisk (*) wildcard character. 
        /// Do not include spaces in the pattern string. The following string is an example of a file pattern string: 
        /// "HTML File (*.htm; *.html; *.asp)\n*.htm;*.html;*.asp\nText File (*.txt)\n*.txt\n."
        /// </summary>
        /// <param name="ppszFormatList">Pointer to a string that contains pairs of format filter strings</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.GetFormatList(out string ppszFormatList)
        {
            char Endline = (char)'\n';
            string FormatList = string.Format(CultureInfo.InvariantCulture, "My Editor (*{0}){1}*{0}{1}{1}", MyExtension, Endline);
            ppszFormatList = FormatList;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Loads the file content into the textbox
        /// </summary>
        /// <param name="pszFilename">Pointer to the full path name of the file to load</param>
        /// <param name="grfMode">file format mode</param>
        /// <param name="fReadOnly">determines if teh file should be opened as read only</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.Load(string pszFilename, uint grfMode, int fReadOnly)
        {
            if (pszFilename == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            loading = true;
            int hr = VSConstants.S_OK;
            try
            {
                // Show the wait cursor while loading the file
                IVsUIShell VsUiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                if (VsUiShell != null)
                {
                    // Note: we don't want to throw or exit if this call fails, so
                    // don't check the return code.
                    hr = VsUiShell.SetWaitCursor();
                }

                // Load the file
                StreamReader str = new StreamReader(pszFilename);
                string rtfSignature = "{\\rtf";
                string lineRead = null;
                try
                {
                    lineRead = str.ReadLine();
                }
                finally
                {
                    str.Close();
                }
                if (lineRead.Contains(rtfSignature))
                {
                    //try loading with Rich Text initially
                    editorControl.RichTextBoxControl.LoadFile(pszFilename, RichTextBoxStreamType.RichText);
                }
                else
                {
                    editorControl.RichTextBoxControl.LoadFile(pszFilename, RichTextBoxStreamType.PlainText);
                }

                isDirty = false;

                //Determine if the file is read only on the file system
                FileAttributes fileAttrs = File.GetAttributes(pszFilename);

                int isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;

                //Set readonly if either the file is readonly for the user or on the file system
                if (0 == isReadOnly && 0 == fReadOnly)
                    SetReadOnly(false);
                else
                    SetReadOnly(true);


                // Notify to the property window that some of the selected objects are changed
                ITrackSelection track = TrackSelection;
                if (null != track)
                {
                    hr = track.OnSelectChange((ISelectionContainer)selContainer);
                    if (ErrorHandler.Failed(hr))
                        return hr;
                }

                // Hook up to file change notifications
                if (String.IsNullOrEmpty(fileName) || 0 != String.Compare(fileName, pszFilename, true, CultureInfo.CurrentCulture))
                {
                    fileName = pszFilename;
                    SetFileChangeNotification(pszFilename, true);

                    // Notify the load or reload
                    NotifyDocChanged();
                }
            }
            finally
            {
                loading = false;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines whether an object has changed since being saved to its current file
        /// </summary>
        /// <param name="pfIsDirty">true if the document has changed</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.IsDirty(out int pfIsDirty)
        {
            if (isDirty)
            {
                pfIsDirty = 1;
            }
            else
            {
                pfIsDirty = 0;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Save the contents of the textbox into the specified file. If doing the save on the same file, we need to
        /// suspend notifications for file changes during the save operation.
        /// </summary>
        /// <param name="pszFilename">Pointer to the file name. If the pszFilename parameter is a null reference 
        /// we need to save using the current file
        /// </param>
        /// <param name="remember">Boolean value that indicates whether the pszFileName parameter is to be used 
        /// as the current working file.
        /// If remember != 0, pszFileName needs to be made the current file and the dirty flag needs to be cleared after the save.
        ///                   Also, file notifications need to be enabled for the new file and disabled for the old file 
        /// If remember == 0, this save operation is a Save a Copy As operation. In this case, 
        ///                   the current file is unchanged and dirty flag is not cleared
        /// </param>
        /// <param name="nFormatIndex">Zero based index into the list of formats that indicates the format in which 
        /// the file will be saved</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.Save(string pszFilename, int fRemember, uint nFormatIndex)
        {
            int hr = VSConstants.S_OK;
            bool doingSaveOnSameFile = false;
            // If file is null or same --> SAVE
            if (pszFilename == null || pszFilename == fileName)
            {
                fRemember = 1;
                doingSaveOnSameFile = true;
            }

            //Suspend file change notifications for only Save since we don't have notifications setup
            //for SaveAs and SaveCopyAs (as they are different files)
            if (doingSaveOnSameFile)
                this.SuspendFileChangeNotification(pszFilename, 1);

            try
            {
                editorControl.RichTextBoxControl.SaveFile(pszFilename, RichTextBoxStreamType.RichText);
            }
            catch (ArgumentException)
            {
                hr = VSConstants.E_FAIL;
            }
            catch (IOException)
            {
                hr = VSConstants.E_FAIL;
            }
            finally
            {
                //restore the file change notifications
                if (doingSaveOnSameFile)
                    this.SuspendFileChangeNotification(pszFilename, 0);
            }

            if (VSConstants.E_FAIL == hr)
                return hr;

            //Save and Save as
            if (fRemember != 0)
            {
                //Save as
                if (null != pszFilename && !fileName.Equals(pszFilename))
                {
                    SetFileChangeNotification(fileName, false); //remove notification from old file
                    SetFileChangeNotification(pszFilename, true); //add notification for new file
                    fileName = pszFilename;     //cache the new file name
                }
                isDirty = false;
                SetReadOnly(false);             //set read only to false since you were successfully able
                //to save to the new file                                                    
            }

            ITrackSelection track = TrackSelection;
            if (null != track)
            {
                hr = track.OnSelectChange((ISelectionContainer)selContainer);
            }

            // Since all changes are now saved properly to disk, there's no need for a backup.
            backupObsolete = false;
            return hr;
        }

        #endregion


        #region IVsPersistDocData Members

        /// <summary>
        /// Used to determine if the document data has changed since the last time it was saved
        /// </summary>
        /// <param name="pfDirty">Will be set to 1 if the data has changed</param>
        /// <returns>S_OK if the function succeeds</returns>
        int IVsPersistDocData.IsDocDataDirty(out int pfDirty)
        {
            return ((IPersistFileFormat)this).IsDirty(out pfDirty);
        }

        /// <summary>
        /// Saves the document data. Before actually saving the file, we first need to indicate to the environment
        /// that a file is about to be saved. This is done through the "SVsQueryEditQuerySave" service. We call the
        /// "QuerySaveFile" function on the service instance and then proceed depending on the result returned as follows:
        /// If result is QSR_SaveOK - We go ahead and save the file and the file is not read only at this point.
        /// If result is QSR_ForceSaveAs - We invoke the "Save As" functionality which will bring up the Save file name 
        ///                                dialog 
        /// If result is QSR_NoSave_Cancel - We cancel the save operation and indicate that the document could not be saved
        ///                                by setting the "pfSaveCanceled" flag
        /// If result is QSR_NoSave_Continue - Nothing to do here as the file need not be saved
        /// </summary>
        /// <param name="dwSave">Flags which specify the file save options:
        /// VSSAVE_Save        - Saves the current file to itself.
        /// VSSAVE_SaveAs      - Prompts the User for a filename and saves the file to the file specified.
        /// VSSAVE_SaveCopyAs  - Prompts the user for a filename and saves a copy of the file with a name specified.
        /// VSSAVE_SilentSave  - Saves the file without prompting for a name or confirmation.  
        /// </param>
        /// <param name="pbstrMkDocumentNew">Pointer to the path to the new document</param>
        /// <param name="pfSaveCanceled">value 1 if the document could not be saved</param>
        /// <returns></returns>
        int IVsPersistDocData.SaveDocData(Microsoft.VisualStudio.Shell.Interop.VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
        {
            pbstrMkDocumentNew = null;
            pfSaveCanceled = 0;
            int hr = VSConstants.S_OK;

            switch (dwSave)
            {
                case VSSAVEFLAGS.VSSAVE_Save:
                case VSSAVEFLAGS.VSSAVE_SilentSave:
                    {
                        IVsQueryEditQuerySave2 queryEditQuerySave = (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));

                        // Call QueryEditQuerySave
                        uint result = 0;
                        hr = queryEditQuerySave.QuerySaveFile(
                                fileName,        // filename
                                0,    // flags
                                null,            // file attributes
                                out result);    // result
                        if (ErrorHandler.Failed(hr))
                            return hr;

                        // Process according to result from QuerySave
                        switch ((tagVSQuerySaveResult)result)
                        {
                            case tagVSQuerySaveResult.QSR_NoSave_Cancel:
                                // Note that this is also case tagVSQuerySaveResult.QSR_NoSave_UserCanceled because these
                                // two tags have the same value.
                                pfSaveCanceled = ~0;
                                break;

                            case tagVSQuerySaveResult.QSR_SaveOK:
                                {
                                    // Call the shell to do the save for us
                                    IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                                    hr = uiShell.SaveDocDataToFile(dwSave, (IPersistFileFormat)this, fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                                    if (ErrorHandler.Failed(hr))
                                        return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_ForceSaveAs:
                                {
                                    // Call the shell to do the SaveAS for us
                                    IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                                    hr = uiShell.SaveDocDataToFile(VSSAVEFLAGS.VSSAVE_SaveAs, (IPersistFileFormat)this, fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                                    if (ErrorHandler.Failed(hr))
                                        return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_NoSave_Continue:
                                // In this case there is nothing to do.
                                break;

                            default:
                                throw new NotSupportedException("Unsupported result from QEQS");
                        }
                        break;
                    }
                case VSSAVEFLAGS.VSSAVE_SaveAs:
                case VSSAVEFLAGS.VSSAVE_SaveCopyAs:
                    {
                        // Make sure the file name as the right extension
                        if (String.Compare(MyExtension, System.IO.Path.GetExtension(fileName), true, CultureInfo.CurrentCulture) != 0)
                        {
                            fileName += MyExtension;
                        }
                        // Call the shell to do the save for us
                        IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                        hr = uiShell.SaveDocDataToFile(dwSave, (IPersistFileFormat)this, fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                        if (ErrorHandler.Failed(hr))
                            return hr;
                        break;
                    }
                default:
                    throw new ArgumentException("Unsupported Save flag");
            };

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Loads the document data from the file specified
        /// </summary>
        /// <param name="pszMkDocument">Path to the document file which needs to be loaded</param>
        /// <returns>S_Ok if the method succeeds</returns>
        int IVsPersistDocData.LoadDocData(string pszMkDocument)
        {
            return ((IPersistFileFormat)this).Load(pszMkDocument, 0, 0);
        }

        /// <summary>
        /// Used to set the initial name for unsaved, newly created document data
        /// </summary>
        /// <param name="pszDocDataPath">String containing the path to the document. We need to ignore this parameter
        /// </param>
        /// <returns>S_OK if the mthod succeeds</returns>
        int IVsPersistDocData.SetUntitledDocPath(string pszDocDataPath)
        {
            return ((IPersistFileFormat)this).InitNew(MyFormat);
        }

        /// <summary>
        /// Returns the Guid of the editor factory that created the IVsPersistDocData object
        /// </summary>
        /// <param name="pClassID">Pointer to the class identifier of the editor type</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.GetGuidEditorType(out Guid pClassID)
        {
            return ((IPersistFileFormat)this).GetClassID(out pClassID);
        }

        /// <summary>
        /// Close the IVsPersistDocData object
        /// </summary>
        /// <returns>S_OK if the function succeeds</returns>
        int IVsPersistDocData.Close()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines if it is possible to reload the document data
        /// </summary>
        /// <param name="pfReloadable">set to 1 if the document can be reloaded</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.IsDocDataReloadable(out int pfReloadable)
        {
            // Allow file to be reloaded
            pfReloadable = 1;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Renames the document data
        /// </summary>
        /// <param name="grfAttribs"></param>
        /// <param name="pHierNew"></param>
        /// <param name="itemidNew"></param>
        /// <param name="pszMkDocumentNew"></param>
        /// <returns></returns>
        int IVsPersistDocData.RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            // TODO:  Add EditorPane.RenameDocData implementation
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Reloads the document data
        /// </summary>
        /// <param name="grfFlags">Flag indicating whether to ignore the next file change when reloading the document data.
        /// This flag should not be set for us since we implement the "IVsDocDataFileChangeControl" interface in order to 
        /// indicate ignoring of file changes
        /// </param>
        /// <returns>S_OK if the mthod succeeds</returns>
        int IVsPersistDocData.ReloadDocData(uint grfFlags)
        {
            return ((IPersistFileFormat)this).Load(fileName, grfFlags, 0);
        }

        /// <summary>
        /// Called by the Running Document Table when it registers the document data. 
        /// </summary>
        /// <param name="docCookie">Handle for the document to be registered</param>
        /// <param name="pHierNew">Pointer to the IVsHierarchy interface</param>
        /// <param name="itemidNew">Item identifier of the document to be registered from VSITEM</param>
        /// <returns></returns>
        int IVsPersistDocData.OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
        {
            //Nothing to do here
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsFileChangeEvents Members

        /// <summary>
        /// Notify the editor of the changes made to one or more files
        /// </summary>
        /// <param name="cChanges">Number of files that have changed</param>
        /// <param name="rgpszFile">array of the files names that have changed</param>
        /// <param name="rggrfChange">Array of the flags indicating the type of changes</param>
        /// <returns></returns>
        int IVsFileChangeEvents.FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t**** Inside FilesChanged ****"));

            //check the different parameters
            if (0 == cChanges || null == rgpszFile || null == rggrfChange)
                return VSConstants.E_INVALIDARG;

            //ignore file changes if we are in that mode
            if (ignoreFileChangeLevel != 0)
                return VSConstants.S_OK;

            for (uint i = 0; i < cChanges; i++)
            {
                if (!String.IsNullOrEmpty(rgpszFile[i]) && String.Compare(rgpszFile[i], fileName, true, CultureInfo.CurrentCulture) == 0)
                {
                    // if the readonly state (file attributes) have changed we can immediately update
                    // the editor to match the new state (either readonly or not readonly) immediately
                    // without prompting the user.
                    if (0 != (rggrfChange[i] & (int)_VSFILECHANGEFLAGS.VSFILECHG_Attr))
                    {
                        FileAttributes fileAttrs = File.GetAttributes(fileName);
                        int isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;
                        SetReadOnly(isReadOnly != 0);
                    }
                    // if it looks like the file contents have changed (either the size or the modified
                    // time has changed) then we need to prompt the user to see if we should reload the
                    // file. it is important to not syncronisly reload the file inside of this FilesChanged
                    // notification. first it is possible that there will be more than one FilesChanged 
                    // notification being sent (sometimes you get separate notifications for file attribute
                    // changing and file size/time changing). also it is the preferred UI style to not
                    // prompt the user until the user re-activates the environment application window.
                    // this is why we use a timer to delay prompting the user.
                    if (0 != (rggrfChange[i] & (int)(_VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Size)))
                    {
                        if (!fileChangedTimerSet)
                        {
                            FileChangeTrigger = new Timer();
                            fileChangedTimerSet = true;
                            FileChangeTrigger.Interval = 1000;
                            FileChangeTrigger.Tick += new EventHandler(this.OnFileChangeEvent);
                            FileChangeTrigger.Enabled = true;
                        }
                    }
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Notify the editor of the changes made to a directory
        /// </summary>
        /// <param name="pszDirectory">Name of the directory that has changed</param>
        /// <returns></returns>
        int IVsFileChangeEvents.DirectoryChanged(string pszDirectory)
        {
            //Nothing to do here
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsDocDataFileChangeControl Members

        /// <summary>
        /// Used to determine whether changes to DocData in files should be ignored or not
        /// </summary>
        /// <param name="fIgnore">a non zero value indicates that the file changes should be ignored
        /// </param>
        /// <returns></returns>
        int IVsDocDataFileChangeControl.IgnoreFileChanges(int fIgnore)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside IgnoreFileChanges ****"));

            if (fIgnore != 0)
            {
                ignoreFileChangeLevel++;
            }
            else
            {
                if (ignoreFileChangeLevel > 0)
                    ignoreFileChangeLevel--;

                // We need to check here if our file has changed from "Read Only"
                // to "Read/Write" or vice versa while the ignore level was non-zero.
                // This may happen when a file is checked in or out under source
                // code control. We need to check here so we can update our caption.
                FileAttributes fileAttrs = File.GetAttributes(fileName);
                int isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;
                SetReadOnly(isReadOnly != 0);
            }
            return VSConstants.S_OK;
        }
        #endregion

        #region File Change Notification Helpers

        /// <summary>
        /// In this function we inform the shell when we wish to receive 
        /// events when our file is changed or we inform the shell when 
        /// we wish not to receive events anymore.
        /// </summary>
        /// <param name="pszFileName">File name string</param>
        /// <param name="fStart">TRUE indicates advise, FALSE indicates unadvise.</param>
        /// <returns>Result of teh operation</returns>
        private int SetFileChangeNotification(string pszFileName, bool fStart)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside SetFileChangeNotification ****"));

            int result = VSConstants.E_FAIL;

            //Get the File Change service
            if (null == vsFileChangeEx)
                vsFileChangeEx = (IVsFileChangeEx)GetService(typeof(SVsFileChangeEx));
            if (null == vsFileChangeEx)
                return VSConstants.E_UNEXPECTED;

            // Setup Notification if fStart is TRUE, Remove if fStart is FALSE.
            if (fStart)
            {
                if (vsFileChangeCookie == VSConstants.VSCOOKIE_NIL)
                {
                    //Receive notifications if either the attributes of the file change or 
                    //if the size of the file changes or if the last modified time of the file changes
                    result = vsFileChangeEx.AdviseFileChange(pszFileName,
                        (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Attr | _VSFILECHANGEFLAGS.VSFILECHG_Size | _VSFILECHANGEFLAGS.VSFILECHG_Time),
                        (IVsFileChangeEvents)this,
                        out vsFileChangeCookie);
                    if (vsFileChangeCookie == VSConstants.VSCOOKIE_NIL)
                        return VSConstants.E_FAIL;
                }
            }
            else
            {
                if (vsFileChangeCookie != VSConstants.VSCOOKIE_NIL)
                {
                    result = vsFileChangeEx.UnadviseFileChange(vsFileChangeCookie);
                    vsFileChangeCookie = VSConstants.VSCOOKIE_NIL;
                }
            }
            return result;
        }

        /// <summary>
        /// In this function we suspend receiving file change events for
        /// a file or we reinstate a previously suspended file depending
        /// on the value of the given fSuspend flag.
        /// </summary>
        /// <param name="pszFileName">File name string</param>
        /// <param name="fSuspend">TRUE indicates that the events needs to be suspended</param>
        /// <returns></returns>

        private int SuspendFileChangeNotification(string pszFileName, int fSuspend)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside SuspendFileChangeNotification ****"));

            if (null == vsFileChangeEx)
                vsFileChangeEx = (IVsFileChangeEx)GetService(typeof(SVsFileChangeEx));
            if (null == vsFileChangeEx)
                return VSConstants.E_UNEXPECTED;

            if (0 == fSuspend)
            {
                // we are transitioning from suspended to non-suspended state - so force a
                // sync first to avoid asynchronous notifications of our own change
                if (vsFileChangeEx.SyncFile(pszFileName) == VSConstants.E_FAIL)
                    return VSConstants.E_FAIL;
            }

            //If we use the VSCOOKIE parameter to specify the file, then pszMkDocument parameter 
            //must be set to a null reference and vice versa 
            return vsFileChangeEx.IgnoreFile(vsFileChangeCookie, null, fSuspend);
        }
        #endregion

        #region IVsFileBackup Members

        /// <summary>
        /// This method is used to Persist the data to a single file. On a successful backup this 
        /// should clear up the backup dirty bit
        /// </summary>
        /// <param name="pszBackupFileName">Name of the file to persist</param>
        /// <returns>S_OK if the data can be successfully persisted.
        /// This should return STG_S_DATALOSS or STG_E_INVALIDCODEPAGE if there is no way to 
        /// persist to a file without data loss
        /// </returns>
        int IVsFileBackup.BackupFile(string pszBackupFileName)
        {
            try
            {
                editorControl.RichTextBoxControl.SaveFile(pszBackupFileName);
                backupObsolete = false;
            }
            catch (ArgumentException)
            {
                return VSConstants.E_FAIL;
            }
            catch (IOException)
            {
                return VSConstants.E_FAIL;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Used to set the backup dirty bit. This bit should be set when the object is modified 
        /// and cleared on calls to BackupFile and any Save method
        /// </summary>
        /// <param name="pbObsolete">the dirty bit to be set</param>
        /// <returns>returns 1 if the backup dirty bit is set, 0 otherwise</returns>
        int IVsFileBackup.IsBackupFileObsolete(out int pbObsolete)
        {
            if (backupObsolete)
                pbObsolete = 1;
            else
                pbObsolete = 0;
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsToolboxUser Interface
        public int IsSupported(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
        {
            // Create a OleDataObject from the input interface.
            OleDataObject oleData = new OleDataObject(pDO);
            // && editorControl.RichTextBoxControl.CanPaste(DataFormats.GetFormat(DataFormats.UnicodeText))
            // Check if the data object is of type UnicodeText.
            if (oleData.GetDataPresent(DataFormats.UnicodeText))
            {
                return VSConstants.S_OK;
            }

            // In all the other cases return S_FALSE
            return VSConstants.S_FALSE;
        }

        public int ItemPicked(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
        {
            // Create a OleDataObject from the input interface.
            OleDataObject oleData = new OleDataObject(pDO);

            // Check if the picked item is the one we can paste.
            if (oleData.GetDataPresent(DataFormats.UnicodeText))
            {
                object o = null;
                editorControl.TextSelection.Paste(ref o, 0);
            }

            return VSConstants.S_OK;
        }
        #endregion

        /// <summary>
        /// Used to ReadOnly property for the Rich TextBox and correspondingly update the editor caption
        /// </summary>
        /// <param name="_isFileReadOnly">Indicates whether the file loaded is Read Only or not</param>
        private void SetReadOnly(bool _isFileReadOnly)
        {
            this.editorControl.RichTextBoxControl.ReadOnly = _isFileReadOnly;

            //update editor caption with "[Read Only]" or "" as necessary
            IVsWindowFrame frame = (IVsWindowFrame)GetService(typeof(SVsWindowFrame));
            string editorCaption = "";
            if (_isFileReadOnly)
                editorCaption = this.GetResourceString("@100");
            ErrorHandler.ThrowOnFailure(frame.SetProperty((int)__VSFPROPID.VSFPROPID_EditorCaption, editorCaption));
            backupObsolete = true;
        }

        /// <summary>
        /// This event is triggered when one of the files loaded into the environment has changed outside of the
        /// editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileChangeEvent(object sender, System.EventArgs e)
        {
            //Disable the timer
            FileChangeTrigger.Enabled = false;

            string message = this.GetResourceString("@101");    //get the message string from the resource
            IVsUIShell VsUiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            int result = 0;
            Guid tempGuid = Guid.Empty;
            if (VsUiShell != null)
            {
                //Show up a message box indicating that the file has changed outside of VS environment
                ErrorHandler.ThrowOnFailure(VsUiShell.ShowMessageBox(0, ref tempGuid, fileName, message, null, 0,
                    OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_QUERY, 0, out result));
            }
            //if the user selects "Yes", reload the current file
            if (result == (int)DialogResult.Yes)
            {
                ErrorHandler.ThrowOnFailure(((IVsPersistDocData)this).ReloadDocData(0));
            }

            fileChangedTimerSet = false;
        }

        /// <summary>
        /// This method loads a localized string based on the specified resource.
        /// </summary>
        /// <param name="resourceName">Resource to load</param>
        /// <returns>String loaded for the specified resource</returns>
        internal string GetResourceString(string resourceName)
        {
            string resourceValue;
            IVsResourceManager resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
            if (resourceManager == null)
            {
                throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
            }
            Guid packageGuid = myPackage.GetType().GUID;
            int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            return resourceValue;
        }

        /// <summary>
        /// This function asks to the QueryEditQuerySave service if it is possible to
        /// edit the file.
        /// </summary>
        private bool CanEditFile()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t**** CanEditFile called ****"));

            // Check the status of the recursion guard
            if (gettingCheckoutStatus)
                return false;

            try
            {
                // Set the recursion guard
                gettingCheckoutStatus = true;

                // Get the QueryEditQuerySave service
                IVsQueryEditQuerySave2 queryEditQuerySave = (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));

                // Now call the QueryEdit method to find the edit status of this file
                string[] documents = { this.fileName };
                uint result;
                uint outFlags;

                // Note that this function can popup a dialog to ask the user to checkout the file.
                // When this dialog is visible, it is possible to receive other request to change
                // the file and this is the reason for the recursion guard.
                int hr = queryEditQuerySave.QueryEditFiles(
                    0,              // Flags
                    1,              // Number of elements in the array
                    documents,      // Files to edit
                    null,           // Input flags
                    null,           // Input array of VSQEQS_FILE_ATTRIBUTE_DATA
                    out result,     // result of the checkout
                    out outFlags    // Additional flags
                );
                if (ErrorHandler.Succeeded(hr) && (result == (uint)tagVSQueryEditResult.QER_EditOK))
                {
                    // In this case (and only in this case) we can return true from this function.
                    return true;
                }
            }

            finally
            {
                gettingCheckoutStatus = false;
            }
            return false;
        }

        /// <summary>
        /// This event is triggered when there contents of the file are changed inside the editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.ITrackSelection.OnSelectChange(Microsoft.VisualStudio.Shell.Interop.ISelectionContainer)")]
        private void OnTextChange(object sender, System.EventArgs e)
        {
            // During the load operation the text of the control will change, but
            // this change must not be stored in the status of the document.
            if (!loading)
            {
                // The only interesting case is when we are changing the document
                // for the first time
                if (!isDirty)
                {
                    // Check if the QueryEditQuerySave service allow us to change the file
                    if (!CanEditFile())
                    {
                        // We can not change the file (e.g. a checkout operation failed),
                        // so undo the change and exit.
                        editorControl.RichTextBoxControl.Undo();
                        return;
                    }

                    // It is possible to change the file, so update the status.
                    isDirty = true;
                    ITrackSelection track = TrackSelection;
                    if (null != track)
                    {
                        // Note: here we don't need to check the return code.
                        track.OnSelectChange((ISelectionContainer)selContainer);
                    }
                    backupObsolete = true;
                }
            }
        }

        /// <summary>
        /// This event is triggered when the control's GotFocus event is fired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGotFocus(object sender, System.EventArgs e)
        {
            if (null == FNFStatusbarTrigger)
                FNFStatusbarTrigger = new Timer();

            FileChangeTrigger.Interval = 1000;
            FNFStatusbarTrigger.Tick += new EventHandler(this.OnSetStatusBar);
            FNFStatusbarTrigger.Start();
        }

        private void OnSetStatusBar(object sender, System.EventArgs e)
        {
            FNFStatusbarTrigger.Stop();
            ErrorHandler.ThrowOnFailure(((IVsStatusbarUser)this).SetInfo());
        }

        #region IVsStatusbarUser Members

        /// <summary>
        /// This is the IVsStatusBarUser function that will update our status bar.
        /// Note that the IDE calls this function only when our document window is
        /// initially activated.
        /// </summary>
        /// <returns> Hresult that represents success or failure.</returns>
        int IVsStatusbarUser.SetInfo()
        {
            // Call the helper function that updates the status bar insert mode
            int hrSetInsertMode = SetStatusBarInsertMode();

            // Call the helper function that updates the status bar selection mode
            int hrSetSelectionMode = SetStatusBarSelectionMode();

            // Call the helper function that updates the status bar position
            int hrSetPosition = SetStatusBarPosition();

            return (hrSetInsertMode == VSConstants.S_OK &&
                    hrSetSelectionMode == VSConstants.S_OK &&
                    hrSetPosition == VSConstants.S_OK) ? VSConstants.S_OK : VSConstants.E_FAIL;
        }

        /// <summary>
        /// Helper function that updates the insert mode displayed on the status bar.
        /// This is the text that is displayed in the right side of the status bar that
        /// will either say INS or OVR.
        /// </summary>
        /// <returns> Hresult that represents success or failure.</returns>
        int SetStatusBarInsertMode()
        {
            // Get the IVsStatusBar interface
            IVsStatusbar statusBar = GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            if (statusBar == null)
                return VSConstants.E_FAIL;

            // Set the insert mode based on our editorControl.richTextBoxCtrl.Overstrike value.  If 1 is passed
            // in then it will display OVR and if 0 is passed in it will display INS.
            object insertMode = (object)(this.editorControl.Overstrike ? 1 : 0);
            return statusBar.SetInsMode(ref insertMode);
        }

        /// <summary>
        /// This is an extra command handler that we will use to check when the insert
        /// key is pressed.  Note that even if we detect that the insert key is pressed
        /// we are not setting the handled property to true, so other event handlers will
        /// also see it.
        /// </summary>
        /// <param name="sender"> Not used.</param>
        /// <param name="e"> KeyEventArgs instance that we will use to get the key that was pressed.</param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // If the key pressed is the insert key...
            if (e.KeyValue == 45)
            {
                // Toggle our stored insert value
                this.editorControl.Overstrike = !this.editorControl.Overstrike;

                // Call the function to update the status bar insert mode
                SetStatusBarInsertMode();
            }
        }

        /// <summary>
        /// Helper function that updates the selection mode displayed on the status
        /// bar.  Right now we only support stream selection.
        /// </summary>
        /// <returns> Hresult that represents success or failure.</returns>
        int SetStatusBarSelectionMode()
        {
            // Get the IVsStatusBar interface.
            IVsStatusbar statusBar = GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            if (statusBar == null)
                return VSConstants.E_FAIL;

            // Set the selection mode.  Since we only support stream selection we will
            // always pass in zero here.  Passing in one would make "COL" show up
            // just to the left of the insert mode on the status bar.
            object selectionMode = 0;
            return statusBar.SetSelMode(ref selectionMode);
        }

        /// <summary>
        /// Helper function that updates the cursor position displayed on the status bar.
        /// </summary>
        /// <returns> Hresult that represents success or failure.</returns>
        int SetStatusBarPosition()
        {
            // Get the IVsStatusBar interface.
            IVsStatusbar statusBar = GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            if (statusBar == null)
                return VSConstants.E_FAIL;

            // If there is no selection then textBox1.SelectionStart will tell us
            // the position of the cursor.  If there is a selection then this value will tell
            // us the position of the "left" side of the selection (the side of the selection that
            // has the smaller index value).
            int startIndex = editorControl.RichTextBoxControl.SelectionStart;

            // If the cursor is at the end of the selection then we need to add the selection
            // length to the index value.
            if ((editorControl.TextSelection.Flags & (int)tom.tomConstants.tomSelStartActive) == 0)
                startIndex += editorControl.RichTextBoxControl.SelectionLength;

            // Call the function that gets the (zero-based) line index based on the buffer index.
            int lineNumber = editorControl.RichTextBoxControl.GetLineFromCharIndex(startIndex);

            // To get the (zero-based) character number subtract the index of the first character
            // on this line from the buffer index.
            int charNumber = startIndex - editorControl.RichTextBoxControl.GetFirstCharIndexFromLine(lineNumber);

            // Call the SetLineChar function, making sure to add one to our line and
            // character values since the values we get from the RichTextBox calls
            // are zero based.
            object line = (object)(lineNumber + 1);
            object chr = (object)(charNumber + 1);

            // Call the IVsStatusBar's SetLineChar function and return it's hresult
            return statusBar.SetLineChar(ref line, ref chr);
        }

        #endregion

        #region IVsFindTarget Members

        /// <summary>
        /// Return the object that was requested
        /// </summary>
        /// <param name="propid">Id of the requested object</param>
        /// <param name="pvar">Object returned</param>
        /// <returns>HResult</returns>
        int IVsFindTarget.GetProperty(uint propid, out object pvar)
        {
            pvar = null;

            switch (propid)
            {
                case (uint)__VSFTPROPID.VSFTPROPID_DocName:
                    {
                        // Return a copy of the file name
                        pvar = fileName;
                        break;
                    }
                case (uint)__VSFTPROPID.VSFTPROPID_InitialPattern:
                case (uint)__VSFTPROPID.VSFTPROPID_InitialPatternAggressive:
                    {
                        // Return the selected text
                        GetInitialSearchString(out pvar);
                        //pvar = editorControl.RichTextBoxControl.SelectedText;
                        break;
                    }
                case (uint)__VSFTPROPID.VSFTPROPID_WindowFrame:
                    {
                        // Return the Window frame
                        pvar = (IVsWindowFrame)GetService(typeof(SVsWindowFrame));
                        break;
                    }
                case (uint)__VSFTPROPID.VSFTPROPID_IsDiskFile:
                    {
                        // We currently assume the file is on disk
                        pvar = true;
                        break;
                    }
                default:
                    {
                        return VSConstants.E_NOTIMPL;
                    }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grfOptions"></param>
        /// <param name="ppSpans"></param>
        /// <param name="ppTextImage"></param>
        int IVsFindTarget.GetSearchImage(uint grfOptions, IVsTextSpanSet[] ppSpans, out IVsTextImage ppTextImage)
        {
            //set the IVsTextSpanSet object
            if (null != ppSpans && ppSpans.Length > 0)
            {
                ppSpans[0] = (IVsTextSpanSet)this;
            }

            //set the IVsTextImage object
            ppTextImage = (IVsTextImage)this;

            //attach this text image to the span
            if (null != ppSpans && ppSpans.Length > 0)
            {
                ErrorHandler.ThrowOnFailure(ppSpans[0].AttachTextImage(ppTextImage));
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Retrieve a previously stored object
        /// </summary>
        /// <returns>The object that is being asked</returns>
        int IVsFindTarget.GetFindState(out object ppunk)
        {
            ppunk = findState;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Search for the string in the text of our editor.
        /// Options specify how we do the search. No need to implement this since we implment IVsTextImage
        /// </summary>
        /// <param name="pszSearch">Search string</param>
        /// <param name="grfOptions">Search options</param>
        /// <param name="fResetStartPoint">Is this a new search?</param>
        /// <param name="pHelper">We are not using it</param>
        /// <param name="pResult">True if we found the search string</param>
        int IVsFindTarget.Find(string pszSearch, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out uint pResult)
        {
            pResult = 0;

            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Bring the focus to a specific position in the document
        /// </summary>
        /// <param name="pts">Location where to move the cursor to</param>
        int IVsFindTarget.NavigateTo(TextSpan[] pts)
        {
            int hr = VSConstants.S_OK;

            // Activate the window
            IVsWindowFrame frame = (IVsWindowFrame)GetService(typeof(SVsWindowFrame));
            if (frame != null)
                hr = frame.Show();
            else
                return VSConstants.E_NOTIMPL;

            // Now navigate to the specified location (if any)
            if (ErrorHandler.Succeeded(hr) && (null != pts) && (pts.Length > 0))
            {
                // first set start location
                int NewPosition = editorControl.RichTextBoxControl.GetFirstCharIndexFromLine(pts[0].iStartLine);
                NewPosition += pts[0].iStartIndex;
                if (NewPosition > editorControl.RichTextBoxControl.Text.Length)
                    NewPosition = editorControl.RichTextBoxControl.Text.Length;
                editorControl.RichTextBoxControl.SelectionStart = NewPosition;

                // now set the length of the selection
                NewPosition = editorControl.RichTextBoxControl.GetFirstCharIndexFromLine(pts[0].iEndLine);
                NewPosition += pts[0].iEndIndex;
                if (NewPosition > editorControl.RichTextBoxControl.Text.Length)
                    NewPosition = editorControl.RichTextBoxControl.Text.Length;
                int length = NewPosition - editorControl.RichTextBoxControl.SelectionStart;
                if (length >= 0)
                    editorControl.RichTextBoxControl.SelectionLength = length;
                else
                    editorControl.RichTextBoxControl.SelectionLength = 0;
            }
            return hr;
        }

        /// <summary>
        /// Get current cursor location
        /// </summary>
        /// <param name="pts">Current location</param>
        /// <returns>Hresult</returns>
        int IVsFindTarget.GetCurrentSpan(TextSpan[] pts)
        {
            if (null == pts || 0 == pts.Length)
                return VSConstants.E_INVALIDARG;

            pts[0].iStartIndex = editorControl.GetColumnFromIndex(editorControl.RichTextBoxControl.SelectionStart);
            pts[0].iEndIndex = editorControl.GetColumnFromIndex(editorControl.RichTextBoxControl.SelectionStart + editorControl.RichTextBoxControl.SelectionLength);
            pts[0].iStartLine = editorControl.RichTextBoxControl.GetLineFromCharIndex(editorControl.RichTextBoxControl.SelectionStart);
            pts[0].iEndLine = editorControl.RichTextBoxControl.GetLineFromCharIndex(editorControl.RichTextBoxControl.SelectionStart + editorControl.RichTextBoxControl.SelectionLength);

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Highlight a given text span. No need to implement
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        int IVsFindTarget.MarkSpan(TextSpan[] pts)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Replace a string in the text. No need to implement since we implement IVsTextImage
        /// </summary>
        /// <param name="pszSearch">string containing the search text</param>
        /// <param name="pszReplace">string xontaining the replacement text</param>
        /// <param name="grfOptions">Search options available</param>
        /// <param name="fResetStartPoint">flag to reset the search start point</param>
        /// <param name="pHelper">IVsFindHelper interface object</param>
        /// <param name="pfReplaced">returns whether replacement was successful or not</param>
        /// <returns></returns>
        int IVsFindTarget.Replace(string pszSearch, string pszReplace, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out int pfReplaced)
        {
            pfReplaced = 0;

            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Store an object that will later be returned
        /// </summary>
        /// <returns>The object that is being stored</returns>
        int IVsFindTarget.SetFindState(object pUnk)
        {
            findState = pUnk;
            return VSConstants.S_OK;
        }


        /// <summary>
        /// This implementation does not use notification
        /// </summary>
        /// <param name="notification"></param>
        int IVsFindTarget.NotifyFindTarget(uint notification)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Specify which search option we support.
        /// </summary>
        /// <param name="pfImage">Do we support IVsTextImage?</param>
        /// <param name="pgrfOptions">Supported options</param>
        int IVsFindTarget.GetCapabilities(bool[] pfImage, uint[] pgrfOptions)
        {
            // We do support IVsTextImage
            if (pfImage != null && pfImage.Length > 0)
                pfImage[0] = true;

            if (pgrfOptions != null && pgrfOptions.Length > 0)
            {
                pgrfOptions[0] = (uint)__VSFINDOPTIONS.FR_Backwards;        //Search backwards from the insertion point
                pgrfOptions[0] |= (uint)__VSFINDOPTIONS.FR_MatchCase;       //Match the case while searching
                pgrfOptions[0] |= (uint)__VSFINDOPTIONS.FR_WholeWord;       //Match whole word while searching
                pgrfOptions[0] |= (uint)__VSFINDOPTIONS.FR_Selection;       //Search in selected text only
                pgrfOptions[0] |= (uint)__VSFINDOPTIONS.FR_ActionMask;      //Find/Replace capabilities

                // Only support selection if something is selected
                if (editorControl.RichTextBoxControl.SelectionLength == 0)
                    pgrfOptions[0] &= ~((uint)__VSFINDOPTIONS.FR_Selection);

                //if the file is read only, don't support replace
                if (editorControl.RichTextBoxControl.ReadOnly)
                    pgrfOptions[0] &= ~((uint)__VSFINDOPTIONS.FR_Replace | (uint)__VSFINDOPTIONS.FR_ReplaceAll);
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Return the Screen coordinates of the matched string. No need to implement
        /// </summary>
        /// <param name="prc"></param>
        /// <returns></returns>
        int IVsFindTarget.GetMatchRect(RECT[] prc)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        /// <summary>
        /// Function to return the string to be used in the "Find What" field of the find window. Will return
        /// null if no text is selected or if there are multiple lines of text selected.
        /// </summary>
        /// <param name="pvar">the string to be returned</param>
        private void GetInitialSearchString(out object pvar)
        {
            //If no text is selected, return null
            if (0 == editorControl.RichTextBoxControl.SelectionLength)
            {
                pvar = null;
                return;
            }

            //Now check if multiple lines have been selected
            int endIndex = editorControl.RichTextBoxControl.SelectionStart + editorControl.RichTextBoxControl.SelectionLength;
            int endline = editorControl.RichTextBoxControl.GetLineFromCharIndex(endIndex);
            int startline = editorControl.RichTextBoxControl.GetLineFromCharIndex(editorControl.RichTextBoxControl.SelectionStart);
            if (startline != endline)
            {
                pvar = null;
                return;
            }

            pvar = editorControl.RichTextBoxControl.SelectedText;
        }

        #region IVsTextImage members

        /// <summary>
        /// To return the number of characters in the text image. No need to implement
        /// </summary>
        /// <param name="pcch">contain the number of characters</param>
        /// <returns></returns>
        int IVsTextImage.GetCharSize(out int pcch)
        {
            pcch = 0;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// To return the number of lines in the text image
        /// </summary>
        /// <param name="pcLines">pointer to the number of lines in the text image</param>
        /// <returns>S_OK</returns>
        int IVsTextImage.GetLineSize(out int pcLines)
        {
            //get the number of the lines in the control
            int len = editorControl.RichTextBoxControl.Lines.Length;
            pcLines = len;

            return VSConstants.S_OK;
        }

        /// <summary>
        /// To return the buffer address of the given text address. No need to implement
        /// </summary>
        /// <param name="ta">contains the TextAddress</param>
        /// <param name="piOffset">will contain the ofset from the start of the buffer</param>
        /// <returns></returns>
        int IVsTextImage.GetOffsetOfTextAddress(TextAddress ta, out int piOffset)
        {
            piOffset = 0;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// To return the text address of the given buffer address. No need to implement
        /// </summary>
        /// <param name="iOffset">offset from the start of the buffer</param>
        /// <param name="pta">will contain the TextAddress</param>
        /// <returns></returns>
        int IVsTextImage.GetTextAddressOfOffset(int iOffset, TextAddress[] pta)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Notification for a text span replacement
        /// </summary>
        /// <param name="dwFlags">Flags used for the replace</param>
        /// <param name="pts">Contains the TextSpan to be replaced</param>
        /// <param name="cch">count of characters in pchText</param>
        /// <param name="pchText">the replacement text</param>
        /// <param name="ptsChanged">TextSpan of the replaced text</param>
        /// <returns></returns>
        int IVsTextImage.Replace(uint dwFlags,
                                 TextSpan[] pts,
                                 int cch,
                                 string pchText,
                                 TextSpan[] ptsChanged
            )
        {
            //pts contains the span of the item which is to be replaced
            if (null == pts || 0 == pts.Length)
                return VSConstants.E_INVALIDARG;

            if (null == pchText)
                return VSConstants.E_INVALIDARG;

            // first set start location
            int NewPosition = editorControl.RichTextBoxControl.GetFirstCharIndexFromLine(pts[0].iStartLine);
            NewPosition += pts[0].iStartIndex;
            if (NewPosition > editorControl.RichTextBoxControl.Text.Length)
                NewPosition = editorControl.RichTextBoxControl.Text.Length;
            editorControl.RichTextBoxControl.SelectionStart = NewPosition;

            // now set the length of the selection
            NewPosition = editorControl.RichTextBoxControl.GetFirstCharIndexFromLine(pts[0].iEndLine);
            NewPosition += pts[0].iEndIndex;
            if (NewPosition > editorControl.RichTextBoxControl.Text.Length)
                NewPosition = editorControl.RichTextBoxControl.Text.Length;
            int length = NewPosition - editorControl.RichTextBoxControl.SelectionStart;
            if (length >= 0)
                editorControl.RichTextBoxControl.SelectionLength = length;
            else
                editorControl.RichTextBoxControl.SelectionLength = 0;

            //replace the text
            editorControl.RichTextBoxControl.SelectedText = pchText;

            if ((dwFlags & (uint)__VSFINDOPTIONS.FR_Backwards) == 0)
            {
                // In case of forward search we have to place the insertion point at the
                // end of the new text, so it will be skipped during the next call to Find.
                editorControl.RichTextBoxControl.SelectionStart += editorControl.RichTextBoxControl.SelectionLength;
            }
            else
            {
                // If the search is backward, then set the end postion at the
                // beginning of the new text.
                editorControl.RichTextBoxControl.SelectionLength = 0;
            }

            //set the ptsChanged to the TextSpan of the replaced text
            if (null != ptsChanged && ptsChanged.Length > 0)
            {
                ptsChanged[0].iStartIndex = editorControl.GetColumnFromIndex(editorControl.RichTextBoxControl.SelectionStart);
                ptsChanged[0].iEndIndex = editorControl.GetColumnFromIndex(editorControl.RichTextBoxControl.SelectionStart + editorControl.RichTextBoxControl.SelectionLength);
                ptsChanged[0].iStartLine = editorControl.RichTextBoxControl.GetLineFromCharIndex(editorControl.RichTextBoxControl.SelectionStart);
                ptsChanged[0].iEndLine = editorControl.RichTextBoxControl.GetLineFromCharIndex(editorControl.RichTextBoxControl.SelectionStart + editorControl.RichTextBoxControl.SelectionLength);
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// To return the number of characters in a TextSpan
        /// </summary>
        /// <param name="pts">The TextSpan structure</param>
        /// <param name="pcch">will contain the number of characters</param>
        /// <returns></returns>
        int IVsTextImage.GetSpanLength(TextSpan[] pts, out int pcch)
        {
            pcch = 0;
            if (null == pts || 0 == pts.Length)
                return VSConstants.E_INVALIDARG;

            int startIndex = editorControl.GetIndexFromLineAndColumn(pts[0].iStartLine, pts[0].iStartIndex);
            if (startIndex < 0)
                return VSConstants.E_INVALIDARG;

            int endIndex = editorControl.GetIndexFromLineAndColumn(pts[0].iEndLine, pts[0].iEndIndex);
            if (endIndex < 0)
                return VSConstants.E_INVALIDARG;

            pcch = Math.Abs(endIndex - startIndex);

            return VSConstants.S_OK;
        }

        /// <summary>
        /// to return the text of a TextSpan as a BSTR
        /// </summary>
        /// <param name="pts">the TextSpan structure</param>
        /// <param name="pbstrText">the BSTR text</param>
        /// <returns></returns>
        int IVsTextImage.GetTextBSTR(TextSpan[] pts, out string pbstrText)
        {
            pbstrText = null;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// To return the text of a TextSpan. No need to implement
        /// </summary>
        /// <param name="pts">TextSpan structure</param>
        /// <param name="cch">number of characters to return</param>
        /// <param name="psz">will contain the text</param>
        /// <returns></returns>
        int IVsTextImage.GetText(TextSpan[] pts, int cch, ushort[] psz)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// To return the length of a particular line
        /// </summary>
        /// <param name="iLine">zero based line number</param>
        /// <param name="piLength">will contain the length</param>
        /// <returns></returns>
        int IVsTextImage.GetLineLength(int iLine, out int piLength)
        {
            int numberOfLines = 0;
            piLength = 0;
            ErrorHandler.ThrowOnFailure(((IVsTextImage)this).GetLineSize(out numberOfLines));

            if (iLine < 0 || iLine > numberOfLines - 1)
            {
                return VSConstants.E_INVALIDARG;
            }
            piLength = editorControl.RichTextBoxControl.Lines[iLine].Length;

            return VSConstants.S_OK;
        }

        /// <summary>
        /// To provide line oriented access to the text buffer
        /// </summary>
        /// <param name="grfGet">flags containing information on the line to get</param>
        /// <param name="iLine">zero based line number</param>
        /// <param name="iStartIndex">starting character index of the line</param>
        /// <param name="iEndIndex">ending character index of the line</param>
        /// <param name="pLineData">Will contain the filled LINEDATA structure</param>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        int IVsTextImage.GetLine(uint grfGet,
                                        int iLine,
                                        int iStartIndex,
                                        int iEndIndex,
                                        LINEDATAEX[] pLineData
            )
        {
            if (null == pLineData || 0 == pLineData.Length)
                return VSConstants.E_INVALIDARG;

            //first intialize the Line Data object
            pLineData[0].iLength = 0;
            pLineData[0].pszText = IntPtr.Zero;
            pLineData[0].iEolType = EOLTYPE.eolCR;
            pLineData[0].pAttributes = IntPtr.Zero;
            pLineData[0].dwFlags = (ushort)LINEDATAEXFLAGS.ldfDefault;
            pLineData[0].dwReserved = 0;
            pLineData[0].pAtomicTextChain = IntPtr.Zero;

            int lineCount = editorControl.RichTextBoxControl.Lines.Length;
            if ((iLine < 0) || (iLine >= lineCount) || (iStartIndex < 0) || (iEndIndex < 0) ||
                (iStartIndex > iEndIndex))
            {
                return VSConstants.E_INVALIDARG;
            }

            string lineText = editorControl.RichTextBoxControl.Lines[iLine];
            // If the line is empty then do not attempt to calculate the span in the normal way; just return.
            if (string.IsNullOrEmpty(lineText) && iStartIndex == 0 && iEndIndex == 0)
                return VSConstants.S_OK;
            int lineLength = lineText.Length;

            //Error if startIndex is greater than the line length
            if (iStartIndex >= lineLength || iEndIndex >= lineLength)
                return VSConstants.E_INVALIDARG;

            int spanLength = iEndIndex - iStartIndex + 1;

            //Error in arguments if the span length is greater than the line length
            if (spanLength > lineLength)
                return VSConstants.E_INVALIDARG;

            //If we are looking for a subset of the line i.e. a line span
            if (0 != (grfGet & (uint)GLDE_FLAGS.gldeSubset))
            {
                pLineData[0].iLength = spanLength;
                string spanText = lineText.Substring(iStartIndex, spanLength);
                pLineData[0].pszText = new IntPtr();
                pLineData[0].pszText = Marshal.StringToCoTaskMemAuto(spanText);
            }
            //else we need to return the complete line
            else
            {
                pLineData[0].iLength = lineLength;
                pLineData[0].pszText = new IntPtr();
                pLineData[0].pszText = Marshal.StringToCoTaskMemAuto(lineText);
            }

            return VSConstants.S_OK;

        }

        /// <summary>
        /// Release the LINEDATAEX structure
        /// </summary>
        /// <param name="pLineData">pointer to the LINEDATAEX structure</param>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        int IVsTextImage.ReleaseLine(LINEDATAEX[] pLineData)
        {
            if (null == pLineData || 0 == pLineData.Length)
                return VSConstants.E_INVALIDARG;

            //clear the Line Data object
            pLineData[0].iLength = 0;
            Marshal.FreeCoTaskMem(pLineData[0].pszText);
            pLineData[0].iEolType = EOLTYPE.eolNONE;
            pLineData[0].pAttributes = IntPtr.Zero;
            pLineData[0].dwFlags = (ushort)LINEDATAEXFLAGS.ldfDefault;
            pLineData[0].dwReserved = 0;
            pLineData[0].pAtomicTextChain = IntPtr.Zero;

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Registers the environment to revieve notifications of text image changes.
        /// </summary>
        /// <param name="pSink">Object requesting notification on text image changes</param>
        /// <param name="pCookie">Handle for the event sink</param>
        /// <returns></returns>
        int IVsTextImage.AdviseTextImageEvents(IVsTextImageEvents pSink, out uint pCookie)
        {
            //We don't use this
            pCookie = 0;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Cancels notification for text image changes
        /// </summary>
        /// <param name="Cookie">Handle to the event sink</param>
        /// <returns></returns>
        int IVsTextImage.UnadviseTextImageEvents(uint Cookie)
        {
            //We don't use this
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Notification from the environment that it is locking an image
        /// </summary>
        /// <param name="grfLock">the locking flag</param>
        /// <returns></returns>
        int IVsTextImage.LockImage(uint grfLock)
        {
            //We only allow one reader/writer
            if (!lockImage)
            {
                lockImage = true;
                return VSConstants.S_OK;
            }
            else
                return VSConstants.E_FAIL;
        }

        /// <summary>
        /// Notification from the environment that the text image is not in use
        /// </summary>
        /// <param name="grfLock">the locking flag</param>
        /// <returns></returns>
        int IVsTextImage.UnlockImage(uint grfLock)
        {
            lockImage = false;
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsTextSpanSet Members

        /// <summary>
        /// The environment uses this to get a text image
        /// </summary>
        /// <param name="pText">Pointer to the text image</param>
        /// <returns></returns>
        int IVsTextSpanSet.AttachTextImage(object pText)
        {
            if (null == pText)
                return VSConstants.E_INVALIDARG;

            if (null != spTextImage)
            {
                if (spTextImage.Equals(pText))
                    return VSConstants.S_OK;
            }

            spTextImage = (IVsTextImage)this;

            //get the number of lines in the Text Image
            int lineCount = 0;
            ErrorHandler.ThrowOnFailure(spTextImage.GetLineSize(out lineCount));

            //create a text span for the entire text image
            TextSpan textSpan = new TextSpan();
            textSpan.iStartLine = 0;
            textSpan.iStartIndex = 0;
            textSpan.iEndLine = 0;

            //get the length of the last line
            int lastLineLength = 0;
            if (lineCount > 0)
            {
                textSpan.iEndLine = lineCount - 1;
                ErrorHandler.ThrowOnFailure(spTextImage.GetLineLength(lineCount - 1, out lastLineLength));
            }

            //set the end index corresponding to the last line length
            textSpan.iEndIndex = lastLineLength;

            //add it to the text span array
            textSpanArray.Add(textSpan);

            return VSConstants.S_OK;
        }

        /// <summary>
        /// To Release a text image
        /// </summary>
        /// <returns></returns>
        int IVsTextSpanSet.Detach()
        {
            spTextImage = null;
            textSpanArray.RemoveRange(0, textSpanArray.Count);

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Not needed to be implmented
        /// </summary>
        /// <returns></returns>
        int IVsTextSpanSet.SuspendTracking()
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Not needed to be implemented
        /// </summary>
        /// <returns></returns>
        int IVsTextSpanSet.ResumeTracking()
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// To add the TExtSpan to an array at the specified location
        /// </summary>
        /// <param name="cEl">the index to insert</param>
        /// <param name="pSpan">the TextSpan object</param>
        /// <returns></returns>
        int IVsTextSpanSet.Add(int cEl, TextSpan[] pSpan)
        {
            if (null == pSpan || 0 == pSpan.Length)
                return VSConstants.E_INVALIDARG;

            if (cEl < 0)
                return VSConstants.E_INVALIDARG;

            textSpanArray.Insert(cEl, pSpan[0]);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Returns the number of text spans in the array
        /// </summary>
        /// <param name="pcel">will contain the count</param>
        /// <returns></returns>
        int IVsTextSpanSet.GetCount(out int pcel)
        {
            pcel = textSpanArray.Count;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Return the text span at the requested index
        /// </summary>
        /// <param name="iEl">the index</param>
        /// <param name="pSpan">will contain the TextSpan returned</param>
        /// <returns></returns>
        int IVsTextSpanSet.GetAt(int iEl, TextSpan[] pSpan)
        {
            if (iEl >= textSpanArray.Count || iEl < 0)
                return VSConstants.E_INVALIDARG;

            if (null == pSpan || 0 == pSpan.Length)
                return VSConstants.E_INVALIDARG;

            pSpan[0] = (TextSpan)textSpanArray[iEl];

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Clear up the text span array
        /// </summary>
        /// <returns></returns>
        int IVsTextSpanSet.RemoveAll()
        {
            textSpanArray.RemoveRange(0, textSpanArray.Count);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// No need to implement this
        /// </summary>
        /// <param name="sortOptions"></param>
        /// <returns></returns>
        int IVsTextSpanSet.Sort(uint SortOptions)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// No need to implement this
        /// </summary>
        /// <param name="pEnum"></param>
        /// <returns></returns>
        int IVsTextSpanSet.AddFromEnum(IVsEnumTextSpans pEnum)
        {
            return VSConstants.E_NOTIMPL;
        }
        #endregion

        #region IVsTextBuffer Members

        /*The IVsTextBuffer interface is used to provide just general information about the Text Buffer used
        by the Editor. For our sample this is just provided so that the find in files scenario will work 
        properly.  It isn't necesary to implement most of the methods for this
        scenario to work correctly.*/

        public int GetLanguageServiceID(out Guid pguidLangService)
        {
            pguidLangService = Guid.Empty;
            return VSConstants.E_NOTIMPL;
        }

        public int GetLastLineIndex(out int piLine, out int piIndex)
        {
            //Initialize the parameters first
            piLine = 0;
            piIndex = 0;

            int totalLines = editorControl.RichTextBoxControl.Lines.Length;
            if (totalLines > 0)
                piLine = totalLines - 1;
            int lineLen = editorControl.RichTextBoxControl.Lines[piLine].Length;
            piIndex = lineLen >= 1 ? lineLen - 1 : lineLen;

            return VSConstants.S_OK;
        }

        public int GetLengthOfLine(int iLine, out int piLength)
        {
            piLength = 0;
            int totalLines = editorControl.RichTextBoxControl.Lines.Length;

            if (iLine < 0 || iLine >= totalLines)
                return VSConstants.E_INVALIDARG;

            piLength = editorControl.RichTextBoxControl.Lines[iLine].Length;

            return VSConstants.S_OK;
        }

        public int GetLineCount(out int piLineCount)
        {
            piLineCount = editorControl.RichTextBoxControl.Lines.Length;
            return VSConstants.E_NOTIMPL;
        }

        public int GetLineIndexOfPosition(int iPosition, out int piLine, out int piColumn)
        {
            //Initialize the parameters first
            piLine = 0;
            piColumn = 0;

            return VSConstants.E_NOTIMPL;
        }

        public int GetPositionOfLine(int iLine, out int piPosition)
        {
            piPosition = 0;

            return VSConstants.E_NOTIMPL;
        }

        public int GetPositionOfLineIndex(int iLine, int iIndex, out int piPosition)
        {
            piPosition = 0;

            return VSConstants.E_NOTIMPL;
        }

        public int GetSize(out int piLength)
        {
            piLength = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetStateFlags(out uint pdwReadOnlyFlags)
        {
            pdwReadOnlyFlags = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetUndoManager(out IOleUndoManager ppUndoManager)
        {
            ppUndoManager = null;
            return VSConstants.E_NOTIMPL;
        }

        public int InitializeContent(string pszText, int iLength)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int LockBuffer()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int LockBufferEx(uint dwFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reload(int fUndoable)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int SetLanguageServiceID(ref Guid guidLangService)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int SetStateFlags(uint dwReadOnlyFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int UnlockBuffer()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int UnlockBufferEx(uint dwFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reserved1()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reserved2()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reserved3()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reserved4()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reserved5()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reserved6()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reserved7()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reserved8()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reserved9()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reserved10()
        {
            return VSConstants.E_NOTIMPL;
        }
        #endregion

        #region IVsTextView Members

        /*This interface contains methods to manage the Text View i.e. the editor window which is shown to
        the user. For our sample this is just provided so that the find in files scenario will work 
        properly.  It isn't necesary to implement most of the methods for this
        scenario to work correctly.*/

        int IVsTextView.AddCommandFilter(IOleCommandTarget pNewCmdTarg, out IOleCommandTarget ppNextCmdTarg)
        {
            ppNextCmdTarg = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.CenterColumns(int iLine, int iLeftCol, int iColCount)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.CenterLines(int iTopLine, int iCount)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.ClearSelection(int fMoveToAnchor)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.CloseView()
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.EnsureSpanVisible(TextSpan span)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.GetBuffer(out IVsTextLines ppBuffer)
        {
            ppBuffer = (IVsTextLines)this;
            return VSConstants.S_OK;
        }

        int IVsTextView.GetCaretPos(out int piLine, out int piColumn)
        {
            piLine = 0;
            piColumn = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.GetLineAndColumn(int iPos, out int piLine, out int piIndex)
        {
            piLine = 0;
            piIndex = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.GetLineHeight(out int piLineHeight)
        {
            piLineHeight = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.GetNearestPosition(int iLine, int iCol, out int piPos, out int piVirtualSpaces)
        {
            piPos = 0;
            piVirtualSpaces = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.GetPointOfLineColumn(int iLine, int iCol, Microsoft.VisualStudio.OLE.Interop.POINT[] ppt)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.GetScrollInfo(int iBar, out int piMinUnit, out int piMaxUnit,
                        out int piVisibleUnits, out int piFirstVisibleUnit)
        {
            piMinUnit = 0;
            piMaxUnit = 0;
            piVisibleUnits = 0;
            piFirstVisibleUnit = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.GetSelectedText(out string pbstrText)
        {
            pbstrText = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.GetSelection(out int piAnchorLine,
                    out int piAnchorCol,
                    out int piEndLine,
                    out int piEndCol)
        {
            piAnchorLine = 0;
            piAnchorCol = 0;
            piEndLine = 0;
            piEndCol = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.GetSelectionDataObject(out Microsoft.VisualStudio.OLE.Interop.IDataObject ppIDataObject)
        {
            ppIDataObject = null;
            return VSConstants.E_NOTIMPL;
        }

        TextSelMode IVsTextView.GetSelectionMode()
        {
            return TextSelMode.SM_STREAM;
        }

        int IVsTextView.GetSelectionSpan(TextSpan[] pSpan)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.GetTextStream(int iTopLine,
                    int iTopCol,
                    int iBottomLine,
                    int iBottomCol,
                    out string pbstrText
            )
        {
            pbstrText = null;
            return VSConstants.E_NOTIMPL;
        }

        IntPtr IVsTextView.GetWindowHandle()
        {
            return IntPtr.Zero;
        }

        int IVsTextView.GetWordExtent(int iLine,
                    int iCol,
                    uint dwFlags,
                    TextSpan[] pSpan
            )
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.HighlightMatchingBrace(uint dwFlags, uint cSpans, TextSpan[] rgBaseSpans)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.Initialize(IVsTextLines pBuffer,
                IntPtr hwndParent,
                uint InitFlags,
                INITVIEW[] pInitView
            )
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.PositionCaretForEditing(int iLine, int cIndentLevels)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.RemoveCommandFilter(IOleCommandTarget pCmdTarg)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.ReplaceTextOnLine(int iLine,
                int iStartCol,
                int iCharsToReplace,
                string pszNewText,
                int iNewLen
            )
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.RestrictViewRange(int iMinLine, int iMaxLine, IVsViewRangeClient pClient)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.SendExplicitFocus()
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.SetBuffer(IVsTextLines pBuffer)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.SetCaretPos(int iLine, int iColumn)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.SetScrollPosition(int iBar, int iFirstVisibleUnit)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.SetSelection(int iAnchorLine, int iAnchorCol, int iEndLine, int iEndCol)
        {
            // first set start location
            int startPosition = editorControl.GetIndexFromLineAndColumn(iAnchorLine, iAnchorCol);
            if (startPosition < 0)
                return VSConstants.E_INVALIDARG;
            editorControl.RichTextBoxControl.SelectionStart = startPosition;

            // now set the length of the selection
            int endPosition = editorControl.GetIndexFromLineAndColumn(iEndLine, iEndCol);
            if (endPosition < 0)
                return VSConstants.E_INVALIDARG;
            int length = endPosition - editorControl.RichTextBoxControl.SelectionStart;
            if (length >= 0)
                editorControl.RichTextBoxControl.SelectionLength = length;
            else
                editorControl.RichTextBoxControl.SelectionLength = 0;
            return VSConstants.S_OK;
        }

        int IVsTextView.SetSelectionMode(TextSelMode iSelMode)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.SetTopLine(int iBaseLine)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.UpdateCompletionStatus(IVsCompletionSet pCompSet, uint dwFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.UpdateTipWindow(IVsTipWindow pTipWindow, uint dwFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextView.UpdateViewFrameCaption()
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IVsTextViewEvents Members

        /*This interface is used as a notifier for the events that are occurring on the Text View.
        For our sample this is just provided so that the find in files scenario will work 
        properly.  It isn't necesary to implement any of the methods. */

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pView")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "iOldLine")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "iNewLine")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "p")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "i")]
        public void OnChangeCaretLine(IVsTextView pView, int iNewLine, int iOldLine)
        {
            //Not Implemented
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pView")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "iVisibleUnits")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "iMinUnit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "iMaxUnits")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "iFirstVisibleUnit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "iBar")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "p")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "i")]
        public void OnChangeScrollInfo(IVsTextView pView,
                    int iBar,
                    int iMinUnit,
                    int iMaxUnits,
                    int iVisibleUnits,
                    int iFirstVisibleUnit
            )
        {
            //Not Implemented
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "p")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pView")]
        public void OnKillFocus(IVsTextView pView)
        {
            //Not Implemented
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pView")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pBuffer")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "p")]
        public void OnSetBuffer(IVsTextView pView, IVsTextLines pBuffer)
        {
            //Not Implemented
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "p")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pView")]
        public void OnSetFocus(IVsTextView pView)
        {
            //Not Implemented
        }
        #endregion

        #region IVsCodeWindow Members

        /* This interface is used for hosting of the views for a text buffer. Multiple views can be enclosed
        with the code window. 
        Since our editor support the LOGVIEWID_TextView logical view, we need to implment this interface 
        for find in files scenario to work properly.  
        It isn't necesary to implement most of the methods for this scenario to work correctly. */

        int IVsCodeWindow.GetPrimaryView(out IVsTextView ppView)
        {
            ppView = (IVsTextView)this;
            return VSConstants.S_OK;
        }

        int IVsCodeWindow.GetSecondaryView(out IVsTextView ppView)
        {
            ppView = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsCodeWindow.GetLastActiveView(out IVsTextView ppView)
        {
            ppView = (IVsTextView)this;
            return VSConstants.S_OK;
        }

        int IVsCodeWindow.Close()
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsCodeWindow.GetBuffer(out IVsTextLines ppBuffer)
        {
            ppBuffer = (IVsTextLines)this;
            return VSConstants.S_OK;
        }

        int IVsCodeWindow.GetEditorCaption(READONLYSTATUS dwReadOnly, out string pbstrEditorCaption)
        {
            pbstrEditorCaption = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsCodeWindow.GetViewClassID(out Guid pclsidView)
        {
            pclsidView = Guid.Empty;
            return VSConstants.E_NOTIMPL;
        }

        int IVsCodeWindow.SetBaseEditorCaption(string[] pszBaseEditorCaption)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsCodeWindow.SetBuffer(IVsTextLines pBuffer)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsCodeWindow.SetViewClassID(ref Guid clsidView)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IVsTextLines Members

        /* This interface is used for a line-oriented access to the contents of the text buffer. 
         For our sample all methods return E_NOTIMPL. This is needed for Find/Replace to work approproiately.
         The Caller just does a QueryInterface for this particular interface, but does not use any 
         of the methods available on the interface*/

        int IVsTextLines.AdviseTextLinesEvents(IVsTextLinesEvents pSink, out uint pdwCookie)
        {
            pdwCookie = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.UnadviseTextLinesEvents(uint dwCookie)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.CanReplaceLines(int iStartLine,
                    int iStartIndex,
                    int iEndLine,
                    int iEndIndex,
                    int iNewLen
            )
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.CopyLineText(int iStartLine,
                    int iStartIndex,
                    int iEndLine,
                    int iEndIndex,
                    IntPtr pszBuf,
                    ref int pcchBuf
            )
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.CreateEditPoint(int iLine, int iIndex, out Object ppEditPoint)
        {
            ppEditPoint = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.CreateLineMarker(int iMarkerType,
                    int iStartLine,
                    int iStartIndex,
                    int iEndLine,
                    int iEndIndex,
                    IVsTextMarkerClient pClient,
                    IVsTextLineMarker[] ppMarker
            )
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.CreateTextPoint(int iLine, int iIndex, out Object ppTextPoint)
        {
            ppTextPoint = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.EnumMarkers(int iStartLine,
                    int iStartIndex,
                    int iEndLine,
                    int iEndIndex,
                    int iMarkerType,
                    uint dwFlags,
                    out IVsEnumLineMarkers ppEnum
            )
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.FindMarkerByLineIndex(int iMarkerType,
                    int iStartingLine,
                    int iStartingIndex,
                    uint dwFlags,
                    out IVsTextLineMarker ppMarker
            )
        {
            ppMarker = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.GetMarkerData(int iTopLine, int iBottomLine, MARKERDATA[] pMarkerData)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.ReleaseMarkerData(MARKERDATA[] pMarkerData)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.GetLineData(int iLine, LINEDATA[] pLineData, MARKERDATA[] pMarkerData)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.ReleaseLineData(LINEDATA[] pLineData)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.GetLineDataEx(uint dwFlags,
                    int iLine,
                    int iStartIndex,
                    int iEndIndex,
                    LINEDATAEX[] pLineData,
                    MARKERDATA[] pMarkerData
            )
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.ReleaseLineDataEx(LINEDATAEX[] pLineData)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.IVsTextLinesReserved1(int iLine, LINEDATA[] pLineData, int fAttributes)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.GetLineText(int iStartLine,
                    int iStartIndex,
                    int iEndLine,
                    int iEndIndex,
                    out string pbstrBuf
            )
        {
            pbstrBuf = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.GetPairExtents(TextSpan[] pSpanIn, TextSpan[] pSpanOut)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.ReplaceLines(int iStartLine,
                    int iStartIndex,
                    int iEndLine,
                    int iEndIndex,
                    IntPtr pszText,
                    int iNewLen,
                    TextSpan[] pChangedSpan
            )
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.ReplaceLinesEx(uint dwFlags,
                    int iStartLine,
                    int iStartIndex,
                    int iEndLine,
                    int iEndIndex,
                    IntPtr pszText,
                    int iNewLen,
                    TextSpan[] pChangedSpan
            )
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsTextLines.ReloadLines(int iStartLine,
                    int iStartIndex,
                    int iEndLine,
                    int iEndIndex,
                    IntPtr pszText,
                    int iNewLen,
                    TextSpan[] pChangedSpan
            )
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}
