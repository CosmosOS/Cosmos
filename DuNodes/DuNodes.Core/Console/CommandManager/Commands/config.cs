using System;
using System.Collections.Generic;
using Cosmos.HAL.BlockDevice;
using DuNodes.HAL.Extensions;
using DuNodes.HAL.FileSystem.Base;
using DuNodes.System.Core;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class config : CommandBase
    {
        public override void launch(string[] args)
        {
            Console.Menu menu = new Console.Menu("Configuration menu");
            var keyboardLayoutEntry = new Console.Menu.Entry("Set the keyboard layout");
            var DriveEntry = new Console.Menu.Entry("Format the current partition (cause reboot)");
            var AZERTY = new Console.Menu.Entry("Set the keyboard layout to AZERTY", true, "setAZERTY");
            var QWERTY = new Console.Menu.Entry("Set the keyboard layout to QWERTY", true, "setQWERTY");
            var Format = new Console.Menu.Entry("Confirm format current partition (cause reboot)", true, "Format");
            var BackMenu = new Console.Menu.Entry("Back to main menu", false, "", true);
            var ExitMenu = new Console.Menu.Entry("Exit configuration Menu", false, "",false, true);
            keyboardLayoutEntry.InnerEntries.Add(AZERTY);
            keyboardLayoutEntry.InnerEntries.Add(QWERTY);
            keyboardLayoutEntry.InnerEntries.Add(BackMenu);
            keyboardLayoutEntry.InnerEntries.Add(ExitMenu);
            DriveEntry.InnerEntries.Add(Format);
            DriveEntry.InnerEntries.Add(BackMenu);
            DriveEntry.InnerEntries.Add(ExitMenu);
            menu.entries.Add(keyboardLayoutEntry);
            menu.entries.Add(DriveEntry);
            menu.entries.Add(ExitMenu);

            var returnValue = menu.Show();

            switch (returnValue)
            {
                case "setAZERTY":
                    ENV.currentMapKey = "1";
                    KeyBoardLayout.SwitchKeyLayoutByString(ENV.currentMapKey);
                    break;

                case "setQWERTY":
                    ENV.currentMapKey = "0";
                    KeyBoardLayout.SwitchKeyLayoutByString(ENV.currentMapKey);
                    break;

                case "Format":
                    System.Console.Console.WriteLine("Reboot will occur after format... ", ConsoleColor.Blue, true);
                    System.Console.Console.WriteLine("Please type Volume Name. :  ", ConsoleColor.Blue, true);
                    new DNFS(((Partition)BlockDevice.Devices[0])).Format(System.Console.Console.ReadLine());
                    KernelExtensionsHAL.Reboot();
                    break;

            }

            Configuration.Configuration.saveConfiguration();
            //Save configuration
        }

        public override void cancelled()
        {
            throw new NotImplementedException();
        }

        public override void pause()
        {
            throw new NotImplementedException();
        }

        public override void finished()
        {
            throw new NotImplementedException();
        }


    }
}
