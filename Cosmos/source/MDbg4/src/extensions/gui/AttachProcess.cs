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

using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorMetadata;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.CorPublish;

namespace gui
{
    partial class AttachProcess : Form
    {
        public AttachProcess()
        {
            InitializeComponent();

            RefreshProcesses();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            RefreshProcesses();
        }

        class Item
        {
            public Item(int pid, string stName)
            {
                m_stName = stName;
                m_pid = pid;
            }

            public override string ToString()
            {
                return m_stName;
            }

            string m_stName;
            int m_pid;
            public int Pid
            {
                get { return m_pid; }
            }
        }

        void RefreshProcesses()
        {
            this.listBoxProcesses.Items.Clear();

            CorPublish cp = null;

            int curPid = System.Diagnostics.Process.GetCurrentProcess().Id;
            try
            {
                int count = 0;

                cp = new CorPublish();
                {                   
                    foreach (CorPublishProcess cpp in cp.EnumProcesses())
                    {
                        if (curPid != cpp.ProcessId)  // let's hide our process
                        {
                            string version = CorDebugger.GetDebuggerVersionFromPid(cpp.ProcessId);
                            string s = "[" + cpp.ProcessId + "] [ver=" + version + "] " + cpp.DisplayName;
                            this.listBoxProcesses.Items.Add(new Item(cpp.ProcessId, s));
                            count++;
                        }
                    }

                } // using

                if (count == 0)
                {
                    this.listBoxProcesses.Items.Add(new Item(0, "(No active processes)"));
                }
            }            
            catch(Exception)
            {
                if (cp == null)
                {
                    this.listBoxProcesses.Items.Add(new Item(0, "(Can't enumerate processes"));
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            m_pid = 0;
            this.Close();
        }

        int m_pid;

        // Get the pid selected from the dialog.
        public int SelectedPid
        {
            get { return m_pid; }
        }


        private void buttonAttach_Click(object sender, EventArgs e)
        {
            object o = this.listBoxProcesses.SelectedItem;
            Item x = (Item)o;
            m_pid = x.Pid;

            this.Close();

        } // end refresh
    }
}