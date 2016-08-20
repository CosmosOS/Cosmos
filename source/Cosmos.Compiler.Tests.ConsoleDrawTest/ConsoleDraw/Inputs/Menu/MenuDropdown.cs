using ConsoleDraw.Windows.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDraw.Inputs
{
    public class MenuDropdown : FullWindow
    {
        private List<MenuItem> MenuItems;
        
        public MenuDropdown(int Xpostion, int Ypostion, List<MenuItem> menuItems, Window parentWindow)
            : base(Xpostion, Ypostion, 20, menuItems.Count() + 2, parentWindow)
        {

            for (var i = 0; i < menuItems.Count(); i++)
            {
                menuItems[i].ParentWindow = this;
                menuItems[i].Width = this.Width - 2;
                menuItems[i].Xpostion = Xpostion + i + 1;
                menuItems[i].Ypostion = this.PostionY + 1;
            }

            MenuItems = menuItems;


            Inputs.AddRange(MenuItems);
            
            CurrentlySelected = MenuItems.FirstOrDefault();

            BackgroundColour = ConsoleColor.DarkGray;
            Draw();
            MainLoop();
        }

        
    }
}
