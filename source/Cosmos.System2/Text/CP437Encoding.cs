//#define COSMOSDEBUG
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{
    /// <summary>
    /// CP437Enconding class, represent CP437 encoding. See also: <seealso cref="SingleByteEncoding"/>.
    /// </summary>
    internal class CP437Enconding : SingleByteEncoding
    {
        /// <summary>
        /// Debugger instance of the "System" ring with the "CP437 Encoding" tag.
        /// </summary>
        private static Debugger myDebugger = new Debugger("System", "CP437 Encoding");

        /// <summary>
        /// Create new instance of the <see cref="CP437Enconding"/> class.
        /// </summary>
        internal CP437Enconding()
        {
            myDebugger.SendInternal("CP437Enconding Setting CodePageTable only one time...");

            CodePageTable = new char[] {
                'Ç' , 'ü' , 'é' , 'â' , 'ä' , 'à' , 'å' , 'ç' , 'ê' , 'ë' , 'è' , 'ï' , 'î' , 'ì' , 'Ä' , 'Å' ,
                'É' , 'æ' , 'Æ' , 'ô' , 'ö' , 'ò' , 'û' , 'ù' , 'ÿ' , 'Ö' , 'Ü' , '¢' , '£' , '¥' , '₧' , 'ƒ' ,
                'á' , 'í' , 'ó' , 'ú' , 'ñ' , 'Ñ' , 'ª' , 'º' , '¿' , '⌐' , '¬' , '½' , '¼' , '¡' , '«' , '»' ,
                '░' , '▒' , '▓' , '│' , '┤' , '╡' , '╢' , '╖' , '╕' , '╣' , '║' , '╗' , '╝' , '╜' , '╛' , '┐' ,
                '└' , '┴' , '┬' , '├' , '─' , '┼' , '╞' , '╟' , '╚' , '╔' , '╩' , '╦' , '╠' , '═' , '╬' , '╧' ,
                '╨' , '╤' , '╥' , '╙' , '╘' , '╒' , '╓' , '╫' , '╪' , '┘' , '┌' , '█' , '▄' , '▌' , '▐' , '▀' ,
                'α' , 'ß' , 'Γ' , 'π' , 'Σ' , 'σ' , 'µ' , 'τ' , 'Φ' , 'Θ' , 'Ω' , 'δ' , '∞' , 'φ' , 'ε' , '∩' ,
                '≡' , '±' , '≥' , '≤' , '⌠' , '⌡' , '÷' , '≈' , '°' , '∙' , '·' , '√' , 'ⁿ' , '²' , '■' , '\x00A0'
            };
        }

        /// <summary>
        /// Get encoding body name.
        /// </summary>
        public override string BodyName => "IBM437";

        /// <summary>
        /// Get encoding codepage.
        /// </summary>
        public override int CodePage => 437;
    }
}
