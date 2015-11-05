//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using System.Reflection;

using Microsoft.Samples.Debugging.MdbgEngine;

namespace Microsoft.Samples.Tools.Mdbg.Extension
{
    // Source window that represents a raw source file.
    internal class SourceViewerForm : SourceViewerBaseForm
    {
        // parent - main containing window that this source window lives inside of.
        // path - unique string identifier for source window (full pathname)
        // lines - content
        SourceViewerForm(MainForm parent,string path,ArrayList lines)
        {
            m_lines = lines;
            BeginInit(parent);

            InitLines("{0,4}:", lines, null);

            EndInit(path);
        }

        // Get the object that keys this SourceViewer in our global hash.         
        protected override object GetHashKey()
        {
            return this.Text;            
        }

        // Public build function.
        // Fails to build (and returns null) if it can't load the file.
        static public SourceViewerForm Build(MainForm parent,string path)
        {
            ArrayList lines = LoadLinesFromFile(path);
            if (lines == null) { return null; }

            return new SourceViewerForm(parent, path, lines);
        }

        private static ArrayList LoadLinesFromFile(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                try
                {
                    ArrayList lines = new ArrayList();

                    string s = sr.ReadLine();
                    while (s != null)
                    {
                        lines.Add(s);
                        s = sr.ReadLine();
                    }
                    return lines;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        
        #region Expose lines
        // Lines in the file.
        ArrayList m_lines;
                
        // Get the 1-based line from the source file.
        public string GetLine(int line)
        {
            if (line < 1 || line > m_lines.Count)
                throw new ArgumentOutOfRangeException("line", line, "Line is out of range");
            return (string) m_lines[line-1];
        }

        // Get 1-base value for number of lines in the file.
        public int Count
        {
            get
            {
                return m_lines.Count + 1;
            }
        }
        #endregion Expose lines

        // Track if current source-position is in this doc.
        // Null if no current source, or if it's  in another doc.
        MDbgSourcePosition m_pos;

        protected override void OnBreakWorker()
        {
            ClearHighlight();

            m_pos = MainForm.CurrentSource;
            if (m_pos == null)
            {
                return;
            }
            if (m_pos.Path != this.Text)
            {
                m_pos = null;
            }

            // Hilight in current source
            if (m_pos != null)
            {
                HighlightStatementAtPos(m_pos);
            }
        }

        protected override void OnRunWorker()
        {            
            m_pos = null;
        }
                
        protected override void DrawGlyphWorker(PaintEventArgs e)
        {
            // base class does nothing.
            // Draw current source-line arrows in the glyph bar.
            MDbgSourcePosition pos = this.m_pos;
            if (pos != null)
            {
                {
                    int iLine = pos.Line;

                    Bitmap bmp = this.MainForm.IsCurrentSourceActive ? m_glyphs.CurrentLineArrow : m_glyphs.NotCurrentLineArrow;
                    this.DrawGlyphAtLine(e, bmp, iLine);
                }
            }

            DrawBreakpoints(e);

        }

        void DrawBreakpoints(PaintEventArgs e)
        {
            foreach (BreakpointPair p in m_Breakpoints)
            {
                int iLine = p.m_iLine;
                Bitmap bmp = p.m_bp.IsBound ? m_glyphs.Breakpoint : m_glyphs.UnboundBreakpoint;
                this.DrawGlyphAtLine(e, bmp, iLine);
            }
        }

        // Do a source-level step over.
        protected override bool DoStepOver()
        {
            MainForm.AsyncProcessEnteredText("next");
            return true;
        }
        // Do a source-level step over.
        protected override bool DoStepIn()
        {
            MainForm.AsyncProcessEnteredText("step");
            return true;
        }

        #region Breakpoint support
        // Unsorted List of the MDbgBreakpoint objects in this file.
        // Only access from UI thread.
        ArrayList m_Breakpoints = new ArrayList();

        // We don't have a good way of getting the line number from an MDbgBreakpoint, 
        // so we have to store it in a pair.
        class BreakpointPair
        {
            public BreakpointPair(MDbgBreakpoint bp, int iLine)
            {
                m_bp = bp;
                m_iLine = iLine;
            }
            public MDbgBreakpoint m_bp;
            public int m_iLine;
        }

        // Toggle a breakpoint at the given line.
        // Called on UI thread.
        protected override void ToggleBreakpointAtLine(int row) 
        {
            // Currently, we require an active process. 
            // An alternative is if the source file remembered the unbound breakpoints and then bound them
            // when the process started (and modules got loaded).
            if (!GuiExtension.Shell.Debugger.Processes.HaveActive)
            {
                return;
            }

            // See if we have anything to remove.
            foreach (BreakpointPair p in m_Breakpoints)
            {
                if (p.m_iLine == row)
                {
                    MainForm.ExecuteOnWorkerThreadIfStoppedAndBlock(delegate(MDbgProcess process)
                    {
                        p.m_bp.Delete();
                    });
                    m_Breakpoints.Remove(p);
                    return;
                }
            }

            // Ideally, BreakpointCollection would fire events for new breakpoints being added and removed.
            // This would be good because:
            // - the Gui would then know about breakpoints set from the command line.
            // - the Gui could then set breakpoints by issuing a Text Command (instead of
            //   duplicating the logic again here).

            // Nothing to remove, so must be adding.
            MDbgBreakpoint b = null;
            MainForm.ExecuteOnWorkerThreadIfStoppedAndBlock(delegate(MDbgProcess proc)
            {
                MDbgBreakpointCollection c = proc.Breakpoints;                                
                {
                    b = c.CreateBreakpoint(this.Text, row);
                }
            });
            Debug.Assert(b != null);
            m_Breakpoints.Add(new BreakpointPair(b, row));
        }

        #endregion Breakpoint support
    }


    /// <summary>
    /// SourceViwerForm
    /// One of these for each source-file.
    /// </summary>
    
    // A source window base class provides several features:
    // - glyph management (placing icons in a left side bar next to given lines)
    // - key trappings 
    // - highlighting active statements
    // - a left bar source-line column
    abstract internal class SourceViewerBaseForm : System.Windows.Forms.Form
    {
        #region formatting
        abstract public class FormatBase
        {
            public abstract void DoFormat(RichTextBox box, int[] rowMapping);
        }
        // format an entire row to a given color
        public class FormatRow : FormatBase
        {
            // Format an entire row the given color.
            public FormatRow(int row, Color c)
            {
                m_row = row;
                m_color = c;
            }
            int m_row;
            Color m_color;

            public override void DoFormat(RichTextBox box, int[] rowMapping)
            {
                int startIdx = rowMapping[m_row];
                int endIdx = (m_row == rowMapping.Length - 1) ? box.TextLength : rowMapping[m_row + 1];


                box.Select(startIdx, endIdx - startIdx);
                box.SelectionColor = m_color;
            }
        }

        // Format a subsection of a row to a given font, color
        public class FormatSpan : FormatBase
        {
            // Format an entire row the given color.
            public FormatSpan(int row, int columnStart, int columnEnd, Font f, Color c)
            {
                m_row = row;
                m_columnStart = columnStart;
                m_columnEnd = columnEnd;

                Debug.Assert(0 <= m_columnStart);
                Debug.Assert(m_columnStart < m_columnEnd);

                m_color = c;                
                m_font = f;
            }
            int m_row;
            int m_columnStart;
            int m_columnEnd;
            Color m_color;
            Font m_font;

            public override void DoFormat(RichTextBox box, int[] rowMapping)
            {
                int startRowIdx = rowMapping[m_row];
                int endRowIdx = (m_row == rowMapping.Length - 1) ? box.TextLength : rowMapping[m_row + 1];
                int rowLen = endRowIdx - startRowIdx;
                
                Debug.Assert(m_columnEnd <= rowLen);

                int spanLen = m_columnEnd - m_columnStart;

                box.Select(startRowIdx + m_columnStart-1, spanLen);
                if (m_color != Color.Empty)
                {
                    box.SelectionColor = m_color;
                }
                if (m_font != null)
                {
                    box.SelectionFont = m_font;
                }
            }
        }
        #endregion

        private IContainer components;

        #region Initialization
        // Initialization function. called by derived ctor.

        // parent - main containing window that this source window lives inside of.
        // path - unique string identifier for source window (full pathname)
        // lines - content
        protected void BeginInit(MainForm parent)
        {
            if (m_glyphs == null)
            {
                m_glyphs = new Glyphs();
            }

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // Set window properties
            MdiParent = parent;
            Debug.Assert(parent == MainForm);

            // Still not visisible. We return from here, and then caller will initialize content
            // and then call EndInit().
        }

        // Derived class ctor calls after this.
        protected void EndInit(string title)
        {
            Debug.Assert(m_lineOffset != null); // Derived class should have initialized this.
            Debug.Assert(this.richText.Lines.Length == m_lineOffset.Length + 1);

            Visible=true;
            Text = title;
        }

        // Initialize text box contents with 'lines' array.
        // if 'index' is non-null, then we use it as a format string to provide an index column on the left 
        // that gives line number. The format takes 1 integer parameter for the 1-based line number.
        // This cooperates with GetOffsetFromPos(). 
        protected void InitLines(string indexFormat, ArrayList lines, ArrayList formats)
        {
            // Copy string content to window.
            m_lineOffset = new int[lines.Count];
            int index = 0;
            int c = 0;
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
            {
                m_lineOffset[c++] = index;

                string lineNoStr = (indexFormat != null) ? String.Format(CultureInfo.InvariantCulture, indexFormat, c) : "";
                m_indexBarWidth = lineNoStr.Length;

                sb.Append(lineNoStr);
                sb.Append(line).Append((char)13).Append((char)10);
                index += m_indexBarWidth + line.Length + 1;
            }

            // Set the text
            this.richText.Text = sb.ToString();

            if (formats != null)
            {
                foreach (FormatBase f in formats)
                {
                    f.DoFormat(this.richText, m_lineOffset);
                }
                this.richText.SelectionLength = 0;
            }
        }
        #endregion Initialization

        // Get the MainForm that this source file is docked in.
        protected MainForm MainForm
        {
            get { return (MainForm)MdiParent; }
        }
        
        // Get the object that keys this SourceViewer in our global hash. 
        protected abstract object GetHashKey();

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            // if disposing==true, then we release managed + unmanaged resources.
            // if disposing==false, then we only release unmanaged resources.
            if( disposing )
            {
                m_sourceList.Remove(this.GetHashKey());
                if (m_ActiveSourceFile == this)
                {
                    m_ActiveSourceFile = null;
                }
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        //#region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.richText = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // richText
            // 
            this.richText.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.richText.BackColor = System.Drawing.SystemColors.Window;
            this.richText.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richText.HideSelection = false;
            this.richText.Location = new System.Drawing.Point(37, 0);
            this.richText.Name = "richText";
            this.richText.ReadOnly = true;
            this.richText.Size = new System.Drawing.Size(255, 273);
            this.richText.TabIndex = 0;
            this.richText.Text = "richText";
            this.richText.WordWrap = false;
            this.richText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richText_KeyDown);
            this.richText.VScroll += new System.EventHandler(this.richText_VScroll);
            this.richText.Invalidated += new System.Windows.Forms.InvalidateEventHandler(this.richText_VScroll);
            this.richText.TextChanged += new System.EventHandler(this.richText_TextChanged);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // SourceViewerForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.richText);
            this.BackColor = Color.FromArgb(192, 192, 192);
            this.Name = "SourceViewerForm";
            this.Text = "SourceViewer";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.SourceViewerForm_Paint);
            this.Resize += new System.EventHandler(this.SourceViewerForm_Resize);
            this.ResumeLayout(false);

        }
        //#endregion

