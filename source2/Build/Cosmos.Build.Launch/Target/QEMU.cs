using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Launch.Target {
  public class QEMU : Target {
      //TODO: This is the old QEMU stuff, need to remove during cleanup
    public override void Execute() {
      string xBuildPath = @"C:\source\Cosmos\Build\";
      string xToolsPath = xBuildPath + @"Tools\";
      bool xEnableDebugger = false;

      //"Named pipe: Cosmos Debugger as client, QEmu as server", "-serial pipe:CosmosDebug");
      //"Named pipe: Cosmos Debugger as server, QEmu as client", "-serial pipe_client:CosmosDebug");
      //"TCP: Cosmos Debugger as client, QEmu as server on port 4444", "-serial tcp:127.0.0.1:4444,server");
      //"TCP: Cosmos Debugger as server on port 4444, QEmu as client", "-serial tcp:127.0.0.1:4444");
      string xDebugComMode = "";

      var xProcess = Process.Execute(xToolsPath + @"qemu\qemu.exe"
        // HD image
          , ""//xHDString
        // Path for BIOS, VGA BIOS, and keymaps
          + " -L ."
        // CD ROM image
          + " -cdrom \"" + xBuildPath + "Cosmos.iso\""
        // Boot CD ROM
          + " -boot d"
        // Audio hardware
      //    + (String.IsNullOrEmpty(aAudioCard as String) ? "" : " -soundhw " + aAudioCard)
        // Setup serial port
        // Might allow serial file later for post debugging of CPU
        // etc since serial to TCP on a byte level is likely highly innefficient
        // with the packet overhead
        //
        // COM1
          + (xEnableDebugger ? " " + xDebugComMode : " -serial null")
        // COM2
      //    + " -serial file:\"" + buildPath + "COM2-output.dbg\""
        // Enable acceleration if we are not using GDB
      //    + (options.UseGDB ? " -S -s" : " -kernel-kqemu")
      //  // Ethernet card
      //    + (String.IsNullOrEmpty(aNetworkCard as String) ? "" : string.Format(" -net nic,model={0},macaddr=52:54:00:12:34:57", aNetworkCard))
      //  //+ " -redir tcp:5555::23" //use f.instance 'telnet localhost 5555' or 'http://localhost:5555/' to access machine
      //    + (String.IsNullOrEmpty(aNetworkCard as String) ? "" : (options.UseNetworkTAP ? " -net tap,ifname=CosmosTAP" : " -net user")) //requires TAP installed on development computer
          , xToolsPath + @"qemu", false, true);
    }
  }
}
