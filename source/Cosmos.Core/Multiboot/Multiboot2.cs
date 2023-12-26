/*
* PROJECT:          Cosmos Development
* CONTENT:          Multiboot2 class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
* RESOURCES:        https://www.gnu.org/software/grub/manual/multiboot2/multiboot.html
*/

using Cosmos.Core.Multiboot.Tags;
using IL2CPU.API.Attribs;

namespace Cosmos.Core.Multiboot
{
    /// <summary>
    /// Multiboot2 class. Used for multiboot parsing.
    /// </summary>
    public unsafe class Multiboot2
    {
        #region Properties

        public static BasicMemoryInformation* BasicMemoryInformation { get; set; }
        public static bool IsVBEAvailable => Framebuffer->Address != 753664; // Some kinda default number.
        public static Framebuffer* Framebuffer { get; set; }
        public static MemoryMap* MemoryMap { get; set; }
        public static EFI64* EFI64 { get; set; }
        public static AcpiOld* AcpiOld { get; set; }
        public static AcpiNew* AcpiNew { get; set; }

        #endregion

        #region Methods

        /// /// <summary>
        /// Parse multiboot2 structure
        /// </summary>
        public static void Init()
        {
            if (!isInitialized)
            {
                isInitialized = true;

                uint MbAddress = GetMBIAddress();

                MB2Tag* Tag;

                for (Tag = (MB2Tag*)(MbAddress + 8); Tag->Type != 0; Tag = (MB2Tag*)((byte*)Tag + (Tag->Size + 7 & ~7)))
                {
                    switch (Tag->Type)
                    {
                        case 4:
                            BasicMemoryInformation = (BasicMemoryInformation*)Tag;
                            break;
                        case 6:
                            MemoryMap = (MemoryMap*)Tag;
                            break;
                        case 7:
                            // Ignore because we use Framebuffer tags now :)
                            //VBEInfo = (VBEInfo*)Tag;
                            break;
                        case 8:
                            Framebuffer = (Framebuffer*)Tag;
                            break;
                        case 12:
                            EFI64 = (EFI64*)Tag;
                            break;
                        case 14:
                            AcpiOld = (AcpiOld*)Tag;
                            break;
                        case 15:
                            AcpiNew = (AcpiNew*)Tag;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Get MemLower
        /// </summary>
        /// <returns>MemLower</returns>
        public static uint GetMemLower()
        {
            return BasicMemoryInformation->MemLower;
        }

        /// <summary>
        /// Get MemUpper
        /// </summary>
        /// <returns>MemUpper</returns>
        public static uint GetMemUpper()
        {
            return BasicMemoryInformation->MemUpper;
        }

        /// <summary>
        /// Checks if Multiboot returned a memory map
        /// </summary>
        /// <returns>True if is available, false if not</returns>
        public static bool MemoryMapExists() => MemoryMap != null;

        /// /// <summary>
        /// Get Multiboot address. Plugged.
        /// </summary>
        /// <returns>The Multiboot Address</returns>
        [PlugMethod(PlugRequired = true)]
        public static uint GetMBIAddress() => throw null;

        #endregion

        #region Fields

        private static bool isInitialized = false;

        #endregion
    }
}