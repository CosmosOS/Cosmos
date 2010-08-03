using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common
{
    public class BuildProperties : PropertiesBase
    {

        public String OutputPath
        {
            get { return this.GetProperty("OutputPath"); }
            set { this.SetProperty("OutputPath", value); }
        }
        public TargetHost Target
        {
            get { return this.GetProperty("BuildTarget", TargetHost.VMWare); }
            set { this.SetProperty("BuildTarget", value); }
        }
        public Framework Framework
        {
            get { return this.GetProperty("Framework", Common.Framework.MicrosoftNET); }
            set { this.SetProperty("Framework", value); }
        }
        public Boolean UseInternalAssembler
        {
            get { return this.GetProperty("UseInternalAssembler", false); }
            set { this.SetProperty("UseInternalAssembler", value); }
        }
        public VMwareFlavor VMWareFlavor {
            get { return GetProperty("VMWareFlavor", VMwareFlavor.Player); }
            set { SetProperty("VMWareFlavor", value); }
        }
    }
}