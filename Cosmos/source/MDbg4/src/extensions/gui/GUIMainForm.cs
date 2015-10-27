//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Text;


using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;  

namespace Microsoft.Samples.Tools.Mdbg.Extension
{

    // Main gui form.
    // We make it sealed because we implement the IMDbgIO interface.
    // See FxCop rule: http://www.gotdotnet.com/team/fxcop/docs/rules.aspx?url=/Design/InterfaceMethodsShouldBeCallableByChildTypes.html
    public sealed class MainForm : System.Windows.Forms.Form, IMDbgIO, IMDbgIO2
    {
        private IContainer components;

        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem QuitUICmd;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem windowsTileCmd;
        private System.Windows.Forms.MenuItem windowCascadeCmd;
        private System.Windows.Forms.MenuItem testNewCmd;
        private System.Windows.Forms.StatusBar statusBar1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.TextBox cmdInput;
        private System.Windows.Forms.RichTextBox cmdHistory;
        private System.Windows.Forms.MenuItem AboutCmd;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.MenuItem menuItem2;
        private MenuItem menuItemView;
        private MenuItem menuItemViewOpen;
        private MenuItem menuItemViewClose;
        private MenuItem menuItem7;
        private MenuItem menuItemCommands;
        private MenuItem menuItemLaunch;
        private MenuItem menuItem6;
        private MenuItem menuItemAttach;
        private MenuItem menuItemDetach;
        private MenuItem menuItemKill;
        private MenuItem menuItem4;
        private MenuItem menuItemViewIlOrSource;
        private System.Windows.Forms.MenuItem breakCmd;

        public MainForm(IMDbgShell ui)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            UpdateShowIlMenu();
            Debug.Assert(ui!=null);
            m_ui = ui;
            
            cmdHistory.Text=" ";
            SourceViewerForm.ClearSourceFiles();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }

                if (m_initComplete != null)
                {
                    m_initComplete.Dispose();
                }

                if (m_inputDoneEvent != null)
                {
                    m_inputDoneEvent.Dispose();
                }

