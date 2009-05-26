using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace IL2CPU.PostAssembler
{

    public  class LWAssemblyCollection : IEnumerable<LWInstruction>
    {

        string mAssemblyName;
        IDictionary<string, LWMethod> methods = new Dictionary<string, LWMethod>(); //lookup Method 
     //   List<LWMethod> methods = new List<LWMethod>();   //if needs to be sorted use a dictionary and list


        public LWAssemblyCollection(string assemblyName)
        {
            mAssemblyName = assemblyName;
        }

        public void Add(LWMethod method)
        {
            //methods.Add(method);
            methods.Add(method.Name, method);
        }


        //For inlining 
        public void Remove(LWMethod method)
        {
            //methods.Remove(method);
            methods.Remove(method.Name);
        }

        public IEnumerator<LWInstruction> GetEnumerator()
        {
            return new LWAssemblyEnumerator(this);
        }



        IEnumerator IEnumerable.GetEnumerator()
        {

            return GetEnumerator();
        }


        public IEnumerator<LWMethod> MethodEnumerator
        {
            get
            {
                return methods.Values.GetEnumerator();
            }
        }




    }
}
