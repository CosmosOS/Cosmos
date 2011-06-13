using System;
using System.Collections.Generic;
using System.Text;
using Orvid.Graphics;

namespace OForms.Windows
{
    /// <summary>
    /// The possible states of an OWindow.
    /// </summary>
    public enum OWindowState
    {
        /// <summary>
        /// The minimized state.
        /// </summary>
        Minimized,
        /// <summary>
        /// The maximized state.
        /// </summary>
        Maximized,
        /// <summary>
        /// The open state.
        /// </summary>
        Open,
        /// <summary>
        /// The active state.
        /// </summary>
        Active
    }

    /// <summary>
    /// This class describes a single window.
    /// </summary>
    public class OWindow : OControl
    {
        public override void Draw(Image img)
        {
            DrawWindowBorder();
            for (int i = 0; i < Controls.Count; i++)
            {
                //Controls[i].Draw();
            }
        }

#warning TODO: Finish this method.
        private void DrawWindowBorder()
        {
        }

        protected LinkedList<OControl> controls = new LinkedList<OControl>();
        /// <summary>
        /// The controls that this window contains.
        /// </summary>
        public LinkedList<OControl> Controls
        {
            get
            {
                return controls;
            }
        }
        protected OWindowState windowState = OWindowState.Active;
        /// <summary>
        /// This is to determine if the window
        /// was maximized when it's state was changed.
        /// </summary>
        private bool maxWhenChanged = false;
        /// <summary>
        /// The state of the window.
        /// </summary>
        public OWindowState WindowState
        {
            get
            {
                return windowState;
            }
            set
            {
                if (windowState != OWindowState.Minimized)
                {
                    if (windowState == OWindowState.Maximized)
                    {
                        maxWhenChanged = true;
                    }
                    else
                    {
                        maxWhenChanged = false;
                    }
                    windowState = value;
                }
                else
                {
                    if (maxWhenChanged)
                    {
                        windowState = OWindowState.Maximized;
                    }
                    else
                    {
                        windowState = OWindowState.Active;
                    }
                }
            }
        }
    }
}
