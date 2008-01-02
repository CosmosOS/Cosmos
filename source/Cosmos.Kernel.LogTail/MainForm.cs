using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Cosmos.Kernel.LogTail.Handlers;

namespace Cosmos.Kernel.LogTail
{
    public partial class MainForm : Form
    {
        private Handlers.LogHandler[] _handlers;
        private string _file;
        private FileSystemWatcher _watcher;
        private bool _watching = true;
        private long _count = 0;
        private FileStream _fs;
        
        public MainForm()
        {
            InitializeComponent();
            BuildTabs();
        }

        public MainForm(string file) : this()
        {
            _file = file;
            string dir = Path.GetDirectoryName(_file);
            string pattern = Path.GetFileName(_file);

            _watcher = new FileSystemWatcher(dir, pattern);
            _watcher.Changed += new FileSystemEventHandler(_watcher_Changed);
            _watcher.Created += new FileSystemEventHandler(_watcher_Created);
            _watcher.Deleted += new FileSystemEventHandler(_watcher_Deleted);
            _watcher.EnableRaisingEvents = true;

            if (File.Exists(_file))
                CreateReader();
        }

        private void CreateReader()
        {
            
            _fs = new FileStream(_file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            _watching = true;
            _watcher_Changed(this, new FileSystemEventArgs(WatcherChangeTypes.All, "", ""));
        }

        private XmlReader CreateXmlReader()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;

            return XmlTextReader.Create(_fs, settings);
        }

        void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            _watching = false;
            _fs.Close();
        }

        void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            CreateReader();
        }

        void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (_watching)
            {
                XmlReader _reader = CreateXmlReader();
                while (_reader.Read())
                {
                    LogMessage message = new LogMessage(_reader.Name);
                    for (int i = 0; i < _reader.AttributeCount; i++)
                    {
                        _reader.MoveToAttribute(i);
                        message.AddAttribute(_reader.Name, _reader.Value);
                    }
                    foreach (LogHandler handler in this._handlers)
                        handler.HandleMessage(message);
                }
            }
        }

        private void BuildTabs()
        {
            _handlers = Handlers.LogHandler.GetHandlers();
            foreach (Handlers.LogHandler handler in _handlers)
            {
                TabPage page = new TabPage();
                page.Text = handler.Title;
                page.Controls.Add(handler);
                handler.Dock = DockStyle.Fill;
                handler.Clear();
                tabControl.TabPages.Add(page);
            }
        }
    }
}
