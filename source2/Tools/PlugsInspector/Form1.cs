using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Cosmos.IL2CPU;
using System.Reflection;

namespace PlugsInspector
{
    public partial class Form1 : Form
    {
        PlugManager plugManager;
        bool LoadingPlugs = false;

        public Form1()
        {
            InitializeComponent();

            //Force it to load/include all plugs assemblies so all types are correctly found
            //Otherwise the CLR's delay loading techniques block us...
            AssembliesPreloader.LoadAllAssemblies();
            

            plugManager = new PlugManager((Exception ex) => {
                AddExceptionEntry(ex.Message);
            }, this.ScanMethod, null);
            plugManager.ThrowExceptions = false;
        }

        private void LoadPlugsButton_Click(object sender, EventArgs e)
        {
            if (!LoadingPlugs)
            {
                PlugsListBox.Items.Clear();
                ExceptionsListBox.Items.Clear();

                new Task(LoadPlugs).Start();
            }
        }
        private void LoadPlugs()
        {
            if (LoadingPlugs)
                return;

            try
            {
                LoadingPlugs = true;

                plugManager.Clean();

                plugManager.FindPlugImpls();
                plugManager.ScanFoundPlugs();
                foreach (Type pluggedMethodType in plugManager.PlugImpls.Keys)
                {
                    string name = "";
                    if (pluggedMethodType.DeclaringType != null)
                    {
                        name += pluggedMethodType.DeclaringType.Name + ".";
                    }
                    name += pluggedMethodType.Name;
                    AddPlugEntry(name);
                }
                foreach (Type pluggedMethodType in plugManager.PlugImplsInhrt.Keys)
                {
                    string name = "";
                    if (pluggedMethodType.DeclaringType != null)
                    {
                        name += pluggedMethodType.DeclaringType.Name + ".";
                    }
                    name += pluggedMethodType.Name;
                    AddPlugEntry(name);
                }
                foreach (Type pluggedFieldType in plugManager.PlugFields.Keys)
                {
                    string name = "";
                    if (pluggedFieldType.DeclaringType != null)
                    {
                        name += pluggedFieldType.DeclaringType.Name + ".";
                    }
                    name += pluggedFieldType.Name;
                    AddPlugEntry(name);
                }
            }
            catch(Exception ex)
            {
                AddPlugEntry("Error loading plugs: " + ex.Message);
            }
            finally
            {
                LoadingPlugs = false;
            }
        }
        private void ScanMethod(MethodBase aMethod, bool aIsPlug, object sourceItem)
        {
            string name = "";
            if (aMethod.DeclaringType != null)
            {
                name += aMethod.DeclaringType.Name + ".";
            }
            name += aMethod.Name;
            AddPlugEntry(name);
        }

        private delegate void AddPlugEntryCallback(string text);
        private void AddPlugEntry(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AddPlugEntryCallback(this.AddPlugEntry), new object[] { text });
            }
            else
            {
                this.PlugsListBox.Items.Add(text);
            }
        }
        private delegate void AddExceptionEntryCallback(string text);
        private void AddExceptionEntry(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AddExceptionEntryCallback(this.AddExceptionEntry), new object[] { text });
            }
            else
            {
                this.ExceptionsListBox.Items.Add(text);
            }
        }
    }
}
