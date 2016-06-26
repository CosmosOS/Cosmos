using Cosmos.IL2CPU.Plugs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Plugs.System.Security.Cryptography
{
    [Plug(Target = typeof(global::System.Security.Cryptography.CryptoConfig))]
    class CryptoConfigImpl
    {

	///<summary>
	///The stock .NET constructor for CryptoConfig deals with dictionaries we don't need.
	///</summary>		
	public static void CCtor() {

	}

        /// <summary>Indicates whether the runtime should enforce the policy to create only Federal Information Processing Standard (FIPS) certified algorithms.</summary>
        /// <returns>true to enforce the policy; otherwise, false. </returns>
        public static bool AllowOnlyFipsAlgorithms
        {
            get
            {
                return false;
            }
        }

    }
}
