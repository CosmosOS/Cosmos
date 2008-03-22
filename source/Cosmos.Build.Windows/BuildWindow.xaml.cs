using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class BuildWindow : Window {
        protected class CaptureWriter : TextWriter {
            protected StringBuilder mLine = new StringBuilder();
            public delegate void NewLineDelegate(string aLine);
            protected NewLineDelegate mOnNewLine;

            public CaptureWriter(NewLineDelegate aOnNewLine) {
                mOnNewLine += aOnNewLine;
            }

            public override Encoding Encoding {
                get { return null; }
            }

            public override void Write(char aValue) {
                if (aValue == "\r"[0]) {
                } else if (aValue == "\n"[0]) {
                    mOnNewLine(mLine.ToString());
                    mLine.Length = 0;
                } else {
                    mLine.Append(aValue);
                }
            }
        }

        public void NewLine(string aLine) {
            lboxLog.SelectedIndex = lboxLog.Items.Add(aLine);
        }

        public BuildWindow() {
            InitializeComponent();
            var xWriter = new CaptureWriter(NewLine);
            Console.SetOut(xWriter);
        }
    }
}
