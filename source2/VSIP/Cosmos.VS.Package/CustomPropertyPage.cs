using System; 
using System.Collections; 
using System.Collections.Generic; 
using System.Diagnostics; 
using System.Drawing; 
using System.Runtime.InteropServices; 
using System.Windows.Forms; 
using Microsoft.VisualStudio; 
using Microsoft.VisualStudio.OLE.Interop; 
using Microsoft.VisualStudio.Project; 
using Microsoft.VisualStudio.Shell.Interop; 
using Help = Microsoft.VisualStudio.VSHelp.Help; 
using IServiceProvider = System.IServiceProvider;
using Cosmos.Build.Common;

namespace Cosmos.VS.Package {
    public partial class CustomPropertyPage : UserControl, IPropertyPage {

		private static List<CustomPropertyPage> _pageList = new List<CustomPropertyPage>();
		protected static CustomPropertyPage[] Pages
		{ get { return CustomPropertyPage._pageList.ToArray(); } }

        private ProjectNode _projectMgr; 
        private ProjectConfig[] _projectConfigs; 
        private IPropertyPageSite _site; 
        private bool _dirty;
        private string _title; 
        private string _helpKeyword;
		private Microsoft.VisualStudio.Project.Automation.OAProject _project;

        public CustomPropertyPage() { 
            _projectMgr = null; 
            _projectConfigs = null;
			_project = null;
            _site = null; 
            _dirty = false;
			this.IgnoreDirty = false;
            _title = string.Empty; 
            _helpKeyword = string.Empty;

			System.Diagnostics.Debug.Print(String.Format("{0}->Created", this.GetType().Name));
        }

		public virtual PropertiesBase Properties
		{ get { return null; } }

        public virtual string Title 
        { 
            get 
            { 
                if (_title == null) 
                { 
                    _title = string.Empty; 
                } 
                return _title; 
            } 
            set 
            { 
                _title = value; 
            } 
        } 
 
        protected virtual string HelpKeyword 
        { 
            get 
            { 
                if (_helpKeyword == null) 
                { 
                    _helpKeyword = string.Empty; 
                } 
                return _helpKeyword; 
            } 
            set 
            { 
                _title = value; 
            } 
        } 
 
        public bool IsDirty 
        { 
            get 
            { 
                return _dirty; 
            } 
            set 
            {
				if (this.IgnoreDirty == false)
				{
					if (_dirty != value)
					{
						_dirty = value;
						if (_site != null)
						{
							_site.OnStatusChange((uint)(_dirty ? PropPageStatus.Dirty : PropPageStatus.Clean));
						}
					}
				}
            } 
		}

		public bool IgnoreDirty
		{ get; set; }
	 
	        protected ProjectNode ProjectMgr 
	        { 
	            get 
	            { 
	                return _projectMgr; 
	            } 
	        } 
	 
	        protected ProjectConfig[] ProjectConfigs 
	        { 
	            get 
	            { 
	                return _projectConfigs; 
	            } 
	        }

			protected Microsoft.VisualStudio.Project.Automation.OAProject Project
			{
				get
				{
					return _project;
				}
			}

	        protected virtual void FillProperties() 
	        {
				System.Diagnostics.Debug.Print(String.Format("{0}->FillProperties", this.GetType().Name));
			}
 
            protected virtual void FillConfigurations()
            {
				System.Diagnostics.Debug.Print(String.Format("{0}->FillConfigs", this.GetType().Name));
            }
	 
	        public void ApplyChanges()
	        {
				System.Diagnostics.Debug.Print(String.Format("{0}->ApplyChanges", this.GetType().Name));

				if (this.Properties != null)
				{
					Dictionary<String, String> properties = this.Properties.GetProperties();

					foreach (KeyValuePair<String, String> pair in properties)
					{ this.SetConfigProperty(pair.Key, pair.Value); }

					this.IsDirty = false;
				}
			}

			public virtual void SetConfigProperty(String name, String value)
			{
				CCITracing.TraceCall();
				if (value == null)
				{ value = String.Empty; }

				if (this.ProjectMgr != null)
				{
					foreach (ProjectConfig config in this.ProjectConfigs)
					{ config.SetConfigurationProperty(name, value); }
					this.ProjectMgr.SetProjectFileDirty(true);
				}
			}

			public virtual String GetConfigProperty(String name)
			{
				String value;

				value = this.ProjectConfigs[0].GetConfigurationProperty(name, true);

				return value;
			}
	 
	        protected virtual void Initialize() 
	        {
				System.Diagnostics.Debug.Print(String.Format("{0}->Initialize", this.GetType().Name));
			} 
	 
	        protected virtual bool CheckInput() 
	        { return true; } 
	 
	        protected void MarkPageChanged() 
	        { 
	            IsDirty = true; 
	        }

        protected string GetComboValue(ComboBox comboBox) 
	        { 
	            string selectedItem = comboBox.SelectedItem as string; 
	            if (selectedItem != null) 
	            { 
	                return selectedItem; 
	            } 
	            return string.Empty; 
	        } 
	 
	        protected void AddComboBoxItems(ComboBox comboBox, params string[] items) 
	        { 
	            foreach (string item in items) 
	            { 
	                comboBox.Items.Add(item); 
	            } 
	        } 
	 
	        private bool ParseBoolean(string value, bool defaultValue) 
	        { 
	            if (!string.IsNullOrEmpty(value)) 
	            { 
	                try 
	                { 
	                    return bool.Parse(value); 
	                } 
	                catch 
	                {} 
	            } 
	            return defaultValue; 
	        } 
	
