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
    public partial class BuildOptionsWindow : Window, IBuildConfiguration {

        protected Block mOptionsBlockPrefix;
        public BuildOptionsWindow() {
            InitializeComponent();

            Loaded += delegate(object sender, RoutedEventArgs e) {
                this.Activate();
            };
            butnBuild.Click += new RoutedEventHandler(butnBuild_Click);

            rdioQEMU.Checked += new RoutedEventHandler(rdioTarget_Checked);
            rdioQEMU.Unchecked += new RoutedEventHandler(rdioTarget_Unchecked);
            rdioPXE.Checked += new RoutedEventHandler(rdioPXE_Checked);
            rdioPXE.Unchecked += new RoutedEventHandler(rdioPXE_Unchecked);

            spanBuildPath.Inlines.Add(Builder.GetBuildPath());

            mOptionsBlockPrefix = paraQEMUOptions.PreviousBlock;
            RootDoc.Blocks.Remove(paraQEMUOptions);
            RootDoc.Blocks.Remove(paraPXEOptions);
        }

        void rdioPXE_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraPXEOptions);
        }

        void rdioPXE_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraPXEOptions);
        }

        void rdioTarget_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraQEMUOptions);
        }

        void rdioTarget_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraQEMUOptions);
        }

        void butnBuild_Click(object sender, RoutedEventArgs e) {
            if (rdioQEMU.IsChecked.Value) {
                if (chckQEMUUseGDB.IsChecked.Value) {
                    if (chckQEMUUseHD.IsChecked.Value) {
                        mTarget = Builder.Target.QEMU_GDB_With_Hard_Disk_Image;
                    } else {
                        mTarget = Builder.Target.QEMU_GDB;
                    }
                } else {
                    if (chckQEMUUseHD.IsChecked.Value) {
                        mTarget = Builder.Target.QEMU_With_Hard_Disk_Image;
                    } else {
                        mTarget = Builder.Target.QEMU;
                    }
                }
            } else if (rdioVMWare.IsChecked.Value) {
                mTarget = Builder.Target.ISO;
            } else if (rdioVPC.IsChecked.Value) {
                mTarget = Builder.Target.ISO;
            } else if (rdioISO.IsChecked.Value) {
                mTarget = Builder.Target.ISO;
            } else if (rdioPXE.IsChecked.Value) {
                mTarget = Builder.Target.PXE;
            }
            Close();
        }

        #region IBuildConfiguration Members

        private Builder.Target mTarget;
        public Builder.Target Target {
            get {
                return mTarget;
            }
            set {
            }
        }

        public bool Compile {
            get {
                return buildCheckBox.IsChecked.Value;
            }
            set {
                buildCheckBox.IsChecked = value;
            }
        }

        #endregion
    }
}
