using System.Text;

using Cosmos.IL2CPU.API;

namespace Cosmos.Core_Plugs.System.Text
{
    [Plug(Target = typeof(ASCIIEncoding))]
    public class ASCIIEncodingImpl
    {

        // TODO: remove this function
        // old comment :Plug string.GetStringFromEncoding instead
        /*public static string GetString(byte[] aBytes, int aIndex, int aCount) {
          if (aBytes.Length == 0) {
            return string.Empty;
          } else {
            return new string(Encoding.ASCII.GetChars(aBytes, aIndex, aCount));
            /*var xChars = new char[aBytes.Length];
            for (int i = 0; i < aBytes.Length; i++) {
              xChars[i] = (char)aBytes[i];
            }
            return new string(xChars);
          }
        }*/
    }
}