                if (m_inputEvent != null)
                {
                    m_inputEvent.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        //#region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.testNewCmd = new System.Windows.Forms.MenuItem();
            this.QuitUICmd = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.breakCmd = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItemLaunch = new System.Windows.Forms.MenuItem();
            this.menuItemAttach = new System.Windows.Forms.MenuItem();
            this.menuItemDetach = new System.Windows.Forms.MenuItem();
            this.menuItemKill = new System.Windows.Forms.MenuItem();
            this.menuItemView = new System.Windows.Forms.MenuItem();
            this.menuItemViewOpen = new System.Windows.Forms.MenuItem();
            this.menuItemViewClose = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.windowsTileCmd = new System.Windows.Forms.MenuItem();
            this.windowCascadeCmd = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItemCommands = new System.Windows.Forms.MenuItem();
            this.AboutCmd = new System.Windows.Forms.MenuItem();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cmdHistory = new System.Windows.Forms.RichTextBox();
            this.cmdInput = new System.Windows.Forms.TextBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItemViewIlOrSource = new System.Windows.Forms.MenuItem();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem2,
            this.menuItemView,
            this.menuItem5,
            this.menuItem3});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.testNewCmd,
            this.QuitUICmd});
            this.menuItem1.Text = "&File";
            // 
            // testNewCmd
            // 
            this.testNewCmd.Index = 0;
            this.testNewCmd.Text = "&Open Source ...";
            this.testNewCmd.Click += new System.EventHandler(this.testNewCmd_Click);
            // 
            // QuitUICmd
            // 
            this.QuitUICmd.Index = 1;
            this.QuitUICmd.Text = "&Quit";
            this.QuitUICmd.Click += new System.EventHandler(this.QuitUICmd_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.breakCmd,
            this.menuItem6,
            this.menuItemLaunch,
            this.menuItemAttach,
            this.menuItemDetach,
            this.menuItemKill,
            this.menuItem4,
            this.menuItemViewIlOrSource});
            this.menuItem2.Text = "&Debug";
            // 
            // breakCmd
            // 
            this.breakCmd.Index = 0;
            this.breakCmd.Text = "&Break";
            this.breakCmd.Click += new System.EventHandler(this.breakCmd_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 1;
            this.menuItem6.Text = "-";
            // 
            // menuItemLaunch
            // 
            this.menuItemLaunch.Index = 2;
            this.menuItemLaunch.Text = "Launch ...";
            this.menuItemLaunch.Click += new System.EventHandler(this.menuItemLaunch_Click);
            // 
            // menuItemAttach
            // 
            this.menuItemAttach.Index = 3;
            this.menuItemAttach.Text = "Attach ...";
            this.menuItemAttach.Click += new System.EventHandler(this.menuItemAttach_Click);
            // 
            // menuItemDetach
            // 
            this.menuItemDetach.Index = 4;
            this.menuItemDetach.Text = "Detach";
            this.menuItemDetach.Click += new System.EventHandler(this.menuItemDetach_Click);
            // 
            // menuItemKill
            // 
            this.menuItemKill.Index = 5;
            this.menuItemKill.Text = "Kill";
            this.menuItemKill.Click += new System.EventHandler(this.menuItemKill_Click);
            // 
            // menuItemView
            // 
            this.menuItemView.Index = 2;
            this.menuItemView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemViewOpen,
            this.menuItemViewClose,
            this.menuItem7});
            this.menuItemView.Text = "Tools";
            this.menuItemView.Click += new System.EventHandler(this.menuItemView_Click);
            // 
            // menuItemViewOpen
            // 
            this.menuItemViewOpen.Index = 0;
            this.menuItemViewOpen.Text = "Open &All";
            this.menuItemViewOpen.Click += new System.EventHandler(this.menuItemViewOpen_Click);
            // 
            // menuItemViewClose
            // 
            this.menuItemViewClose.Index = 1;
            this.menuItemViewClose.Text = "Close All";
            this.menuItemViewClose.Click += new System.EventHandler(this.menuItemViewClose_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 2;
            this.menuItem7.Text = "-";
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 3;
            this.menuItem5.MdiList = true;
            this.menuItem5.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.windowsTileCmd,
            this.windowCascadeCmd});
            this.menuItem5.Text = "&Window";
            // 
            // windowsTileCmd
            // 
            this.windowsTileCmd.Index = 0;
            this.windowsTileCmd.Text = "&Tile";
            // 
            // windowCascadeCmd
            // 
            this.windowCascadeCmd.Index = 1;
            this.windowCascadeCmd.Text = "&Cascade";
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 4;
            this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemCommands,
            this.AboutCmd});
            this.menuItem3.Text = "&Help";
            // 
            // menuItemCommands
            // 
            this.menuItemCommands.Index = 0;
            this.menuItemCommands.Text = "Commands";
            this.menuItemCommands.Click += new System.EventHandler(this.menuItemCommands_Click);
            // 
            // AboutCmd
            // 
            this.AboutCmd.Index = 1;
            this.AboutCmd.Text = "About...";
            this.AboutCmd.Click += new System.EventHandler(this.AboutCmd_Click);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 411);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(568, 22);
            this.statusBar1.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmdHistory);
            this.panel1.Controls.Add(this.cmdInput);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 195);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.panel1.Size = new System.Drawing.Size(568, 216);
            this.panel1.TabIndex = 4;
            // 
            // cmdHistory
            // 
            this.cmdHistory.BackColor = System.Drawing.SystemColors.ControlLight;
            this.cmdHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdHistory.Location = new System.Drawing.Point(10, 10);
            this.cmdHistory.Name = "cmdHistory";
            this.cmdHistory.ReadOnly = true;
            this.cmdHistory.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.cmdHistory.Size = new System.Drawing.Size(548, 186);
            this.cmdHistory.TabIndex = 9;
            this.cmdHistory.TabStop = false;
            this.cmdHistory.Text = "";
            // 
            // cmdInput
            // 
            this.cmdInput.AcceptsReturn = true;
            this.cmdInput.AcceptsTab = true;
            this.cmdInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cmdInput.Location = new System.Drawing.Point(10, 196);
            this.cmdInput.Multiline = true;
            this.cmdInput.Name = "cmdInput";
            this.cmdInput.Size = new System.Drawing.Size(548, 20);
            this.cmdInput.TabIndex = 8;
            this.cmdInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmdInput_KeyPress);
            this.cmdInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cmdInput_KeyDown);
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 189);
            this.splitter1.Name = "splitter1";
            this.splitter1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitter1.Size = new System.Drawing.Size(568, 6);
            this.splitter1.TabIndex = 0;
            this.splitter1.TabStop = false;
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 6;
            this.menuItem4.Text = "-";
            // 
            // menuItemViewIlOrSource
            // 
            this.menuItemViewIlOrSource.Index = 7;
            this.menuItemViewIlOrSource.Text = "View Il";
            this.menuItemViewIlOrSource.Click += new System.EventHandler(this.menuItem8_Click);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(568, 433);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusBar1);
            this.IsMdiContainer = true;
            this.Menu = this.mainMenu1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            this.Text = "GUI: Simple MDbg Extension";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }
        //#endregion

        // Set title for main form.
        // This also displays key status.
        // The mainform is an MDI window, source viewers are the children windows. WinForms will
        // automatically append the parent window (us) with the active MDI child.
        void SetTitle(bool fStopped)
        {
            string stState = "";
            if (GuiExtension.Shell.Debugger.Processes.HaveActive)
            {
                if (!fStopped)
                {
                    stState = "(RUNNING) ";
                }
                else 
                {
                    stState = "(STOPPED) ";
                }
            }

            this.Text = stState + "GUI: Simple MDbg Extension";
        }

        private class HistoryList
        {
            public HistoryList(int size)
            {
                Debug.Assert(size>1);
                m_historySize = size;
                m_history = new string[m_historySize];                
                Debug.Assert(m_historyPointer == 0);
            }

            public void Add(string text)
            {
                Debug.Assert(text!=null);
                m_currPointer = NextPosition(m_currPointer,1);
                m_history[m_currPointer]=text;
                m_historyPointer=0;
            }

            public string GetPreviousText()
            {
                string res = m_history[NextPosition(m_currPointer,-m_historyPointer)];
                if(res!=null)
                    m_historyPointer = NextPosition(m_historyPointer,+1);
                return res;
            }

            public string GetNextText()
            {
                string res=null;
                if(m_historyPointer>0)
                {
                    m_historyPointer--;
                    res = m_history[NextPosition(m_currPointer,-m_historyPointer)];
                    Debug.Assert(res!=null);
                }
                return res;
            }

            private int NextPosition(int counter,int add)
            {
                
                int r = (counter+add)%m_historySize;
                if(r<0)
                    r+=m_historySize;
                
                Debug.Assert(r>=0 && r<m_historySize);
                return r;
            }

            private string[] m_history;
            private int m_historySize;
            private int m_currPointer;
            private int m_historyPointer;
        }

        HistoryList m_historyList = new HistoryList(15);

        // Trap input to the command window so that we can keep the GUI updated.
        private void cmdInput_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if(e.KeyChar == (char)13)
            {
                e.Handled=true;
                m_historyList.Add(cmdInput.Text);
                AsyncProcessEnteredText(cmdInput.Text);
                cmdInput.Text="";
            }
        }

        private void cmdInput_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            string historyText = null;
            switch(e.KeyCode)
            {
            case Keys.Up:
                historyText = m_historyList.GetPreviousText();
                break;
            case Keys.Down:
                historyText = m_historyList.GetNextText();
                break;
            default:
                return;
            }
            e.Handled=true;
            if(historyText!=null)
                cmdInput.Text=historyText;

        }


        private void MainForm_Load(object sender, System.EventArgs e)
        {
            this.KeyPreview = true;
            Activate();


            this.InitHelperWindows();
        }
        
        private void MainForm_Activated(object sender, System.EventArgs e)
        {
            if(m_savedIO!=null) 
                return;         //this needs to be executed only when we initialize the form first time.
            
            Debug.Assert(m_savedIO==null);
            m_savedIO = m_ui.IO;
            Debug.Assert(m_savedIO!=null);
            m_ui.IO=this;
            
            m_initComplete.Set();
        }

        private void MainForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
            case Keys.F5:
                if (e.Shift)
                {
                    StopDebugging("kill", "terminate the debuggee");
                }
                else
                {
                    AsyncProcessEnteredText("go");
                }
                e.Handled = true;
                break;           
            }

            // Other key comobos are handled by the source-window they're done in.
            // We have focus issues. This main form will always get key notifications (because it's top level).
            // The individual source windows may not have focus (especially since the focus may commonly 
            // be in the input text box). 
            // But certain "global" operations (like stepping) shouldn't require source-window focus. 
            // This may set e.Handled=true;
            SourceViewerBaseForm.HandleGlobalKeyCommand(e);
            
            
        }

        private void AboutCmd_Click(object sender, System.EventArgs e)
        {
            using (AboutForm af = new AboutForm())
            {
                af.ShowDialog();
            }
        }

        
        private void testNewCmd_Click(object sender, System.EventArgs e)
        {
            
            // File dialogs must be on STA.
            DialogResult res = openFileDialog.ShowDialog(this);
            if(res == DialogResult.OK)
            {
                SourceViewerForm.GetSourceFile(this,openFileDialog.FileName);
            }   
        }

        private void QuitUICmd_Click(object sender, System.EventArgs e)
        {
            Visible=false;
        }

        private void breakCmd_Click(object sender, System.EventArgs e)
        {
            DoBreak();
        }

        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try 
            {
                CloseGui();
            } 
            catch(Exception ex)
            {
                MessageBoxOptions mbo = new MessageBoxOptions();
                if (Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft)
                    mbo = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
                MessageBox.Show(this,ex.Message,null,MessageBoxButtons.OK,MessageBoxIcon.Stop,MessageBoxDefaultButton.Button1, mbo);
                e.Cancel = true;
                return;
            }
        }

        // CloseGui closes GUI and returns control back to the shell
        // Called on UI thread.
        public void CloseGui()
        {
            if (m_ui.IO != this)
            {
                throw new InvalidOperationException("Cannot close GUI. IO stream has changed. ");
            }

            m_ui.IO=m_savedIO;
            
            // we need to kick-off input-loop (RunInputLoop) from blocking ReadCommand call.
            // so the trick is we issue a replace IO and issue harmless command "echo" which will
            // just print a message to new IO.            
            this.AsyncProcessEnteredText("echo GUI closed");
        }


        #region Debugger Helper Windows
        // Open all helper windows.
        private void menuItemViewOpen_Click(object sender, EventArgs e)
        {
            foreach (HelperWindowMenuItem i in this.m_HelperWindows)
            {
                i.Show();
            }
        }

        // Close all helper windows.
        private void menuItemViewClose_Click(object sender, EventArgs e)
        {
            foreach (HelperWindowMenuItem i in this.m_HelperWindows)
            {
                Form f = i.CurrentForm;
                if (f != null)
                {
                    f.Close();
                }
            }
        }

        // Initial all the helper windows (eg, the various debugging windows like callstack, module-list, thread-list, etc
        // that help us inspect the process).
        // This will also populate the MainForm's "View" menu.
        void InitHelperWindows()
        {
            // ViewMenu already contains OpenAll + CloseAll entries.
            // Now we add the menu items for the actual tool windows.

            this.m_HelperWindows = new HelperWindowMenuItem[] {
                new HelperWindowMenuItem(this, "&Callstack", typeof(gui.Callstack)),
                new HelperWindowMenuItem(this, "&Locals",    typeof(gui.AutoWatchWindow)),
                new HelperWindowMenuItem(this, "&Modules",   typeof(gui.ModuleWindow)),
                new HelperWindowMenuItem(this, "&Threads",   typeof(gui.ThreadWindow)),
                new HelperWindowMenuItem(this, "&QuickWatch",typeof(gui.QuickViewWindow))
            };
        }

        // Refresh all live helper windows.
        void RefreshHelperWindows()
        {
            foreach (HelperWindowMenuItem item in this.m_HelperWindows)
            {
                gui.DebuggerToolWindow w = item.CurrentForm;
                if (w != null)
                {
                    w.RefreshToolWindow();
                }
            } // foreach
        }

        // List of all possible helper windows. This includes both active + inactive helper windows.
        HelperWindowMenuItem[] m_HelperWindows;

        // Add a menuitem to the view menu. This lets helper windows register on the "View" menu.
        public void AddToViewMenu(MenuItem item)
        {
            this.menuItemView.MenuItems.Add(item);
        }

        // Helper class for menu items for helper windows.
        // This contains the View Menu Item's OnClick handler and associates it to 
        // a helper window type. This contains enough information to create new instances of helper
        // windows (in case the old instance get closed)
        class HelperWindowMenuItem
        {
            // We pass in a type of the form to create rather than encode it as a generic parameter
            // so that we can have an array of the HelperWindowMenuItem.
            public HelperWindowMenuItem(MainForm mainForm, string name, System.Type tForm)
            {
                Debug.Assert(tForm.IsSubclassOf(typeof(gui.DebuggerToolWindow)));
                this.m_mainForm = mainForm;
                this.m_typeHelperForm = tForm;

                EventHandler handler = new EventHandler(this.OnClick);

                using (MenuItem item = new MenuItem(name, handler))
                {
                    this.m_mainForm.AddToViewMenu(item);
                }
            }

            // Handler for when menu item is clicked
            void OnClick(object sender, EventArgs e)
            {
                Show();
            }

            public void Show()
            {
                // If they closed the window, create a new instance of it..
                if ((m_helperForm == null) || m_helperForm.IsDisposed)
                {
                    CreateNewForm();
                }
                // Show as a *modeless* dialog. This means this call will return immediately
                // and the helper dialog will be open as a separate window.
                m_helperForm.Show();
                m_helperForm.Activate();
                m_helperForm.RefreshToolWindow();

            }

            void CreateNewForm()
            {
                // Generics (via the new() constraint) only lets us invoke a default ctor.
                m_helperForm = (gui.DebuggerToolWindow)System.Activator.CreateInstance(this.m_typeHelperForm, new object[] { m_mainForm });
            }

            MainForm m_mainForm;

            // Type of helper to create. If the helper window gets closed, this lets us recreate it.
            System.Type m_typeHelperForm;

            // Dialog for this menu item.
            gui.DebuggerToolWindow m_helperForm;

            // Get current instantation of a form. This may be null if the form is closed.
            // This instance may get recycled.
            public gui.DebuggerToolWindow CurrentForm
            {
                get
                {
                    if (m_helperForm == null) return null;
                    if (m_helperForm.IsDisposed) return null;
                    return m_helperForm;
                }
            }
        } // end HelperWindowMenuItem

        #endregion Debugger Helper Windows


        #region Plumbing For Input / Output
        //-----------------------------------------------------------------------------
        // This contains plumbing to connect the MDbg Shell Input + Output streams
        // to the Gui. This is done by having the GUI implement the IMDbgIO interface.
        // The GUI also ensures that operations occur on the thread that owns the 
        // underlying handle for the main window.
        //-----------------------------------------------------------------------------

        #region Plumbing for Input

        //////////////////////////////////////////////////////////////////////////////////
        //
        // Implementation part
        //
        //////////////////////////////////////////////////////////////////////////////////


        // The m_process tracks the "active" process. It's null if there is no active process
        // or if the process is running. This simplifies users because in both cases, the process
        // is not accessible.
        //
        // A note abou thread safety:
        // This is only set on the UI thread. 
        // The UI thread will pass it as a parameter via the fpWorker delegate to the worker thread.
        // Since the fpWorker is synchronous, the UI thread blocks. Thus it's only read when:
        // 1. on the UI thread 
        // 2. on the worker thread in an fpWorker callback, while the UI thread is blocked waiting
        //    for the fpWorker to finish. 
        // Thus on all cases, the reads are safe against writes.
        //
        // Only the UI thread can issue Go commands (via F5, or the text input box); so the UI thread knows if
        // the debuggee may be running ; and another thread can't change this underneath it.
        // Technically, any Mdbg extension could spin up its own worker thread and Stop/Go the debuggee
        // without the UI knowing. There's no good fix there.        
        MDbgProcess m_process;

        // Gui elements can only access process if it exists and is stopped.        
        // Only called on the UI thread.
        public bool IsProcessStopped
        {
            get { return m_process != null; }
        }

        

        // Implements IMDbgIO.
        // Put the GUI in "input" mode and block waiting for a command to be entered.
        // This gets called by the MDbg main loop. Other threads will queue items into this worker.
        // This is on the Command-thread.
        bool IMDbgIO.ReadCommand(out string command)
        {   
            // Wrapper to ensure UI is updates for Stop/Go are balanced.
            // Update the UI to let it know that we're ready for input.
            BeginInvoke(new MethodInvoker(PreReadCommand));
                        
            bool f = ReadCommandWorker(out command);
            
            if (f)
            {
                // Update UI on way out to notify it we're about to execute this command.
                WritePrompt();
                WriteOutput(m_InputWriteFormat, command + "\n");
            }
            return f;
        }

        // Real worker to get commands and feed them back to the MDbg command loop.
        // This is on the Command-thread.
        bool ReadCommandWorker(out string command)
        {
            while(true)
            {
                // If we already have queued up commands, execute them immediately.
                lock (m_cmdQueue)
                {
                    if (m_cmdQueue.Count > 0)
                    {
                        command = m_cmdQueue.Dequeue();                        
                        return true;
                    }
                }

                // Now actually wait until a command is entered.
                // This is set by either:
                // - the UI sends the MDbg engine commands (via AsyncProcessEnteredText)
                //     this may come from UI elements (such as menu commands), or from edit input control.
                //     representing the console, or closing the GUI 
                //     This may resume the process (eg, "step", "go").
                // - Synchronous worker commands (which are not supposed to be able to resume the process).
                //     These worker functions let the UI thread gather information from ICorDebug (since the
                //     STA UI thread can't call the MTA ICorDebug directly)
                // 
                m_inputEvent.WaitOne();

                // Did we get a delegate work item? If yes, we can process it right now.
                if (m_fpWorker != null)
                {
                    try
                    {
                        Debug.Assert(m_process != null);
                        m_fpWorker(m_process);
                    }
                    catch
                    {
                        Debug.Assert(false, "Unexpected exception on worker thread.");
                        // rethrow it on original thread?
                    }

                    m_fpWorker = null;
                    m_inputDoneEvent.Set();
                    continue; // loop around and try again.
                }
                
            } // loop around.
        }


        // This must be called on the thread that owns the MainForm control's underlying window handle. (UI Thread)
        // This will put the GUI in "Break" (aka Input) mode. 
        private void PreReadCommand()
        {
            try 
            {
                SetCommandInputState(true);
            } 
            catch(Exception e)
            {
                WriteOutput(MDbgOutputConstants.StdError,e.GetBaseException().Message);
            }
        }

        // Coordinates between the mdbg shell input and the gui.
        // This also let GUI components send string commands to the underlying shell.
        // This corresponds to ReadCommand.  ReadCommand will block waiting for a command
        // to be entered here.
        //
        // This must be called on the UI thread.
        //
        // This call WILL NOT BLOCK waiting for the command to execute. 
        // Thus do not assume the command has executed when we return.        
        // Ideally, this should be the last thing a UI thread calls in an event handler.
        // 
        // This should only happen in response to end-user actions (such as pressing key strokes or 
        // entering text in the shell input window).
        // 
        // Input can contain multiple commands separated by newlines.
        //         
        // When the command completes, the command thread will reping the UI to update.
        public void AsyncProcessEnteredText(string input)
        {
            // Only send commands if we're stopped. 
            // We could techically queue the command to execute later; but that may be a bad user experience.
            // Afterall, "later" may be random to them, and the command may run at a random future time.
            if (!this.cmdInput.Enabled)
            {
                MessageBox.Show("Can't do command \n  " + input + "\n while debuggee is running. Use 'Debug|Break' to stop debuggee.",
                     "Can't issue command while running.");
                return;
            }
            
            string [] lines = input.Replace("\r", "").Split('\n');
            lock (m_cmdQueue)
            {
                foreach (string line in lines)
                {                       
                    m_cmdQueue.Enqueue(line);
                }
            }
                        
            // Tell the UI that we're about to start running.
            SetCommandInputState(false);

            // The MDbg main loop's ReadCommand is waiting on this event.
            m_inputEvent.Set();            
        }

        // Format control word for text that we write out for input.
        const string m_InputWriteFormat = "<input>";

        // Format control word for informational text the gui spews to the history window.
        const string m_MetaInfoWriteFormat = "<meta>";
        public static string MetaInfoOutputConstant
        {
            get { return m_MetaInfoWriteFormat; }
        }


        // Coordinates between the mdbg shell input and the gui.
        // This also let GUI components send delegate commands to the underlying shell.
        // This allows an STA UI thread to access MTA ICorDebug objects.
        //
        // This must be called on the UI thread.
        public delegate void FPWorker(MDbgProcess process);
        public void ExecuteOnWorkerThreadIfStoppedAndBlock(FPWorker workerFunction)
        {
            // We should not be issuing commands while the debuggee is running.
            if (!IsProcessStopped)
            {
                // May deadlock if called when process is running (because worker thread is only
                // available when process is stopped).
                // Only the UI thread can Go commands; so the UI thread knows if
                // the debuggee may be running; and another thread can't change this underneath it.
                
                // Protect us.  
                return;
            }
            
                        
            m_fpWorker = workerFunction;

            // The MDbg main loop's ReadCommand is waiting on this event.
            m_inputEvent.Set();

            // Wait for input to finish
            // This won't pump. (but we wouldn't mind if it pumped WM_paint) 
            // Thus we must be gauranteed that the worker won't block.
            m_inputDoneEvent.WaitOne();
        }

        #endregion Plumbing for Input

        #region Plumbing for Output

        //////////////////////////////////////////////////////////////////////////////////
        //
        // Input Output Functions
        //
        //////////////////////////////////////////////////////////////////////////////////

        // This can be called on any thread.
        private void WritePrompt()
        {
            string s = "";

            try
            {
                if (GuiExtension.Shell.Debugger.Processes.HaveActive)
                    s = "[t#:" + GuiExtension.Shell.Debugger.Processes.Active.Threads.Active.Number + "] ";
            }
            catch (MDbgException)
            {
                s = "";
            }
            WriteOutputNoNewLine(MDbgOutputConstants.StdOutput, s + TEXT_PROMPT_STRING, 0, 0);
        }

        // Write string to the MDbg console.
        // txt - string to write. May contain newlines.
        // outputType -  control string for what type of output (error, std, etc)        
        // highlightStart - index into string txt to begin highlighting at.
        // highlightLen - number of characters to highlight.
        //
        // This may be called on either the UI thread or the Mdbg Worker Thread.
        private void WriteOutputNoNewLine(string outputType, string txt, int highlightStart, int highlightLen)
        {
            // RichTextBox will strip all '\r' values. That will make the Text out of sync 
            // with highlightStart and highlightEnd.             
            
            int highlightStartAdjust = highlightStart;
            int highlightLenAdjust = highlightLen;

            for(int idx = txt.IndexOf('\r'); idx != -1; idx = txt.IndexOf('\r', idx+ 1))
            {
                if (idx < highlightStart)
                {
                    highlightStartAdjust--;
                }
                else if (idx < highlightStart + highlightLen)
                {
                    highlightLenAdjust--;
                }
            }

            string txtAdjust = txt;
            SyncWriter sw = new SyncWriter(this, outputType, txtAdjust, highlightStartAdjust, highlightLenAdjust);

            // Async callback. We can't block the worker thread. These will get queued up in order.
            try
            {
                BeginInvoke(new MethodInvoker(sw.InternalWriteOutput));
            }
            catch (InvalidOperationException)
            {
                // If we can't write out to the GUI, at least write out to the cached IO.  
                this.m_savedIO.WriteOutput(outputType, txt);
            }
        }

        // Implements IMDbgIO.
        // Writes output to the shell output window.
        // This is called on the Mdbg worker thread.
        public void WriteOutput(string outputType, string output)
        {
            WriteOutput(outputType, output, 0, 0);
        }

        // Implements IMDbgIO2 version of WriteOutput which allows highlighting.
        public void WriteOutput(string outputType, string message, int highlightStart, int highlightLen)
        {            
            WriteOutputNoNewLine(outputType, message, highlightStart, highlightLen);
        }

        // Helper class to ensure output gets called on correct thread.
        private class SyncWriter
        {
            public SyncWriter(MainForm form, string outputType, string txt, int highlightStart, int highlightLen)
            {
                Debug.Assert(txt != null && form != null);

                if (highlightLen == Int32.MaxValue)
                {
                    highlightLen = txt.Length;
                }

                m_txt = txt;
                m_form = form;
                m_type = outputType;
                m_highlightStart = highlightStart;
                m_highlightLen = highlightLen;
            }

            int m_highlightStart;
            int m_highlightLen;

            
            Font m_boldFont;
            
            // Must be called on the thread that owns the main window's underlying window handle.
            public void InternalWriteOutput()
            {
                RichTextBox box = m_form.cmdHistory;
            
                int start = box.TextLength;
                box.AppendText(m_txt); // places at end, regardless of cursor.
                
                // Format based of type of output.
                // If we wanted to expand on this, we could have a hash from m_type to {Font, Color}. 
                Color c = Color.Empty;
                bool bold = false;
                if (m_type == m_InputWriteFormat)
                {
                    c = Color.Blue;
                    bold = true;
                }
                else if (m_type == m_MetaInfoWriteFormat)
                {
                    c = Color.Purple;
                    bold = true;
                }
                else if (m_type == MDbgOutputConstants.StdError)
                {
                    c = Color.Red;
                }
                else if (m_type == MDbgOutputConstants.Ignore)
                {
                    c = Color.Green;
                }
                
                // Now apply any formatting.
                if ((c != Color.Empty) || bold)
                {
                    box.Select(start, m_txt.Length);
                    if (c != Color.Empty)
                    {
                        box.SelectionColor = c;
                    }
                    if (bold)
                    {
                        // Lazily create bold font if we don't already have it.
                        if (m_boldFont == null)
                        {
                            Font currentFont = box.SelectionFont;

                            m_boldFont = new Font(
                               currentFont.FontFamily,
                               currentFont.Size,
                               FontStyle.Bold
                            );                            
                        }
                        box.SelectionFont = m_boldFont;
                    }

                    box.SelectionLength = 0; // cancel selection.
                }

                // Deal with highlights
                if (m_highlightLen > 0)
                {
                    box.Select(start + m_highlightStart, m_highlightLen);
                    box.SelectionColor = Color.Orange;
                }
                    
                
                // Scroll to range.
                box.Select(box.TextLength, 0);
                box.ScrollToCaret();
            }

            string m_type;
            private string m_txt;
            private MainForm m_form;
        }

        #endregion Plumbing for Output

        #endregion Plumbing For Input / Output

        // Set whether the GUI is in "Run-mode" or "Break-mode"
        // OnOff = true if we're stopping; false if we're going to start running        
        // This will also set the ActiveProcess property so that other events on the 
        // UI thread can see if it's safe to access Mdbg objects.
        // This must be called on the UI thread.
        private void SetCommandInputState(bool OnOff)
        {
            // Enable / disable UI elements.
            cmdInput.Enabled = OnOff;
            breakCmd.Enabled=!OnOff;

            // Although the underlying MDbg engine supports multiple processes,
            // We'll only support 1 process from the UI to keep things simple.             
            bool fHasProcess = GuiExtension.Debugger.Processes.HaveActive;

            // If we're stopped, and we don't already have a process, then allow creating one.
            bool fAllowCreate = OnOff && !fHasProcess;
            menuItemLaunch.Enabled = fAllowCreate;
            menuItemAttach.Enabled = fAllowCreate;

            // If we're stopped, and we do have a process, allow killing it.
            bool fAllowKill = OnOff && fHasProcess;
            menuItemDetach.Enabled = fAllowKill;
            menuItemKill.Enabled = fAllowKill;

           
            SetTitle(OnOff);

            if(OnOff)
            {
                // Enter "Break" Mode
                if (fHasProcess)
                {
                    m_process = GuiExtension.Debugger.Processes.Active;
                }
                else
                {
                    m_process = null;
                }

                Activate();                                 // bring GUI up when we e.g. hit breakpoint 
                this.Cursor = Cursors.Default;

                ShowCurrentLocation(); // calculate current source location.
                SourceViewerForm.OnBreak();

                //WritePrompt();
                cmdInput.Focus();
            }
            else
            {
                m_process = null;
                
                // Enter "Run" mode
                m_CurrentSourcePosition = null;
                SourceViewerForm.OnRun();

                this.Cursor = Cursors.AppStarting;
            }            
        }


        #region Tracking Current Source Position
        // Track the current source location for the currently selected frame.  
        // This may or may-not be the leafmost frame. (IsCurrentSourceActive tracks this)
        // Null if we don't have one (such as if we're stopped in place without any symbols or running).
        public MDbgSourcePosition CurrentSource
        {
            get { return m_CurrentSourcePosition; }
        }

        // Returns true if CurrentSource property represents source at the leafmost (active) frame.
        // This parameter is meaningless is CurrentSource is null.
        public bool IsCurrentSourceActive
        {
            get { return m_fCurrentIsActive; }
        }

        bool m_fCurrentIsActive;
        MDbgSourcePosition m_CurrentSourcePosition;

        #endregion Tracking Current Source Position

        // Update windows (source, callstack,etc) to show our current source location.
        // This may be called when any UI element needs to update the UI (for example, the user selects
        // a different active frame in the callstack window).
        // This must be called on the UI thread.
        public void ShowCurrentLocation()
        {
            m_CurrentSourcePosition = null;

            // Update tool windows. If we don't have an active process / threads, they'll update accordingly.
            RefreshHelperWindows();

            
            if(this.IsProcessStopped
               && GuiExtension.Shell.Debugger.Processes.Active.Threads.HaveActive)
            {
                // Show source file.
                MDbgThreadCollection tc = null;
                MDbgThread thread = null;

                
                MDbgFrame fCurrent = null;
                MDbgSourcePosition pos = null;
                MDbgFunction function = null;
                string stCurrentFrame = "?";

                this.ExecuteOnWorkerThreadIfStoppedAndBlock(delegate(MDbgProcess proc)
                {
                    Debug.Assert(proc != null);
                    Debug.Assert(!proc.IsRunning);

                    tc = proc.Threads;
                    thread = tc.Active;
                    if (!thread.HaveCurrentFrame)
                    {
                        WriteOutput(MDbgOutputConstants.StdError, "No frame for current thread #" + thread.Number);                        
                        return;
                    }

                    fCurrent = thread.CurrentFrame;
                    function = fCurrent.Function;
                    pos = fCurrent.SourcePosition;

                    stCurrentFrame = fCurrent.ToString();                    
                });
                if (fCurrent == null)
                {
                    return;
                }


                bool fActive = (fCurrent == thread.BottomFrame);

                // Update current cached location
                m_CurrentSourcePosition = pos;
                this.m_fCurrentIsActive = fActive;

                // Try to display source.
                bool fOk = false;

                bool fShowIL = m_fShowIL;
                
                if (!fShowIL && (pos != null))
                {          
                    // We're not in IL-mode, so show source-lines.
                    fOk = OpenSourceFile(pos.Path);
                }
                else
                {
                    // If we're in IL mode (or if we have no source), then show the IL!
                    fOk = OpenSourceFile(function);
                }

                // Must always refresh the existing source forms. Even if we don't have a current source form,
                // we'll need to let our previous source know that it's lost focus.
                SourceViewerForm.OnBreak();
                if (fOk)
                {
                    return;
                }

                if (pos != null)
                {
                    WriteOutput(MDbgOutputConstants.StdError, "Cannot display position in file: " + pos.Path);
                }
                else
                {
                    WriteOutput(MDbgOutputConstants.StdError, "No source for current frame: " + stCurrentFrame);
                }


            }

            //SourceViewerForm.ClearCurrentSelection();
        }


        // this is called on the UI thread.
        bool OpenSourceFile(MDbgFunction function)
        {
            // Update source viewer
            SourceViewerBaseForm f = SourceViewerBaseForm.GetSourceFile(this, function);
            return ShowSourceFile(f);            
        }

        // this is called on the UI thread.
        bool OpenSourceFile(string fname)
        {
            SourceViewerBaseForm f = SourceViewerBaseForm.GetSourceFile(this, fname);
            return ShowSourceFile(f);            
        }

        // this is called on the UI thread.
        bool ShowSourceFile(SourceViewerBaseForm f)
        {
            if (f == null)
            {
                return false;
            }


            f.Focus();
            cmdInput.Focus();
            return true;
        }


        // Do an Async Break.
        // This is used to asynchronously get the program from a Run state to a stopped state.
        // This is called on the UI thread.
        private void DoBreak()
        {
            // UI thread can't call Async Break. So spin up a helper thread to do it for us.
            // We can't use our normal worker thread because that's not available when the process
            // is running.
            WriteOutput(m_MetaInfoWriteFormat, "[Async Break requested from UI]");

            
            Thread t = new Thread(delegate()
            {
                try
                {
                    GuiExtension.Shell.Debugger.Processes.Active.AsyncStop().WaitOne();
                }
                catch (Exception e)
                {
                    WriteOutput(MDbgOutputConstants.StdError, e.Message);
                }
            });

            t.SetApartmentState(ApartmentState.MTA);
            t.IsBackground = true;
            t.Start();
            t.Join();
        }
       

        public AutoResetEvent InitComplete
        {
            get
            {
                return m_initComplete;
            }
        }
        
        //const string TEXT_ERROR="Error: ";
        const string TEXT_PROMPT_STRING="mdbg> ";

        // Event used to block the shell waiting for input
        private AutoResetEvent m_inputEvent = new AutoResetEvent(false);        
        private AutoResetEvent m_inputDoneEvent = new AutoResetEvent(false);

        private AutoResetEvent m_initComplete = new AutoResetEvent(false);        
        System.Collections.Generic.Queue<string> m_cmdQueue = new System.Collections.Generic.Queue<string>();

        private FPWorker m_fpWorker; // send a worker delegate

        // The GUI form implements IMDbgIO. It will redirect the Shell's IO interface to the GUI.
        // We remeber the old IO instance so that we can restore it once the GUI gets closed.
        private IMDbgIO m_savedIO;

        // Hold onto the underlying shell object so that we can redirect some of its subojects (like the IMdbgIO)
        // to the GUI.
        private IMDbgShell m_ui;

        private void menuItemView_Click(object sender, EventArgs e)
        {
        
        }

        private void menuItemCommands_Click(object sender, EventArgs e)
        {
            this.AsyncProcessEnteredText("help");
        }

        // Prompt the user to stop debugging (via the given Mdbg command).
        // This is shared by detach + kill commands.
        // Returns True if it stops debugging the current process, else false.
        // This is called on the UI thread.
        private bool StopDebugging(string command, string desc)
        {
            if (!this.IsProcessStopped)
            {
                // Nothing to do. Not sure how we'd ever get here because the 
                // UI should have disabled us, but just in case ...
                return false; 
            }

            // Kill the current process.
            string caption = "Stop debugging ('" + command + "')?";
            string text = "Do you want to " + desc +" and stop debugging?";
            DialogResult x = MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (x == DialogResult.Yes)
            {
                // This will execute the command asynchronously on another thread.
                this.AsyncProcessEnteredText(command);
            }

            return false;
        }

        private void menuItemKill_Click(object sender, EventArgs e)
        {
            StopDebugging("kill", "terminate the debuggee");
        }

        private void menuItemDetach_Click(object sender, EventArgs e)
        {
            StopDebugging("detach", "detach from the debuggee");
        }

        // Kick off menu to launch a process.
        private void menuItemLaunch_Click(object sender, EventArgs e)
        {
            // @@ Pumping test.
            /*
            MessageBox.Show("About to test pumping");

            // TEST PUMPING. STA thread is blocked in pumping wait.
            AutoResetEvent e2 = new AutoResetEvent(false);
            e2.WaitOne();


            MessageBox.Show("Done testing pumping");
            */

            if (GuiExtension.Debugger.Processes.HaveActive)
            {
                // Nothing to do. Not sure how we'd ever get here because the 
                // UI shoul dhave disabled us, but just in case ...
                return;
            }

            using (gui.LaunchProcess form = new gui.LaunchProcess())
            {
                form.ShowDialog();

                // If null, then they cancelled.
                if (form.ProcessName == null)
                {
                    return;
                }

                System.IO.Directory.SetCurrentDirectory(form.WorkingDir);
                string cmd = "run " + form.ProcessName + " " + form.Arguments;

                // We want console apps to have their own console.
                // Run the app.
                this.AsyncProcessEnteredText("mo nc on\n" + cmd);
            }
        }

        private void menuItemAttach_Click(object sender, EventArgs e)
        {
            if (GuiExtension.Debugger.Processes.HaveActive)
            {
                // Nothing to do. Not sure how we'd ever get here because the 
                // UI shoul dhave disabled us, but just in case ...
                return;
            }

            using (gui.AttachProcess form = new gui.AttachProcess())
            {
                form.ShowDialog();
                int pid = form.SelectedPid;

                if (pid == 0)
                {
                    return;
                }

                string cmd = "attach " + pid;

                // Run the app.
                this.AsyncProcessEnteredText(cmd);
            }
        }

        // Are we showing IL or source? True if user is in IL-view. False if they're in Source-mode view.  
        bool m_fShowIL = false;

        // Update the text on the "Show Il" / "Show Source" menu.
        void UpdateShowIlMenu()
        {
            // This menu item signifies toggling to the other mode. So we show them the other mode. 
            if (!m_fShowIL)
            {
                // if m_fShowIl = false, then we're currently viewing source. So the menu item
                // means toggle to IL, so it says "view IL".
                menuItemViewIlOrSource.Text = "View Il";                                
            }
            else
            {
                menuItemViewIlOrSource.Text = "View Source";
            }
        }

        private void menuItem8_Click(object sender, EventArgs e)
        {
            m_fShowIL = !m_fShowIL; // toggle
            UpdateShowIlMenu();
                        
            // Need to refresh UI to show update.
            this.ShowCurrentLocation();
            this.Invalidate();
        }

    } // end MainForm
}
