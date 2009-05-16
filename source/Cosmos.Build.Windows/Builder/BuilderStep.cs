using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Compiler.Builder
{
    //TODO consider IAsynch result for multi threaded
    public abstract class BuilderStep
    {
        protected readonly BuildOptions options;
        protected readonly string ToolsPath;
        protected readonly string AsmPath;

        //prob shouldnt use  but want to make the steps easy 
        protected BuilderStep(BuildOptions options)
        {
            this.options = options;

            ToolsPath = options.BuildPath + @"Tools\";
            AsmPath = ToolsPath + @"asm\";
        }


        internal BuildFileUtils buildFileUtils = new BuildFileUtils();

        //we need these events for instrumenting and if we want Asynch ones .
        //HACK must be a better way to hook up easy and not use static will do atm.
        static public event Action<string> Started;
        static public event Action<string, Object> Completed; //Leave these public so we dont need to worry about mem leaks


        protected virtual void Init()
        {
            OnStarted();
        }

        public abstract void Execute();

        protected virtual void Finish()
        {
            OnCompleted();
        }

        protected virtual void OnStarted()
        {
            if (Started != null)
                Started(this.GetType().ToString());

        }

        protected virtual void OnCompleted()
        {
            OnCompleted(null);
        }

        protected virtual void OnCompleted(Object obj)
        {
            if (Completed != null)
                Completed(this.GetType().ToString(), obj);

        }

        protected string BuildPath
        {
            get
            {
                return options.BuildPath; 
            }
        }

        //virtual' oobject BuildResult;

    }
}
