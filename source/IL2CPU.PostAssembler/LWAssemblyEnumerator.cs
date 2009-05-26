using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace IL2CPU.PostAssembler
{
    public class LWAssemblyEnumerator :IEnumerator<LWInstruction>
    {
      //  LWAssemblyCollection mCollection;
        IEnumerator<LWMethod> methodEnumerator;
        IEnumerator<LWInstruction> instructionEnumerator;

       

        public LWAssemblyEnumerator(LWAssemblyCollection collection)
        {
            methodEnumerator = collection.MethodEnumerator;
        }

        public LWInstruction Current
        {
            get {return InstructionEnumerator.Current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
            return;
        }



        

        public bool MoveNext()
        {

            if (InstructionEnumerator.MoveNext() == false)
            {
                instructionEnumerator = null; 
                return methodEnumerator.MoveNext();

            }
            else
                return true;  

        }

        public void Reset()
        {
             methodEnumerator.Reset();
            instructionEnumerator = null; 
        }


        private IEnumerator<LWInstruction> InstructionEnumerator
        {
            get
            {
                if (instructionEnumerator == null)
                    instructionEnumerator = methodEnumerator.Current.Instructions.GetEnumerator();
                
                return instructionEnumerator; 
            }
        }
    }
}
