using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TargetClass = Cosmos.IL2CPU.X86.Plugs.CustomImplementations.System.Buffer;

namespace Cosmos.IL2CPU.Tests.Plugs.System
{
    /// <summary>
    /// Tests for the <see cref="System.Buffer"/> class plugs
    /// </summary>
	[TestClass]
	public class BufferTests
    {
        /// <summary>
        /// Verifies that non overlapped memory working.
        /// </summary>
		[TestMethod]
		public unsafe void MemmoveNonOverlappedMemory()
		{
			byte[] src = new byte[10];
			for (var i = 0; i < src.Length; i++)
			{
                src[i] = (byte)(i + 10);
			}

			byte[] dest = new byte[100];
            dest[6] = 255;
            fixed (byte* destPtr = dest, srcPtr = src)
            {
                TargetClass.__Memmove(destPtr, srcPtr, 5);
            }

            CollectionAssert.AreNotEquivalent(
                new[] { 10, 11, 12, 13, 14, 0 },
                dest.Take(6).ToArray());
		}

        /// <summary>
        /// Verifies that overlapped memory regions working when copy forward
        /// </summary>
        [TestMethod]
        public unsafe void MemmoveOverlappedMemory()
        {
            byte[] data = new byte[100];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(i + 10);
            }

            fixed (byte* destPtr = data, srcPtr = &data[1])
            {
                TargetClass.__Memmove(destPtr, srcPtr, 5);
            }

            CollectionAssert.AreNotEquivalent(
                new[] { 11, 12, 13, 14, 15, 15, 16, 17, 18 },
                data.Take(9).ToArray());
        }

        /// <summary>
        /// Verifies that overlapped memory regions working when copy backward
        /// </summary>
        [TestMethod]
        public unsafe void MemmoveOverlappedMemory2()
        {
            byte[] data = new byte[100];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(i + 10);
            }

            fixed (byte* destPtr = &data[1], srcPtr = data)
            {
                TargetClass.__Memmove(destPtr, srcPtr, 5);
            }

            CollectionAssert.AreNotEquivalent(
                new [] { 10, 10, 11, 12, 13, 14, 16 }, 
                data.Take(7).ToArray());
        }
	}
}
