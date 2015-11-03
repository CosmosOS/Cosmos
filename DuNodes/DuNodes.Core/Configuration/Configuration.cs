using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Common.Extensions;

namespace DuNodes.System.Configuration
{
    public static class Configuration
    {
        public static void LoadConfiguration()
        {
       //   if (!HAL.FileSystem.Base.FileSystem.Root.DirectoryExist("/", "DNSYS"))
                HAL.FileSystem.Base.FileSystem.Root.makeDir("/DNSYS", "DN");

            if (HAL.FileSystem.Base.FileSystem.Root.FileExist("/DNSYS", "Settings.sys"))
            {
                var read = HAL.FileSystem.Base.FileSystem.Root.readFile("/DNSYS/Settings.sys");
                var confStr = read.GetUtf8String(0, (uint)read.Length);
                var splittedConf = confStr.Split(';');
                for (int i = 0; i < splittedConf.Length; i++)
                {
                    var splittedLine = splittedConf[i].Split(':');
                    switch (splittedLine[0])
                    {
                        case "KeyLayout":
                            ENV.currentMapKey = splittedLine[1];
                            break;
                    }
                }
                Console.Console.WriteLine("...Settings.sys loaded", ConsoleColor.Blue, true);

            }
            else
            {
                var content = "KeyLayout:1;";
                var contentByte = content.GetUtf8Bytes(0, (uint)content.Length);
                HAL.FileSystem.Base.FileSystem.Root.saveFile(contentByte, "/DNSYS/Settings.sys", "DN");
                ENV.currentMapKey = "1";
                Console.Console.WriteLine("...Settings.sys created", ConsoleColor.Blue, true);

            }
        }
    }
}
