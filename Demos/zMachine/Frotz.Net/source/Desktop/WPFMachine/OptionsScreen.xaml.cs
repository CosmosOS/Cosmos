using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFMachine {
    /// <summary>
    /// Interaction logic for OptionsScreen.xaml
    /// </summary>
    public partial class OptionsScreen : Window, IComparer<FontFamily> {
        public OptionsScreen() {
            InitializeComponent();

            var settings = Properties.Settings.Default;

            List<FontFamily> fixedWidthFonts = new List<System.Windows.Media.FontFamily>();
            List<FontFamily> otherWidthFonts = new List<System.Windows.Media.FontFamily>();

            double maxFixedHeight = -1;
            double maxPropHeight = -1;

            FontFamily fixedWidthCurrent = null;
            FontFamily propWidthCurrent = null;

            int count = 0;

            foreach (var ff in Fonts.SystemFontFamilies)
            {
                count++;

                FormattedText ft = new FormattedText("i", System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, new Typeface(ff.Source), 10, Brushes.Black);

                var s = new Size(ft.Width, ft.Height);

                ft = new FormattedText("w", System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, new Typeface(ff.Source), 10, Brushes.Black);

                if (ft.Width == s.Width)
                {
                    fixedWidthFonts.Add(ff);

                    maxFixedHeight = Math.Max(maxFixedHeight, ft.Height);
                    if (ff.Source == settings.FixedWidthFont)
                    {
                        fixedWidthCurrent = ff;
                    }
                }
             
                otherWidthFonts.Add(ff);
                maxPropHeight = Math.Max(maxPropHeight, ft.Height);
                if (ff.Source == settings.ProportionalFont)
                {
                    propWidthCurrent = ff;
                }

            }

            fixedWidthFonts.Sort(this);
            otherWidthFonts.Sort(this);


            fddFixedWidth.ItemsSource = fixedWidthFonts;
            fddProportional.ItemsSource = otherWidthFonts;

            fddFixedWidth.SelectedItem = fixedWidthCurrent;
            fddProportional.SelectedItem = propWidthCurrent;

            tbFontSize.Text = settings.FontSize.ToString();

            ccForeColor.SelectedColor = settings.DefaultForeColor;
            ccBackColor.SelectedColor = settings.DefaultBackColor;
            ccInputColor.SelectedColor = settings.DefaultInputColor;

            tbLastPlayedCount.Text = settings.LastPlayedGamesCount.ToString();

            cbShowDebug.IsChecked = settings.ShowDebugMenu;

            String dirs = settings.GameDirectoryList;

            if (dirs != "")
            {
                String[] temp = dirs.Split(';');
                for (int i = 0; i < temp.Length; i++)
                {
                    addDirectory(temp[i]);
                }
            }

            tbLeftMargin.Text = settings.FrotzLeftMargin.ToString();
            tbRightMargin.Text = settings.FrotzRightMargin.ToString();
            tbContextLines.Text = settings.FrotzContextLines.ToString();
            tbUndoSlots.Text = settings.FrotzUndoSlots.ToString();
            tbScriptColumns.Text = settings.FrotzScriptColumns.ToString();

            cbPiracy.IsChecked = settings.FrotzPiracy;
            cbExpandAbbreviations.IsChecked = settings.FrotzExpandAbbreviations;
            cbIgnoreErrors.IsChecked = settings.FrotzIgnoreErrors;
            cbAttrAssignment.IsChecked = settings.FrotzAttrAssignment;
            cbAttrTesting.IsChecked = settings.FrotzAttrTesting;
            cbObjLocating.IsChecked = settings.FrotzObjLocating;
            cbObjMovement.IsChecked = settings.FrotzObjMovement;

            cbSaveQuetzal.IsChecked = settings.FrotzSaveQuetzal;
            cbSound.IsChecked = settings.FrotzSound;
        }

        void addDirectory(String dir)
        {
            Options.GameDirectory gd = new Options.GameDirectory(dir);
            gd.Click += new RoutedEventHandler(gd_Click);
            spGameList.Children.Add(gd);

            gdListRow.Height = new GridLength(spGameList.Children.Count * 30);
        }

        void gd_Click(object sender, RoutedEventArgs e)
        {
            Options.GameDirectory gd = sender as Options.GameDirectory;
            spGameList.Children.Remove(gd);

            gdListRow.Height = new GridLength(spGameList.Children.Count * 30);
        }

        public int Compare(FontFamily x, FontFamily y) {
            return String.Compare(x.Source, y.Source);
        }

        private void ok_Click(object sender, RoutedEventArgs e) {
            var settings = Properties.Settings.Default;

            FontFamily ff = fddFixedWidth.SelectedItem as FontFamily;
            if (ff != null) {
                Properties.Settings.Default.FixedWidthFont = ff.Source;
            }

            ff = fddProportional.SelectedItem as FontFamily;
            if (ff != null) {
                Properties.Settings.Default.ProportionalFont = ff.Source;
            }

            Properties.Settings.Default.FontSize = Convert.ToInt32(tbFontSize.Text);

            Properties.Settings.Default.DefaultForeColor = ccForeColor.SelectedColor;
            Properties.Settings.Default.DefaultBackColor = ccBackColor.SelectedColor;
            Properties.Settings.Default.DefaultInputColor = ccInputColor.SelectedColor;

            Properties.Settings.Default.LastPlayedGamesCount = Convert.ToInt32(tbLastPlayedCount.Text);

            settings.ShowDebugMenu = cbShowDebug.IsChecked ?? false;
            

            List<String> dirs = new List<string>();
            foreach (Options.GameDirectory gd in spGameList.Children)
            {
                dirs.Add(gd.Directory);
            }

            Properties.Settings.Default.GameDirectoryList = 
            String.Join(";", dirs.ToArray());

            settings.FrotzPiracy = cbPiracy.IsChecked ?? false;
            settings.FrotzExpandAbbreviations = cbExpandAbbreviations.IsChecked ?? false;
            settings.FrotzAttrAssignment = cbAttrAssignment.IsChecked ?? false;
            settings.FrotzAttrTesting = cbAttrTesting.IsChecked ?? false;
            settings.FrotzObjMovement = cbObjMovement.IsChecked ?? false;
            settings.FrotzObjLocating = cbObjLocating.IsChecked ?? false;

            settings.FrotzSaveQuetzal = cbSaveQuetzal.IsChecked ?? false;
            settings.FrotzSound = cbSound.IsChecked ?? false;

            settings.FrotzLeftMargin = Convert.ToUInt16(tbLeftMargin.Text);
            settings.FrotzRightMargin = Convert.ToUInt16(tbRightMargin.Text);
            settings.FrotzContextLines= Convert.ToUInt16(tbContextLines.Text);
            settings.FrotzUndoSlots = Convert.ToInt32(tbUndoSlots.Text);
            settings.FrotzScriptColumns = Convert.ToInt32(tbScriptColumns.Text);

            Properties.Settings.Default.Save();

            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void DockPanel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ok_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                cancel_Click(this, new RoutedEventArgs());
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ok_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                cancel_Click(this, new RoutedEventArgs());
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                addDirectory(fbd.SelectedPath);
            }
        }
    }
}