	        void IPropertyPage.SetPageSite(IPropertyPageSite pPageSite) 
	        {
				System.Diagnostics.Debug.Print(String.Format("{0}->SetPageSite", this.GetType().Name));

	            _site = pPageSite;
	        } 
	 
	        void IPropertyPage.Activate(IntPtr hWndParent, RECT[] pRect, int bModal) 
	        {
				System.Diagnostics.Debug.Print(String.Format("{0}->Activate", this.GetType().Name));

	            CreateControl(); 
	            Initialize(); 
	            NativeMethods.SetParent(Handle, hWndParent);

				CustomPropertyPage._pageList.Add(this);
	            FillConfigurations();

				this.IgnoreDirty = true;
				FillProperties();
				this.IgnoreDirty = false;
	        } 
	 
	        void IPropertyPage.Deactivate() 
	        {
				System.Diagnostics.Debug.Print(String.Format("{0}->Deactivate", this.GetType().Name));

				CustomPropertyPage._pageList.Remove(this);
	            Dispose(); 
	        }

			void IPropertyPage.GetPageInfo(PROPPAGEINFO[] pPageInfo)
			{
				System.Diagnostics.Debug.Print(String.Format("{0}->GetPageInfo", this.GetType().Name));

	            PROPPAGEINFO info = new PROPPAGEINFO();

				this.Size = new Size(492, 288);

	            info.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO)); 
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
				System.Diagnostics.Debug.Print(String.Format("{0}->SetObjects", this.GetType().Name));

	            if (count > 0) { 
	                if (punk[0] is ProjectConfig) { 
	                    ArrayList configs = new ArrayList(); 
	                    for(int i = 0; i < count; i++) { 
	                        ProjectConfig config = (ProjectConfig)punk[i]; 
	                        if (_projectMgr == null) { 
	                            _projectMgr = config.ProjectMgr; 
	                        } 
	                        configs.Add(config); 
	                    } 
	                    _projectConfigs = (ProjectConfig[])configs.ToArray(typeof(ProjectConfig)); 

                  // For ProjectNodes we will get one of these
	                } else if (punk[0] is NodeProperties) { 
	                    if (_projectMgr == null) { 
	                        _projectMgr = (punk[0] as NodeProperties).Node.ProjectMgr; 
	                    } 
	 
	                    Dictionary<string, ProjectConfig> configsMap = new Dictionary<string, ProjectConfig>(); 
	 
	                    for(int i = 0; i < count; i++) 
	                    { 
	                        NodeProperties property = (NodeProperties)punk[i]; 
	                        IVsCfgProvider provider; 
	                        ErrorHandler.ThrowOnFailure(property.Node.ProjectMgr.GetCfgProvider(out provider)); 
	                        uint[] expected = new uint[1]; 
	                        ErrorHandler.ThrowOnFailure(provider.GetCfgs(0, null, expected, null)); 
	                        if (expected[0] > 0) 
	                        { 
	                            ProjectConfig[] configs = new ProjectConfig[expected[0]]; 
	                            uint[] actual = new uint[1]; 
	                            provider.GetCfgs(expected[0], configs, actual, null); 
	 
	                            foreach(ProjectConfig config in configs) { 
	                                if (!configsMap.ContainsKey(config.ConfigName)) { 
	                                    configsMap.Add(config.ConfigName, config); 
	                                } 
	                            } 
	                        } 
	                    } 
	 
	                    if (configsMap.Count > 0) { 
	                        if (_projectConfigs == null) { 
	                            _projectConfigs = new ProjectConfig[configsMap.Keys.Count]; 
	                        } 
	                        configsMap.Values.CopyTo(_projectConfigs, 0); 
	                    } 
	                } 
	            } else  { 
	                _projectMgr = null; 
	            } 
	 
	            /* This code calls FillProperties without Initialize call
	            if (_projectMgr != null)
	            {
	                FillProperties();
	            }
	            */

				if ((_projectMgr != null) && (_project == null))
				{
					_project = new Microsoft.VisualStudio.Project.Automation.OAProject(_projectMgr);
				}
	        } 
	 
	        void IPropertyPage.Show(uint nCmdShow) 
	        {
				System.Diagnostics.Debug.Print(String.Format("{0}->Show", this.GetType().Name));

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
	            return (IsDirty ? VSConstants.S_OK : VSConstants.S_FALSE); 
	        } 
	 
	        int IPropertyPage.Apply() 
	        {
				System.Diagnostics.Debug.Print(String.Format("{0}->Apply", this.GetType().Name));

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
	            IServiceProvider serviceProvider = _site as IServiceProvider; 
	            if (serviceProvider != null) 
	            { 
	                Help helpService = serviceProvider.GetService(typeof(Help)) as Help; 
	                if (helpService != null) 
	                { 
	                    helpService.DisplayTopicFromF1Keyword(HelpKeyword); 
	                } 
	            } 
	        } 
	 
	        int IPropertyPage.TranslateAccelerator(MSG[] pMsg) 
	        { 
	            MSG msg = pMsg[0]; 
	 
	            if ((msg.message < NativeMethods.WM_KEYFIRST || msg.message > NativeMethods.WM_KEYLAST) && 
	                (msg.message < NativeMethods.WM_MOUSEFIRST || msg.message > NativeMethods.WM_MOUSELAST)) 
	            { 
	                return 1; 
	            } 
	 
	            return (NativeMethods.IsDialogMessageA(Handle, ref msg)) ? 0 : 1; 
	        } 
	
	    } 
}