        // The main source window.
        protected System.Windows.Forms.RichTextBox richText;

        // List of all open source windows. May be indexed multiple ways.
        // SourceFiles are indexed by filename. VirtualSource windows are indexed by MDbgFunction object.
        private static Hashtable m_sourceList = new Hashtable();

        // Mapping of where each line starts. 
        // m_lineOffset[line#] = character index into rich text box where line# begins.
        // This is useful to quickly map line index into character index.
        // There should be a method on RichTextBox to do this, but there's not.
        protected int[] m_lineOffset;

        // Width of the index bar on the left.
        int m_indexBarWidth;

        // Timer used to ensure glyphs are in sync with window position.
        private Timer timer1;

        protected static Glyphs m_glyphs;

        #region Source File collection management
        public static void ClearSourceFiles()
        {
            m_sourceList.Clear();
        }
        
        // Add the given source viewer to the global collection of source files.
        static void AddSourceViewer(SourceViewerBaseForm source)
        {
            Debug.Assert(source != null);
            object key = source.GetHashKey();
            Debug.Assert(key != null);

            m_sourceList.Add(key, source);
        }

        // Track the "active" source file. The UI can use this as the default
        // target for input (eg, who processes the stepping commands).
        static SourceViewerBaseForm m_ActiveSourceFile;

