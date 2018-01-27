//#define COSMOSDEBUG
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{
    internal class CP437Enconding : SingleByteEncoding
    {
        private static Debugger myDebugger = new Debugger("System", "CP437 Encoding");

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

        public override string BodyName => "IBM437";

        public override int CodePage => 437;
    }
}
