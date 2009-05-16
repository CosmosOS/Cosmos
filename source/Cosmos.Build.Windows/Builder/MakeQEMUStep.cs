using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Indy.IL2CPU;

namespace Cosmos.Compiler.Builder
{
    public class MakeQEMUStep : BuilderStep
    {
        Process result;

                public MakeQEMUStep(BuildOptions options)
            : base(options)
        {
        }



        override public void Execute()
        {
            Init();

            bool aDebugger = (options.DebugMode != DebugMode.None);

            string aDebugComMode = (String)options.QEmuDebugComType[options.DebugComMode];

            object aNetworkCard = (String)options.QEmuNetworkCard[options.NetworkCard];
            object aAudioCard = (String)options.QEmuAudioCard[options.AudioCard];



            new MakeISOStep(options).Execute();  //TODO shouldnt builder make this ?

            //From v0.9.1 Qemu requires forward slashes in path
            string buildPath = BuildPath.Replace('\\', '/');

            buildFileUtils.RemoveFile(buildPath + "serial-debug.txt");
            // QEMU Docs - http://fabrice.bellard.free.fr/qemu/qemu-doc.html
            if (File.Exists(buildPath + "COM1-output.dbg"))
            {
                File.Delete(buildPath + "COM1-output.dbg");
            }
            if (File.Exists(buildPath + "COM2-output.dbg"))
            {
                File.Delete(buildPath + "COM2-output.dbg");
            }
            string xHDString = "";
            if (options.CreateHDImage)
            {
                if (File.Exists(buildPath + "hda.img"))
                {
                    xHDString += "-hda \"" + buildPath + "hda.img\" ";
                }
                if (File.Exists(buildPath + "hdb.img"))
                {
                    xHDString += "-hdb \"" + buildPath + "hdb.img\" ";
                }
                if (File.Exists(buildPath + "hdd.img"))
                {
                    xHDString += "-hdb \"" + buildPath + "hdd.img\" ";
                }
            }

            var xProcess = Global.Call(ToolsPath + @"qemu\qemu.exe"
                // HD image
                , xHDString
                // Path for BIOS, VGA BIOS, and keymaps
                + " -L ."
                // CD ROM image
                + " -cdrom \"" + buildPath + "Cosmos.iso\""
                // Boot CD ROM
                + " -boot d"
                // Audio hardware
                + (String.IsNullOrEmpty(aAudioCard as String) ? "" : " -soundhw " + aAudioCard)
                // Setup serial port
                // Might allow serial file later for post debugging of CPU
                // etc since serial to TCP on a byte level is likely highly innefficient
                // with the packet overhead
                //
                // COM1
                + (aDebugger ? " " + aDebugComMode : " -serial null")
                // COM2
                + " -serial file:\"" + buildPath + "COM2-output.dbg\""
                // Enable acceleration if we are not using GDB
                + (options.UseGDB ? " -S -s" : " -kernel-kqemu")
                // Ethernet card
                + (String.IsNullOrEmpty(aNetworkCard as String) ? "" : string.Format(" -net nic,model={0},macaddr=52:54:00:12:34:57", aNetworkCard))
                //+ " -redir tcp:5555::23" //use f.instance 'telnet localhost 5555' or 'http://localhost:5555/' to access machine
                + (String.IsNullOrEmpty(aNetworkCard as String) ? "" : (options.UseNetworkTAP ? " -net tap,ifname=CosmosTAP" : " -net user")) //requires TAP installed on development computer
                , ToolsPath + @"qemu", false, true);

            if (options.UseGDB)
            {
                //TODO: If the host is really busy, sometimes GDB can run before QEMU finishes loading.
                //in this case, GDB says "program not running". Not sure how to fix this properly.
                // Add a sleep? :)
                Global.Call(ToolsPath + "gdb.exe"
                    , buildPath + @"output.bin" + " --eval-command=\"target remote:1234\" --eval-command=\"b _CODE_REQUESTED_BREAK_\" --eval-command=\"c\""
                    , ToolsPath + @"qemu\", false, false);
            }


            Finish(); //note could pass via result
        }

        public Process Result
        {
            get { return result; }

        }



    }
}
