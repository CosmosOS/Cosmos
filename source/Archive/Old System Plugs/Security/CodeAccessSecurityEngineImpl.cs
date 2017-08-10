using System;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Security
{
    [Plug(TargetName = "System.Security.CodeAccessSecurityEngine, mscorlib", IsMicrosoftdotNETOnly = true)]
    public class CodeAccessSecurityEngineImpl
    {

        //TODO check if ref is linked right
        public void Check(object demand,
            [FieldType(Name = "System.Threading.StackCrawlMark, mscorlib")] ref AttributeTargets stackMark,
            bool isPermSet)
        {
            // no implementation yet, before threading not needed
        }
    }
}
