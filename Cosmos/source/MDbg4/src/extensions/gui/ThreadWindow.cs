//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

#endregion

using System.Globalization;
using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;  
using Microsoft.Samples.Tools.Mdbg.Extension;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using System.Diagnostics;

namespace gui
{
    // Tool window to display thread list.
    // Deriving from a Generic class seems to confuse the Designer in VS2005 Beta 1. One workaround is to
    // switch the derived class to "Form", use the designer, and then restore the derived class so we can build.
    partial class ThreadWindow : 
        DebuggerListWindow<MDbgThread>
        //Form
    {
        public ThreadWindow(MainForm mainForm)
            : base(mainForm, "Threads not available while process is running")
        {
            InitializeComponent();

            // Prep the context menu that we use to Freeze / Thaw threads.
            m_menu = new ContextMenu();
            m_menu.Popup += new EventHandler(this.Popup);                        
            this.ContextMenu = m_menu;


            this.listBox1.DoubleClick += new EventHandler(this.OnSelectionChanged);


            this.Text = "Thread List";
        }

        #region Context Menu to Freeze / Thaw Threads
        ContextMenu m_menu;

        // Called when ContextMenu is about to popup
        // Called on UI thread.
        void Popup(object sender, EventArgs args)
        {
            // Don't process UI events if process is running.
            if (!this.MainForm.IsProcessStopped)
            {
                return;
            }

            string st = null;

            MDbgThread t = this.SelectedItem;
            if (t == null)
            {
                return;
            }

            // Get selection.
            MainForm.ExecuteOnWorkerThreadIfStoppedAndBlock(delegate(MDbgProcess proc)
                {           
                st = IsFrozen(t) ? "Thaw" : "Freeze";
                st += " thread=" + t.Number;
                });
            if (st == null)
            {
                return;
            }

            m_menu.MenuItems.Clear();
            m_menu.MenuItems.Add(st, new EventHandler(this.OnThreadFrozenToggled));

        }

        // Invoked when ContextMenu item is selected to toggle Frozen/Thawed status.
        // This is invoked on the currently selected thread, and that can't change while the ContextMenu is up.
        // Called on UI thread.
        void OnThreadFrozenToggled(Object sender, EventArgs args)
        {
            MDbgThread t = this.SelectedItem;

            MainForm.ExecuteOnWorkerThreadIfStoppedAndBlock(delegate(MDbgProcess proc)
                {
                    Debug.Assert(proc != null);
                    Debug.Assert(!proc.IsRunning);

                    CorDebugThreadState state = t.CorThread.DebugState;
                    bool fFrozen = (state & CorDebugThreadState.THREAD_SUSPEND) == CorDebugThreadState.THREAD_SUSPEND;
                    if (fFrozen)
                    {
                        // Thaw the thread
                        state &= ~CorDebugThreadState.THREAD_SUSPEND;
                    }
                    else
                    {
                        // Freeze the thread
                        state |= CorDebugThreadState.THREAD_SUSPEND;
                    }
                    t.CorThread.DebugState = state;
                });

            // Need to redraw the window.
            this.RefreshToolWindow();
        }

        // Is the MDbgThread frozen?
        // Must be called on worker thread.
        static bool IsFrozen(MDbgThread t)
        {
            CorDebugThreadState state = t.CorThread.DebugState;
            bool fFrozen = (state & CorDebugThreadState.THREAD_SUSPEND) == CorDebugThreadState.THREAD_SUSPEND;
            return fFrozen;
        }

        #endregion Context Menu to Freeze / Thaw Threads

        protected override ListBox ListBox
        {
            get { return this.listBox1; }
        }

        // Called when user selects a different thread in the list.
        // Called on UI thread.
        void OnSelectionChanged(Object sender, EventArgs args)
        {
            MDbgThread t = this.SelectedItem;
            if (t == null)
            {
                return;
            }
            // If we have an threads in the list, then we must have an active process and 
            // valid thread collection.
            if (!this.MainForm.IsProcessStopped)
            {
                return;
            }

            MainForm.ExecuteOnWorkerThreadIfStoppedAndBlock(delegate(MDbgProcess proc)
            {
                Debug.Assert(proc != null);
                Debug.Assert(!proc.IsRunning);

                proc.Threads.Active = t;
            });

            this.RefreshMainWindow();
            
        }

        // Refresh the Threads window.
        public override void RefreshWhenStopped()
        {
            ListBox.ObjectCollection items = this.Items;
            items.Clear();

            
            string[] values = null;
            MDbgThread[] threads = null;

            MainForm.ExecuteOnWorkerThreadIfStoppedAndBlock(delegate(MDbgProcess proc)
            {
                Debug.Assert(proc != null);
                Debug.Assert(!proc.IsRunning);

                MDbgThread tActive = proc.Threads.HaveActive ? (proc.Threads.Active) : null;

                values = new string[proc.Threads.Count];
                threads = new MDbgThread[values.Length];
                int idx = 0;

                foreach (MDbgThread t in proc.Threads)
                {
                    string stFrame = "<unknown>";

                    if (t.BottomFrame != null)
                    {
                        stFrame = t.BottomFrame.Function.FullName;
                    }
                    string stActive = (t == tActive) ? "*" : " ";

                    string stFrozen = IsFrozen(t) ? "(FROZEN) " : "";

                    string s = stActive + "(" + t.Number + ") TID=" + t.Id + ", " + stFrozen + stFrame;
                    //this.AddItem(s, t);
                    values[idx]  = s;
                    threads[idx] = t;
                    idx++;

                }
            });

            for (int i = 0; i < values.Length; i++)
            {
                this.AddItem(values[i], threads[i]);
            }

        }

    } // end class ThreadWindow

} // end GUI namespace