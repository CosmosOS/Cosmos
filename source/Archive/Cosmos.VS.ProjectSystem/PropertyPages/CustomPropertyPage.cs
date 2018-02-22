using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Cosmos.Build.Common;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.ProjectSystem.PropertyPages
{
    public partial class CustomPropertyPage : UserControl, IPropertyPage
    {
        private static readonly List<CustomPropertyPage> _pageList = new List<CustomPropertyPage>();
        private bool _dirty;
        private string _helpKeyword;

        private IPropertyPageSite _site;
        private string _title;

        public CustomPropertyPage()
        {
            ProjectMgr = null;
            ProjectConfigs = null;
            Project = null;
            _site = null;
            _dirty = false;
            IgnoreDirty = false;
            _title = string.Empty;
            _helpKeyword = string.Empty;
        }

        protected static CustomPropertyPage[] Pages => _pageList.ToArray();

        public virtual PropertiesBase Properties => null;

        public virtual string Title
        {
            get
            {
                if (_title == null)
                    _title = string.Empty;
                return _title;
            }
            set => _title = value;
        }

        protected virtual string HelpKeyword
        {
            get
            {
                if (_helpKeyword == null)
                    _helpKeyword = string.Empty;
                return _helpKeyword;
            }
            set => _title = value;
        }

        public bool IsDirty
        {
            get { return _dirty; }
            set
            {
                if (IgnoreDirty == false)
                    if (_dirty != value)
                    {
                        _dirty = value;
                        if (_site != null)
                            _site.OnStatusChange((uint) (_dirty ? PropPageStatus.Dirty : PropPageStatus.Clean));
                    }
            }
        }

        public bool IgnoreDirty { get; set; }

        public ProjectNode ProjectMgr { get; private set; }

        protected ProjectConfig[] ProjectConfigs { get; private set; }

        protected OAProject Project { get; private set; }

        void IPropertyPage.SetPageSite(IPropertyPageSite pPageSite)
        {
            _site = pPageSite;
        }

        void IPropertyPage.Activate(IntPtr hWndParent, RECT[] pRect, int bModal)
        {
            CreateControl();
            Initialize();
            NativeMethods.SetParent(Handle, hWndParent);

            _pageList.Add(this);
            FillConfigurations();

            IgnoreDirty = true;
            FillProperties();
            IgnoreDirty = false;
        }

        void IPropertyPage.Deactivate()
        {
            _pageList.Remove(this);
            Dispose();
        }

        void IPropertyPage.GetPageInfo(PROPPAGEINFO[] pPageInfo)
        {
            PROPPAGEINFO info = new PROPPAGEINFO();

            Size = new Size(492, 288);

            info.cb = (uint) Marshal.SizeOf(typeof(PROPPAGEINFO));
            info.dwHelpContext = 0;
            info.pszDocString = null;
            info.pszHelpFile = null;
            info.pszTitle = Title;
            info.SIZE.cx = Width;
            info.SIZE.cy = Height;
            pPageInfo[0] = info;
        }

        void IPropertyPage.SetObjects(uint count, object[] punk)
        {
            if (count > 0)
            {
                if (punk[0] is ProjectConfig)
                {
                    ArrayList configs = new ArrayList();
                    for (int i = 0; i < count; i++)
                    {
                        ProjectConfig config = (ProjectConfig) punk[i];
                        if (ProjectMgr == null)
                            ProjectMgr = config.ProjectMgr;
                        configs.Add(config);
                    }
                    ProjectConfigs = (ProjectConfig[]) configs.ToArray(typeof(ProjectConfig));

                    // For ProjectNodes we will get one of these
                }
                else if (punk[0] is NodeProperties)
                {
                    if (ProjectMgr == null)
                        ProjectMgr = (punk[0] as NodeProperties).Node.ProjectMgr;

                    var configsMap = new Dictionary<string, ProjectConfig>();

                    for (int i = 0; i < count; i++)
                    {
                        NodeProperties property = (NodeProperties) punk[i];
                        IVsCfgProvider provider;
                        ErrorHandler.ThrowOnFailure(property.Node.ProjectMgr.GetCfgProvider(out provider));
                        var expected = new uint[1];
                        ErrorHandler.ThrowOnFailure(provider.GetCfgs(0, null, expected, null));
                        if (expected[0] > 0)
                        {
                            var configs = new ProjectConfig[expected[0]];
                            var actual = new uint[1];
                            provider.GetCfgs(expected[0], configs, actual, null);

                            foreach (ProjectConfig config in configs)
                                if (!configsMap.ContainsKey(config.ConfigName))
                                    configsMap.Add(config.ConfigName, config);
                        }
                    }

                    if (configsMap.Count > 0)
                    {
                        if (ProjectConfigs == null)
                            ProjectConfigs = new ProjectConfig[configsMap.Keys.Count];
                        configsMap.Values.CopyTo(ProjectConfigs, 0);
                    }
                }
            }
            else
            {
                ProjectMgr = null;
            }

            /* This code calls FillProperties without Initialize call
            if (_projectMgr != null)
            {
                FillProperties();
            }
            */

            if (ProjectMgr != null && Project == null)
                Project = new OAProject(ProjectMgr);
        }

        void IPropertyPage.Show(uint nCmdShow)
        {
            Visible = true;
            Show();
        }

        void IPropertyPage.Move(RECT[] pRect)
        {
            RECT r = pRect[0];

            Location = new Point(r.left, r.top);
            Size = new Size(r.right - r.left, r.bottom - r.top);
        }

        int IPropertyPage.IsPageDirty()
        {
            return IsDirty ? VSConstants.S_OK : VSConstants.S_FALSE;
        }

        int IPropertyPage.Apply()
        {
            if (IsDirty)
            {
                if (ProjectMgr == null)
                {
                    System.Diagnostics.Debug.Assert(false);
                    return VSConstants.E_INVALIDARG;
                }

                if (CheckInput())
                {
                    ApplyChanges();
                    IsDirty = false;
                }
                else
                {
                    return VSConstants.S_FALSE;
                }
            }
            return VSConstants.S_OK;
        }

        void IPropertyPage.Help(string pszHelpDir)
        {
            //    IServiceProvider serviceProvider = _site as IServiceProvider;
            //    if (serviceProvider != null)
            //    {
            //        Help helpService = serviceProvider.GetService(typeof(Help)) as Help;
            //        if (helpService != null)
            //        {
            //            helpService.DisplayTopicFromF1Keyword(HelpKeyword);
            //        }
            //    }
        }

        int IPropertyPage.TranslateAccelerator(MSG[] pMsg)
        {
            MSG msg = pMsg[0];

            if ((msg.message < NativeMethods.WM_KEYFIRST || msg.message > NativeMethods.WM_KEYLAST) &&
                (msg.message < NativeMethods.WM_MOUSEFIRST || msg.message > NativeMethods.WM_MOUSELAST))
                return 1;

            return NativeMethods.IsDialogMessageA(Handle, ref msg) ? 0 : 1;
        }

        protected virtual void FillProperties()
        {
        }

        protected virtual void FillConfigurations()
        {
        }

        public virtual void ApplyChanges()
        {
            if (Properties != null)
            {
                var properties = Properties.GetProperties();
                var independentProperties = Properties.ProjectIndependentProperties;
                foreach (var pair in properties)
                {
                    string propertyName = pair.Key;

                    //if (independentProperties.Contains(propertyName))
                    //    SetProjectProperty(pair.Key, pair.Value);
                    //else
                    //    SetConfigProperty(pair.Key, pair.Value);

                    SetProjectProperty(pair.Key, pair.Value);
                }

                IsDirty = false;
            }
        }

        /// <summary>
        ///     Sets project specific property.
        /// </summary>
        /// <param name="name">Name of the property to set.</param>
        /// <param name="value">Value of the property.</param>
        public virtual void SetProjectProperty(string name, string value)
        {
            CCITracing.TraceCall();
            if (value == null)
                value = string.Empty;

            if (ProjectMgr != null)
                ProjectMgr.SetProjectProperty(name, value);
        }

        public virtual void SetConfigProperty(string name, string value)
        {
            CCITracing.TraceCall();
            if (value == null)
                value = string.Empty;

            if (ProjectMgr != null)
            {
                foreach (ProjectConfig config in ProjectConfigs)
                    config.SetConfigurationProperty(name, value);
                ProjectMgr.SetProjectFileDirty(true);
            }
        }

        public virtual string GetConfigProperty(string aName)
        {
            return ProjectConfigs[0].GetConfigurationProperty(aName, true);
        }

        protected virtual void Initialize()
        {
        }

        protected virtual bool CheckInput()
        {
            return true;
        }

        protected void MarkPageChanged()
        {
            IsDirty = true;
        }

        protected string GetComboValue(ComboBox comboBox)
        {
            string selectedItem = comboBox.SelectedItem as string;
            if (selectedItem != null)
                return selectedItem;
            return string.Empty;
        }

        protected void AddComboBoxItems(ComboBox comboBox, params string[] items)
        {
            foreach (string item in items)
                comboBox.Items.Add(item);
        }

        private bool ParseBoolean(string value, bool defaultValue)
        {
            if (!string.IsNullOrEmpty(value))
                try
                {
                    return bool.Parse(value);
                }
                catch
                {
                }
            return defaultValue;
        }
    }
}