        // There's one SourceViewerForm instance for each source-file
        // Get the instance for the new source file.
        // Called on UI thread.
        public static SourceViewerForm GetSourceFile(MainForm parent,string path)
        {
            path = CommandBase.Shell.FileLocator.GetFileLocation(path);
            SourceViewerForm source = (SourceViewerForm)m_sourceList[path];
            if(source==null)
            {
                source = SourceViewerForm.Build(parent, path);
                if (source != null)
                {
                    AddSourceViewer(source);
                }
            }
            m_ActiveSourceFile = source;
            return source;
        }
        // There's one SourceViewerForm instance for each source-file
        // Get the instance for the new source file.
        // Called on UI thread.
        public static SourceViewerBaseForm GetSourceFile(MainForm parent, MDbgFunction function)
        {            
            SourceViewerBaseForm source = (SourceViewerBaseForm)m_sourceList[function];
            if (source == null)
            {
                source = new VirtualSourceViewerForm(parent, function);
                AddSourceViewer(source);
            }
            m_ActiveSourceFile = source;
            return source;
        }
        #endregion Source File collection management

        #region Mark current source
        // Called when shell is breaking. This lets all source files update 
        // to mark the current stopping location.
        // This may be called multiple times in a row to refresh the source window
        // (such as if the current frame / thread changes).
        // Called on UI thread.
        public static void OnBreak()
        {
            foreach(SourceViewerBaseForm s in m_sourceList.Values)
            {
                s.OnBreakWorker();
            }
        }

