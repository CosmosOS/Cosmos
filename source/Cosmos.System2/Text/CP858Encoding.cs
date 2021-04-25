//#define COSMOSDEBUG
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{
    /// <summary>
    /// CP858Enconding class, represent CP858 encoding. See also: <seealso cref="SingleByteEncoding"/>.
    /// </summary>
    internal class CP858Enconding : SingleByteEncoding
    {
        /// <summary>
        /// Debugger instance of the "System" ring with the "CP858 Encoding" tag.
        /// </summary>
        private static Debugger myDebugger = new Debugger("System", "CP858 Encoding");

        /// <summary>
        /// Create new instance of the <see cref="CP858Enconding"/> class.
        /// </summary>
        internal CP858Enconding()
        {
            myDebugger.SendInternal($"CP858Enconding Setting CodePageTable only one time...");

            CodePageTable = new char[]
            {
                'Ç', 'ü', 'é', 'â', 'ä', 'à', 'å', 'ç', 'ê', 'ë', 'è', 'ï', 'î', 'ì', 'Ä', 'Å',
                'É', 'æ', 'Æ', 'ô', 'ö', 'ò', 'û', 'ù', 'ÿ', 'Ö', 'Ü', 'ø', '£', 'Ø', '×', 'ƒ',
                'á', 'í', 'ó', 'ú', 'ñ', 'Ñ', 'ª', 'º', '¿', '®', '¬', '½', '¼', '¡', '«', '»',
                '░', '▒', '▓', '│', '┤', 'Á', 'Â', 'À', '©', '╣', '║', '╗', '╝', '¢', '¥', '┐',
                '└', '┴', '┬', '├', '─', '┼', 'ã', 'Ã', '╚', '╔', '╩', '╦', '╠', '═', '╬', '¤',
                'ð', 'Ð', 'Ê', 'Ë', 'È', '€', 'Í', 'Î', 'Ï', '┘', '┌', '█', '▄', '¦', 'Ì', '▀',
                'Ó', 'ß', 'Ô', 'Ò', 'õ', 'Õ', 'µ', 'þ', 'Þ', 'Ú', 'Û', 'Ù', 'ý', 'Ý', '¯', '´',
                '\u00AD', '±', '‗', '¾', '¶', '§', '÷', '¸', '°', '¨', '·', '¹', '³', '²', '■', '\u00A0'
            };
        }

        /// <summary>
        /// Get encoding body name.
        /// </summary>
        public override string BodyName => "IBM00858";

        /// <summary>
        /// Get encoding codepage.
        /// </summary>
        public override int CodePage => 858;
    }
}
