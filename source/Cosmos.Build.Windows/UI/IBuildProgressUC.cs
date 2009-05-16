using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Builder
{
    public interface IBuildProgressUC
    {
        void NewError (string error);
        void BuildProgressChanged(BuildProgress progress); 
    }


    //DO not pump to UI unless there is more than 1% change! 
    //not a great design.
    public class BuildProgress
    {
        public int? MaxMethods =0;
        public int? MethodsProcessed =0;
        public int? MaxFields =0;
        public int? FieldsProcessed  =0;
        public string Step = "Init";

     


        public int MethodProgressPercent
        {
            get
            {
                if (MaxMethods == null || MethodsProcessed == null || MaxMethods.Value < 1)
                    return 0;

                return MethodsProcessed.Value * 100 / MaxMethods.Value;
            }
        }

        public int FieldProgressPercent
        {
            get
            {
                if (MaxFields == null || FieldsProcessed == null || MaxFields.Value < 1)
                    return 0;

                return FieldsProcessed.Value * 100 / MaxFields.Value;
            }
        }

    }
}