        // Called when the shell is about to run again. This lets all source files
        // clear any markings about break state.
        // Called on UI thread. 
        public static void OnRun()
        {
            foreach (SourceViewerBaseForm s in m_sourceList.Values)
            {
                s.ClearHighlight();
                s.OnRunWorker();
            }
        }

        protected abstract void OnBreakWorker();
        protected abstract void OnRunWorker();



        #region Raw Highlighting
        // Support for highlighting the current source line

        // Track most recent hilight so that we can clear it when source moves.
        // line = -1 if not set.
        int m_iHighlightStartOffset = -1;
        int m_iHiglightLength;

        // Clear the previous highlight set by HighlightStatement.
        protected void ClearHighlight()
        {
            if (this.m_iHighlightStartOffset != -1)
            {
                //int oldIdx = richText.SelectionStart;

                // Restore to default
                richText.Select(this.m_iHighlightStartOffset, this.m_iHiglightLength);
                richText.SelectionBackColor = Color.White;
                // Don't touch foreground color because that may trash previous formatting.
                //richText.SelectionColor = Color.Black;

                richText.SelectionLength = 0;

                this.m_iHighlightStartOffset = -1;
            }
        }
        
        // Highlight the statement at the current character (index, len).
        // These are in raw coordinates for the text box.
        // Clears previous highlight.
        void HighlightStatement(int index, int len, bool fActive)
        {
            ClearHighlight();

            //int oldIdx = richText.SelectionStart;

            this.m_iHighlightStartOffset = index;
            this.m_iHiglightLength = len;

            richText.Select(index, len);
            richText.SelectionBackColor = fActive ? Color.Yellow : Color.LightGreen;
            // Don't touch foreground color because that may trash previous formatting.
            //richText.SelectionColor = Color.Black;

            richText.SelectionLength = 0;
        }

        #endregion Raw Highlighting

        // Highlight the statement for the given Source Position
        protected void HighlightStatementAtPos(MDbgSourcePosition pos)
        {
            HighlightRangeWorker(
                pos.StartLine,pos.StartColumn,pos.EndLine,pos.EndColumn, 
                this.MainForm.IsCurrentSourceActive);
        }

