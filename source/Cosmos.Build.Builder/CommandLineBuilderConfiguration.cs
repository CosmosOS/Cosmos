using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cosmos.Build.Builder
{
    internal class CommandLineBuilderConfiguration : IBuilderConfiguration
    {
        public bool NoVsLaunch => GetSwitch();
        public bool UserKit => GetSwitch();
        public string VsPath => GetOption();

        private readonly Dictionary<string, string> _args;

        public CommandLineBuilderConfiguration(string[] args)
        {
            _args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var arg in args)
            {
                var keyValue = arg.Split('=');

                if (keyValue.Length > 0)
                {
                    var key = keyValue[0].Remove(0, 1);

                    _args.Add(key, null);

                    if (keyValue.Length > 1)
                    {
                        _args[key] = keyValue[1];
                    }
                }
            }
        }

        private bool GetSwitch([CallerMemberName] string name = null) =>
            _args.ContainsKey(name) && !String.Equals(_args[name], "False", StringComparison.OrdinalIgnoreCase);

        private string GetOption([CallerMemberName] string name = null)
        {
            if (_args.TryGetValue(name, out var value))
            {
                return value;
            }

            return null;
        }
    }
}
