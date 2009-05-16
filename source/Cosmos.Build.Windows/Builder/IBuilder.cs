using System;
using System.Diagnostics;
using Indy.IL2CPU;
namespace Cosmos.Compiler.Builder
{
    interface IBuilder
    {

        
        void BeginCompile(BuildOptions options);
        
        event Action CompileCompleted;  //handle UI 
        event Action BuildCompleted;

        event Action<BuildProgress> BuildProgress;
        //event Action<int, int> CompilingMethods;
        //event Action<int, int> CompilingStaticFields;
        event Action<LogSeverityEnum, string> LogMessage;

        string BuildPath {get;}

        DebugWindow DebugWindow {get;}   //HACK pass in event


        //string[] GetPlugs();
        //void Link();
        //event Action<Indy.IL2CPU.LogSeverityEnum, string> LogMessage;
        //void MakeISO();
        //void MakePXE();
        //Process MakeQEMU(bool aUseHDImage, bool aGDB, bool aDebugger, string aDebugComMode, bool aUseNetworkTap, object aNetworkCard, object aAudioCard);
        //void MakeUSB(char aDrive);
        //void MakeVHD();
        //void MakeVMWare(bool useVMWareServer);
        //void MakeVPC();
    }
}
