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

namespace Cosmos.Build.Windows {
    /// <summary>
    /// Interaction logic for BuildOptionsWindow.xaml
    /// </summary>
    public partial class BuildOptionsWindow : Window, IBuildConfiguration {


        public BuildOptionsWindow() {
            InitializeComponent();

            KeyDown += new KeyEventHandler(BuildOptionsWindow_KeyDown);
            Loaded += new RoutedEventHandler(BuildOptionsWindow_Loaded);
            
            textBuildPath.Text = Builder.GetBuildPath();
            textBuildPath.IsEnabled = false;

            foreach (var xTarget in Enum.GetNames(typeof(Builder.Target))) {
                lboxTargets.Items.Add((lboxTargets.Items.Count + 1).ToString() + ": " + xTarget.Replace('_', ' '));
            }
        }

        void BuildOptionsWindow_Loaded(object sender, RoutedEventArgs e) {
            //Stupid window always shows up behind console, bring it up.
            this.Activate();
        }

        void BuildOptionsWindow_KeyDown(object sender, KeyEventArgs e) {
            var xConverter = new KeyConverter();
            char xChar = xConverter.ConvertToString(e.Key)[0];
            if (Char.IsDigit(xChar)) {
                int xValue = int.Parse(xChar.ToString());
                if (xValue > 0) {
                    if (xValue <= lboxTargets.Items.Count) {
                        string xType = (string)(lboxTargets.Items[xValue - 1]);
                        Hide();
                        var xBuilder = new Builder();
                        target = (Builder.Target)Enum.Parse(typeof(Builder.Target), xType.Remove(0, 3).Replace(' ', '_'));
                        Close();
                    }
                }
                e.Handled = true;
            }
        }

        #region IBuildConfiguration Members

        private Builder.Target target;
        public Builder.Target Target
        {
            get
            {
                return target;
            }
            set
            {

            }
        }

        public bool Compile
        {
            get
            {
                return buildCheckBox.IsChecked.Value;
            }
            set
            {
                buildCheckBox.IsChecked = value;
            }
        }

        #endregion
    }
}