        // Highlight an entire row
        protected void HighlightRow(int line, bool isActive)
        {            
            if (line < 1 || line > m_lineOffset.Length)               
            {
                throw new ArgumentOutOfRangeException("line", line, "Line is out of range");
            }           

            int startOff = GetOffsetFromPos(line, 0);
            int endOff = GetOffsetFromPos(line + 1, 0);

            this.HighlightStatement(startOff, endOff - startOff, isActive);

            ScrollToRow(line, line);
        }

        // Highlight a logical region in the source file. 
        // values are all 1-based, and refer to "logical" offsets (do not include the left Index bar).
        protected void HighlightRangeWorker(int startLine, int startCol, int endLine, int endCol, bool fActive)
        {
            if (endLine > m_lineOffset.Length)
            {
                throw new ArgumentOutOfRangeException("endLine", endLine, "endLine is out of range");
            }
            if (startLine < 1 || startLine > endLine)
            {
                throw new ArgumentOutOfRangeException("startLine", startLine, "startLine is out of range");
            }

                       

            // handle special case, where col==0 -- the compiler didn't emit line information.
            if(startCol==0)
                startCol = 1;
            if(endCol==0)
                endCol = startCol;

            int startOff = GetOffsetFromPos(startLine,startCol);            
            int endOff = GetOffsetFromPos(endLine,endCol);

            this.HighlightStatement(startOff, endOff - startOff, fActive);

            ScrollToRow(startLine, endLine);
        }

        // Scroll such that startLine and endLine are in view.
        void ScrollToRow(int startLine, int endLine)
        {
            // scroll to center.
            const int RANGE = 3;
            richText.Select(GetOffsetFromPos(startLine - RANGE, 0), 0);
            richText.Select(GetOffsetFromPos(endLine + RANGE, 0), 0);

            // Update IP markers
            InvalideGlyphBar();
        }

        #endregion Mark current source

        // Get the adjusted character indext from a (line, col) pair into the real document. 
        // This adjusts for the line numbers we added to the left margin of the document.
        // This is useful because many RichText operations require character indexes.
        private int GetOffsetFromPos(int line,int col)
        {
            if(col>1)
                col += (m_indexBarWidth - 1); // if col is specified >0 (but is 1-based)
            
            if(line<1)
                return 0;

            if(line>m_lineOffset.Length)
                return richText.TextLength;

            int lineOffset = m_lineOffset[line-1];
            int nextLineOfffset = (line==m_lineOffset.Length)?richText.TextLength:m_lineOffset[line];
            return (lineOffset+col>nextLineOfffset)?nextLineOfffset:lineOffset+col;
        }



        // Let source window handle F9 is pressed at a row.
        protected virtual void ToggleBreakpointAtLine(int row)
        {
            // Default source window does nothing.
        }
        // Let source window handle step-over (F10).
        // Return true if handled; false elsewise.
        protected virtual bool DoStepOver()
        {
            // Default source window does nothing.
            return false;
        }
        // Let source window handle step-over (F10).
        // Return true if handled; false elsewise.
        protected virtual bool DoStepIn()
        {
            // Default source window does nothing.
            return false;
        }

        // Handle key input to the source document.
        // This only gets called if the source window has focus. This should 
        // The Main Form also handles key input for most non-source specific commands (such as stepping)
        private void richText_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
            case Keys.F9:
                int row = 1+richText.GetLineFromCharIndex(richText.SelectionStart);
                this.ToggleBreakpointAtLine(row);
                break;
            
