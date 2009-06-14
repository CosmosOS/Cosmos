using System;

namespace Cosmos.BuildEngine
{
    public class CompilationResult
    {
        public String[] Data;
        public String[] Code;

        public CompilationResult(String[] data, String[] code)
        {
            Data = data;
            Code = code;
        }
    }
}