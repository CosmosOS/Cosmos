//#define COSMOSDEBUG
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{
    /// <summary>
    /// Represents the CP437 encoding.
    /// </summary>
    /// <remarks>
    /// See also: <seealso cref="SingleByteEncoding"/>.
    /// </remarks>
    internal class CP437Encoding : SingleByteEncoding
    {
        /// <summary>
        /// Create new instance of the <see cref="CP437Encoding"/> class.
        /// </summary>
        internal CP437Encoding()
        {
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

        public override string BodyName => "IBM437";

        public override int CodePage => 437;
    }
}
