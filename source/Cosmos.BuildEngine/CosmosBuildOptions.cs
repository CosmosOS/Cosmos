using System;
using System.Collections.Generic;

namespace Cosmos.BuildEngine
{
    public abstract class BuildOption
    {
        public readonly String OptionName;
        public readonly String OptionDescription;

        public BuildOption(String Name, String Description)
        {
            OptionName = Name;
            OptionDescription = Description;
        }
    }
    public sealed class BuildOption_Boolean : BuildOption
    {
        public readonly bool OptionValue;

        public BuildOption_Boolean(String Name, String Description, bool DefaultValue)
            : base(Name, Description)
        {
            OptionValue = DefaultValue;
        }
    }
    public sealed class BuildOption_SelectOne : BuildOption
    {
        public readonly String OptionValue;
        public readonly String[] OptionOptions;

        public BuildOption_SelectOne(String Name, String Description, String[] Options, int DefaultOption)
            : base(Name, Description)
        {
            OptionOptions = Options;
            OptionValue = OptionOptions[DefaultOption];
        }
    }
    public sealed class BuildOption_String : BuildOption
    {
        public readonly String OptionValue;
        public delegate bool dVerifyOption(String OptionValue, out String VerificationFailureMessage);
        public readonly dVerifyOption OptionVerifier;

        public BuildOption_String(String Name, String Description, String DefaultValue)
            : base(Name, Description)
        {
            OptionValue = DefaultValue;
            OptionVerifier = delegate(String optionvalue, out String VerificationFailureMessage)
            {
                VerificationFailureMessage = "";
                return true;
            };
        }
        public BuildOption_String(String Name, String Description, String DefaultValue, dVerifyOption Verifier)
            : base(Name, Description)
        {
            OptionValue = DefaultValue;
            OptionVerifier = Verifier;
        }
    }
}