            default:
                return;
            }
            e.Handled=true;
        }

        // Handle key commands that don't require source focus (like Stepping).
        static public void HandleGlobalKeyCommand(System.Windows.Forms.KeyEventArgs e)
        {
            // Find th
            SourceViewerBaseForm f = m_ActiveSourceFile;
            if (m_ActiveSourceFile != null)
            {
                f.HandleGlobalKeyCommandWorker(e);
            }
        }

        // Have a given instance of the source window handle a keyboard input.
        protected void HandleGlobalKeyCommandWorker(System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F9:
                    int row = 1 + richText.GetLineFromCharIndex(richText.SelectionStart);
                    this.ToggleBreakpointAtLine(row);
                    break;

                case Keys.F10:
                    e.Handled = this.DoStepOver();
                    return;

                case Keys.F11:
                    if (e.Shift)
                    {
                        MainForm.AsyncProcessEnteredText("out");
                    }
                    else
                    {
                        e.Handled = this.DoStepIn();
                        return;
                    }
                    break;
                default:
                    return;
            }
            e.Handled = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // If there is an image and it has a location, 
            // paint it when the Form is repainted.
            base.OnPaint(e);

            DrawGlyphWorker(e);
        }

        protected virtual void DrawGlyphWorker(PaintEventArgs e)
        {
            // base class does nothing.
        }
       
        // Invalidate the Glyph Bar.
        public void InvalideGlyphBar()
        {
            this.Invalidate(new Rectangle(0, 0, MARGIN_X, this.Height));
        }

        
        // Helper to draw a glyph at the current row.   
        protected void DrawGlyphAtLine(PaintEventArgs e, Image image, int line)
        {
            if (line <= 0) return;

            Graphics g = e.Graphics;


            int index = m_lineOffset[line - 1];

            Point p = this.richText.GetPositionFromCharIndex(index);

            p.X = MARGIN_X - image.Width - 1;

            int FontHeight = 20;
            p.Y += ((FontHeight - image.Height) / 2);

            g.DrawImage(image, p);
        }

    

        private void SourceViewerForm_Paint(object sender, PaintEventArgs e)
        {            
            // Create a local version of the graphics object for the PictureBox.
            //Graphics g = e.Graphics;
        }

        private void richText_TextChanged(object sender, EventArgs e)
        {
        
        }

        // Width of the glyph bar in pixels
        const int MARGIN_X = 40;

        // Resize to maintain the glyph bar on the left.
        private void SourceViewerForm_Resize(object sender, EventArgs e)
        {
            Control control = (Control)sender;

            Size sizeParent = control.ClientSize;

            this.richText.Location = new System.Drawing.Point(MARGIN_X, 0);
            this.richText.Size = new System.Drawing.Size(sizeParent.Width - MARGIN_X, sizeParent.Height);
        }

        // We want to scroll the glyph bar when we scroll the text box.
        // We don't always get this event (eg, when the scroll bar is dragged), so we have a timer backstop as well.
        private void richText_VScroll(object sender, EventArgs e)
        {            
            this.InvalideGlyphBar();            
        }

        // Have a timer to forcibly update the glyph bar.
        // This guarantees we'll refresh the glyph bar even if we miss other notifications.
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.InvalideGlyphBar();
        } 


        // Class to hold resources for glyphs.
        // Singleton, shared by multiple SourceViewerForms.
        protected class Glyphs : IDisposable
        {
            public Glyphs()
            {
                Assembly thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();

                string stName = "gui.CurrentLineArrow.bmp";
                m_curLine = new Bitmap(thisAssembly.GetManifestResourceStream(
                        stName));

                stName = "gui.Breakpoint.bmp";
                m_breakpoint = new Bitmap(thisAssembly.GetManifestResourceStream(
                        stName));

                stName = "gui.NotCurrentLineArrow.bmp";
                this.m_notCurLine = new Bitmap(thisAssembly.GetManifestResourceStream(
                        stName));

                stName = "gui.UnboundBreakpoint.bmp";
                this.m_unboundBreakpoint = new Bitmap(thisAssembly.GetManifestResourceStream(
                        stName));
            }

            public void Dispose()
            {
                m_curLine.Dispose();
                m_notCurLine.Dispose();
                m_breakpoint.Dispose();
                m_unboundBreakpoint.Dispose();
            }

            Bitmap m_curLine;            
            public Bitmap CurrentLineArrow
            {
                get { return m_curLine; }
            }

            Bitmap m_notCurLine;
            public Bitmap NotCurrentLineArrow
            {
                get { return m_notCurLine; }
            }

            Bitmap m_breakpoint;
            public Bitmap Breakpoint
            {
                get { return m_breakpoint; }
            }

            Bitmap m_unboundBreakpoint;
            public Bitmap UnboundBreakpoint
            {
                get { return m_unboundBreakpoint; }
            }
        } // end class Glyphs



    } // end SourceViewerForm



} // end namespace
