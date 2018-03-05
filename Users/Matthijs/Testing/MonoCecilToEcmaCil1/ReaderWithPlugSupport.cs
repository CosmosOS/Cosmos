using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using IL2CPU.API;

namespace MonoCecilToEcmaCil1
{
    public class ReaderWithPlugSupport: Reader
    {
        public void AddPlugAssembly(string path)
        {
            var xAssembly = AssemblyDefinition.ReadAssembly(path);
            foreach (ModuleDefinition xModule in xAssembly.Modules)
            {
                foreach (TypeDefinition xType in xModule.Types)
                {
                    TypeReference xTargetType=null;

                    foreach (CustomAttribute xAttrib in xType.CustomAttributes)
                    {
                        if (xAttrib.Constructor.DeclaringType.FullName == typeof(PlugAttribute).FullName)
                        {
                            
                            throw new NotImplementedException();
                            break;
                        }
                    }
                    //if (!xHasPlugAttrib)
                    //{
                    //    continue;
                    //}

                    //x
                }
            }
            throw new NotImplementedException();
        }

        private Dictionary<QueuedType, QueuedMethod> mPlugTypes = new Dictionary<QueuedType, QueuedMethod>();

        protected override void ScanMethodBody(QueuedMethod aMethod, EcmaCil.MethodMeta aMethodMeta)
        {
            base.ScanMethodBody(aMethod, aMethodMeta);
        }
    }
}