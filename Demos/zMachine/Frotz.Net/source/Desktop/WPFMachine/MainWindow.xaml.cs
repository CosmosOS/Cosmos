using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;

using System.Threading;

using WPFMachine.Screen;
using Frotz.Screen;
using Frotz.Constants;

namespace WPFMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ZMachineScreen _screen;
        Thread _zThread;
        List<String> LastPlayedGames = new List<string>();

        bool closeOnQuit = false;

        String _storyFileName;
        Frotz.Blorb.Blorb _blorbFile;

        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            InitializeComponent();

            Properties.Settings.Default.Upgrade();

            Border b = new Border();
            b.BorderThickness = new Thickness(1);
            b.BorderBrush = Brushes.Black;

            // _screen = new Screen.TextControlScreen(this);
            _screen = new Absolute.AbsoluteScreen(this);
            pnlScreenPlaceholder.Children.Add(b);

            b.Child = (UIElement)_screen;
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);

            if (Properties.Settings.Default.LastPlayedGames != null)
            {
                var games = Properties.Settings.Default.LastPlayedGames.Split('|');
                LastPlayedGames = new List<string>(games);
            }

            buildMainMenu();

            this.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);

            this.TextInput += new TextCompositionEventHandler(MainWindow_TextInput);
            this.PreviewKeyDown += new KeyEventHandler(MainWindow_PreviewKeyDown);

            statusBottom.Visibility = System.Windows.Visibility.Hidden;

            setFrotzOptions();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("EX:" + e.ExceptionObject);
        }

        void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // I can capture the arrow keys here
            if (!mnuInGame.IsFocused)
            {
                char c = '\0';

                switch (e.Key)
                {
                    case Key.Tab:
                        c = '\t'; break;
                    case Key.Up:
                        c = (char)Frotz.Constants.CharCodes.ZC_ARROW_UP;
                        break;
                    case Key.Down:
                        c = (char)Frotz.Constants.CharCodes.ZC_ARROW_DOWN;
                        break;
                    case Key.Left:
                        c = (char)Frotz.Constants.CharCodes.ZC_ARROW_LEFT;
                        break;
                    case Key.Right:
                        c = (char)Frotz.Constants.CharCodes.ZC_ARROW_RIGHT;
                        break;
                }

                if (c != 0)
                {
                    _screen.AddInput(c);
                    e.Handled = true;

                }
            }
        }

        void MainWindow_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (_screen != null)
            {
                if (e.Text.Length > 0)
                {
                    _screen.AddInput(e.Text[0]);
                }

                if (e.SystemText.Length > 0)
                {
                    ushort newKey = convertAltText(e.SystemText);

                    if (newKey != '\0')
                    {
                        _screen.AddInput((char)newKey);
                    }
                }
            }
        }

        // I'd like to make this a return char
        private ushort convertAltText(String text)
        {
            char k = text.ToLower()[0];
            switch (k)
            {
                case 'h': return CharCodes.ZC_HKEY_HELP;
                case 'd': return CharCodes.ZC_HKEY_DEBUG;
                case 'p': return CharCodes.ZC_HKEY_PLAYBACK;
                case 'r': return CharCodes.ZC_HKEY_RECORD;

                case 's': return CharCodes.ZC_HKEY_SEED;
                case 'u': return CharCodes.ZC_HKEY_UNDO;
                case 'n': return CharCodes.ZC_HKEY_RESTART;
                case 'x': return CharCodes.ZC_HKEY_QUIT;
            }

            return 0;
        }

        private void buildMainMenu()
        {
            miRecentGames.Items.Clear();
            miGames.Items.Clear();

            foreach (String s in LastPlayedGames)
            {
                MenuItem mi = new MenuItem();
                mi.Header = s;
                mi.Tag = s;
                mi.Click += new RoutedEventHandler(miMru_Click);
                miRecentGames.Items.Add(mi);
            }

            setupGameDirectories();
        }

        private void setupGameDirectories()
        {

            String gameDirectories = Properties.Settings.Default.GameDirectoryList;

            miGames.Items.Clear();

            if (!String.IsNullOrWhiteSpace(gameDirectories))
            {
                String[] list = gameDirectories.Split(';');
                if (list.Length == 1)
                {
                    addFilesInPath(list[0], miGames, true);

                    if (miGames.Items.Count == 1)
                    {
                        MenuItem mi = miGames.Items[0] as MenuItem;
                        List<MenuItem> items = new List<MenuItem>();


                        foreach (MenuItem i in mi.Items)
                        {
                            items.Add(i);
                        }

                        foreach (MenuItem i in items)
                        {
                            mi.Items.Remove(i);
                            miGames.Items.Add(i);
                        }

                        miGames.Items.Remove(mi);
                    }

                    // DirectoryInfo di 
                }
                else
                {
                    // TODO Make Recurse an option
                    foreach (String dir in gameDirectories.Split(';'))
                    {
                        try
                        {
                            addFilesInPath(dir, miGames, true);
                        }
                        catch (DirectoryNotFoundException) { }
                        catch (ArgumentException) { }
                    }
                }
                miGames.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                miGames.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void addFilesInPath(String Path, MenuItem Parent, bool Recurse)
        {
            DirectoryInfo di = new DirectoryInfo(Path);
            MenuItem miRoot = new MenuItem();
            miRoot.Header = di.Name;

            if (Recurse == true)
            {
                foreach (var sub in di.GetDirectories())
                {
                    addFilesInPath(sub.FullName, miRoot, Recurse);
                }
            }

            foreach (var fi in di.GetFiles())
            {
                switch (fi.Extension.ToLower())
                {
                    case ".z1":
                    case ".z2":
                    case ".z3":
                    case ".z4":
                    case ".z5":
                    case ".z6":
                    case ".z7":
                    case ".z8":
                    case ".zblorb":
                    case ".dat":
                        addGameItem(fi.FullName, miRoot);
                        break;
                }
            }

            if (miRoot.Items.Count > 0)
            {
                Parent.Items.Add(miRoot);
            }
        }

        private void addGameItem(String Path, MenuItem parent)
        {
            var fi = new System.IO.FileInfo(Path);
            MenuItem mi = new MenuItem();
            mi.Header = fi.Name;
            mi.Tag = Path;
            mi.Click += new RoutedEventHandler(miMru_Click);

            parent.Items.Add(mi);
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualHeight > 0 && this.ActualWidth > 0)
            {
                _screen.SetCharsAndLines();
                stsItemSize.Content = String.Format("{0}x{1}", _screen.Metrics.Rows, _screen.Metrics.Columns);
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            String[] tempArgs = Environment.GetCommandLineArgs();
            String[] args = new string[tempArgs.Length - 1];
            if (args.Length > 0)
            {
                Array.Copy(tempArgs, 1, args, 0, args.Length);
                // closeOnQuit = true;
                StartThread(args);
            }
        }

        private void StartThread(String[] args)
        {
            _zThread = new Thread(new ParameterizedThreadStart(ZMachineThread));
            _zThread.IsBackground = true;
            _zThread.Start(args);
        }

        public void ZMachineThread(Object argsO)
        {
            String[] args = (String[])argsO;
            if (args.Length > 0 && args[0] == "last" && LastPlayedGames.Count > 0)
            {
                args[0] = LastPlayedGames[LastPlayedGames.Count - 1];
            }

            try
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    mnuInGame.Visibility = System.Windows.Visibility.Visible;
                    mnuMain.Visibility = System.Windows.Visibility.Collapsed;
                    _screen.Focus();

                    if (Properties.Settings.Default.ShowDebugMenu == true)
                    {
                        miDebugInfo.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        miDebugInfo.Visibility = System.Windows.Visibility.Collapsed;
                    }

                }));
                Frotz.os_.SetScreen((IZScreen)_screen);

                ZColorCheck.resetDefaults();

                _screen.GameSelected += new EventHandler<GameSelectedEventArgs>(_screen_GameSelected);
                Frotz.Generic.main.MainFunc((String[])args);

                Dispatcher.Invoke(new Action(delegate
                {
                    _screen.Reset();
                }));

                if (closeOnQuit)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        this.Close();
                    }));
                }
            }
            catch (ZMachineException)
            { // Noop
            }
            catch (ThreadAbortException)
            { // TODO It may be wise to handle this
            }
            catch (Exception ex)
            {
                MessageBox.Show("EX:" + ex);
            }
            finally
            {
                _screen.GameSelected -= new EventHandler<GameSelectedEventArgs>(_screen_GameSelected);
                Dispatcher.Invoke(new Action(delegate
                {
                    buildMainMenu();

                    mnuInGame.Visibility = System.Windows.Visibility.Collapsed;
                    mnuMain.Visibility = System.Windows.Visibility.Visible;

                    this.Title = "FrotzNET";
                }));
            }
        }

        void _screen_GameSelected(object sender, GameSelectedEventArgs e)
        {
            String s = e.StoryFileName;

            for (int i = 0; i < LastPlayedGames.Count; i++)
            {
                if (String.IsNullOrWhiteSpace(LastPlayedGames[i]) || String.Compare(LastPlayedGames[i], s, true) == 0)
                {
                    LastPlayedGames.RemoveAt(i--);
                }
            }

            LastPlayedGames.Add(s);


            while (LastPlayedGames.Count > Properties.Settings.Default.LastPlayedGamesCount)
            {
                LastPlayedGames.RemoveAt(0);
            }

            Properties.Settings.Default.LastPlayedGames = String.Join("|", LastPlayedGames.ToArray());
            Properties.Settings.Default.Save();

            _storyFileName = e.StoryFileName;
            _blorbFile = e.BlorbFile;

            miGameInfo.IsEnabled = (_blorbFile != null);
        }

        private void setFrotzOptions()
        {
            var settings = Properties.Settings.Default;


            Frotz.Generic.main.option_context_lines = settings.FrotzContextLines;
            Frotz.Generic.main.option_left_margin = settings.FrotzLeftMargin;
            Frotz.Generic.main.option_right_margin = settings.FrotzRightMargin;
            Frotz.Generic.main.option_script_cols = settings.FrotzScriptColumns;
            Frotz.Generic.main.option_undo_slots = settings.FrotzUndoSlots;

            Frotz.Generic.main.option_attribute_assignment = settings.FrotzAttrAssignment;
            Frotz.Generic.main.option_attribute_testing = settings.FrotzAttrTesting;
            Frotz.Generic.main.option_expand_abbreviations = settings.FrotzExpandAbbreviations;
            Frotz.Generic.main.option_ignore_errors = settings.FrotzIgnoreErrors;
            Frotz.Generic.main.option_object_locating = settings.FrotzObjLocating;
            Frotz.Generic.main.option_object_movement = settings.FrotzObjMovement;
            Frotz.Generic.main.option_piracy = settings.FrotzPiracy;

            Frotz.Generic.main.option_save_quetzal = settings.FrotzSaveQuetzal;
            Frotz.Generic.main.option_sound = settings.FrotzSound;



        }

        #region Menu Events
        private void mnuQuitGame_Click(object sender, RoutedEventArgs e)
        {
            if (_zThread != null)
            {
                Frotz.Generic.main.abort_game_loop = true;
            }
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void miMru_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            String game = mi.Tag as String;

            if (_zThread != null)
            {
                // Should never get here since the menu isn't show while a game is in progress
                _zThread.Abort();
            }

            StartThread(new String[] { game });

        }

        void miOptions_Click(object sender, RoutedEventArgs e)
        {

            OptionsScreen os = new OptionsScreen();
            os.Owner = this;
            os.ShowDialog();

            _screen.setFontInfo();
            _screen.SetCharsAndLines();

            setupGameDirectories();
            setFrotzOptions();
        }

        void miStartNewStory_Click(object sender, RoutedEventArgs e)
        {
            if (_zThread != null)
            {
                _zThread.Abort();
            }
            StartThread(new String[0]);
        }

        void miExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void miGameInfo_Click(object sender, RoutedEventArgs e)
        {
            BlorbMetadata bm = new BlorbMetadata(_blorbFile);
            bm.Owner = this;
            bm.ShowDialog();
        }

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aw = new AboutWindow();
            aw.Owner = this;
            aw.ShowDialog();
        }
        #endregion

        private void miDebugInfo_Click(object sender, RoutedEventArgs e)
        {

            byte[] buffer = Frotz.os_.GetStoryFile();
            if (_blorbFile != null && _blorbFile.ZCode != null)
            {
                buffer = _blorbFile.ZCode;
            }
            else
            {
                FileStream fs = new FileStream(_storyFileName, FileMode.Open);
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();
            }

            try
            {
                var info = ZTools.InfoDump.main(buffer, new String[0]);

                Window w = new Window();
                TabControl tc = new TabControl();

                foreach (var val in info)
                {
                    createTextBox(tc, val.Header, val.Text);
                }

                String temp = ZTools.txd.main(buffer, new String[0]);
                String endOfCode = "[END OF CODE]";

                int index = temp.IndexOf(endOfCode, StringComparison.OrdinalIgnoreCase);


                if (index == -1)
                {
                    info.Add(new ZTools.InfoDump.ZToolInfo("TXD", temp));

                    createTextBox(tc, "TXD", temp);
                }
                else
                {
                    index += endOfCode.Length;

                    addTabItem(tc, "TXD - Code", new Support.ZInfoTXD(temp.Substring(0, index), 0));
                    addTabItem(tc, "TXD - Strings", new Support.ZInfoTXD(temp.Substring(index + 1), 1));
                }

                w.Content = tc;

                w.Show();
            }
            catch (ArgumentException ae)
            {
                MessageBox.Show("Exception\r\n" + ae);
            }
        }

        private void createTextBox(TabControl tc, String header, String text)
        {

            TextBox tb = new TextBox();
            tb.Text = text;
            tb.FontFamily = new FontFamily("Courier New");

            ScrollViewer sv = new ScrollViewer();
            sv.Content = tb;

            addTabItem(tc, header, sv);
        }

        private void addTabItem(TabControl tc, String header, Control c)
        {
            TabItem ti = new TabItem();
            ti.Header = header;
            ti.Content = c;
            tc.Items.Add(ti);
        }

        private void miHistory_Click(object sender, RoutedEventArgs e)
        {
            DockPanel d = ((Absolute.AbsoluteScreen)_screen).Scrollback.DP;
            
            Window w = new Window();
            w.Content = d;
            w.Owner = this;

#if !temp
            w.ShowDialog();
            w.Content = null;
#else
            w.Show();
#endif
        }
    }
}
