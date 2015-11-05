using System;
using System.Collections.Generic;
using DuNodes.System.Core;

namespace DuNodes.System.Console
{
    public static partial class Console
    {
        public partial class Menu
        {
            public List<Entry> entries { get; set; }
            public string Title { get; set; }

            public Menu(string title)
            {
                entries = new List<Entry>();
                Title = title;
            }

            public string Show()
            {
                //Show the console menu and navigate into until exit.
                var entriesLevel = entries;
                var exit = false;
                while (exit == false)
                {
                    Clear();
                    WriteLine("-------" + Title + "-------", ConsoleColor.Blue,true);
                    for (int i = 0; i < entriesLevel.Count; i++)
                    {
                        WriteLine(i + " - " + entriesLevel[i].text);
                    }
                    Console.Write("Type your choice : ");
                    var choice = ReadLine();
                    var choiceint = Int32.Parse(choice);

                    while (choiceint > entriesLevel.Count - 1)
                    {
                        WriteLine("Option does not exist.");
                        Console.Write("Type your choice : ");
                        choice = ReadLine();
                        choiceint = Int32.Parse(choice);
                    }

                    var selected = entriesLevel[choiceint];
                    if (selected.isBack)
                    {
                        entriesLevel = entries;
                    }else if (selected.isExit)
                    {
                        exit = true;
                       
                    }else if (selected.isExecute)
                    {
                        return selected.executeValue;
                    }
                    else
                    {
                     
                        entriesLevel = new List<Entry>();

                        entriesLevel = selected.InnerEntries;
                   
                    }
                }
                return "";
            }

        }
    }
}
