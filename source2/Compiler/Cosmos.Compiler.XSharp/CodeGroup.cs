using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
    public class CodeGroup {

        public void AssembleDataSection() {
        }

        public void Assemble() {
            var xThisType = this.GetType();
            foreach (var xType in xThisType.GetNestedTypes()) {
                if (xType.IsSubclassOf(typeof(CodeBlock))) {
                    var xCtor = xType.GetConstructor(new Type[0]);
                    var xBlock = (CodeBlock)(xCtor.Invoke(new Object[0]));

                    // Issue label for the routine
                    xBlock.Label = xThisType.Name + "_" + xType.Name;
                    // Assemble the routine itself
                    xBlock.Assemble();
                    // Issue the return for the routine
                    xBlock.Return();
                }
            }
        }

    }
}
