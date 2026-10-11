// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.Core.Security;

namespace Cosmos.Kernel.Plugs.System.Security.Cryptography;

/// <summary>
/// Backs <c>System.Security.Cryptography.RandomNumberGenerator</c> with the
/// kernel's CSPRNG. On Linux the BCL reaches OpenSSL through
/// <c>Interop.Crypto.GetRandomBytes</c>, a library the kernel does not carry:
/// left unplugged, a kernel that touches <c>RandomNumberGenerator</c> fails to
/// link (undefined <c>SystemNative_LoadLibrary</c> and friends, from the lazy
/// P/Invoke resolver). <c>Fill</c>, <c>GetBytes</c>, <c>GetInt32</c>,
/// <c>GetNonZeroBytes</c> and the instance from <c>Create</c> all end in this
/// one private member, so plugging it also keeps <c>Interop.Crypto</c>'s
/// static constructor, which would initialize OpenSSL, out of the image.
/// BouncyCastle seeds every <c>SecureRandom</c> through it.
/// </summary>
[Plug("System.Security.Cryptography.RandomNumberGeneratorImplementation")]
public static class RandomNumberGeneratorImplementationPlug
{
    /// <summary>
    /// Writes <paramref name="count"/> random bytes at <paramref name="pbBuffer"/>;
    /// the BCL only calls it with a positive count. The target returns
    /// <see langword="void"/>, and so must this plug: a plug whose return type
    /// disagrees with its target patches in IL that ILC compiles to a body
    /// that always throws.
    /// </summary>
    [PlugMember]
    internal static unsafe void GetBytes(byte* pbBuffer, int count)
    {
        KernelRandom.Fill(pbBuffer, count);
    }
}
