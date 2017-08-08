using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFMachine
{
    public class GameSelectedEventArgs : EventArgs
    {
        public String StoryFileName { get; private set; }
        public Frotz.Blorb.Blorb BlorbFile { get; private set; }

        public GameSelectedEventArgs(String StoryFileName, Frotz.Blorb.Blorb BlorbFile)
        {
            this.StoryFileName = StoryFileName;
            this.BlorbFile = BlorbFile;
        }
    }
}
