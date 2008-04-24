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
	public partial class ViewSourceWindow: Window {
        protected List<Run> mLines = new List<Run>();
        protected FontFamily mFont = new FontFamily("Courier New");

		public ViewSourceWindow() {
			InitializeComponent();
		}

        public void LoadSourceFile(string aPathname) {
            var xSourceCode = System.IO.File.ReadAllLines(aPathname);
            var xPara = new Paragraph();
            fdsvSource.Document = new FlowDocument();
            fdsvSource.Document.PageWidth = 100 * 96;
            fdsvSource.Document.Blocks.Add(xPara);
            int xLineNo = 0;
            foreach (var xLine in xSourceCode) {
                Run xRun;
                
                xLineNo++;
                xRun = new Run(xLineNo.ToString().PadLeft(6) + " ");
                xRun.MouseDown += new MouseButtonEventHandler(xRun_MouseDown);
                xRun.FontFamily = mFont;
                xRun.Cursor = Cursors.Arrow;
                xRun.Background = Brushes.LightGray;
                xPara.Inlines.Add(xRun);

                xRun = new Run(xLine);
                xRun.FontFamily = mFont;
                mLines.Add(xRun);
                xPara.Inlines.Add(xRun);

                xPara.Inlines.Add(new LineBreak());
            }
        }

        void xRun_MouseDown(object sender, MouseButtonEventArgs e) {
            var xRun = (Run)sender;
            if (xRun.Background == Brushes.Red) {
                xRun.Background = Brushes.LightGray;
            } else {
                xRun.Background = Brushes.Red;
            }
        }

        protected void Select(int aLine, int aColBegin, int aLength) {
            if (aLength != 0) {
                var xPara = (Paragraph)fdsvSource.Document.Blocks.FirstBlock;
                var xSelectedLine = mLines[aLine];
                string xText = xSelectedLine.Text;
                if (aLength == -1) {
                    aLength = xText.Length - aColBegin;
                }

                if (aColBegin > 0) {
                    var xRunLeft = new Run(xText.Substring(0, aColBegin - 1));
                    xRunLeft.FontFamily = mFont;
                    xPara.Inlines.InsertBefore(xSelectedLine, xRunLeft);
                }

                var xRunSelected = new Run(xText.Substring(aColBegin, aLength));
                xRunSelected.FontFamily = mFont;
                xRunSelected.Background = Brushes.Red;
                xPara.Inlines.InsertBefore(xSelectedLine, xRunSelected);

                if (aColBegin + aLength < xText.Length) {
                    var xRunRight = new Run(xText.Substring(aColBegin + aLength));
                    xRunRight.FontFamily = mFont;
                    xPara.Inlines.InsertBefore(xSelectedLine, xRunRight);
                }

                xPara.Inlines.Remove(xSelectedLine);
            }
        }

        public void SelectText(int aLineBegin, int aColBegin, int aLineEnd, int aColEnd) {
            aLineBegin--;
            aColBegin--;
            aLineEnd--;
            aColEnd--;
            //Currently can only be called once - need to fix it to reset so it can be called multiple times
            if (aLineBegin == aLineEnd) {
                Select(aLineBegin, aColBegin, aColEnd - aColBegin);
            } else {
                Select(aLineBegin, aColBegin, -1);
                for (int i = aLineBegin + 1; i <= aLineEnd - 1; i++) {
                    Select(i, 0, -1);
                }
                Select(aLineEnd, 0, aColEnd + 1);
            }
        }

	}
}