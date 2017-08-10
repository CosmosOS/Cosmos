using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;

namespace WPFMachine
{
    public class FontInfo
    {
        public String Name { get; private set; }
        public FontFamily Family { get; private set; }

        private double _size;
        public double Size
        {
            get { return _size; }
            set
            {
                _size = value;
                PointSize = Size * (96.0 / 72.0);
            }
        }


        public double PointSize { get; private set; }
        public Typeface Typeface { get; set; }

        public FontInfo(String Name, double Size)
        {
            this.Name = Name;
            this.Size = Size;

            this.Family = new FontFamily(Name);
            this.Typeface = new Typeface(Name);
        }

        public FontInfo(String Name, double Size, FontFamily Family)
        {
            this.Name = Name;
            this.Size = Size;
            
            this.Family = Family;
            this.Typeface = Family.GetTypefaces().First();
            
        }
    }
}
