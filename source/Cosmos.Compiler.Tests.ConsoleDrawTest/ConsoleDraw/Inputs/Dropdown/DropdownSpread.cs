using ConsoleDraw.Windows.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDraw.Inputs
{
    public class DropdownSpread : FullWindow
    {
        private List<DropdownItem> DropdownItems = new List<DropdownItem>();
        public Dropdown root;

        public DropdownSpread(int Xpostion, int Ypostion, List<String> options, Window parentWindow, Dropdown root)
            : base(Xpostion, Ypostion, 20, options.Count(), parentWindow)
        {
            for (var i = 0; i < options.Count(); i++)
            {
                var item = new DropdownItem(options[i], Xpostion + i, "option" + i, this);

                item.Action = delegate() {
                    root.Text = ((DropdownItem)CurrentlySelected).Text;
                    root.Draw();
                };

                DropdownItems.Add(item);
            }

            Inputs.AddRange(DropdownItems);

            CurrentlySelected = DropdownItems.FirstOrDefault(x => x.Text == root.Text);

            BackgroundColour = ConsoleColor.DarkGray;
            Draw();
            MainLoop();
        }
    }
}
