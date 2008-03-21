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
        protected Builder mBuilder;

        public BuildOptionsWindow(Builder aBuilder) {
            InitializeComponent();
            mBuilder = aBuilder;

            Loaded += delegate(object sender, RoutedEventArgs e) {
                this.Activate();
            };

            butnBuild.Click += new RoutedEventHandler(butnBuild_Click);
            butnCancel.Click += new RoutedEventHandler(butnCancel_Click);

            rdioQEMU.Checked += new RoutedEventHandler(rdioTarget_Checked);
            rdioQEMU.Unchecked += new RoutedEventHandler(rdioTarget_Unchecked);
            rdioVMWare.Checked += new RoutedEventHandler(rdioVMWare_Checked);
            rdioVMWare.Unchecked += new RoutedEventHandler(rdioVMWare_Unchecked);
            rdioVPC.Checked += new RoutedEventHandler(rdioVPC_Checked);
            rdioVPC.Unchecked += new RoutedEventHandler(rdioVPC_Unchecked);
            rdioISO.Checked += new RoutedEventHandler(rdioISO_Checked);
            rdioISO.Unchecked += new RoutedEventHandler(rdioISO_Unchecked);
            rdioPXE.Checked += new RoutedEventHandler(rdioPXE_Checked);
            rdioPXE.Unchecked += new RoutedEventHandler(rdioPXE_Unchecked);

            spanBuildPath.Inlines.Add(mBuilder.BuildPath);
            spanISOPath.Inlines.Add(mBuilder.BuildPath + "Cosmos.iso");

            mOptionsBlockPrefix = paraQEMUOptions.PreviousBlock;
            //RootDoc.Blocks.Remove(paraQEMUOptions);
            RootDoc.Blocks.Remove(paraVMWareOptions);
            RootDoc.Blocks.Remove(paraVPCOptions);
            RootDoc.Blocks.Remove(paraISOOptions);
            RootDoc.Blocks.Remove(paraPXEOptions);

            LoadSettingsFromRegistry();
        }

        void rdioISO_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraISOOptions);
        }
        void rdioISO_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraISOOptions);
        }

        void rdioVPC_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraVPCOptions);
        }
        void rdioVPC_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraVPCOptions);
        }

        void rdioVMWare_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraVMWareOptions);
        }
        void rdioVMWare_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraVMWareOptions);
        }

        void rdioPXE_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraPXEOptions);
        }
        void rdioPXE_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraPXEOptions);
        }

        void rdioTarget_Checked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix, paraQEMUOptions);
        }
        void rdioTarget_Unchecked(object sender, RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraQEMUOptions);
        }

        void butnBuild_Click(object sender, RoutedEventArgs e) {
			mTarget = Builder.Target.QEMU;
			if (rdioQEMU.IsChecked.Value) {
                if (chckQEMUUseGDB.IsChecked.Value) {
                    if (chckQEMUUseHD.IsChecked.Value) {
                        mTarget = Builder.Target.QEMU_GDB_HardDisk;
                    } else {
                        mTarget = Builder.Target.QEMU_GDB;
                    }
                } else {
                    if (chckQEMUUseHD.IsChecked.Value) {
                        mTarget = Builder.Target.QEMU_HardDisk;
                    }
                }
            } else if (rdioVMWare.IsChecked.Value) {
                mTarget = Builder.Target.VMWare;
            } else if (rdioVPC.IsChecked.Value) {
                mTarget = Builder.Target.VPC;
            } else if (rdioISO.IsChecked.Value) {
                mTarget = Builder.Target.ISO;
            } else if (rdioPXE.IsChecked.Value) {
                mTarget = Builder.Target.PXE;
            }

            SaveSettingsToRegistry();
            this.DialogResult = true;
        }

        void butnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        void SaveSettingsToRegistry()
        {
            string key = "BuildType";
            if (rdioQEMU.IsChecked.Value)
                BuildRegistry.Write(key, "QEMU");
            else if (rdioVMWare.IsChecked.Value)
                BuildRegistry.Write(key, "VMWare");
            else if (rdioVPC.IsChecked.Value)
                BuildRegistry.Write(key, "VPC");
            else if (rdioISO.IsChecked.Value)
                BuildRegistry.Write(key, "ISO");
            else if (rdioPXE.IsChecked.Value)
                BuildRegistry.Write(key, "PXE");
            
            key = "UseGDB";
            BuildRegistry.Write(key, chckQEMUUseGDB.IsChecked.Value.ToString());

            key = "CreateHDImage";
            BuildRegistry.Write(key, chckQEMUUseHD.IsChecked.Value.ToString());

            key = "SkipIL";
            BuildRegistry.Write(key, buildCheckBox.IsChecked.Value.ToString());
        }

        void LoadSettingsFromRegistry()
        {
            string buildType = BuildRegistry.Read("BuildType");
            switch (buildType)
            {
                case "QEMU":
                    rdioQEMU.IsChecked = true;
                    break;
                case "VMWare":
                    rdioVMWare.IsChecked = true;
                    break;
                case "VPC":
                    rdioVPC.IsChecked = true;
                    break;
                case "ISO":
                    rdioISO.IsChecked = true;
                    break;
                case "PXE":
                    rdioPXE.IsChecked = true;
                    break;
            }


            bool useGDB;
            bool.TryParse(BuildRegistry.Read("UseGDB"), out useGDB);
            chckQEMUUseGDB.IsChecked = useGDB;

            bool createHDimg;
            bool.TryParse(BuildRegistry.Read("CreateHDImage"), out createHDimg);
            chckQEMUUseHD.IsChecked = createHDimg;

            bool skipIL;
            bool.TryParse(BuildRegistry.Read("SkipIL"), out skipIL);
            buildCheckBox.IsChecked = skipIL;

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
                return !buildCheckBox.IsChecked.Value;
            }
            set {
                buildCheckBox.IsChecked = value;
            }
        }

        #endregion
    }
}
