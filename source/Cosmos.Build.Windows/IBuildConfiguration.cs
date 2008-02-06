using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Windows
{
    public interface IBuildConfiguration
    {
        Builder.Target Target { get; set; }
        bool Compile { get; set; }
    }

    public class BuildConfiguration : IBuildConfiguration
    {
        public BuildConfiguration(Builder.Target target)
            : this(target, true)
        {
        }

        public BuildConfiguration(Builder.Target target, bool compile)
        {
            this.target = target;
            this.compile = compile;
        }

        #region IBuildConfiguration Members

        private Builder.Target target;
        public Builder.Target Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
            }
        }

        private bool compile;
        public bool Compile
        {
            get
            {
                return compile;
            }
            set
            {
                compile = value;
            }
        }

        #endregion
    }
}
